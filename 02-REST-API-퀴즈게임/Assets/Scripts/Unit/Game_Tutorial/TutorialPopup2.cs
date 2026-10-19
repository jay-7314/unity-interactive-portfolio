using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup2 : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] PlayerGamer playerGamer;
    [SerializeField] LeftBtnManager leftBtnManager;
    [SerializeField] GameObject tutorial_Second ,startGame_text, tutorial_Text, game_Tutorial, gameBg;
    [SerializeField] Text leftTxt, rightTxt;
    [SerializeField] AudioSource popupSound;
    [SerializeField] RightBtnManager rightbtnManager;
    [SerializeField] RawImage[] leftCircles, rightCircles;
    [SerializeField] Texture[] leftCirclePopups, rightCirclePopups;
    [SerializeField] RawImage[] leftArrow, rightArrow;
    [SerializeField] RawImage wanttoClick;
    [SerializeField] float moveSpeed = 400f;
    [SerializeField] Texture[] leftCircleDefaults, rightCircleDefaults;

    float[] xRightPos = {-95, -287, -480, -660, -850};
    float[] xLeftPos = {95, 287, 480, 660, 850};

    float startXLeft = -961f;
    float startXRight = 961f;

    bool isLeftFinish = false;
    bool isRightFinish = false;

    int leftNextIndex = 1;
    int rightNextIndex = 1;
    System.Action[] leftActions = new System.Action[5];
    System.Action[] rightActions = new System.Action[5];

    private void OnEnable()
    {
        // 텍스처 초기화
        for (int i = 0; i < leftCircles.Length; i++)
        {
            leftCircles[i].texture = leftCircleDefaults[i];
            leftCircles[i].gameObject.SetActive(false);
            leftArrow[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < rightCircles.Length; i++)
        {
            rightCircles[i].texture = rightCircleDefaults[i];
            rightCircles[i].gameObject.SetActive(false);
            rightArrow[i].gameObject.SetActive(false);
        }

        tutorial_Text.SetActive(true);

        leftNextIndex = 1;
        rightNextIndex = 1;
        isLeftFinish = false;
        isRightFinish = false;

        StartCoroutine(WaitforFrame());
        GotoTitle.Instance.StartTimer(wanttoClick);
    }
    private void OnDisable()
    {
        StopAllCoroutines();

        if (ButtonManager.instance != null)
        {
            for (int i = 0; i < leftActions.Length; i++)
            {
                if (leftActions[i] == null) continue;
                switch (i)
                {
                    case 0: ButtonManager.instance.OnYellowButton -= leftActions[i]; break;
                    case 1: ButtonManager.instance.OnBlueButton -= leftActions[i]; break;
                    case 2: ButtonManager.instance.OnGreenButton -= leftActions[i]; break;
                    case 3: ButtonManager.instance.OnRedButton -= leftActions[i]; break;
                    case 4: ButtonManager.instance.OnRedButton -= leftActions[i]; break;
                }
                leftActions[i] = null;
            }

            for (int i = 0; i < rightActions.Length; i++)
            {
                if (rightActions[i] == null) continue;
                switch (i)
                {
                    case 0: ButtonManager.instance.OnWhiteButton -= rightActions[i]; break;
                    case 1: ButtonManager.instance.OnWhiteButton -= rightActions[i]; break;
                    case 2: ButtonManager.instance.OnBlackButton -= rightActions[i]; break;
                    case 3: ButtonManager.instance.OnOrangeButton -= rightActions[i]; break;
                    case 4: ButtonManager.instance.OnWhiteBlueButton -= rightActions[i]; break;
                }
                rightActions[i] = null;
            }
        }
    }

    IEnumerator WaitforFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);
        LoadManager.Instance.LoadText(JsonManager.popup.Tutorial_Second_Text[0].Finish_Tutorial_Txt[0], leftTxt, networkManager.lang);
        LoadManager.Instance.LoadText(JsonManager.popup.Tutorial_Second_Text[0].Finish_Tutorial_Txt[0], rightTxt, networkManager.lang);
        tutorial_Text.SetActive(true);
        SetCircleStartPos(leftCircles[0], startXLeft);
        leftCircles[0].gameObject.SetActive(true);

        SetCircleStartPos(rightCircles[0], startXRight);
        rightCircles[0].gameObject.SetActive(true);

        StartCoroutine(MoveLeftCircles());
        StartCoroutine(MoveRightCircles());
    }


    void SetCircleStartPos(RawImage circle, float x)
    {
        Vector2 pos = circle.rectTransform.anchoredPosition;
        pos.x = x;
        circle.rectTransform.anchoredPosition = pos;
    }

    IEnumerator MoveLeftCircles()
    {
        while (true)
        {
            for (int i = 0; i < leftCircles.Length; i++)
            {
                if (!leftCircles[i].gameObject.activeSelf) continue;

                float currentX = leftCircles[i].rectTransform.anchoredPosition.x;
                float targetX = xRightPos[i]; 
                float newX = currentX + moveSpeed * Time.deltaTime; 
                newX = Mathf.Min(newX, targetX);
                leftCircles[i].rectTransform.anchoredPosition =
                    new Vector2(newX, leftCircles[i].rectTransform.anchoredPosition.y);
            }

            CheckLeftNext();

            int last = leftCircles.Length - 1;
            if (leftCircles[last].gameObject.activeSelf &&
                leftCircles[last].rectTransform.anchoredPosition.x >= xRightPos[last])
            {
                yield return StartCoroutine(WaitForLeftButtons());
                yield break;
            }

            yield return null;
        }
    }

    void CheckLeftNext()
    {
        if (leftNextIndex >= leftCircles.Length) return;

        float prevX = leftCircles[leftNextIndex - 1].rectTransform.anchoredPosition.x;
        if (prevX >= xRightPos[leftNextIndex - 1] - 80f)
        {
            SetCircleStartPos(leftCircles[leftNextIndex], startXLeft);
            leftCircles[leftNextIndex].gameObject.SetActive(true);
            leftNextIndex++;
        }
    }

    IEnumerator WaitForLeftButtons()
    {
        tutorial_Text.SetActive(false);

        for (int i = 0; i < leftCircles.Length; i++)
        {
            leftArrow[i].gameObject.SetActive(true);
            yield return StartCoroutine(WaitForLeftButton(i));
        }
        isLeftFinish = true;
        leftTxt.gameObject.SetActive(true);
        AllClear();
    }

    IEnumerator WaitForLeftButton(int index)
    {
        bool pressed = false;

        leftActions[index] = () =>
        {
            leftCircles[index].texture = leftCirclePopups[index];
            popupSound.Play();
            leftArrow[index].gameObject.SetActive(false);
            leftBtnManager.PressLeftBtn(index);
            playerGamer.AddLeftCount();
            GotoTitle.Instance.ResetTimer(wanttoClick);
            pressed = true;
        };

        switch (index)
        {
            case 0: ButtonManager.instance.OnYellowButton += leftActions[index]; break;
            case 1: ButtonManager.instance.OnBlueButton += leftActions[index]; break;
            case 2: ButtonManager.instance.OnGreenButton += leftActions[index]; break;
            case 3: ButtonManager.instance.OnRedButton += leftActions[index]; break;
            case 4: ButtonManager.instance.OnRedButton += leftActions[index]; break;
        }

        yield return new WaitUntil(() => pressed);

        // 정상 해제
        switch (index)
        {
            case 0: ButtonManager.instance.OnYellowButton -= leftActions[index]; break;
            case 1: ButtonManager.instance.OnBlueButton -= leftActions[index]; break;
            case 2: ButtonManager.instance.OnGreenButton -= leftActions[index]; break;
            case 3: ButtonManager.instance.OnRedButton -= leftActions[index]; break;
            case 4: ButtonManager.instance.OnRedButton -= leftActions[index]; break;
        }
    }

    IEnumerator MoveRightCircles()
    {
        while (true)
        {
            for (int i = 0; i < rightCircles.Length; i++)
            {
                if (!rightCircles[i].gameObject.activeSelf) continue;

                float currentX = rightCircles[i].rectTransform.anchoredPosition.x;
                float targetX = xLeftPos[i]; 
                float newX = currentX - moveSpeed * Time.deltaTime; 
                newX = Mathf.Max(newX, targetX);
                rightCircles[i].rectTransform.anchoredPosition =
                    new Vector2(newX, rightCircles[i].rectTransform.anchoredPosition.y);
            }

            CheckRightNext();

            int last = rightCircles.Length - 1;
            if (rightCircles[last].gameObject.activeSelf &&
                rightCircles[last].rectTransform.anchoredPosition.x <= xLeftPos[last])
            {
                yield return StartCoroutine(WaitForRightButtons());
                yield break;
            }

            yield return null;
        }
    }

    void CheckRightNext()
    {
        if (rightNextIndex >= rightCircles.Length) return;

        float prevX = rightCircles[rightNextIndex - 1].rectTransform.anchoredPosition.x;
        if (prevX <= xLeftPos[rightNextIndex - 1] + 80)
        {
            SetCircleStartPos(rightCircles[rightNextIndex], startXRight);
            rightCircles[rightNextIndex].gameObject.SetActive(true);
            rightNextIndex++;
        }
    }

    IEnumerator WaitForRightButtons()
    {
        tutorial_Text.SetActive(false);

        for (int i = 0; i < rightCircles.Length; i++)
        {
            rightArrow[i].gameObject.SetActive(true);
            yield return StartCoroutine(WaitForRightButton(i));
        }
        isRightFinish = true;
        rightTxt.gameObject.SetActive(true);
        AllClear();
    }

    IEnumerator WaitForRightButton(int index)
    {
        bool pressed = false;

        rightActions[index] = () =>
        {
            rightCircles[index].texture = rightCirclePopups[index];
            popupSound.Play();
            rightArrow[index].gameObject.SetActive(false);
            rightbtnManager.PressRightBtn(index);
            playerGamer.AddRightCount();
            GotoTitle.Instance.ResetTimer(wanttoClick);
            pressed = true;
        };

        switch (index)
        {
            case 0: ButtonManager.instance.OnWhiteButton += rightActions[index]; break;
            case 1: ButtonManager.instance.OnWhiteButton += rightActions[index]; break;
            case 2: ButtonManager.instance.OnBlackButton += rightActions[index]; break;
            case 3: ButtonManager.instance.OnOrangeButton += rightActions[index]; break;
            case 4: ButtonManager.instance.OnWhiteBlueButton += rightActions[index]; break;
        }

        yield return new WaitUntil(() => pressed);

        switch (index)
        {
            case 0: ButtonManager.instance.OnWhiteButton -= rightActions[index]; break;
            case 1: ButtonManager.instance.OnWhiteButton -= rightActions[index]; break;
            case 2: ButtonManager.instance.OnBlackButton -= rightActions[index]; break;
            case 3: ButtonManager.instance.OnOrangeButton -= rightActions[index]; break;
            case 4: ButtonManager.instance.OnWhiteBlueButton -= rightActions[index]; break;
        }
    }

    void AllClear()
    {
        if(isLeftFinish && isRightFinish)
        {
            GotoTitle.Instance.StopTimer(wanttoClick);
            StartCoroutine(StartGameText());
        }
    }

    IEnumerator StartGameText()
    {
        yield return CoroutineData.GetWaitForSeconds(1f);
        leftTxt.gameObject.SetActive(false);
        rightTxt.gameObject.SetActive(false);

        yield return CoroutineData.GetWaitForSeconds(0.5f);
        tutorial_Second.SetActive(false);
        game_Tutorial.SetActive(false);
        startGame_text.SetActive(true);
    }

}
