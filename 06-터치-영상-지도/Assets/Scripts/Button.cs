using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TouchVideoButton : MonoBehaviour
{
    [Header("레이아웃 (JSON)")]
    [SerializeField] int buildingId;         // 0~9, 건물마다 다르게 지정

    [Header("레이아웃 기준점 (anchor = pivot)")]
    [SerializeField] Vector2 anchorPivot = new Vector2(0.5f, 0f);

    [Header("버튼 영상 (idle → touch)")]
    [SerializeField] VideoPlayer buttonVP;
    [SerializeField] RawImage buttonImage;

    [Header("메인 영상")]
    [SerializeField] VideoPlayer vp;
    [SerializeField] RawImage vpImage;

    [Header("버튼 최상단 표시")]
    [Tooltip("같은 부모 안에서 버튼을 맨 위(마지막 자식)로 올립니다.")]
    [SerializeField] bool bringButtonToTop = true;

    [Tooltip("켜면 계층 구조와 상관없이 Canvas 정렬 순서로 항상 위에 그립니다.")]
    [SerializeField] bool useCanvasOverride = false;
    [SerializeField] int buttonSortingOrder = 100;

    [Header("전체 버튼 숨김")]
    [Tooltip("어느 건물을 클릭하든 모든 버튼이 사라졌다가 이 시간(초) 뒤 다시 켜집니다.")]
    [SerializeField] float restoreDelay = 5f;

    // 씬에 있는 모든 TouchVideoButton
    private static readonly List<TouchVideoButton> all = new List<TouchVideoButton>();
    // true인 동안은 idle 버튼이 켜지지 않고, 클릭도 막힘
    private static bool buttonsLocked;

    // JSON에서 읽어오는 파일명
    private string idleFileName;
    private string touchFileName;
    private string mainFileName;

    private bool isBusy;
    private bool isPlayingTouch;
    private bool layoutApplied;
    private bool ownsLock;             // 이 건물이 숨김 타이머를 돌리는 중인지

    private void Start()
    {
        // JsonManager.Awake()가 먼저 실행된 뒤에 적용
        ApplyLayout();
    }

    private void OnEnable()
    {
        all.Add(this);

        buttonVP.prepareCompleted += OnButtonPrepared;
        buttonVP.loopPointReached += OnButtonClipEnd;
        buttonVP.errorReceived += OnVideoError;
        vp.prepareCompleted += OnMainPrepared;
        vp.loopPointReached += OnMainEnd;
        vp.errorReceived += OnVideoError;

        isBusy = false;
        vpImage.enabled = false;

        // 처음 켜질 때는 파일명이 아직 없으므로 Start에서 재생,
        // 껐다 다시 켜질 때만 여기서 idle 재생
        if (layoutApplied) ShowIdle();
    }

    private void OnDisable()
    {
        all.Remove(this);

        // 타이머를 돌리던 건물이 꺼지면 잠금이 영원히 남지 않도록 해제
        if (ownsLock)
        {
            ownsLock = false;
            buttonsLocked = false;
        }

        buttonVP.prepareCompleted -= OnButtonPrepared;
        buttonVP.loopPointReached -= OnButtonClipEnd;
        buttonVP.errorReceived -= OnVideoError;
        vp.prepareCompleted -= OnMainPrepared;
        vp.loopPointReached -= OnMainEnd;
        vp.errorReceived -= OnVideoError;
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"[{name}] 영상 오류: {message} (url={source.url})");
    }

    #region 레이아웃
    private void ApplyLayout()
    {
        Building b = JsonManager.GetBuilding(buildingId);
        if (b == null) return;

        idleFileName = b.idleVideo;
        touchFileName = b.touchVideo;
        mainFileName = b.mainVideo;

        SetRect(vpImage.rectTransform,
                b.buildingposX, b.buildingposY,
                b.buildingwidth, b.buildingheight);
        SetRect(buttonImage.rectTransform,
                b.buttonposX, b.buttonposY,
                b.buttonwidth, b.buttonheight);

        LayoutRebuilder.ForceRebuildLayoutImmediate(vpImage.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonImage.rectTransform);

        BringButtonToTop();

        layoutApplied = true;
        ShowIdle();   // 파일명이 준비된 뒤에 idle 시작
    }

    private void SetRect(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = anchorPivot;
        rt.anchorMax = anchorPivot;
        rt.pivot = anchorPivot;
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }

    // 버튼을 최상단으로 올림
    private void BringButtonToTop()
    {
        // 방법 1: 같은 부모 안에서 마지막 자식으로 (가장 위에 그려짐)
        if (bringButtonToTop)
            buttonImage.transform.SetAsLastSibling();

        // 방법 2: Canvas 정렬 순서로 계층과 무관하게 항상 위에 표시
        if (useCanvasOverride)
        {
            Canvas c = buttonImage.GetComponent<Canvas>();
            if (c == null) c = buttonImage.gameObject.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = buttonSortingOrder;

            // Canvas를 붙이면 터치(클릭)를 받으려면 GraphicRaycaster가 필요
            if (buttonImage.GetComponent<GraphicRaycaster>() == null)
                buttonImage.gameObject.AddComponent<GraphicRaycaster>();
        }
    }
    #endregion

    #region 전체 버튼 숨김 / 복구
    // 모든 버튼을 숨기고, restoreDelay 초 뒤에 다시 켬
    private void HideAllAndRestore()
    {
        buttonsLocked = true;
        ownsLock = true;

        foreach (var b in all)
            b.HideButton();

        StartCoroutine(RestoreRoutine());
    }

    private IEnumerator RestoreRoutine()
    {
        yield return new WaitForSeconds(restoreDelay);

        ownsLock = false;
        buttonsLocked = false;

        // 영상 재생 중인 건물은 건너뜀 (끝나면 OnMainEnd에서 idle 복귀)
        foreach (var b in all)
        {
            if (!b.isBusy) b.ShowIdle();
        }
    }

    private void HideButton()
    {
        // 이미 터치 → 메인 영상 흐름 중이면 건드리지 않음
        if (isBusy) return;

        buttonVP.Stop();
        buttonImage.enabled = false;
    }
    #endregion

    #region 영상 로직
    private string GetPath(string fileName)
    {
        return Path.Combine(Application.streamingAssetsPath, fileName);
    }

    // idle 버튼의 On Click에 연결
    public void OnTouch()
    {
        if (buttonsLocked) return;
        if (isBusy || string.IsNullOrEmpty(touchFileName)) return;
        isBusy = true;

        // 어느 건물을 눌러도 모든 버튼 숨김 → restoreDelay 초 후 복구
        HideAllAndRestore();

        PlayButtonClip(touchFileName, false);
    }

    // touch 끝 → 버튼 화면 숨기고 메인 영상 재생
    private void OnButtonClipEnd(VideoPlayer source)
    {
        if (!isPlayingTouch) return;   // idle 루프 무시
        isPlayingTouch = false;

        source.Stop();
        buttonImage.enabled = false;

        vp.Stop();
        vp.source = VideoSource.Url;
        vp.url = GetPath(mainFileName);
        vp.Prepare();                  // 준비되면 OnMainPrepared에서 재생
    }

    private void OnMainPrepared(VideoPlayer source)
    {
        vpImage.enabled = true;
        source.Play();
    }

    // 메인 영상 끝 → idle 복귀 (잠금 중이면 복구 시점까지 대기)
    private void OnMainEnd(VideoPlayer source)
    {
        source.Stop();
        vpImage.enabled = false;
        isBusy = false;
        ShowIdle();
    }

    private void ShowIdle()
    {
        if (buttonsLocked) return;     // 숨김 시간 동안은 idle 안 켬
        PlayButtonClip(idleFileName, true);
    }

    private void PlayButtonClip(string fileName, bool loop)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning($"[{name}] 영상 파일명이 비어 있습니다 (id={buildingId})");
            return;
        }

        isPlayingTouch = (fileName == touchFileName);

        buttonVP.Stop();
        buttonVP.source = VideoSource.Url;
        buttonVP.url = GetPath(fileName);
        buttonVP.isLooping = loop;
        buttonVP.Prepare();            // 준비되면 OnButtonPrepared에서 재생
    }

    private void OnButtonPrepared(VideoPlayer source)
    {
        // 숨김 중에 준비가 끝난 idle 영상은 켜지 않음
        if (buttonsLocked && !isPlayingTouch) return;

        buttonImage.enabled = true;
        source.Play();
    }
    #endregion
}