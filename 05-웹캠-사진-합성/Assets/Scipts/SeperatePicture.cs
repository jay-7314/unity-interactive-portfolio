using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CircleCropData
{
    [Range(0f, 1f)] public float centerX;
    [Range(0f, 1f)] public float centerY;
    [Range(0f, 1f)] public float radiusX;
    [Range(0f, 1f)] public float radiusY;
}

public class SeperatePicture : MonoBehaviour
{
    [SerializeField] RawImage areaImg;
    [SerializeField] RawImage resultImg;
    [SerializeField] RawImage camImg;
    [SerializeField] RawImage frontImg;
    [SerializeField] CircleCropData[] cropDatas;
    [SerializeField] PageController pageController;

    public static Texture2D CroppedResult;

    private void OnEnable()
    {
        StartCoroutine(CaptureArea());
    }

    IEnumerator CaptureArea()
    {
        yield return new WaitForSeconds(15f);
        yield return new WaitForEndOfFrame();
        pageController.currentIndex++;
        RectTransform rect = areaImg.GetComponent<RectTransform>();
        int width = (int)rect.rect.width;
        int height = (int)rect.rect.height;

        Texture2D camTex = ResizeTextureWithUV(camImg, width, height);
        Texture2D frontTex = ResizeTexture(frontImg.texture, width, height);

        // 1. 전체 합성
        Texture2D fullTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int px = 0; px < width; px++)
        {
            for (int py = 0; py < height; py++)
            {
                Color camColor = camTex.GetPixel(px, py);
                Color frontColor = frontTex.GetPixel(px, py);
                fullTex.SetPixel(px, py, Color.Lerp(camColor, frontColor, frontColor.a));
            }
        }
        fullTex.Apply();
        resultImg.texture = fullTex;

        // 2. 타원 크롭
        int idx = SaveStatus.Instance.imgIndex;
        CircleCropData crop = cropDatas[idx];

        int cx = (int)(crop.centerX * width);
        int cy = height - (int)(crop.centerY * height);
        int rx = (int)(crop.radiusX * width);
        int ry = (int)(crop.radiusY * height);
        int diameterX = rx * 2;
        int diameterY = ry * 2;

        Texture2D circleTex = new Texture2D(diameterX, diameterY, TextureFormat.RGBA32, false);
        for (int px = 0; px < diameterX; px++)
        {
            for (int py = 0; py < diameterY; py++)
            {
                float nx = (px - rx) / (float)rx;
                float ny = (py - ry) / (float)ry;
                if (nx * nx + ny * ny > 1f) { circleTex.SetPixel(px, py, Color.clear); continue; }

                int srcX = Mathf.Clamp(cx - rx + px, 0, width - 1);
                int srcY = Mathf.Clamp(cy - ry + py, 0, height - 1);
                circleTex.SetPixel(px, py, fullTex.GetPixel(srcX, srcY));
            }
        }
        circleTex.Apply();
        CroppedResult = circleTex;

        Destroy(camTex);
        Destroy(frontTex);
    }

    Texture2D ResizeTextureWithUV(RawImage rawImage, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        RenderTexture prev = RenderTexture.active;

        Rect uv = rawImage.uvRect;
        Graphics.Blit(rawImage.texture, rt, new Vector2(uv.width, uv.height), new Vector2(uv.x, uv.y));

        RenderTexture.active = rt;
        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    Texture2D ResizeTexture(Texture source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        RenderTexture prev = RenderTexture.active;
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;
        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (cropDatas == null || areaImg == null) return;

        RectTransform rect = areaImg.GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        float width = Vector3.Distance(corners[0], corners[3]);
        float height = Vector3.Distance(corners[0], corners[1]);
        Vector3 rectCenter = (corners[0] + corners[2]) * 0.5f;

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(rectCenter, new Vector3(width, height, 0));

        Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan };

        for (int i = 0; i < cropDatas.Length; i++)
        {
            CircleCropData crop = cropDatas[i];

            float cx = corners[0].x + crop.centerX * width;
            float cy = corners[0].y + (1f - crop.centerY) * height;
            Vector3 center = new Vector3(cx, cy, rectCenter.z);

            float rx = crop.radiusX * width;
            float ry = crop.radiusY * height;

            Gizmos.color = colors[i % colors.Length];
            DrawWireEllipse(center, rx, ry);
            Gizmos.DrawSphere(center, width * 0.01f);
            UnityEditor.Handles.Label(center + Vector3.up * (ry + height * 0.02f), $"[{i}] rx:{rx:F1} ry:{ry:F1}");
        }
    }

    void DrawWireEllipse(Vector3 center, float rx, float ry, int segments = 36)
    {
        float angleStep = 360f / segments;
        Vector3 prev = center + new Vector3(rx, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * rx, Mathf.Sin(angle) * ry, 0);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}