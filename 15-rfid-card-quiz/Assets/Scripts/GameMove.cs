using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using TMPro;

public class GameMove : MonoBehaviour
{
    #region 인스펙터에서 받는것들
    [SerializeField] private GameObject[] Questions;
    [SerializeField] private GameObject[] Answers;
    [SerializeField] private GameObject[] CirclePositions;

    [SerializeField] private GameObject TitleSentenceGameObject;
    [SerializeField] private GameObject CheckPoint;
    [SerializeField] private GameObject ReturnGame;
    [SerializeField] private GameObject People;
    [SerializeField] private GameObject RFID_ReaderImage;
    [SerializeField] private GameObject ProplemPallet;
    [SerializeField] private GameObject PointCheck;
    [SerializeField] private TextMeshProUGUI WrongText;
    [SerializeField] private Transform RFID_Image;

    [SerializeField] private Sprite Title_Sentence1;
    [SerializeField] private Sprite Title_Sentence;
    [SerializeField] private Sprite Success_Sentence;
    [SerializeField] private Sprite Wrong_Sentence;
    [SerializeField] private Sprite Success_People;
    [SerializeField] private Sprite Wrong_People;
    [SerializeField] private Sprite Circle_Success;
    [SerializeField] private Sprite Circle_Wrong;
    [SerializeField] private Sprite Final_Wrong;

    [SerializeField] private Image answerText;

    [Header("결과 이미지 오브젝트 (각자 원하는 위치에 미리 배치해 두기)")]
    [Tooltip("Element 0 = 1회차에 정답, 1 = 2회차에 정답, 2 = 3회차에 정답")]
    [SerializeField] private GameObject[] successImageObjects;
    [Tooltip("3회 모두 오답일 때 보여줄 이미지")]
    [SerializeField] private GameObject finalWrongImageObject;

    [Header("Flip Animation Settings")]
    [SerializeField] private float riseHeight = 40f;
    [SerializeField] private float riseDuration = 0.35f;
    [SerializeField] private float flipDuration = 0.6f;
    [SerializeField] private float settleDuration = 0.3f;

    [Header("정답 설정")]
    [SerializeField] private string correctAnswer = "반하"; // Stage1 정답

    [Header("시도 횟수 설정")]
    [SerializeField] private int maxAttempts = 3;

    [Header("타임아웃(전체 리셋) 설정")]
    [SerializeField] private Games games; // 3개 중 하나를 랜덤 활성화하는 관리자

    private int revealedCount = 0;
    private int attemptCount = 0; // 지금까지 시도한 횟수 (0부터 시작, 오답/정답 시도할 때마다 증가)

    private float idleTimer = 0f;
    private bool idleTimerRunning = false;

    private string defaultWrongMessage; // WrongText에 원래 들어있던 오답 문구 (3회 오답 후 복원용)

    [Header("3회 오답 시 WrongText 위치")]
    [SerializeField] private float finalWrongTextY = 120f; // 3회 오답일 때만 적용할 anchoredPosition Y값

    private RectTransform wrongTextRect;
    private float defaultWrongTextY; // WrongText의 원래 Y 위치 (복원용)

    #endregion

    private void Awake()
    {
        // 3회 오답 때 WrongText 내용과 위치를 바꾸므로, 원래 문구와 Y 위치를 미리 저장해 둔다
        if (WrongText != null)
        {
            defaultWrongMessage = WrongText.text;
            wrongTextRect = WrongText.rectTransform;
            defaultWrongTextY = wrongTextRect.anchoredPosition.y;
        }
    }

    // WrongText의 Y 위치만 변경 (X는 그대로 유지)
    private void SetWrongTextY(float y)
    {
        if (wrongTextRect == null) return;

        Vector2 pos = wrongTextRect.anchoredPosition;
        pos.y = y;
        wrongTextRect.anchoredPosition = pos;
    }

    private void Start()
    {
        for (int i = 0; i < Questions.Length; i++)
        {
            int index = i;
            Button btn = Questions[i].GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => QuestionsClicked(index));
            }
        }
    }

    private void OnEnable()
    {
        ResetGameFull();
        // 시작 화면에서는 타이머를 돌리지 않는다
        idleTimer = 0f;
        idleTimerRunning = false;
    }

    private void OnDisable()
    {
        idleTimerRunning = false;

        DOTween.Kill(this);

        foreach (var q in Questions)
        {
            if (q != null)
            {
                RectTransform qRect = q.GetComponent<RectTransform>();
                if (qRect != null) DOTween.Kill(qRect);
            }
        }

        foreach (var a in Answers)
        {
            if (a != null)
            {
                RectTransform aRect = a.GetComponent<RectTransform>();
                if (aRect != null) DOTween.Kill(aRect);
            }
        }
    }

    private void Update()
    {
        if (!idleTimerRunning) return;

        idleTimer += Time.deltaTime;

        float timeoutSeconds = (JsonManager.rfid != null) ? JsonManager.rfid.ReturnTimer : 30f;

        if (idleTimer >= timeoutSeconds)
        {
            idleTimerRunning = false;
            StartCoroutine(ReturnToTitleCor());

            if (games != null)
            {
                games.ActivateRandomGame();
            }
        }
    }

    // 타임아웃으로 처음(타이틀)으로 돌아갈 때 호출. 로그 전송용.
    IEnumerator ReturnToTitleCor()
    {
        Logger.Instance.SendLog("move_idle_timeout");
        yield break;
    }

    // 사용자가 뭔가 조작(카드 클릭, RFID 판정)할 때마다 타이머를 0으로 되돌림
    private void ResetIdleTimer()
    {
        idleTimer = 0f;
        idleTimerRunning = true;
    }

    // 횟수별 결과 이미지 오브젝트를 전부 끈다
    private void HideAllResultImages()
    {
        if (successImageObjects != null)
        {
            for (int i = 0; i < successImageObjects.Length; i++)
            {
                if (successImageObjects[i] != null)
                {
                    successImageObjects[i].SetActive(false);
                }
            }
        }

        if (finalWrongImageObject != null)
        {
            finalWrongImageObject.SetActive(false);
        }
    }

    private void ResetCardsOnly()
    {
        WrongText.gameObject.SetActive(false);
        WrongText.text = defaultWrongMessage; // 3회 오답 때 바뀐 문구를 원래대로 복원
        SetWrongTextY(defaultWrongTextY);     // 3회 오답 때 바뀐 위치를 원래대로 복원

        // 정답 문구는 리셋할 때마다 꺼준다 (정답일 때만 보이도록)
        if (answerText != null)
        {
            answerText.gameObject.SetActive(false);
        }

        ReturnGame.SetActive(false);
        PointCheck.SetActive(false);
        ProplemPallet.SetActive(false);
        People.SetActive(false);
        CheckPoint.SetActive(false);
        HideAllResultImages();

        for (int i = 0; i < Questions.Length; i++)
        {
            Questions[i].SetActive(true);

            RectTransform qRect = Questions[i].GetComponent<RectTransform>();
            if (qRect != null)
            {
                qRect.localRotation = Quaternion.identity;
            }

            CanvasGroup qCanvasGroup = Questions[i].GetComponent<CanvasGroup>();
            if (qCanvasGroup != null)
            {
                qCanvasGroup.alpha = 1f;
                qCanvasGroup.interactable = true;
                qCanvasGroup.blocksRaycasts = true;
            }

            Button btn = Questions[i].GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = true;
            }
        }

        for (int i = 0; i < Answers.Length; i++)
        {
            Answers[i].SetActive(false);

            RectTransform aRect = Answers[i].GetComponent<RectTransform>();
            if (aRect != null)
            {
                aRect.localRotation = Quaternion.identity;
            }
        }

        if (RFID_Image != null)
        {
            foreach (Transform child in RFID_Image)
            {
                child.gameObject.SetActive(false);
            }
        }

        if (RFID_ReaderImage != null)
        {
            RFID_ReaderImage.SetActive(false);
        }

        if (TitleSentenceGameObject != null)
        {
            TitleSentenceGameObject.GetComponent<Image>().sprite = Title_Sentence;
        }

        revealedCount = 0;
    }

    private void ResetGameFull()
    {
        ResetCardsOnly();

        attemptCount = 0;

        for (int i = 0; i < CirclePositions.Length; i++)
        {
            CirclePositions[i].SetActive(false);
        }
    }


    public void QuestionsClicked(int index)
    {
        ResetIdleTimer(); // 첫 카드 클릭부터 타이머 시작, 이후 클릭은 0으로 되돌림
        TitleSentenceGameObject.GetComponent<Image>().sprite = Title_Sentence1;
        FlipCard(Questions[index], Answers[index]);
    }
    #region 카드 뒤집기
    private void FlipCard(GameObject question, GameObject answer)
    {
        RectTransform qRect = question.GetComponent<RectTransform>();
        RectTransform aRect = answer.GetComponent<RectTransform>();

        Button btn = question.GetComponent<Button>();
        if (btn != null) btn.interactable = false;

        Vector2 originalPos = qRect.anchoredPosition;

        CanvasGroup qCanvasGroup = question.GetComponent<CanvasGroup>();
        if (qCanvasGroup == null) qCanvasGroup = question.AddComponent<CanvasGroup>();

        Sequence seq = DOTween.Sequence();

        seq.Append(qRect.DOAnchorPosY(originalPos.y + riseHeight, riseDuration)
            .SetEase(Ease.OutQuad));

        seq.Append(qRect.DOLocalRotate(new Vector3(0, 90, 0), flipDuration / 2f)
            .SetEase(Ease.InOutSine));

        seq.AppendCallback(() =>
        {
            qCanvasGroup.alpha = 0f;
            qCanvasGroup.interactable = false;
            qCanvasGroup.blocksRaycasts = false;
            answer.SetActive(true);
        });

        seq.Append(aRect.DOLocalRotate(Vector3.zero, flipDuration / 2f)
            .SetEase(Ease.InOutSine));

        seq.OnComplete(() =>
        {
            revealedCount++;

            if (revealedCount >= Questions.Length)
            {
                if (RFID_ReaderImage != null)
                {
                    RFID_ReaderImage.SetActive(true);
                }
            }
        });
    }

    #endregion

    public void ActivateRFIDImage(string userName)
    {
        if (RFID_Image == null) return;

        string target = userName?.Trim().Normalize();
        bool found = false;

        foreach (Transform child in RFID_Image)
        {
            string childName = child.name.Trim().Normalize();
            bool isMatch = childName == target;
            child.gameObject.SetActive(isMatch);

            if (isMatch) found = true;
        }

        if (!found)
        {
            // 실제 자식 이름들을 길이와 함께 출력해서 어디가 다른지 확인
            string names = "";
            foreach (Transform child in RFID_Image)
                names += $"'{child.name}'(len={child.name.Length}) ";

            Debug.LogWarning($"[RFID] {gameObject.name}의 RFID_Image('{RFID_Image.name}') 안에서 '{userName}'(len={userName?.Length})을 못 찾음. 자식 목록: {names}");
        }
    }

    public bool IsRFIDReaderActive()
    {
        return gameObject.activeInHierarchy
            && RFID_ReaderImage != null
            && RFID_ReaderImage.activeSelf;
    }

    // ArduinoManager가 RFID 태깅 2초 뒤 호출
    public void CheckRFIDAnswer(string rfidValue)
    {
        if (!IsRFIDReaderActive())
        {
            return;
        }

        ResetIdleTimer();

        bool isCorrect = (rfidValue == correctAnswer);

#if UNITY_EDITOR
        Debug.Log($"[정답 비교] gameObject='{gameObject.name}', rfidValue='{rfidValue}'(len={rfidValue?.Length}), correctAnswer='{correctAnswer}'(len={correctAnswer?.Length}), 일치={isCorrect}");
#endif

        // 이번 시도의 순번을 기록에 사용 (0-based index)
        int thisAttemptIndex = attemptCount;
        attemptCount++;

        // 이전 시도에서 켜진 결과 이미지가 남지 않도록 먼저 모두 끈다
        HideAllResultImages();

        // 이전 시도에서 켜진 정답 문구도 먼저 끈다 (정답일 때만 다시 켠다)
        if (answerText != null)
        {
            answerText.gameObject.SetActive(false);
        }

        // 해당 시도 번호의 Circle 마커를 활성화하고 정답/오답 스프라이트 지정
        if (thisAttemptIndex >= 0 && thisAttemptIndex < CirclePositions.Length)
        {
            GameObject circle = CirclePositions[thisAttemptIndex];
            circle.SetActive(true);

            Image circleImage = circle.GetComponent<Image>();
            if (circleImage != null)
            {
                circleImage.sprite = isCorrect ? Circle_Success : Circle_Wrong;
            }
        }

        if (isCorrect)
        {
            Debug.Log($"정답입니다! ({rfidValue}) - {thisAttemptIndex + 1}회차에 정답");

            // 몇 회차에 맞췄든 정답이면 항상 활성화
            if (answerText != null)
            {
                answerText.gameObject.SetActive(true);
            }

            TitleSentenceGameObject.GetComponent<Image>().sprite = Success_Sentence;
            CheckPoint.SetActive(true);
            People.SetActive(true);
            ProplemPallet.SetActive(true);
            PointCheck.SetActive(true);
            People.GetComponent<Image>().sprite = Success_People;

            // 맞춘 횟수에 해당하는 이미지 오브젝트 활성화 (각자 미리 배치한 위치에 나타남)
            if (successImageObjects != null
                && thisAttemptIndex >= 0
                && thisAttemptIndex < successImageObjects.Length
                && successImageObjects[thisAttemptIndex] != null)
            {
                successImageObjects[thisAttemptIndex].SetActive(true);
            }

            for (var i = 0; i < Answers.Length; i++)
            {
                Answers[i].gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log($"오답입니다. ({rfidValue})");

            bool isGameOver = attemptCount >= maxAttempts;

            TitleSentenceGameObject.GetComponent<Image>().sprite = Wrong_Sentence;
            CheckPoint.SetActive(true);
            People.SetActive(true);
            ProplemPallet.SetActive(true);
            People.GetComponent<Image>().sprite = Wrong_People;
            PointCheck.SetActive(false);

            ReturnGame.SetActive(!isGameOver);

            if (isGameOver)
            {
                Debug.Log("게임오버: 시도 횟수를 모두 소진했습니다.");
                TitleSentenceGameObject.GetComponent<Image>().sprite = Final_Wrong;

                // 3회 모두 오답일 때 보여줄 이미지 오브젝트 활성화
                if (finalWrongImageObject != null)
                {
                    finalWrongImageObject.SetActive(true);
                }

                // 3회 모두 틀렸을 때: WrongText를 정답 안내 문구로 교체하고 Y 위치 변경
                WrongText.text = $"증상에 필요한 약재는 {correctAnswer} 입니다.";
                SetWrongTextY(finalWrongTextY);
            }
            else
            {
                // 아직 기회가 남았을 때: 기존 오답 문구와 원래 위치 유지
                WrongText.text = defaultWrongMessage;
                SetWrongTextY(defaultWrongTextY);
            }

            WrongText.gameObject.SetActive(true);

            for (var i = 0; i < Answers.Length; i++)
            {
                Answers[i].gameObject.SetActive(false);
            }
        }
    }

    public void ReturnToGameBtn()
    {
        ResetCardsOnly();
        ResetIdleTimer();
        GotoHome();
    }

    public void GotoHome()
    {
        Logger.Instance.SendLog("move_idle");
    }
}