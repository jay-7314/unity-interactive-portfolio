using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LastPage : MonoBehaviour
{
    [SerializeField] VideoPlayer lastvideo;
    [SerializeField] Mic mic; // 추가
    [SerializeField] GameObject selectvideo, lastPage, modalPage;
    private VideoData currentVideoData;
    private VideoPlayer currentPreloaded;

    private void OnEnable()
    {
        StartCoroutine(ReturntoTitle());
    }

    public void SetVideo(VideoData videodata)
    {
        currentVideoData = videodata;

        VideoPlayer preloaded = LoadManager.Instance.GetPreloadedVideo(videodata);
        if (preloaded != null)
        {
            currentPreloaded = preloaded;

            lastvideo.Stop();
            lastvideo.targetTexture = preloaded.targetTexture;

            RawImage raw = lastvideo.GetComponent<RawImage>();
            if (raw != null) raw.texture = preloaded.targetTexture;

            // 처음엔 정지 상태
            preloaded.time = 0;
            preloaded.Stop();

            // Mic에 프리로드된 VideoPlayer 전달
            mic.SetVideoPlayer(preloaded);
        }
        else
        {
            LoadManager.Instance.LoadVideoWithoutPlay(videodata, lastvideo);
            mic.SetVideoPlayer(lastvideo);
        }
    }

    public void ReturnToselectPage()
    {
        if (currentPreloaded != null)
        {
            currentPreloaded.Stop();
            currentPreloaded = null;
        }
        lastPage.SetActive(false);
        modalPage.SetActive(false);
        selectvideo.SetActive(true);
    }

    public void PlayFromBeginning()
    {
        VideoPlayer preloaded = LoadManager.Instance.GetPreloadedVideo(currentVideoData);
        if (preloaded != null)
        {
            preloaded.time = 0;
            preloaded.Play();
        }
        else if (lastvideo != null)
        {
            lastvideo.Stop();
            lastvideo.time = 0;
            lastvideo.Play();
        }
    }

    IEnumerator ReturntoTitle()
    {
        ArduinoManager.Instance.SendMessage(JsonManager.poly.OffLamp);
        yield return CoroutineData.GetWaitForSeconds(JsonManager.poly.Lasttimer);
        CommonFunc.instance.ReturnToTitle();
    }
}