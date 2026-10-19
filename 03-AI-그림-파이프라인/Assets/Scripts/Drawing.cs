using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LineRenderer를 전혀 사용하지 않는다.
/// drawingPanel(=Paper) 위에 있는 RawImage에 연결된 Texture2D에
/// 직접 픽셀을 찍어서 그림을 그린다.
///
/// 이 방식이 이전 LineRenderer 방식과 근본적으로 다른 점:
/// - Main Camera가 렌더링하는 3D world 좌표계를 전혀 사용하지 않는다.
/// - Canvas의 Render Mode(Overlay/Camera)와 무관하게 항상 동작한다.
/// - "카메라 절두체 밖으로 좌표가 벗어난다"는 문제 자체가 발생할 수 없다
///   (텍스처 픽셀 좌표는 카메라와 아무 관계가 없기 때문).
/// </summary>
public class Drawing : MonoBehaviour
{
    [Header("Drawing")]
    [SerializeField] private float minDistance = 2f; // 픽셀 단위

    [Header("UI")]
    [SerializeField] public RectTransform drawingPanel; // Paper의 RectTransform
    [SerializeField] private RawImage drawingSurface;   // Paper에 붙어있는 RawImage. 비워두면 drawingPanel에서 자동으로 찾는다.

    [Header("색상 팔레트 (Hex 코드)")]
    // 팔레트 UI에 나열된 순서와 반드시 같은 순서로 맞출 것.
    // 순서: Eraser(흰색), Black, Brown, Skin, Pink, Purple, Blue, Green, Yellow, Orange, Red
    [SerializeField]
    private string[] colorHexCodes = new string[]
    {
        "#FFFFFF", // 0: Eraser (흰색)
        "#000000", // 1: Black
        "#865716", // 2: Brown
        "#F9EDCD", // 3: Skin
        "#E468A8", // 4: Pink
        "#A05AEA", // 5: Purple
        "#2BA5E3", // 6: Blue
        "#4FE32B", // 7: Green
        "#FFE44D", // 8: Yellow
        "#E56D22", // 9: Orange
        "#E52222", // 10: Red
    };

    [Header("색상 팔레트 버튼 (선택 시 sprite 교체용)")]
    // colorHexCodes와 반드시 같은 순서, 같은 개수로 맞출 것.
    // 비워두면(연결 안 하면) 색상 변경 기능은 그대로 동작하고 sprite 교체만 생략된다.
    [SerializeField] private ColorPaletteButton[] colorPaletteButtons;

    private int selectedColorIndex = -1;

    [Header("저장 개수 제한")]
    [SerializeField] private int maxSavedDrawings = 5;

    private Texture2D texture;
    private int texWidth;
    private int texHeight;

    private Color currentColor = Color.black;
    private float currentWidth = 6f; // 픽셀 단위 굵기

    private bool isDrawing = false;
    private Vector2Int lastPixelPos;

    // 실행 취소/저장용으로 스트로크 기록이 필요하면 이 리스트를 사용
    // (좌표는 텍스처 픽셀 좌표, 좌하단 (0,0) 기준)
    private List<List<Vector2Int>> strokes = new List<List<Vector2Int>>();
    private List<Vector2Int> currentStroke;

    void Awake()
    {
        SetupTexture();
    }

    void OnEnable()
    {
        // 패널 크기가 Awake 시점에 아직 확정 안 됐을 수 있으므로 한 번 더 시도
        if (texture == null)
            SetupTexture();
        else
            Clear(); // 이전에 그렸던 그림 흔적 제거

        StartCoroutine(ReturnToTitleCor());
    }

    private void SetupTexture()
    {
        if (drawingSurface == null && drawingPanel != null)
        {
            drawingSurface = drawingPanel.GetComponent<RawImage>();
        }

        if (drawingSurface == null)
        {
            Debug.LogError("[Drawing] RawImage를 찾을 수 없습니다. Paper 오브젝트에 RawImage 컴포넌트를 추가하고 Drawing Surface 필드에 연결해주세요.");
            return;
        }

        texWidth = Mathf.Max(1, Mathf.RoundToInt(drawingPanel.rect.width));
        texHeight = Mathf.Max(1, Mathf.RoundToInt(drawingPanel.rect.height));

        texture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        ClearTexturePixels();
        texture.Apply();

        drawingSurface.texture = texture;
        drawingSurface.color = Color.white; // RawImage의 Color가 흰색이어야 텍스처 색이 그대로 나온다

        Debug.Log($"[Drawing] 텍스처 준비 완료. size={texWidth}x{texHeight}");
    }

    private void ClearTexturePixels()
    {
        Color[] fill = new Color[texWidth * texHeight];
        for (int i = 0; i < fill.Length; i++)
            fill[i] = Color.white;
        texture.SetPixels(fill);
    }

    void Update()
    {
        if (texture == null)
            return;

        if (!RectTransformUtility.RectangleContainsScreenPoint(
            drawingPanel,
            Input.mousePosition))
        {
            if (isDrawing)
                EndStroke();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            BeginStroke();
        }
        else if (Input.GetMouseButton(0))
        {
            ContinueStroke();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndStroke();
        }
    }

    /// <summary>
    /// 마우스 스크린 좌표를 drawingPanel 기준 로컬 좌표로 바꾼 뒤,
    /// 텍스처 픽셀 좌표(좌하단 (0,0) 기준)로 변환한다.
    /// </summary>
    private bool TryGetPixelPoint(out Vector2Int pixelPoint)
    {
        pixelPoint = default;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingPanel,
                Input.mousePosition,
                null, // Overlay Canvas 기준. Screen Space - Camera라면 해당 카메라를 넣어야 함
                out Vector2 localPoint))
        {
            return false;
        }

        Vector2 pivotOffset = new Vector2(
            drawingPanel.rect.width * drawingPanel.pivot.x,
            drawingPanel.rect.height * drawingPanel.pivot.y);

        int px = Mathf.RoundToInt(localPoint.x + pivotOffset.x);
        int py = Mathf.RoundToInt(localPoint.y + pivotOffset.y);

        pixelPoint = new Vector2Int(px, py);
        return true;
    }

    private void BeginStroke()
    {
        if (!TryGetPixelPoint(out Vector2Int pixelPoint))
            return;

        isDrawing = true;
        currentStroke = new List<Vector2Int> { pixelPoint };

        DrawDot(pixelPoint, currentColor, Mathf.RoundToInt(currentWidth));
        texture.Apply();

        lastPixelPos = pixelPoint;

        Debug.Log($"[Drawing] Stroke 시작. pixel={pixelPoint}, width={currentWidth}, color={currentColor}");
    }

    private void ContinueStroke()
    {
        if (!isDrawing)
            return;

        if (!TryGetPixelPoint(out Vector2Int pixelPoint))
            return;

        if (Vector2Int.Distance(pixelPoint, lastPixelPos) < minDistance)
            return;

        DrawLine(texture, lastPixelPos.x, lastPixelPos.y, pixelPoint.x, pixelPoint.y, currentColor, Mathf.RoundToInt(currentWidth));
        texture.Apply();

        currentStroke.Add(pixelPoint);
        lastPixelPos = pixelPoint;
    }

    private void EndStroke()
    {
        if (!isDrawing)
            return;

        isDrawing = false;

        if (currentStroke != null && currentStroke.Count > 0)
            strokes.Add(currentStroke);

        currentStroke = null;
    }

    public void ChangeColor(int index)
    {
        if (index < 0 || index >= colorHexCodes.Length)
        {
            Debug.LogError($"[Drawing] ChangeColor: index {index}가 colorHexCodes 범위를 벗어났습니다 (0~{colorHexCodes.Length - 1}).");
            return;
        }

        string hex = colorHexCodes[index];

        if (ColorUtility.TryParseHtmlString(hex, out Color parsedColor))
        {
            currentColor = parsedColor;
            Debug.Log($"[Drawing] 색상 변경. index={index}, hex={hex}, color={currentColor}");
        }
        else
        {
            Debug.LogError($"[Drawing] ChangeColor: '{hex}'는 올바른 hex 코드가 아닙니다 (예: #FF0000).");
            return;
        }

        UpdatePaletteSelectionVisual(index);
    }

    /// <summary>
    /// 새로 선택된 버튼은 선택 sprite로, 이전에 선택되어 있던 버튼은 기본 sprite로 되돌린다.
    /// </summary>
    private void UpdatePaletteSelectionVisual(int newIndex)
    {
        if (colorPaletteButtons == null || colorPaletteButtons.Length == 0)
            return;

        // 이전 선택 해제
        if (selectedColorIndex >= 0 &&
            selectedColorIndex < colorPaletteButtons.Length &&
            colorPaletteButtons[selectedColorIndex] != null)
        {
            colorPaletteButtons[selectedColorIndex].SetSelected(false);
        }

        // 새 선택 적용
        if (newIndex >= 0 &&
            newIndex < colorPaletteButtons.Length &&
            colorPaletteButtons[newIndex] != null)
        {
            colorPaletteButtons[newIndex].SetSelected(true);
        }

        selectedColorIndex = newIndex;
    }

    public void ChangeWidth(float width)
    {
        // 안전장치: 너무 작은 값이 들어와도 최소 굵기를 보장한다.
        currentWidth = Mathf.Max(width, 2f);
    }

    public void Clear()
    {
        if (texture == null)
            return;

        ClearTexturePixels();
        texture.Apply();

        strokes.Clear();
        currentStroke = null;
        isDrawing = false;
    }

    public void MakeToPNG(Action<string> onSaved)
    {
        StartCoroutine(CaptureRoutine(onSaved));
    }

    public IEnumerator CaptureRoutine(Action<string> onSaved)
    {
        yield return new WaitForEndOfFrame();

        if (texture == null)
        {
            Debug.LogWarning("[Drawing] 텍스처가 없습니다. 저장을 건너뜁니다.");
            onSaved?.Invoke(null);
            yield break;
        }

        // 이미 화면에 그려지고 있는 텍스처 그대로를 PNG로 저장한다.
        // 좌표 변환이 전혀 필요 없다 — 이미 픽셀 단위로 그려져 있기 때문.
        byte[] pngBytes = texture.EncodeToPNG();
        string fileName = $"Drawing_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        System.IO.File.WriteAllBytes(path, pngBytes);

        EnforceMaxSavedFiles(Application.persistentDataPath, "Drawing_*.png", maxSavedDrawings);

        onSaved?.Invoke(path);
    }

    private void EnforceMaxSavedFiles(string folderPath, string searchPattern, int maxCount)
    {
        if (!System.IO.Directory.Exists(folderPath))
            return;

        var files = new System.IO.DirectoryInfo(folderPath)
            .GetFiles(searchPattern)
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .ToList();

        for (int i = maxCount; i < files.Count; i++)
        {
            try
            {
                files[i].Delete();
                Debug.Log($"[정리] 오래된 그림 파일 삭제: {files[i].Name}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"파일 삭제 실패: {files[i].FullName}, {e.Message}");
            }
        }
    }

    private void DrawDot(Vector2Int center, Color color, int thickness)
    {
        int half = Mathf.Max(1, thickness) / 2;
        for (int tx = -half; tx <= half; tx++)
        {
            for (int ty = -half; ty <= half; ty++)
            {
                int px = center.x + tx;
                int py = center.y + ty;
                if (px >= 0 && px < texWidth && py >= 0 && py < texHeight)
                    texture.SetPixel(px, py, color);
            }
        }
    }

    private void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color color, int thickness)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        int half = Mathf.Max(1, thickness) / 2;

        while (true)
        {
            for (int tx = -half; tx <= half; tx++)
            {
                for (int ty = -half; ty <= half; ty++)
                {
                    int px = x0 + tx;
                    int py = y0 + ty;

                    if (px >= 0 && px < tex.width &&
                        py >= 0 && py < tex.height)
                    {
                        tex.SetPixel(px, py, color);
                    }
                }
            }

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    IEnumerator ReturnToTitleCor()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.makeCar.returnTimeforDrawing);
        LogSender.Instance.SendLog("move_idle_timeout");
        CommonFunc.instance.ReturnBtnFunc(this.gameObject);
    }

    public void GotoHome()
    {
        CommonFunc.instance.ReturnBtnFunc(this.gameObject);
        Logger.Instance.SendLog("move_idle");
    }
}