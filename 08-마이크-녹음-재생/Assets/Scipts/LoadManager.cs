using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoadManager : MonoBehaviour
{
    public static LoadManager Instance;

    private Dictionary<string, VideoPlayer> preloadedVideos = new Dictionary<string, VideoPlayer>();

    private void Awake()
    {
        Instance = this;
    }

    #region 프리로드
    public void PreloadVideo(VideoData videoData, Action onComplete = null)
    {
        if (preloadedVideos.ContainsKey(videoData.VideoPath))
        {
            onComplete?.Invoke();
            return;
        }

        GameObject go = new GameObject("Preload_" + videoData.VideoPath);
        VideoPlayer vp = go.AddComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.waitForFirstFrame = true;
        vp.skipOnDrop = true;

        RenderTexture rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
        rt.Create();
        vp.targetTexture = rt;

        string filePath = Path.Combine(Application.streamingAssetsPath, videoData.VideoPath);
        vp.source = VideoSource.Url;
        vp.url = filePath;

        vp.prepareCompleted += (v) =>
        {
            preloadedVideos[videoData.VideoPath] = v;
            Debug.Log($"[PreloadVideo] 준비 완료: {videoData.VideoPath}");
            onComplete?.Invoke();
        };

        vp.Prepare();
    }

    public VideoPlayer GetPreloadedVideo(VideoData videoData)
    {
        preloadedVideos.TryGetValue(videoData.VideoPath, out VideoPlayer vp);
        return vp;
    }
    #endregion

    #region 로드용
    public void LoadVideo(VideoData videoData, VideoPlayer vp)
    {
        if (preloadedVideos.TryGetValue(videoData.VideoPath, out VideoPlayer preloaded))
        {
            ApplyPreloaded(preloaded, vp);
            StartCoroutine(PlayAfterFirstFrame(preloaded));
            return;
        }

        SetupVideoPlayer(videoData, vp);
        vp.prepareCompleted -= OnVideoPrepared;
        vp.prepareCompleted += OnVideoPrepared;
        vp.Prepare();
    }

    public void LoadVideoWithoutPlay(VideoData videoData, VideoPlayer vp)
    {
        if (preloadedVideos.TryGetValue(videoData.VideoPath, out VideoPlayer preloaded))
        {
            ApplyPreloaded(preloaded, vp);
            return;
        }

        SetupVideoPlayer(videoData, vp);
        vp.prepareCompleted -= OnVideoPrepared_PrepareOnly;
        vp.prepareCompleted += OnVideoPrepared_PrepareOnly;
        vp.Prepare();
    }

    public void PlayVideo(VideoPlayer vp)
    {
        if (vp.isPrepared)
        {
            StartCoroutine(PlayAfterFirstFrame(vp));
        }
        else
        {
            vp.prepareCompleted -= OnVideoPrepared;
            vp.prepareCompleted += OnVideoPrepared;
            vp.Prepare();
        }
    }

    private void ApplyPreloaded(VideoPlayer preloaded, VideoPlayer target)
    {
        target.Stop();
        target.playOnAwake = false;
        target.waitForFirstFrame = true;
        target.skipOnDrop = true;
        target.targetTexture = preloaded.targetTexture;

        RawImage raw = target.GetComponent<RawImage>();
        if (raw != null) raw.texture = preloaded.targetTexture;
    }

    private void SetupVideoPlayer(VideoData videoData, VideoPlayer vp)
    {
        vp.Stop();
        vp.playOnAwake = false;
        vp.waitForFirstFrame = true;
        vp.skipOnDrop = true;

        RawImage raw = vp.GetComponent<RawImage>();
        if (raw == null) raw = vp.AddComponent<RawImage>();

        if (vp.targetTexture == null)
        {
            RenderTexture rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            raw.texture = rt;
        }

        ClearRenderTexture(vp);

        string filePath = Path.Combine(Application.streamingAssetsPath, videoData.VideoPath);
        vp.source = VideoSource.Url;
        vp.url = filePath;
    }

    private void ClearRenderTexture(VideoPlayer vp)
    {
        if (vp.targetTexture == null) return;
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = vp.targetTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = prev;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        StartCoroutine(PlayAfterFirstFrame(vp));
    }

    void OnVideoPrepared_PrepareOnly(VideoPlayer vp)
    {
        // 준비만 완료, 재생 안 함
    }

    IEnumerator PlayAfterFirstFrame(VideoPlayer vp)
    {
        ClearRenderTexture(vp);
        yield return new WaitForEndOfFrame();
        vp.Play();
    }
    #endregion
}