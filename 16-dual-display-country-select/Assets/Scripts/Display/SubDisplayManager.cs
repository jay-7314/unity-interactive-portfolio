using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SubDisplayManager : MonoBehaviour
{
    #region 인스펙터
    public static SubDisplayManager instance;

    [Header("영상연결")]
    [SerializeField] private RawImage rawImage;
    [SerializeField] private VideoPlayer videoPlayer;

    public bool isPlayedCheck = false;
    private bool shouldPlayDefault = false;

    #endregion

    #region Func
    private void Awake()
    {
        instance = this;

        //display2 활성화
        if(Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }

        //영상이 끝나면 stop처리
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    private void Start()
    {
        if (!isPlayedCheck)
        {
            PlaySubVideo(); // default 영상
            isPlayedCheck = true;
        }
    }
    #endregion

    #region Video
    public void PlaySubVideo(string id = null, bool isReturnVideo = false)
    {
        string path;

        if (string.IsNullOrEmpty(id))
        {
            if (!isPlayedCheck && !shouldPlayDefault) return;

            path = SubDisPlayJsonManager.DefaultVideoPath;
            isPlayedCheck = true;
            shouldPlayDefault = false;
        }
        else
        {
            SubvideoType type = isReturnVideo ? SubvideoType.Return : SubvideoType.Enter;
            path = SubDisPlayJsonManager.SubMoviePath(id, type);
        }

        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            videoPlayer.Stop();
            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = path;
            videoPlayer.Prepare();
        }
    }


    private void OnPrepared(VideoPlayer source)
    {
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.Play();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        videoPlayer.Stop();
    }
    #endregion
}
