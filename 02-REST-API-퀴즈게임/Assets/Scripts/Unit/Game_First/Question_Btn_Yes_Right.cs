using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Question_Btn_Yes_Right : MonoBehaviour
{
    [SerializeField] Question_Btn_No_Right btn_no_right;
    [SerializeField] QuestionResertManager resertManager;
    [SerializeField] GameObject questions_right;
    [SerializeField] RawImage first_Img, second_Img, btn_on;
    [SerializeField] Texture emptyTexture, blackTexture, btnOnTexture, btnOffTexture;
    [SerializeField] bool isclick, isComplete;
    [SerializeField] AudioSource btnClick;

    [SerializeField] Questions_Txt questions_Txt;
    [SerializeField] NetworkManager networkManager;
    [SerializeField] Text firstDangerText, secondDangerText;
    [SerializeField] RawImage wanttoClick;
    [SerializeField] GameObject questionsNote;

    private void OnEnable()
    {
        first_Img.texture = emptyTexture;
        second_Img.texture = emptyTexture;
        isclick = false;
        isComplete = false;
        StartCoroutine(DelayBtn());
        GotoTitle.Instance.StartTimerWithMessage(wanttoClick, firstDangerText, secondDangerText);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (ButtonManager.instance != null)
            ButtonManager.instance.OnBlackButton -= OnOffClick_Left;
        if (GotoTitle.Instance != null)
            GotoTitle.Instance.StopTimerWithMessage(wanttoClick, firstDangerText, secondDangerText);
    }

    IEnumerator DelayBtn()
    {
        yield return CoroutineData.GetWaitForSeconds(1f);
        ButtonManager.instance.OnBlackButton += OnOffClick_Left;
    }

    void OnOffClick_Left()
    {
        if (isComplete) return;

        btn_no_right.OnYesClicked();
        GotoTitle.Instance.ResetTimerWithMessage(wanttoClick, firstDangerText, secondDangerText);
        StartCoroutine(BtnRoutine());
        if (!isclick)
        {
            first_Img.texture = blackTexture;
            btnClick.Play();
            isclick = true;
        }
        else
        {
            second_Img.texture = blackTexture;
            btnClick.Play();
            isComplete = true;
            btn_no_right.SetComplete();
        }
    }

    
    IEnumerator BtnRoutine()
    {
        btn_on.texture = btnOffTexture;
        yield return CoroutineData.GetWaitForSeconds(0.5f);
        btn_on.texture = btnOnTexture;
    }
    public void OnNoClicked()
    {
        if (isclick)
        {
            first_Img.texture = emptyTexture;
            isclick = false;
        }
    }
    public void SetComplete()
    {
        isComplete = true;
        resertManager.OnRightWaitting();
        int questionId = questions_Txt.selectedId + 1;
        StartCoroutine(networkManager.UpdateQuestion(questionId, "left", "1"));
        questions_right.SetActive(false);
    }
}
