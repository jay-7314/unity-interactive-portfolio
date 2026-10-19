using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoadingPage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] Image orangeBar;
    [SerializeField] private float fakeProgressDuration = 30f;
    [SerializeField] private float fakeProgressCap = 0.9f;

    [Header("페이지 전환 (연출 끝난 뒤 사용)")]
    [SerializeField] private ResultPage resultPage; // ResultPage 스크립트

    [Header("완료 후 재생할 동영상 (LoadingPage의 자식 RawImage)")]
    [SerializeField] private GameObject videoRawImage; // RawImage가 붙은 오브젝트
    [SerializeField] private VideoPlayer videoPlayer;
    [Header("실패 시 이동할 페이지")]
    [SerializeField] private GameObject failPage;

    private float currentProgress = 0f;
    private bool isDone = false;
    private Coroutine progressRoutine;

    private Texture2D pendingImage;
    private string pendingImagePath;

    private void OnEnable()
    {
        DrawingEvents.OnSubmitStarted += StartLoading;
        DrawingEvents.OnResultReady += HandleResultReady;
        DrawingEvents.OnMatchFailed += HandleMatchFailed;
        currentProgress = 0f;
        UpdateText();
        if (orangeBar != null)
            orangeBar.fillAmount = 0f;
    }

    private void OnDisable()
    {
        DrawingEvents.OnSubmitStarted -= StartLoading;
        DrawingEvents.OnResultReady -= HandleResultReady;
        DrawingEvents.OnMatchFailed -= HandleMatchFailed;
    }

    private void HandleResultReady(Texture2D image, string imagePath)
    {
        pendingImage = image;
        pendingImagePath = imagePath;

        CompleteLoading(() =>
        {
            StartCoroutine(PlayVideoThenShowResult());
        });
    }

    private IEnumerator PlayVideoThenShowResult()
    {
        // 진행률 UI 숨기고 영상 UI 보이기
        progressText.gameObject.SetActive(false);
        orangeBar.gameObject.SetActive(false);
        videoRawImage.SetActive(true);

        videoPlayer.Stop();
        videoPlayer.time = 0;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();

        yield return new WaitForSeconds((float)videoPlayer.length);

        Sprite sprite = Sprite.Create(
            pendingImage,
            new Rect(0, 0, pendingImage.width, pendingImage.height),
            new Vector2(0.5f, 0.5f)
        );

        // 영상이 끝나면 즉시(딜레이 없이) ResultPage로 전환
        CommonFunc.instance.ButtonFuncInstant(gameObject, resultPage.gameObject, () =>
        {
            resultPage.ShowResult(sprite, pendingImage);

            // 다음번 로딩을 위해 원래 상태로 복구
            videoRawImage.SetActive(false);
            progressText.gameObject.SetActive(true);
            orangeBar.gameObject.SetActive(true);
        });
    }

    private void HandleMatchFailed(string error)
    {
        isDone = true;
        if (progressRoutine != null) StopCoroutine(progressRoutine);

        CommonFunc.instance.ButtonFuncInstant(gameObject, failPage);
    }

    public void StartLoading()
    {
        currentProgress = 0f;
        isDone = false;
        gameObject.SetActive(true);
        progressRoutine = StartCoroutine(FakeProgressRoutine());
    }

    private IEnumerator FakeProgressRoutine()
    {
        float elapsed = 0f;

        while (elapsed < fakeProgressDuration && !isDone)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fakeProgressDuration;

            currentProgress = Mathf.Lerp(0f, fakeProgressCap, EaseOut(t));
            orangeBar.fillAmount = currentProgress;
            UpdateText();
            yield return null;
        }

        currentProgress = fakeProgressCap;
        orangeBar.fillAmount = currentProgress;
        UpdateText();

        while (!isDone)
        {
            yield return null;
        }
    }

    public void CompleteLoading(Action onCompleteEffectDone)
    {
        isDone = true;
        if (progressRoutine != null) StopCoroutine(progressRoutine);
        StartCoroutine(SnapTo100AndEffect(onCompleteEffectDone));
    }

    private IEnumerator SnapTo100AndEffect(Action onCompleteEffectDone)
    {
        float start = currentProgress;
        float t = 0f;
        float snapDuration = 0.4f;

        while (t < snapDuration)
        {
            t += Time.deltaTime;
            currentProgress = Mathf.Lerp(start, 1f, t / snapDuration);
            orangeBar.fillAmount = currentProgress;
            UpdateText();
            yield return null;
        }

        currentProgress = 1f;
        orangeBar.fillAmount = 1f;
        UpdateText();

        yield return PlayPangEffect();

        onCompleteEffectDone?.Invoke();
    }

    private IEnumerator PlayPangEffect()
    {
        yield return new WaitForSeconds(0.3f);
    }

    private void UpdateText()
    {
        int percent = Mathf.RoundToInt(currentProgress * 100f);
        progressText.text = $"{percent}%";
    }
    private float EaseOut(float t) => 1f - Mathf.Pow(1f - t, 3f);
}