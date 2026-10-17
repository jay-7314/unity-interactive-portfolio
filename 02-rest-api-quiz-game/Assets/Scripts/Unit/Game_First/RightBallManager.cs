using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RightBallManager : MonoBehaviour, IBallManager
{
    [Header("Ball Settings")]
    [SerializeField] GameObject[] ballPrefabs;
    [SerializeField] float moveTimer = 0.8f;
    [SerializeField] float stepSize = 194f;
    [SerializeField] Image warningImg;
    [SerializeField] Image destroyLine;
    [SerializeField] PlayGamePlay playerGamer;
    [SerializeField] RawImage star;
    [SerializeField] RawImage wanttoClick;
    [SerializeField] RightBtnManager rightbtnManager;
    [SerializeField] GameObject game_First, gameBg;
    [SerializeField] GameObject questionsObj;

    [Header("Ball Group Settings")]
    [SerializeField] BallGroup[] ballGroups;
    [SerializeField] Transform maskArea;

    [Header("Pop Settings")]
    [SerializeField] float fadeDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] AudioSource popSound;
    [SerializeField] AudioSource starSound;

    // 캐시
    Transform[] ballSpawnsPos;
    Transform destroyZone;
    RectTransform destroyLineRect;
    Texture starTexture;

    // 상태
    List<int> availableBullets = new List<int>();
    List<Ball> activeBalls = new List<Ball>();
    List<int> freeSlots = new List<int>();
    bool[] occupiedSlots;
    float[] lastSpawnX;
    bool isGamePaused = false;
    bool isWarningPlaying = false;
    bool isProcessing = false;

    // Pool
    BallPool[] ballPools;
    Dictionary<int, PopPool> popPools = new Dictionary<int, PopPool>();

    // 버튼 델리게이트
    System.Action OnBlack, OnOrange, OnPink, OnWhite, OnWhiteBlue;

    public int RoundIndexRight => playerGamer.roundIndexright;
    public bool IsFailed => false;

    private void OnEnable()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        ballSpawnsPos = new Transform[children.Length - 1];
        for (int i = 0; i < ballSpawnsPos.Length; i++)
            ballSpawnsPos[i] = children[i + 1];

        destroyZone = GameObject.FindWithTag("Destroy").transform;
        destroyLineRect = destroyLine.GetComponent<RectTransform>();

        occupiedSlots = new bool[ballSpawnsPos.Length];
        lastSpawnX = new float[ballSpawnsPos.Length];
        for (int i = 0; i < lastSpawnX.Length; i++)
            lastSpawnX[i] = float.MinValue;

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

        isGamePaused = false;
        activeBalls.Clear();
        availableBullets.Clear();
        for (int i = 0; i < ballPrefabs.Length; i++)
            availableBullets.Add(i);

        SpawnFirstBall();
        StartCoroutine(WaitForFrame());
    }

    private void OnDisable()
    {
        isProcessing = false;
        isGamePaused = false;
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
            if (OnBlack != null) ButtonManager.instance.OnBlackButton -= OnBlack;
            if (OnOrange != null) ButtonManager.instance.OnOrangeButton -= OnOrange;
            if (OnPink != null) ButtonManager.instance.OnPinkButton -= OnPink;
            if (OnWhite != null) ButtonManager.instance.OnWhiteButton -= OnWhite;
            if (OnWhiteBlue != null) ButtonManager.instance.OnWhiteBlueButton -= OnWhiteBlue;
        }

        OnBlack = OnOrange = OnPink = OnWhite = OnWhiteBlue = null;
    }

    IEnumerator WaitForFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);

        OnBlack = () => StartCoroutine(delayBtn(0));
        OnOrange = () => StartCoroutine(delayBtn(1));
        OnPink = () => StartCoroutine(delayBtn(2));
        OnWhite = () => StartCoroutine(delayBtn(3));
        OnWhiteBlue = () => StartCoroutine(delayBtn(4));

        ButtonManager.instance.OnBlackButton += OnBlack;
        ButtonManager.instance.OnOrangeButton += OnOrange;
        ButtonManager.instance.OnPinkButton += OnPink;
        ButtonManager.instance.OnWhiteButton += OnWhite;
        ButtonManager.instance.OnWhiteBlueButton += OnWhiteBlue;

        GotoTitle.Instance.StartTimer(wanttoClick);
    }

    IEnumerator delayBtn(int groupIndex)
    {
        yield return CoroutineData.GetWaitForSeconds(0.05f);
        if (isProcessing) yield break;
        GotoTitle.Instance.ResetTimer(wanttoClick);
        TryPopCloseGroup(groupIndex);
    }

    public void StartWarning()
    {
        if (!isWarningPlaying)
        {
            isWarningPlaying = true;
            StartCoroutine(WarningPopup());
        }
    }

    public IEnumerator WarningPopup()
    {
        isWarningPlaying = true;
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

    void TryPopCloseGroup(int groupIndex)
    {
        if (isGamePaused || isProcessing) return;
        if (groupIndex >= ballGroups.Length) return;
        Ball closest = null;
        float minDist = float.MaxValue;
        float zoneX = destroyZone.position.x;
        for (int i = 0; i < activeBalls.Count; i++)
        {
            Ball b = activeBalls[i];
            if (b == null) continue;
            float dist = Mathf.Abs(b.transform.position.x - zoneX);
            if (dist < minDist) { minDist = dist; closest = b; }
        }
        if (closest == null) return;
        BallGroup group = ballGroups[groupIndex];
        BallEntry entry = null;
        foreach (BallEntry e in group.ballEntries)
            if (e.ballId == closest.BallId) { entry = e; break; }
        if (entry == null) return;
        activeBalls.Remove(closest);
        rightbtnManager.PressRightBtnByGroup(groupIndex);
        GameObject popObj = popPools[entry.ballId].Get();
        popObj.GetComponent<RectTransform>().position = closest.GetComponent<RectTransform>().position;
        playerGamer.AddRightCount();
        if (playerGamer.count2 >= 8)
        {
            isProcessing = true;
            isGamePaused = true;
            playerGamer.count2 = 0;
            playerGamer.ChangeTextCount();
            foreach (Ball b in activeBalls)
            {
                if (b == null || b == closest) continue;
                b.OnMoveComplete -= OnBallMoveComplete;
                ballPools[b.BallId].Return(b);
            }
            activeBalls.Clear();
            activeBalls.Add(closest);
            for (int i = 0; i < occupiedSlots.Length; i++)
                occupiedSlots[i] = false;
            availableBullets.Clear();
            for (int i = 0; i < ballPrefabs.Length; i++)
                availableBullets.Add(i);
            QuestionsPopup popup = popObj.GetComponent<QuestionsPopup>();
            if (popup != null)
            {
                popup.Init(questionsObj);
                starTexture = popup.ShowStar(playerGamer.roundIndexright);
                playerGamer.roundIndexright++;
                starSound?.PlayOneShot(starSound.clip);
            }
            closest.TryPop(popObj, fadeDuration,
                onFadeComplete: () =>
                {
                    questionsObj.SetActive(true);
                    GotoTitle.Instance.StopTimer(wanttoClick);
                    if (starTexture != null)
                    {
                        star.texture = starTexture;
                        star.gameObject.SetActive(true);
                    }
                    else
                    {
                        star.texture = null;
                        star.gameObject.SetActive(false);
                    }
                },
                onPopReturn: () =>
                {
                    QuestionsPopup p = popObj.GetComponent<QuestionsPopup>();
                    if (p != null) p.Init(null);
                    popPools[entry.ballId].Return(popObj);
                });
        }
        else
        {
            closest.TryPop(popObj, fadeDuration,
                onFadeComplete: () =>
                {
                    star.texture = null;
                    star.gameObject.SetActive(false);
                },
                onPopReturn: () => popPools[entry.ballId].Return(popObj));
        }
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
        float maxX = float.MinValue;
        for (int i = 0; i < activeBalls.Count; i++)
        {
            Ball b = activeBalls[i];
            if (b == null) continue;
            if (b.transform.position.x > maxX) { maxX = b.transform.position.x; lastSpawnedBall = b; }
        }

        if (lastSpawnedBall != null)
        {
            float moved = lastSpawnedBall.transform.position.x - lastSpawnX[lastSpawnedBall.FactoryIndex];
            if (Mathf.Abs(moved) < stepSize) return -1;
        }

        freeSlots.Clear();
        for (int i = 0; i < occupiedSlots.Length; i++)
            if (!occupiedSlots[i]) freeSlots.Add(i);

        return freeSlots.Count == 0 ? -1 : freeSlots[Random.Range(0, freeSlots.Count)];
    }

    void BallFactory(int factoryIndex)
    {
        if (availableBullets.Count == 0) return;

        int randomIndex = Random.Range(0, availableBullets.Count);
        int ballId = availableBullets[randomIndex];
        availableBullets.RemoveAt(randomIndex);

        occupiedSlots[factoryIndex] = true;
        lastSpawnX[factoryIndex] = ballSpawnsPos[factoryIndex].position.x;

        Ball ball = ballPools[ballId].Get();
        ball.GetComponent<RectTransform>().position = ballSpawnsPos[factoryIndex].position;
        ball.Init(ballId, factoryIndex, this, moveTimer, GetMoveDirection(factoryIndex), stepSize, destroyLineRect, popSound);

        ball.OnMoveComplete += OnBallMoveComplete;
        activeBalls.Add(ball);
    }

    void OnBallMoveComplete(int oldFactoryIndex)
    {
        if (isGamePaused) return;
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
        if (isGamePaused) return;

        if (activeBalls.Count == 0)
        {
            SpawnFirstBall();
            return;
        }

        OnBallMoveComplete(factoryIndex);
    }

    Vector3 GetMoveDirection(int factoryIndex) => Vector3.left;

    public void RestartRound()
    {
        isGamePaused = false;
        isProcessing = false;
        playerGamer.count2 = 0;
        starTexture = null;
        star.texture = null;
        star.gameObject.SetActive(false);

        for (int i = 0; i < occupiedSlots.Length; i++)
            occupiedSlots[i] = false;

        activeBalls.Clear();
        availableBullets.Clear();
        for (int i = 0; i < ballPrefabs.Length; i++)
            availableBullets.Add(i);

        SpawnFirstBall();
        GotoTitle.Instance.StartTimer(wanttoClick);
    }
}