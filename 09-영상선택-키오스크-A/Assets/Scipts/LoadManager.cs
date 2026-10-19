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
    public void LoadVideo(VideoData videoData, VideoPlayer vp)
    {
        RawImage raw = vp.GetComponent<RawImage>();
        if (raw == null) raw = vp.AddComponent<RawImage>();

        RectTransform rawRect = raw.rectTransform;

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
