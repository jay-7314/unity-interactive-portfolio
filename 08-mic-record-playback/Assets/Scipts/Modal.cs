using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Modal : MonoBehaviour
{
    [SerializeField] VideoPlayer modalVideo;
    [SerializeField] Text videosText;
    [SerializeField] GameObject modalpage, lastpage, selectvideo;

    VideoData currentVideoData;

    public void Open(VideoData videodata)
    {
        currentVideoData = videodata;
        gameObject.SetActive(true);
        videosText.text = videodata.videoText;

        // 프리로드된 영상 사용
        VideoPlayer preloaded = LoadManager.Instance.GetPreloadedVideo(videodata);
        if (preloaded != null)
        {
            modalVideo.Stop();
            modalVideo.targetTexture = preloaded.targetTexture;

            RawImage raw = modalVideo.GetComponent<RawImage>();
            if (raw != null) raw.texture = preloaded.targetTexture;

            preloaded.time = 0;
            preloaded.Play();
        }
        else
        {
            // 프리로드 안된 경우 직접 로드
            LoadManager.Instance.LoadVideo(videodata, modalVideo);
        }
    }

    public void CloseModalBtn()
    {
        VideoPlayer preloaded = LoadManager.Instance.GetPreloadedVideo(currentVideoData);
        if (preloaded != null) preloaded.Pause();

        gameObject.SetActive(false);
    }

    public void GotoLastPage()
    {
        modalpage.SetActive(false);
        selectvideo.SetActive(false);
        lastpage.SetActive(true);
        lastpage.GetComponent<LastPage>().SetVideo(currentVideoData);
    }
}