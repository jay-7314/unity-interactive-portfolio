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
    public void PreloadVideo(VideoData videoData, Action<VideoPlayer> onComplete = null)
    {
        if (preloadedVideos.TryGetValue(videoData.VideoPath, out VideoPlayer existing))
        {
            onComplete?.Invoke(existing);
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
            onComplete?.Invoke(v);
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
            vp.targetTexture = preloaded.targetTexture;

            RawImage raw = vp.GetComponent<RawImage>();
            if (raw == null) raw = vp.AddComponent<RawImage>();
            raw.texture = preloaded.targetTexture;

            preloaded.time = 0;
            if (!preloaded.isPlaying)
                preloaded.Play();
            return;
        }

        RawImage rawFallback = vp.GetComponent<RawImage>();
        if (rawFallback == null) rawFallback = vp.AddComponent<RawImage>();

        string filePath = Path.Combine(Application.streamingAssetsPath, videoData.VideoPath);
        vp.source = VideoSource.Url;
        vp.url = filePath;
        vp.prepareCompleted -= OnVideoPrepared;
        vp.prepareCompleted += OnVideoPrepared;
        vp.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
    }
    #endregion
}