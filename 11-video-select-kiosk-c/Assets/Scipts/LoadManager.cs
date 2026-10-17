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

    private void Awake()
    {
        Instance = this;
    }

    #region 로드용
    public void PreloadVideo(VideoData videoData, VideoPlayer vp)
    {
        SetupVideoPlayer(videoData, vp);
        vp.prepareCompleted -= OnVideoPrepared_PrepareOnly;
        vp.prepareCompleted += OnVideoPrepared_PrepareOnly;
        vp.Prepare();
    }

    public void LoadVideo(VideoData videoData, VideoPlayer vp)
    {
        SetupVideoPlayer(videoData, vp);
        vp.prepareCompleted -= OnVideoPrepared;
        vp.prepareCompleted += OnVideoPrepared;
        vp.Prepare();
    }

    public void PlayVideo(VideoPlayer vp)
    {
        if (vp.isPrepared)
        {
            vp.Play();
        }
        else
        {
            vp.prepareCompleted -= OnVideoPrepared;
            vp.prepareCompleted += OnVideoPrepared;
            vp.Prepare();
        }
    }

    private void SetupVideoPlayer(VideoData videoData, VideoPlayer vp)
    {
        // 방법 2: waitForFirstFrame, skipOnDrop 설정
        vp.playOnAwake = false;
        vp.waitForFirstFrame = false;
        vp.skipOnDrop = true;

        // 방법 3: RenderTexture 미리 할당
        RawImage raw = vp.GetComponent<RawImage>();
        if (raw == null) raw = vp.AddComponent<RawImage>();

        if (vp.targetTexture == null)
        {
            RenderTexture rt = new RenderTexture(1920, 1080, 0);
            vp.targetTexture = rt;
            raw.texture = rt;
        }

        string filePath = Path.Combine(Application.streamingAssetsPath, videoData.VideoPath);
        vp.source = VideoSource.Url;
        vp.url = filePath;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    // Preload용 - Play 하지 않음
    void OnVideoPrepared_PrepareOnly(VideoPlayer vp)
    {
        // 준비만 완료, 재생 안 함
    }
    #endregion
}