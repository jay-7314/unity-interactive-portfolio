using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SelectVideo : MonoBehaviour
{
    [SerializeField] GameObject explain, selectVideoPage;
    [SerializeField] Modal modal;
    [SerializeField] VideoPlayer[] selectVideos;

    private Coroutine returnCoroutine;
    private float returnTimer;
    private bool timerPaused = false;

    private void OnEnable()
    {
        for (int i = 0; i < selectVideos.Length; i++)
        {
            int idx = i;
            VideoData videoData = JsonManager.poly.VideoList[i];

            // 프리로드된 VideoPlayer의 RenderTexture 연결
            VideoPlayer preloaded = LoadManager.Instance.GetPreloadedVideo(videoData);
            if (preloaded != null)
            {
                RawImage raw = selectVideos[i].GetComponent<RawImage>();
                if (raw != null) raw.texture = preloaded.targetTexture;
                selectVideos[i].targetTexture = preloaded.targetTexture;

                // 재생
                if (!preloaded.isPlaying)
                    preloaded.Play();
            }
            else
            {
                // 프리로드가 아직 안된 경우 직접 로드
                LoadManager.Instance.LoadVideo(videoData, selectVideos[i]);
            }

            selectVideos[i].GetComponent<Button>().onClick.RemoveAllListeners();
            selectVideos[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                modal.Open(JsonManager.poly.VideoList[idx]);
            });
        }

        StartReturnTimer();
    }

    private void OnDisable()
    {
        StopReturnTimer();
    }

    private void StartReturnTimer()
    {
        StopReturnTimer();
        returnTimer = JsonManager.poly.SelectPageToTitleTimer;
        timerPaused = false;
        returnCoroutine = StartCoroutine(ReturnToTitle());
    }

    private void StopReturnTimer()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
    }

    public void PauseReturnTimer()
    {
        timerPaused = true;
    }

    public void ResumeReturnTimer()
    {
        returnTimer = JsonManager.poly.SelectPageToTitleTimer;
        timerPaused = false;
    }

    IEnumerator ReturnToTitle()
    {
        while (returnTimer > 0f)
        {
            if (!timerPaused)
                returnTimer -= Time.deltaTime;
            yield return null;
        }
        ArduinoManager.Instance.SendMessage(JsonManager.poly.OffLamp);
        CommonFunc.instance.ReturnToTitle();
    }

    public void PrevBtn()
    {
        selectVideoPage.SetActive(false);
        explain.SetActive(true);
    }
}