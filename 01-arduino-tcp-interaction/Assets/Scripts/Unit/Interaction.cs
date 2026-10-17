using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Interaction : MonoBehaviour
{
    Image firstSentance, firstImg, secondImg, thirdImg;
    VideoPlayer secondVp, lastVp;
    bool changePage = false;
    int onCount = 0;

    private bool hasTriggered = false;

    private void OnEnable()
    {
        StopAllCoroutines();
        ClearObject();
        hasTriggered = false;
        onCount = 0;
        ArduinoManager.OnArduinoMsg -= OnArduinoMessage;
        ArduinoManager.OnArduinoMsg += OnArduinoMessage;
        StartCoroutine(InteractionSeq());
    }

    private void OnDisable()
    {
        ArduinoManager.OnArduinoMsg -= OnArduinoMessage;
        LoadManager.Instance.StopReturnTimer();
        ArduinoManager.Instance.SendToArduinoA("Off");
        ClearObject();
    }

    void ClearObject()
    {
        if (firstSentance != null) Destroy(firstSentance.gameObject);
        if (secondVp != null) Destroy(secondVp.gameObject);
        if (lastVp != null) Destroy(lastVp.gameObject);
        if (firstImg != null) Destroy(firstImg.gameObject);
        if (secondImg != null) Destroy(secondImg.gameObject);
        if (thirdImg != null) Destroy(thirdImg.gameObject);
    }

    IEnumerator InteractionSeq()
    {
        var interactionData = JsonManager.uljin.Interaction[0];
        var LM = LoadManager.Instance;

        ArduinoManager.Instance.SendToArduinoA("Off");
        ArduinoManager.Instance.SendToArduinoB("Off");
        yield return new WaitForSeconds(0.3f);

        firstSentance = LM.CreateUIObject<ImageData, Image>("firstSentance", interactionData.FirstSentance[0], LM.LoadImagePos, transform);
        yield return LM.FadeUnit(firstSentance, 1.5f);
        yield return new WaitForSeconds(0.5f);

        secondVp = LM.CreateUIObject<VideoData, VideoPlayer>("SecondVideo", interactionData.SecondVideo[0], LM.LoadVideo, transform);
        secondVp.isLooping = true;
        yield return new WaitForSeconds(2.5f);

        //StartCoroutine(testKey());
        lastVp = LM.CreateUIObject<VideoData, VideoPlayer>("lastVideo", interactionData.LastImage[0], LM.LoadVideo, transform);
        yield return new WaitForSeconds(2.5f);

        thirdImg = LM.CreateUIObject<ImageData, Image>("thirdImg", interactionData.ThirdImage[0], LM.LoadImagePos, transform);

        secondImg = LM.CreateUIObject<ImageData, Image>("secondImg", interactionData.SecondImage[0], LM.LoadImagePos, transform);
        firstImg = LM.CreateUIObject<ImageData, Image>("firstImg", interactionData.FirstImage[0], LM.LoadImagePos, transform);

        secondImg.gameObject.SetActive(false);
        firstImg.gameObject.SetActive(false);

        lastVp.isLooping = true;

        ArduinoManager.Instance.SendToArduinoA("Start");

        LM.ReturnToTitleTimer();
    }

    void OnArduinoMessage(string msg)
    {
        Debug.Log($"[Interaction] Arduino Message: {msg}, hasTriggered: {hasTriggered}, onCount: {onCount}");

        if (msg != "On") return;
        if (hasTriggered)
        {
            return;
        }

        onCount++;

        if (onCount == 1)
        {
            if (thirdImg != null) thirdImg.gameObject.SetActive(false);
            if (secondImg != null) secondImg.gameObject.SetActive(true);
            ArduinoManager.Instance.SendToArduinoB("On");
        }
        else if (onCount == 2)
        {
            if (secondImg != null) secondImg.gameObject.SetActive(false);
            if (firstImg != null) firstImg.gameObject.SetActive(true);
            ArduinoManager.Instance.SendToArduinoB("On1");
        }
        else if (onCount == 3)
        {
            if (firstImg != null) firstImg.gameObject.SetActive(false);
            hasTriggered = true;

            ArduinoManager.Instance.SendToArduinoB("On2");
            ArduinoManager.Instance.SendToArduinoA("Off");

            UIManager.Instance.TargetPage(UIManager.Pagelist.videoPage);

            ClientTCP.Instance.Send("지진소리");
        }
    }

    // 키보드 테스트용
    IEnumerator testKey()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (hasTriggered)
                {
                    yield break;
                }
                onCount++;

                if (onCount == 1)
                {
                    if (thirdImg != null) thirdImg.gameObject.SetActive(false);
                    if (secondImg != null) secondImg.gameObject.SetActive(true);
                    ArduinoManager.Instance.SendToArduinoB("On");
                }
                else if (onCount == 2)
                {
                    if (secondImg != null) secondImg.gameObject.SetActive(false);
                    if (firstImg != null) firstImg.gameObject.SetActive(true);
                    ArduinoManager.Instance.SendToArduinoB("On1");
                }
                else if (onCount == 3)
                {
                    if (firstImg != null) firstImg.gameObject.SetActive(false);
                    hasTriggered = true;

                    ArduinoManager.Instance.SendToArduinoB("On2");
                    ArduinoManager.Instance.SendToArduinoA("Off");
                    UIManager.Instance.TargetPage(UIManager.Pagelist.videoPage);
                    ClientTCP.Instance.Send("지진소리");

                    yield break;
                }
            }
            yield return null;
        }
    }
}