using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameController : MonoBehaviour
{
    [Header("프로그램 종료 버튼")]
    float shutTimer = 0f;                      //0으로 초기값을 넣어진행
    int clickCount = 0;                        //누른 횟수 초기값으로 진행

    [Header("인스펙터 연결")]
    public KinectIRSticker sticker;
    public VideoPlayer videoPlayer;
    public UIJsonManager uijsonManager;

    [SerializeField] private GameObject IRCameraObject;
    [SerializeField] private GameObject videoPlayerObject;
    [SerializeField] private Button SkipBtn;

    private float PosCheckTimer = 0f;
    private bool isPlayingVideo = false;
    private StickerPos currentPos = null;

    private bool Checking = true;

   

    private void Start()
    {
        //스킵버튼 처음에는 비활성화
        SkipBtn.gameObject.SetActive(false);

        //동영상이 끝나면 나타나는 이벤트
        videoPlayer.loopPointReached += OnVideoFinished;

        StartCoroutine(checkPos(0.5f));
    }

    //private void Update()
    //{
    //    float dx = sticker.smoothedPosition.x;
    //    float dy = sticker.smoothedPosition.y;

    //    Debug.Log($"x : {dx:F2}, y : {dy:F2}");
    //}

    IEnumerator checkPos(float delay)
    {
        while (true)
        {
            if (!Checking || isPlayingVideo)
            {
                yield return new WaitForSeconds(delay);
                continue;
            }
            
            float dx = sticker.smoothedPosition.x;
            float dy = sticker.smoothedPosition.y;

            for (int i = 0; i < JsonManager.PosList.pos.Count; i++)
            {
                StickerPos sp = JsonManager.PosList.pos[i];
                float range = sp.stickerRadius;
                if(dx >= sp.stickerposition.x - range && dx<=sp.stickerposition.x + range && dy >= sp.stickerposition.y - range && dy <= sp.stickerposition.y + range)
                {
                    if (currentPos == sp)
                    {
                        PosCheckTimer += delay;
                        if (PosCheckTimer >= JsonManager.PosList.checkPosTimer)
                        {
                            PlayVideo(sp);
                            PosCheckTimer = 0f;
                        }
                    }
                    else
                    {
                        currentPos = sp;
                        PosCheckTimer = 0f;
                    }
                    break;
                }
                else
                {
                    if(currentPos == sp)
                    {
                        currentPos = null;
                        PosCheckTimer = 0f;
                    }
                }

            }
            yield return new WaitForSeconds(delay);

        }
    }

    //동영상 정지 및 초기화
    private void StopVideo()
    {
        if(videoPlayer != null) videoPlayer.Stop();
        if(videoPlayerObject != null) videoPlayerObject.SetActive(false);
        if(SkipBtn != null) SkipBtn.gameObject.SetActive(false);
        if(IRCameraObject != null) IRCameraObject.SetActive(true);

        PosCheckTimer = 0f;
        isPlayingVideo = false;
        currentPos = null;
    }
    //영상이 끝났을때
    private void OnVideoFinished(VideoPlayer vp)
    {
        StopVideo();
        StartCoroutine(EndTimer(3f));
    }
    //스킵버튼 눌렀을때
    public void SkipBtnFunc()
    {
        StopVideo();
    }

    IEnumerator EndTimer(float delay)
    {
        Checking = false;
        yield return new WaitForSeconds(delay);
        Checking = true;
    }

    //동영상 재생처리
    private void PlayVideo(StickerPos sp)
    {
        isPlayingVideo = true;
        currentPos = sp;

        if(IRCameraObject != null) IRCameraObject.SetActive(false);

        if (uijsonManager != null && videoPlayer != null)
        {
            videoPlayerObject.SetActive(true);
            uijsonManager.LoadVideo(sp.video, videoPlayer);
        }
        //스킵버튼 활성화
        StartCoroutine(ShowSkipBtn(5f));
    }

    //스킵버튼
    IEnumerator ShowSkipBtn(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            SkipBtn.gameObject.SetActive(true);
        }
    }

    // 프로그램 종료로직 버튼
    public void ShutdownOnClick()
    {
        shutTimer += Time.deltaTime;
        if (shutTimer >= 3)
        {
            clickCount = 0;
            shutTimer = 0f;
        }
        else
        {
            clickCount++;
            if (clickCount >= 10)
            {
                Application.Quit();
            }
        }
    }
}
