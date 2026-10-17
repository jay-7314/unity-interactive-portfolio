using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class video2 : MonoBehaviour
{
    [SerializeField] VideoPlayer vp;
    [SerializeField] RawImage rawImage;
    [SerializeField] GameObject answer1_finish, video1_answer;
    [SerializeField] PageController pageController;

    private void OnEnable()
    {
        rawImage.color = Color.clear;

        vp.Stop();
        vp.time = 0;
        vp.loopPointReached -= OnVideoEnd;
        vp.loopPointReached += OnVideoEnd;

        vp.prepareCompleted -= OnPrepared;
        vp.prepareCompleted += OnPrepared;
        vp.Prepare();
    }

    private void OnDisable()
    {
        vp.loopPointReached -= OnVideoEnd;
        vp.prepareCompleted -= OnPrepared;
    }

    void OnPrepared(VideoPlayer vp)
    {
        rawImage.color = Color.white;
        vp.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        pageController.Nextpage();
    }
}