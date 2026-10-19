using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecondSpawn : MonoBehaviour, IBallManager
{
    [SerializeField] GameObject[] ballPrefabs;
    [SerializeField] float moveTimer = 0.8f;
    [SerializeField] float stepSize = 160f;
    [SerializeField] Image warningImg;
    [SerializeField] Image destroyLine;
    [SerializeField] Text timerText;
    [SerializeField] LeftBtnManager leftbtnManager;
    [SerializeField] RightBtnManager rightbtnManager;
    [SerializeField] GameObject clearImg;
    [SerializeField] GameOver gameover;
    [SerializeField] RawImage wanttoClick;

    [Header("Ball Group Settings")]
    [SerializeField] BallGroup[] ballGroups;
    [SerializeField] Transform maskArea;

    [Header("Pop Settings")]
    [SerializeField] float fadeDuration = 0.5f;
    public bool IsFailed => isFailed;

    [Header("Speed Settings")]
    [SerializeField] float minMoveTimer = 0.1f;
    [SerializeField] float speedUpInterval = 5f;
    [SerializeField] float speedUpAmount = 0.05f;

    [Header("Game Settings")]
    [SerializeField] float gameDuration = 60f;
    [SerializeField] PointManager pointManager;
    [SerializeField] Texture failedTexture;
    [SerializeField] GameObject gameoverObj;

    [SerializeField] AudioSource popSound;
    [SerializeField] AudioSource timerSound;

    // 캐시
    Transform[] ballSpawnsPos;
    Transform destroyZone;
    RectTransform destroyLineRect;

    // 상태
    bool isFailed = false;
    bool isGameOver = false;
    bool isWarningPlaying = false;
    bool[] occupiedSlots;
    float[] lastSpawnY;
    float currentMoveTimer;

    List<int> availableBullets = new List<int>();
    List<Ball> activeBalls = new List<Ball>();
    List<int> freeSlots = new List<int>();

    // Pool
    BallPool[] ballPools;
    Dictionary<int, PopPool> popPools = new Dictionary<int, PopPool>();

    // 버튼 델리게이트
    System.Action onRed, onBlue, onYellow, onGreen, onPupple;
    System.Action OnBlack, OnOrange, OnPink, OnWhite, OnWhiteBlue;

    int totalSpawnCount = 0;

    void Start()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        ballSpawnsPos = new Transform[children.Length - 1];
        for (int i = 0; i < ballSpawnsPos.Length; i++)
            ballSpawnsPos[i] = children[i + 1];

        destroyZone = GameObject.FindWithTag("Destroy").transform;
        destroyLineRect = destroyLine.GetComponent<RectTransform>();

        occupiedSlots = new bool[ballSpawnsPos.Length];
        lastSpawnY = new float[ballSpawnsPos.Length];
        for (int i = 0; i < lastSpawnY.Length; i++)
            lastSpawnY[i] = float.MaxValue;

        // Ball Pool 초기화
        ballPools = new BallPool[ballPrefabs.Length];
        for (int i = 0; i < ballPrefabs.Length; i++)
            ballPools[i] = new BallPool(ballPrefabs[i], maskArea, preWarm: 1);

        // Pop Pool 초기화
        popPools.Clear();
        foreach (BallGroup group in ballGroups)
            foreach (BallEntry entry in group.ballEntries)
                if (!popPools.ContainsKey(entry.ballId))
                    popPools[entry.ballId] = new PopPool(entry.popPrefab, maskArea, preWarm: 1);

        for (int i = 0; i < ballPrefabs.Length; i++)
            availableBullets.Add(i);

        currentMoveTimer = moveTimer;

        SpawnFirstBall();
        StartCoroutine(WaitForFrame());
        StartCoroutine(SpeedUpRoutine());
        StartCoroutine(GameTimer());
    }

    IEnumerator WaitForFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);

        onRed = () => StartCoroutine(delayBtn(0, success => { if (success) leftbtnManager.PressLeftBtnByGroup(0); }));
        onBlue = () => StartCoroutine(delayBtn(1, success => { if (success) leftbtnManager.PressLeftBtnByGroup(1); }));
        onYellow = () => StartCoroutine(delayBtn(2, success => { if (success) leftbtnManager.PressLeftBtnByGroup(2); }));
        onGreen = () => StartCoroutine(delayBtn(3, success => { if (success) leftbtnManager.PressLeftBtnByGroup(3); }));
        onPupple = () => StartCoroutine(delayBtn(4, success => { if (success) leftbtnManager.PressLeftBtnByGroup(4); }));
        OnBlack = () => StartCoroutine(delayBtn(5, success => { if (success) rightbtnManager.PressRightBtnByGroup(0); }));
        OnOrange = () => StartCoroutine(delayBtn(6, success => { if (success) rightbtnManager.PressRightBtnByGroup(1); }));
        OnPink = () => StartCoroutine(delayBtn(7, success => { if (success) rightbtnManager.PressRightBtnByGroup(2); }));
        OnWhite = () => StartCoroutine(delayBtn(8, success => { if (success) rightbtnManager.PressRightBtnByGroup(3); }));
        OnWhiteBlue = () => StartCoroutine(delayBtn(9, success => { if (success) rightbtnManager.PressRightBtnByGroup(4); }));

        ButtonManager.instance.OnRedButton += onRed;
        ButtonManager.instance.OnBlueButton += onBlue;
        ButtonManager.instance.OnYellowButton += onYellow;
        ButtonManager.instance.OnGreenButton += onGreen;
        ButtonManager.instance.OnPuppleButton += onPupple;
        ButtonManager.instance.OnBlackButton += OnBlack;
        ButtonManager.instance.OnOrangeButton += OnOrange;
        ButtonManager.instance.OnPinkButton += OnPink;
        ButtonManager.instance.OnWhiteButton += OnWhite;
        ButtonManager.instance.OnWhiteBlueButton += OnWhiteBlue;

        GotoTitle.Instance.StartTimer(wanttoClick);
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        foreach (Ball b in activeBalls)
        {
            if (b == null) continue;
            b.OnMoveComplete -= OnBallMoveComplete;
            ballPools[b.BallId].Return(b);
        }
        activeBalls.Clear();
        popPools.Clear();

        if (ButtonManager.instance != null)
        {
            if (onRed != null) ButtonManager.instance.OnRedButton -= onRed;
            if (onBlue != null) ButtonManager.instance.OnBlueButton -= onBlue;
            if (onYellow != null) ButtonManager.instance.OnYellowButton -= onYellow;
            if (onGreen != null) ButtonManager.instance.OnGreenButton -= onGreen;
            if (onPupple != null) ButtonManager.instance.OnPuppleButton -= onPupple;
            if (OnBlack != null) ButtonManager.instance.OnBlackButton -= OnBlack;
            if (OnOrange != null) ButtonManager.instance.OnOrangeButton -= OnOrange;
            if (OnPink != null) ButtonManager.instance.OnPinkButton -= OnPink;
            if (OnWhite != null) ButtonManager.instance.OnWhiteButton -= OnWhite;
            if (OnWhiteBlue != null) ButtonManager.instance.OnWhiteBlueButton -= OnWhiteBlue;
        }

        onRed = onBlue = onYellow = onGreen = onPupple = null;
        OnBlack = OnOrange = OnPink = OnWhite = OnWhiteBlue = null;

        if (GotoTitle.Instance != null)
            GotoTitle.Instance.StopTimer(wanttoClick);
    }

    IEnumerator delayBtn(int groupIndex, System.Action<bool> onComplete)
    {
        yield return new WaitForSeconds(0.01f);
        bool result = TryPopCloseGroup(groupIndex);
        if (result) GotoTitle.Instance.ResetTimer(wanttoClick);
        onComplete?.Invoke(result);
    }

    IEnumerator GameTimer()
    {
        float remaining = gameDuration;
        int lastSecond = Mathf.CeilToInt(remaining);

        while (remaining > 0f && !isGameOver)
        {
            remaining -= Time.deltaTime;
            remaining = Mathf.Max(remaining, 0f);
            int currentSecond = Mathf.CeilToInt(remaining);

            if (currentSecond < lastSecond)
            {
                lastSecond = currentSecond;
                if (timerSound != null) timerSound.Play();
            }

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(remaining).ToString();

            if (pointManager != null && pointManager.IsCleared)
            {
                isGameOver = true;
                GotoTitle.Instance.StopTimer(wanttoClick);
                StartCoroutine(ClearSequence());
                yield break;
            }

            yield return null;
        }

        if (!isGameOver)
        {
            isFailed = true;
            isGameOver = true;
            GotoTitle.Instance.StopTimer(wanttoClick);
            if (timerText != null) timerText.text = "0";

            for (int i = 0; i < activeBalls.Count; i++)
                if (activeBalls[i] != null) activeBalls[i].ChangeTexture(failedTexture);

            yield return CoroutineData.GetWaitForSeconds(1f);
            gameoverObj.SetActive(true);
        }
    }

    IEnumerator ClearSequence()
    {
        if (timerText != null) timerText.text = "0";

        List<Ball> ballsToTop = new List<Ball>();
        for (int i = 0; i < activeBalls.Count; i++)
            if (activeBalls[i] != null) ballsToTop.Add(activeBalls[i]);

        float zoneY = destroyZone.position.y;
        ballsToTop.Sort((a, b) =>
            Mathf.Abs(a.transform.position.y - zoneY)
            .CompareTo(Mathf.Abs(b.transform.position.y - zoneY)));

        for (int i = 0; i < ballsToTop.Count; i++)
        {
            Ball b = ballsToTop[i];
            if (b == null) continue;

            BallGroup group = null;
            foreach (BallGroup g in ballGroups)
                foreach (BallEntry e in g.ballEntries)
                    if (e.ballId == b.BallId) { group = g; break; }
            if (group == null) continue;

            BallEntry entry = null;
            foreach (BallEntry e in group.ballEntries)
                if (e.ballId == b.BallId) { entry = e; break; }
            if (entry == null) continue;

            GameObject popObj = popPools[entry.ballId].Get();
            popObj.GetComponent<RectTransform>().position = b.GetComponent<RectTransform>().position;
            b.TryPop(popObj, fadeDuration,
                onPopReturn: () => popPools[entry.ballId].Return(popObj));

            yield return CoroutineData.GetWaitForSeconds(0.1f);
        }

        clearImg.SetActive(true);
        yield return CoroutineData.GetWaitForSeconds(2f);
        yield return StartCoroutine(gameover.AllmageFade(true));
    }

    IEnumerator SpeedUpRoutine()
    {
        while (!isGameOver)
        {
            yield return CoroutineData.GetWaitForSeconds(speedUpInterval);
            if (isGameOver) yield break;

            currentMoveTimer = Mathf.Max(currentMoveTimer - speedUpAmount, minMoveTimer);

            for (int i = 0; i < activeBalls.Count; i++)
                if (activeBalls[i] != null) activeBalls[i].UpdateMoveTimer(currentMoveTimer);
        }
    }

    bool TryPopCloseGroup(int groupIndex)
    {
        if (isGameOver) return false;
        if (groupIndex >= ballGroups.Length) return false;

        Ball closest = null;
        float minDist = float.MaxValue;
        float zoneY = destroyZone.position.y;
        for (int i = 0; i < activeBalls.Count; i++)
        {
            Ball b = activeBalls[i];
            if (b == null) continue;
            float dist = Mathf.Abs(b.transform.position.y - zoneY);
            if (dist < minDist) { minDist = dist; closest = b; }
        }
        if (closest == null) return false;

        BallGroup group = ballGroups[groupIndex];
        BallEntry entry = null;
        foreach (BallEntry e in group.ballEntries)
            if (e.ballId == closest.BallId) { entry = e; break; }
        if (entry == null) return false;

        GameObject popObj = popPools[entry.ballId].Get();
        popObj.GetComponent<RectTransform>().position = closest.GetComponent<RectTransform>().position;

        pointManager?.AddPoints();
        closest.TryPop(popObj, fadeDuration,
            onPopReturn: () => popPools[entry.ballId].Return(popObj));
        return true;
    }

    void SpawnFirstBall()
    {
        int spawnIndex = GetRandomFreeSlot();
        if (spawnIndex == -1) return;
        BallFactory(spawnIndex);
    }

    int GetRandomFreeSlot()
    {
        Ball lastSpawnedBall = null;
        float maxY = float.MinValue;
        for (int i = 0; i < activeBalls.Count; i++)
        {
            Ball b = activeBalls[i];
            if (b == null) continue;
            if (b.transform.position.y > maxY) { maxY = b.transform.position.y; lastSpawnedBall = b; }
        }

        if (lastSpawnedBall != null)
        {
            float moved = lastSpawnY[lastSpawnedBall.FactoryIndex] - lastSpawnedBall.transform.position.y;
            if (moved < stepSize) return -1;
        }

        freeSlots.Clear();
        for (int i = 0; i < occupiedSlots.Length; i++)
            if (!occupiedSlots[i]) freeSlots.Add(i);

        return freeSlots.Count == 0 ? -1 : freeSlots[Random.Range(0, freeSlots.Count)];
    }

    void BallFactory(int factoryIndex)
    {
        if (availableBullets.Count == 0) return;
        if (isGameOver) return;

        totalSpawnCount++; 
        Debug.Log($"[스폰 누적] {totalSpawnCount}개");

        int randomIndex = Random.Range(0, availableBullets.Count);
        int ballId = availableBullets[randomIndex];
        availableBullets.RemoveAt(randomIndex);

        occupiedSlots[factoryIndex] = true;
        lastSpawnY[factoryIndex] = ballSpawnsPos[factoryIndex].position.y;

        Ball ball = ballPools[ballId].Get();
        ball.GetComponent<RectTransform>().position = ballSpawnsPos[factoryIndex].position;
        if (isFailed) ball.ChangeTexture(failedTexture);
        ball.Init(ballId, factoryIndex, this, currentMoveTimer, Vector3.down, stepSize, destroyLineRect, popSound, true);

        ball.OnMoveComplete += OnBallMoveComplete;
        activeBalls.Add(ball);
    }

    void OnBallMoveComplete(int oldFactoryIndex)
    {
        if (isGameOver) return;
        int newIndex = GetRandomFreeSlot();
        if (newIndex == -1) return;
        BallFactory(newIndex);
    }

    public void ReturnBall(int ballId, int factoryIndex, Ball ball)
    {
        activeBalls.Remove(ball);
        availableBullets.Add(ballId);
        occupiedSlots[factoryIndex] = false;
        ballPools[ballId].Return(ball);
        if (isGameOver) return;
        OnBallMoveComplete(factoryIndex);
    }

    public void StartWarning()
    {
        if (!isWarningPlaying)
        {
            isWarningPlaying = true;
            StartCoroutine(WarningPopup());
        }
    }

    IEnumerator WarningPopup()
    {
        warningImg.gameObject.SetActive(true);
        float blinkDuration = 0.3f;

        for (int i = 0; i < 3; i++)
        {
            float elapsed = 0f;
            while (elapsed < blinkDuration)
            {
                elapsed += Time.deltaTime;
                AlphaManager.instance.SetAlpha(warningImg, Mathf.Lerp(0f, 1f, elapsed / blinkDuration));
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < blinkDuration)
            {
                elapsed += Time.deltaTime;
                AlphaManager.instance.SetAlpha(warningImg, Mathf.Lerp(1f, 0f, elapsed / blinkDuration));
                yield return null;
            }
        }

        warningImg.gameObject.SetActive(false);
        isWarningPlaying = false;
    }
}