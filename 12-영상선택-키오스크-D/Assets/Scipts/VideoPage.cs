using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPage : MonoBehaviour
{
    [SerializeField] RawImage mainVideo;
    [SerializeField] VideoPlayer videoplayer;
    int currentvideoindex = 0;
    [SerializeField] VideoPlayer[] selectVideos;

    private void OnEnable()
    {
        StartCoroutine(ReturnToTitleCor());

        for (int i = 0; i < selectVideos.Length; i++)
        {
            int idx = i;
            VideoData videoData = JsonManager.poly.VideoList[idx];
            LoadManager.Instance.PreloadVideo(videoData, (preloaded) =>
            {
                RawImage raw = selectVideos[idx].GetComponent<RawImage>();
                if (raw != null) raw.texture = preloaded.targetTexture;
                selectVideos[idx].targetTexture = preloaded.targetTexture;
                if (!preloaded.isPlaying)
                    preloaded.Play();
            });
            selectVideos[i].GetComponent<Button>().onClick.RemoveAllListeners();
            selectVideos[i].GetComponent<Button>().onClick.AddListener(() => SelectVideos(idx));
        }

        // 랜덤 인덱스로 메인 영상 재생
        currentvideoindex = Random.Range(0, selectVideos.Length);
        PlayVideo(currentvideoindex);
    }

    void PlayVideo(int index)
    {
        VideoData videoData = JsonManager.poly.VideoList[index];

        LoadManager.Instance.PreloadVideo(videoData, (preloaded) =>
        {
            // mainVideo RawImage에 연결
            mainVideo.texture = preloaded.targetTexture;

            // videoplayer RawImage에도 연결
            RawImage raw = videoplayer.GetComponent<RawImage>();
            if (raw != null) raw.texture = preloaded.targetTexture;
            videoplayer.targetTexture = preloaded.targetTexture;

            preloaded.time = 0;
            preloaded.Play();
        });
    }

    public void SelectVideos(int index)
    {
        currentvideoindex = index;
        PlayVideo(currentvideoindex);
    }

    IEnumerator ReturnToTitleCor()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.poly.ReturnToTitle);
        CommonFunc.instance.ReturnToTitle();
    }
}