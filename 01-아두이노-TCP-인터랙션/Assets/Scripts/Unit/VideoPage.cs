using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPage : MonoBehaviour
{
    VideoPlayer video;
    Image skip;

    private void OnEnable()
    {
        StartCoroutine(CreateUnit());
    }

    private void OnDisable()
    {
        ClearObject();
        StopAllCoroutines();
        LoadManager.Instance.StopReturnTimer();
    }

    void ClearObject()
    {
        if (video != null) Destroy(video.gameObject);
        if (skip != null) Destroy(skip.gameObject);
    }

    IEnumerator CreateUnit()
    {
        var VideoPagedata = JsonManager.uljin.VideoPage[0];
        var LM = LoadManager.Instance;

        video = LM.CreateUIObject<VideoData, VideoPlayer>("mainVideo", VideoPagedata.Video[0], LM.LoadVideo, transform);
        skip = LM.CreateUIObject<ImageData, Image>("SkipBtn", VideoPagedata.SkipBtn[0], LM.LoadImagePos, transform);

        Button skipAction = skip.AddComponent<Button>();
        skipAction.onClick.AddListener(PageNPoint);

        yield return LM.VideoStop(video);
        PageNPoint();
    }

    void PageNPoint()
    {
        UIManager.Instance.TargetPage(UIManager.Pagelist.inducePage);
        ClientTCP.Instance.Send("지진소리끝");
        ClientTCP.Instance.Send($"수렴 포인트");
    }
}
