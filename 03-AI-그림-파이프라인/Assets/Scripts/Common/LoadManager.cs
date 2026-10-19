using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoadManager : MonoBehaviour
{
    public static LoadManager Instance;
    TMP_FontAsset defaultFont;
    Coroutine returnCoroutine;
    [Header("프로그램 종료 버튼")]
    float shutTimer = 0f;                      //0으로 초기값을 넣어진행
    int clickCount = 0;                        //누른 횟수 초기값으로 진행

    private void Awake()
    {
        Instance = this;
        //defaultFont = Resources.Load<TMP_FontAsset>("Font/Freesentation-7Bold SDF");
    }
    #region 로드용
    //이미지 로드하기
    public void LoadImage(Image img, string path)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, path);
        if (File.Exists(filePath))
        {
            byte[] data = File.ReadAllBytes(filePath);
            Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture2D.LoadImage(data);
            texture2D.filterMode = FilterMode.Point;
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one * 0.5f);
            img.sprite = sprite;
        }
    }

    //이미지 위치값 선언용
    public void LoadImagePos(ImageData imageData, Image img)
    {
        if (img == null && imageData == null) return;
        LoadImage(img, imageData.path);
        if (imageData.imgPos != Vector2.zero || imageData.imgPos == Vector2.zero)
        {
            img.rectTransform.anchoredPosition = imageData.imgPos;
        }
        if (imageData.imgSize != Vector2.zero || imageData.imgSize == Vector2.zero)
        {
            img.rectTransform.sizeDelta = imageData.imgSize;
        }
    }

    //동영상 가져오기 - 동영상은 rawImage에서 가져와야함.
    public void LoadVideo(VideoData videoData, VideoPlayer vp)
    {
        RawImage raw = vp.GetComponent<RawImage>();
        if (raw == null) raw = vp.AddComponent<RawImage>();

        int w = Mathf.RoundToInt(videoData.VideoSize.x);
        int h = Mathf.RoundToInt(videoData.VideoSize.y);

        RectTransform rawRect = raw.rectTransform;
        rawRect.sizeDelta = new Vector2(w, h);
        rawRect.anchoredPosition = videoData.VideoPos;

        RenderTexture rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
        rt.Create();

        raw.texture = rt;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.targetTexture = rt;

        vp.aspectRatio = VideoAspectRatio.FitInside;

        string filePath = Path.Combine(Application.streamingAssetsPath, videoData.VideoPath);
        vp.source = VideoSource.Url;
        vp.url = filePath;

        vp.prepareCompleted -= OnVideoPrepared;
        vp.prepareCompleted += OnVideoPrepared;
        vp.Prepare();
    }


    //텍스트 가져오기
    public void LoadText(TextData textData, TextMeshProUGUI text)
    {
        if (textData == null || text == null) return;
        text.font = defaultFont;
        text.text = textData.TextSentance;
        text.fontSize = textData.TextSize;
        text.rectTransform.anchoredPosition = textData.textPos;
        text.enableWordWrapping = false;
        text.alignment = TextAlignmentOptions.Center;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
    }
    #endregion

    #region 처음으로 돌아가는 로직
    ////처음으로 돌아가는 로직 만들기 - bool값이 true면 바로 진행, false이면 코루틴 시간 설정
    //public void ReturnToTitleTimer()
    //{
    //    if (returnCoroutine != null)
    //    {
    //        StopCoroutine(returnCoroutine);
    //    }
    //    returnCoroutine = StartCoroutine(ReturnTimer(JsonManager.uljin.Common[0].returnTimer));
    //}

    //IEnumerator ReturnTimer(float delay)
    //{
    //    yield return new WaitForSeconds(delay);
    //    if (UIManager.Instance.currentPage != UIManager.Pagelist.title)
    //    {
    //        returnCoroutine = null;
    //        ReturnToTitleBtnFuc();
    //    }
    //}

    //public void ReturnToTitleBtnFuc()
    //{
    //    StopAllVideo();
    //    ArduinoManager.Instance.SendToArduinoB("Off");
    //    ArduinoManager.Instance.SendToArduinoA("Off");
    //    UIManager.Instance.TargetPage(UIManager.Pagelist.title);
    //    OnApplicationQuit();
    //}

    void StopAllVideo()
    {
        VideoPlayer[] players = FindObjectsOfType<VideoPlayer>(true);

        for (int i = 0; i < players.Length; i++)
        {
            VideoPlayer vp = players[i];
            vp.prepareCompleted -= OnVideoPrepared;
            if (vp.isPlaying || vp.isPrepared)
            {
                vp.Stop();
            }
        }
    }

    //정지타이밍을 꺼야 하는 순간에 설정
    public void StopReturnTimer()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
    }
    #endregion

    #region AddComponent 로직 제너럴 상태 Func
    public TComp CreateUIObject<TData, TComp>(string name, TData data, Action<TData, TComp> loadAction, Transform parent) where TComp : Component
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        TComp component = go.AddComponent<TComp>();
        loadAction?.Invoke(data, component);

        return component;
    }
    #endregion

    #region 점점 밝게 하는 로직
    //텍스트
    public IEnumerator FadeUnit(Graphic graphic, float duration)
    {
        float time = 0;
        Color c = graphic.color;
        c.a = 0f;
        graphic.color = c;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Clamp01(time / duration);

            c.a = alpha;
            graphic.color = c;

            yield return null;
        }
        c.a = 1f;
        graphic.color = c;
    }
    #endregion

    #region 동영상 끝나기 전까지는 다음작동 안하게 하는것
    public IEnumerator VideoStop(VideoPlayer vp)
    {
        if (vp == null) yield break;

        while (!vp.isPrepared) yield return null;
        while (!vp.isPlaying) yield return null;
        while (vp.isPlaying) yield return null;

        yield return new WaitForSeconds(0.5f);
    }
    #endregion

    // 프로그램 종료로직 버튼
    public void ShutdownOnClick()
    {
        shutTimer += Time.deltaTime;
        if (shutTimer >= 3)
        {
            clickCount = 0;
            shutTimer = 0f;
        }
        else
        {
            clickCount++;
            if (clickCount >= 10)
            {
                Application.Quit();
            }
        }
    }
}
