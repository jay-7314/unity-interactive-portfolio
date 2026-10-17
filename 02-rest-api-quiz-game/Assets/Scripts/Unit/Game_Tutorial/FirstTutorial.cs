using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstTutorial : MonoBehaviour
{
    [SerializeField] RawImage redBall, orangeBall, wanttoClick;
    [SerializeField] Texture redPopup, orangePopup;
    [SerializeField] AudioSource redballAudio, orangeBallAudio;
    [SerializeField] Image leftArrow, rightArrow, tutorialTextBox;
    [SerializeField] GameObject tutorialFirst ,tutorialSeconds, gameBg, tutorialBg_first;
    [SerializeField] float moveSpeed = 200f;
    [SerializeField] Texture redDefault, orangeDefault;
    [SerializeField] LeftBtnManager leftbtnManager;
    [SerializeField] RightBtnManager rightBtnManager;
    [SerializeField] PlayerGamer playerGamer;
    float redStartPos = -961f;
    float orangeStartPos = 961f;

    float redStopPos = -475f;
    float orangeStopPos = 475f;

    bool redPressed = false;
    bool orangePressed = false;

    private void OnEnable()
    {
        wanttoClick.gameObject.SetActive(false);
        redPressed = false;
        orangePressed = false;
 
        redBall.texture = redDefault;
        orangeBall.texture = orangeDefault;
        redBall.rectTransform.anchoredPosition = new Vector2(redStartPos, redBall.rectTransform.anchoredPosition.y);
        orangeBall.rectTransform.anchoredPosition = new Vector2(orangeStartPos, orangeBall.rectTransform.anchoredPosition.y);

        redBall.gameObject.SetActive(true);
        orangeBall.gameObject.SetActive(true);
        leftArrow.gameObject.SetActive(true);
        rightArrow.gameObject.SetActive(true);
        tutorialTextBox.gameObject.SetActive(true);

        StartCoroutine(StartDelay());

    }
    private void OnDisable()
    {
        StopAllCoroutines();
        if (ButtonManager.instance != null)
        {
            ButtonManager.instance.OnBlueButton -= OnRedPressed;
            ButtonManager.instance.OnBlackButton -= OnOrangePressed;
        }
    }

    IEnumerator StartDelay()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);

        ButtonManager.instance.OnBlueButton += OnRedPressed;
        ButtonManager.instance.OnBlackButton += OnOrangePressed;

        StartCoroutine(MoveBalls());
        StartCoroutine(EraserImg());
        GotoTitle.Instance.StartTimer(wanttoClick);
    }

    IEnumerator EraserImg()
    {
        yield return CoroutineData.GetWaitForSeconds(1f);
        tutorialTextBox.gameObject.SetActive(false);
    }

    void OnRedPressed()
    {
        leftbtnManager.FirstTutorialLeftBtn();
        redBall.texture = redPopup;
        redballAudio.Play();
        leftArrow.gameObject.SetActive(false);
        redPressed = true;
        GotoTitle.Instance.ResetTimer(wanttoClick);
        AllClear();
        playerGamer.AddLeftCount();
        ButtonManager.instance.OnBlueButton -= OnRedPressed;
    }

    void OnOrangePressed()
    {
        rightBtnManager.FirstTutorialRightBtn();
        orangeBall.texture = orangePopup;
        orangeBallAudio.Play();
        rightArrow.gameObject.SetActive(false);
        orangePressed = true;
        GotoTitle.Instance.ResetTimer(wanttoClick);
        AllClear();
        playerGamer.AddRightCount();
        ButtonManager.instance.OnBlackButton -= OnOrangePressed;
    }

    IEnumerator MoveBalls()
    {
        while (true)
        {
            if (!redPressed)
            {
                float redX = redBall.rectTransform.anchoredPosition.x;
                redX = Mathf.Min(redX + moveSpeed * Time.deltaTime, redStopPos);
                redBall.rectTransform.anchoredPosition = new Vector2(redX, redBall.rectTransform.anchoredPosition.y);
            }

            if (!orangePressed)
            {
                float orangeX = orangeBall.rectTransform.anchoredPosition.x;
                orangeX = Mathf.Max(orangeX - moveSpeed * Time.deltaTime, orangeStopPos);
                orangeBall.rectTransform.anchoredPosition = new Vector2(orangeX, orangeBall.rectTransform.anchoredPosition.y);
            }

            bool redDone = redPressed || redBall.rectTransform.anchoredPosition.x >= redStopPos;
            bool orangeDone = orangePressed || orangeBall.rectTransform.anchoredPosition.x <= orangeStopPos;

            yield return null;
        }
    }
    void AllClear()
    {
        if(redPressed && orangePressed)
        {
            GotoTitle.Instance.StopTimer(wanttoClick);
            StartCoroutine(WaitforEndFrame());
        }
    }

    IEnumerator WaitforEndFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(1f);
        redBall.gameObject.SetActive(false);
        orangeBall.gameObject.SetActive(false);
        tutorialSeconds.SetActive(true);
        tutorialFirst.SetActive(false);
    }
}
