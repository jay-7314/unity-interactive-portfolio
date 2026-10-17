using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonFunc : MonoBehaviour
{
    public static CommonFunc instance;
    public RawImage fadePlate;
    public GameObject title;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    [Header("프로그램 종료 버튼")]
    [SerializeField] private int requiredClicks = 10;      // 필요한 클릭 수
    [SerializeField] private float timeLimit = 3f;         // 제한 시간(초)
    private float firstClickTime = 0f;
    private int clickCount = 0;

    private void Awake()
    {
        instance = this;
    }

    // 페이드 전환
    public void ButtonFunc(GameObject obj1, GameObject obj2, Action onComplete = null)
    {
        StartCoroutine(FadeSwap(obj1, obj2, onComplete));
    }

    public void ReturnBtnFunc(GameObject target, Action onComplete = null)
    {
        StartCoroutine(FadeSwap(target, title, onComplete));
    }

    // 페이드 없이 즉시 전환 (영상 종료처럼 딜레이 없이 넘어가야 할 때)
    public void ButtonFuncInstant(GameObject obj1, GameObject obj2, Action onComplete = null)
    {
        if (obj1 != null)
        {
            CanvasGroup cg1 = GetOrAddCanvasGroup(obj1);
            cg1.alpha = 1f;
            obj1.SetActive(false);
        }

        if (obj2 != null)
        {
            CanvasGroup cg2 = GetOrAddCanvasGroup(obj2);
            cg2.alpha = 1f;
            obj2.SetActive(true);
        }

        onComplete?.Invoke();
    }

    private IEnumerator FadeSwap(GameObject fadeOutObj, GameObject fadeInObj, Action onComplete)
    {

        if (fadeOutObj != null)
        {
            CanvasGroup cg1 = GetOrAddCanvasGroup(fadeOutObj);
            yield return StartCoroutine(FadeOutUnit(cg1, fadeDuration));
            fadeOutObj.SetActive(false);
        }

        if (fadeInObj != null)
        {
            CanvasGroup cg2 = GetOrAddCanvasGroup(fadeInObj);
            fadeInObj.SetActive(true);
            yield return StartCoroutine(FadeUnit(cg2, fadeDuration));
        }

        onComplete?.Invoke();
    }
    public IEnumerator FadeUnit(CanvasGroup cg, float duration)
    {
        float time = 0;
        cg.alpha = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(time / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    public IEnumerator FadeOutUnit(CanvasGroup cg, float duration)
    {
        float time = 0;
        cg.alpha = 1f;
        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = 1f - Mathf.Clamp01(time / duration);
            yield return null;
        }
        cg.alpha = 0f;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }

    public void ShutdownBtn()
    {
        // 첫 클릭이거나, 첫 클릭 후 제한 시간이 지났으면 새로 시작
        if (clickCount == 0 || Time.unscaledTime - firstClickTime > timeLimit)
        {
            clickCount = 0;
            firstClickTime = Time.unscaledTime;
        }

        clickCount++;

        if (clickCount >= requiredClicks)
        {
            clickCount = 0;
            Logger.Instance.SendLog("end_kill");
            Application.Quit();
        }
    }
}