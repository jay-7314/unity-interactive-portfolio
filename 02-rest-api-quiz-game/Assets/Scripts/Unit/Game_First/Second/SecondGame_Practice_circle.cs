using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SecondGame_Practice_circle : MonoBehaviour
{
    [SerializeField] List<RawImage> circles;
    [SerializeField] GameObject[] arrowImg;
    [SerializeField] LeftBtnManager leftBtnManager;
    [SerializeField] RightBtnManager rightBtnManager;
    [SerializeField] Texture[] popupImg;
    [SerializeField] GameObject second_TextBG;
    [SerializeField] Text Second_Text2, second_Text3, second_Text4, finish_Text, countDown;
    [SerializeField] float moveSpeed = 200f;
    [SerializeField] AudioSource popSound;
    [SerializeField] RawImage wanttoClick;
    [SerializeField] GameObject secondGameTutorial, secondGameText;
    System.Action[] buttonActions = new System.Action[5];

    float[] yPositions = { 345, 160, 0, -170, -340 };
    float[] stopPositions = { -340f, -170f, 0f, 160f, 345f };
    int nextCircleIndex = 1;

    private void OnEnable()
    {
        finish_Text.gameObject.SetActive(false);
        Second_Text2.gameObject.SetActive(false);
        second_Text3.gameObject.SetActive(false);
        second_Text4.gameObject.SetActive(false);
        countDown.gameObject.SetActive(false);
        second_TextBG.SetActive(false);
        for (int i = 0; i < circles.Count; i++)
            circles[i].gameObject.SetActive(false);
        nextCircleIndex = 1;

        StartCoroutine(StartDelay());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (ButtonManager.instance != null)
        {
            if (buttonActions[0] != null) ButtonManager.instance.OnYellowButton -= buttonActions[0];
            if (buttonActions[1] != null) ButtonManager.instance.OnBlackButton -= buttonActions[1];
            if (buttonActions[2] != null) ButtonManager.instance.OnRedButton -= buttonActions[2];
            if (buttonActions[3] != null) ButtonManager.instance.OnWhiteButton -= buttonActions[3];
            if (buttonActions[4] != null) ButtonManager.instance.OnPinkButton -= buttonActions[4];
        }

        for (int i = 0; i < buttonActions.Length; i++)
            buttonActions[i] = null;
    }

    IEnumerator StartDelay()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);
        GotoTitle.Instance.StartTimer(wanttoClick);
        circles[0].rectTransform.anchoredPosition = new Vector2(circles[0].rectTransform.anchoredPosition.x, yPositions[0]);
        circles[0].gameObject.SetActive(true);
        second_TextBG.SetActive(true);
        Second_Text2.gameObject.SetActive(true);
        StartCoroutine(MoveCircles());
    }

    IEnumerator MoveCircles()
    {
        while (true)
        {
            for (int i = 0; i < circles.Count; i++)
            {
                if (!circles[i].gameObject.activeSelf) continue;

                float currentY = circles[i].rectTransform.anchoredPosition.y;
                float newY = currentY - moveSpeed * Time.deltaTime;
                newY = Mathf.Max(newY, stopPositions[i]);
                circles[i].rectTransform.anchoredPosition = new Vector2(circles[i].rectTransform.anchoredPosition.x, newY);
            }

            CheckActiveNext();

            if (circles[circles.Count - 1].gameObject.activeSelf &&
                circles[circles.Count - 1].rectTransform.anchoredPosition.y <= stopPositions[circles.Count - 1])
            {
                Second_Text2.gameObject.SetActive(false);
                second_Text3.gameObject.SetActive(true);
                yield return StartCoroutine(WaitForButtons());
                yield break;
            }
            yield return null;
        }
    }

    void CheckActiveNext()
    {
        if (nextCircleIndex >= circles.Count) return;

        RectTransform prevCircle = circles[nextCircleIndex - 1].rectTransform;
        if (prevCircle.anchoredPosition.y <= 160f)
        {
            RectTransform nextCircle = circles[nextCircleIndex].rectTransform;
            nextCircle.anchoredPosition = new Vector2(nextCircle.anchoredPosition.x, yPositions[0]);
            nextCircle.gameObject.SetActive(true);
            nextCircleIndex++;
        }
    }

    IEnumerator WaitForButtons()
    {
        yield return CoroutineData.GetWaitForSeconds(1f);
        second_Text3.gameObject.SetActive(false);
        second_TextBG.SetActive(false);

        for (int i = 0; i < circles.Count; i++)
        {
            arrowImg[i].SetActive(true); 
            yield return StartCoroutine(WaitForButton(i));
        }
        GotoTitle.Instance.StopTimer(wanttoClick);

        finish_Text.gameObject.SetActive(true);
        yield return CoroutineData.GetWaitForSeconds(1f);
        finish_Text.gameObject.SetActive(false);
        
        for (int i = 0; i < circles.Count; i++)
            circles[i].gameObject.SetActive(false);

        second_TextBG.SetActive(true);
        second_Text4.gameObject.SetActive(true);
        yield return CoroutineData.GetWaitForSeconds(1f);
        second_TextBG.SetActive(false);
        second_Text4.gameObject.SetActive(false);
        countDown.gameObject.SetActive(true);
    }

    IEnumerator WaitForButton(int index)
    {
        bool pressed = false;

        buttonActions[index] = () =>
        {
            GotoTitle.Instance.ResetTimer(wanttoClick);
            circles[index].texture = popupImg[index];
            arrowImg[index].SetActive(false);
            pressed = true;
            popSound.Play();

            switch (index)
            {
                case 0: leftBtnManager.PressLeftBtnByGroup(2); break;
                case 1: rightBtnManager.PressRightBtnByGroup(0); break;
                case 2: leftBtnManager.PressLeftBtnByGroup(0); break;
                case 3: rightBtnManager.PressRightBtnByGroup(3); break;
                case 4: rightBtnManager.PressRightBtnByGroup(2); break;
            }
        };

        switch (index)
        {
            case 0: ButtonManager.instance.OnYellowButton += buttonActions[index]; break;
            case 1: ButtonManager.instance.OnBlackButton += buttonActions[index]; break;
            case 2: ButtonManager.instance.OnRedButton += buttonActions[index]; break;
            case 3: ButtonManager.instance.OnWhiteButton += buttonActions[index]; break;
            case 4: ButtonManager.instance.OnPinkButton += buttonActions[index]; break;
        }

        yield return new WaitUntil(() => pressed);

        switch (index)
        {
            case 0: ButtonManager.instance.OnYellowButton -= buttonActions[index]; break;
            case 1: ButtonManager.instance.OnBlackButton -= buttonActions[index]; break;
            case 2: ButtonManager.instance.OnRedButton -= buttonActions[index]; break;
            case 3: ButtonManager.instance.OnWhiteButton -= buttonActions[index]; break;
            case 4: ButtonManager.instance.OnPinkButton -= buttonActions[index]; break;
        }

        buttonActions[index] = null;
    }
}
