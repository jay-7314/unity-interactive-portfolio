using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Video1 : MonoBehaviour
{
    [SerializeField] VideoPlayer vp;
    [SerializeField] GameObject modal;
    [SerializeField] RawImage rawImage;

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

    void OnPrepared(VideoPlayer vp)
    {
        vp.Play();
        rawImage.color = Color.white; 
    }

    private void OnDisable()
    {
        vp.loopPointReached -= OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        modal.SetActive(true);
    }
}
