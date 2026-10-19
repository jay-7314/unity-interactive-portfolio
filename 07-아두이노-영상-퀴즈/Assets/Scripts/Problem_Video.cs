using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

enum VideoState
{
    PlayingInfo,
    WaitingAnswer,
    PlayingNext
}

public class Problem_Video : MonoBehaviour
{
    [SerializeField] GameObject title;
    [SerializeField] VideoPlayer vp;
    [SerializeField] GameObject wrongBg;
    [SerializeField] Image removePopup;
    [SerializeField] Image escapePopup;
    [SerializeField] RenderTexture videoRT;

    float idleTimer = 0f;
    float idleLimit;

    VideoState videoState = VideoState.PlayingInfo;

    int currentStep = 0;

    bool[] isPressed;
    bool[] wrongPressed;

    bool isTransitioning = false;
    bool initialCheckDone = false;

    readonly float[] pauseTimes = { 17f, 25f, 34f, 43f, 52f, 62f };
    readonly float[] playTimes = { 19f, 27f, 36f, 45f, 54f, 64f };

    string[] correct = { "On1", "On2", "On3", "On4", "On5", "On6" };
    string[] correct_Escape = { "On1_Off", "On2_Off", "On3_Off", "On4_Off", "On5_Off", "On6_Off" };

    string[] wrong = { "On1_Wrong", "On2_Wrong", "On3_Wrong", "On4_Wrong", "On5_Wrong", "On6_Wrong" };
    string[] wrong_Escape = { "On1_Wrong_Off", "On2_Wrong_Off", "On3_Wrong_Off", "On4_Wrong_Off", "On5_Wrong_Off", "On6_Wrong_Off" };


    private void Start()
    {
        wrongBg.SetActive(true);
        idleLimit = JsonManager.Frozen.IdleLimit;
    }
    void OnEnable()
    {
        ArduinoManager.OnArduinoMsg += OnMessageArduinoMsg;

        isPressed = new bool[correct.Length];
        wrongPressed = new bool[correct.Length];

        videoRT.Release();

        videoState = VideoState.PlayingInfo;
        currentStep = 0;
        isTransitioning = false;
        initialCheckDone = false;
        idleTimer = 0f;

        HidePopups();
    }

    void OnDisable()
    {
        ArduinoManager.OnArduinoMsg -= OnMessageArduinoMsg;
        vp.Stop();
    }

    void Update()
    {
        switch (videoState)
        {
            case VideoState.PlayingInfo:
                if (vp.time >= pauseTimes[0])
                    PauseAndWait();
                break;

            case VideoState.PlayingNext:
                if (vp.time >= pauseTimes[currentStep])
                    PauseAndWait();
                break;

            case VideoState.WaitingAnswer:
                CheckIdle();

                if (initialCheckDone)
                    EvaluateState();

                break;
        }
    }

    void PauseAndWait()
    {
        vp.Pause();
        videoState = VideoState.WaitingAnswer;
        initialCheckDone = false;
        idleTimer = 0f;

        StartCoroutine(InitialEvaluateAfterDelay(1f));
    }

    IEnumerator InitialEvaluateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (videoState != VideoState.WaitingAnswer)
            yield break;

        initialCheckDone = true;
        EvaluateState();
    }

    void EvaluateState()
    {
        if (isTransitioning) return;

        bool escapeActive = false;

        for (int i = 0; i < currentStep; i++)
        {
            if (!isPressed[i])
            {
                ShowEscape();
                escapeActive = true;
                break;
            }
        }

        if (escapeActive) return;

        HidePopups();

        if (wrongPressed[currentStep])
        {
            ShowWrong();
            return;
        }

        if (isPressed[currentStep])
        {
            Correct();
            return;
        }
    }


    void Correct()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        StartCoroutine(PlayFrom(playTimes[currentStep]));

        currentStep++;

        if (currentStep >= pauseTimes.Length)
        {
            StartCoroutine(ReturnToPage());
            return;
        }

        videoState = VideoState.PlayingNext;
        isTransitioning = false;
    }

    void OnMessageArduinoMsg(string msg)
    {
        for (int i = 0; i < correct.Length; i++)
        {
            if (msg == correct[i])
            {
                isPressed[i] = true;
                wrongPressed[i] = false;
            }

            if (msg == correct_Escape[i])
            {
                isPressed[i] = false;
            }

            if (msg == wrong[i])
            {
                wrongPressed[i] = true;
                isPressed[i] = false;
            }

            if (msg == wrong_Escape[i])
            {
                wrongPressed[i] = false;
            }
        }

        if (videoState == VideoState.WaitingAnswer && initialCheckDone)
            EvaluateState();
    }

    void ShowEscape()
    {
        wrongBg.SetActive(true);
        escapePopup.gameObject.SetActive(true);
        removePopup.gameObject.SetActive(false);
    }

    void ShowWrong()
    {
        wrongBg.SetActive(true);
        removePopup.gameObject.SetActive(true);
        escapePopup.gameObject.SetActive(false);
    }

    void HidePopups()
    {
        wrongBg.SetActive(false);
        removePopup.gameObject.SetActive(false);
        escapePopup.gameObject.SetActive(false);
    }

    void CheckIdle()
    {
        if (currentStep < 1) return;

        idleTimer += Time.deltaTime;

        if (idleTimer >= idleLimit)
        {
            InitializePopup();
        }
    }

    IEnumerator ReturnToPage()
    {
        yield return new WaitForSeconds(20f);
        InitializePopup();
    }

    IEnumerator PlayFrom(float time)
    {
        vp.Pause();
        vp.time = time;
        yield return null;
        vp.Play();
    }

    void InitializePopup()
    {
        for (int i = 0; i < isPressed.Length; i++)
        {
            isPressed[i] = false;
            wrongPressed[i] = false;
        }

        videoState = VideoState.PlayingInfo;
        currentStep = 0;
        isTransitioning = false;
        initialCheckDone = false;
        idleTimer = 0f;

        HidePopups();
        wrongBg.SetActive(true);
        title.SetActive(true);
    }

}
