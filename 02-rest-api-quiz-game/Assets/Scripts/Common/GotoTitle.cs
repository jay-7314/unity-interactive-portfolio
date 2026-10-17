using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GotoTitle : MonoBehaviour
{
    public static GotoTitle Instance;
    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameObject title;
    [SerializeField] float wantToClickDelay = 5f;
    [SerializeField] float titleDelay = 10f;
    [SerializeField] float dangerTimer = 3f;
    List<RawImage> wanttoClicks = new List<RawImage>();
    List<Text> firstMsgs = new List<Text>();
    List<Text> secondMsgs = new List<Text>();
    Coroutine titleCor;

    public static event Action OnGoToTitle;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartTimer(RawImage wanttoClick)
    {
        if (!wanttoClicks.Contains(wanttoClick))
            wanttoClicks.Add(wanttoClick);
        if (titleCor == null)
            titleCor = StartCoroutine(TimerCor());
    }

    public void ResetTimer(RawImage wanttoClick)
    {
        wanttoClick.gameObject.SetActive(false);
        if (titleCor != null) StopCoroutine(titleCor);
        titleCor = StartCoroutine(TimerCor());
    }

    public void StopTimer(RawImage wanttoClick)
    {
        wanttoClicks.Remove(wanttoClick);
        wanttoClick.gameObject.SetActive(false);
        if (wanttoClicks.Count == 0)
        {
            if (titleCor != null) StopCoroutine(titleCor);
            titleCor = null;
        }
    }

    IEnumerator TimerCor()
    {
        yield return CoroutineData.GetWaitForSeconds(wantToClickDelay);
        foreach (var img in wanttoClicks)
            if (img != null) img.gameObject.SetActive(true);
        yield return CoroutineData.GetWaitForSeconds(titleDelay);
        wanttoClicks.Clear();
        OnGoToTitle?.Invoke();
        OnGoToTitle = null;
        Instance = null;

        title.SetActive(true);
        StartCoroutine(networkManager.UpdateRestart());
        StartCoroutine(networkManager.ClearRoomInsertPerson());
        yield return Resources.UnloadUnusedAssets();
        SceneManager.LoadScene(0);
    }

    public void StartTimerWithMessage(RawImage msgImage, Text firstMsg, Text secondMsg)
    {
        if (!wanttoClicks.Contains(msgImage))
        {
            wanttoClicks.Add(msgImage);
            firstMsgs.Add(firstMsg);
            secondMsgs.Add(secondMsg);
        }

        if (titleCor != null) StopCoroutine(titleCor);
        titleCor = StartCoroutine(TimerCorWithMessage());
    }

    IEnumerator TimerCorWithMessage()
    {
        yield return CoroutineData.GetWaitForSeconds(wantToClickDelay);
        for (int i = 0; i < wanttoClicks.Count; i++)
        {
            if (wanttoClicks[i] != null) wanttoClicks[i].gameObject.SetActive(true);
            if (firstMsgs[i] != null) firstMsgs[i].gameObject.SetActive(true);
            if (secondMsgs[i] != null) secondMsgs[i].gameObject.SetActive(false);
        }

        yield return CoroutineData.GetWaitForSeconds(titleDelay);
        for (int i = 0; i < wanttoClicks.Count; i++)
        {
            if (firstMsgs[i] != null) firstMsgs[i].gameObject.SetActive(false);
            if (secondMsgs[i] != null) secondMsgs[i].gameObject.SetActive(true);
        }

        yield return CoroutineData.GetWaitForSeconds(dangerTimer);
        for (int i = 0; i < secondMsgs.Count; i++)
        {
            if (wanttoClicks[i] != null) wanttoClicks[i].gameObject.SetActive(false);
            if (secondMsgs[i] != null) secondMsgs[i].gameObject.SetActive(false);
        }

        wanttoClicks.Clear();
        firstMsgs.Clear();
        secondMsgs.Clear();
        OnGoToTitle?.Invoke();
        OnGoToTitle = null;
        Instance = null;
        title.SetActive(true);
        StartCoroutine(networkManager.UpdateRestart());
        StartCoroutine(networkManager.ClearRoomInsertPerson());
        yield return Resources.UnloadUnusedAssets();
        SceneManager.LoadScene(0);
    }

    public void ResetTimerWithMessage(RawImage msgImage, Text firstMsg, Text secondMsg)
    {
        msgImage.gameObject.SetActive(false);
        firstMsg.gameObject.SetActive(false);
        secondMsg.gameObject.SetActive(false);

        if (titleCor != null) StopCoroutine(titleCor);
        titleCor = null;

        int index = wanttoClicks.IndexOf(msgImage);
        if (index >= 0)
        {
            wanttoClicks.RemoveAt(index);
            firstMsgs.RemoveAt(index);
            secondMsgs.RemoveAt(index);
        }

        StartTimerWithMessage(msgImage, firstMsg, secondMsg);
    }
    public void StopTimerWithMessage(RawImage msgImage, Text firstMsg, Text secondMsg)
    {
        int index = wanttoClicks.IndexOf(msgImage);
        if (index >= 0)
        {
            wanttoClicks.RemoveAt(index);
            firstMsgs.RemoveAt(index);
            secondMsgs.RemoveAt(index);
        }

        msgImage.gameObject.SetActive(false);
        firstMsg.gameObject.SetActive(false);
        secondMsg.gameObject.SetActive(false);

        if (wanttoClicks.Count == 0 && titleCor != null)
        {
            StopCoroutine(titleCor);
            titleCor = null;
        }
    }
}
