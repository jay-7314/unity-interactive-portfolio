using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Question_Btn_No_Right : MonoBehaviour
{
    [SerializeField] Question_Btn_Yes_Right btn_yes_right;
    [SerializeField] QuestionResertManager resertManager;
    [SerializeField] GameObject questions_right;
    [SerializeField] RawImage first_Img, second_Img, btn_on;
    [SerializeField] Texture emptyTexture, whiteBlueTexture, btnOnTexture, btnOffTexture;
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
            ButtonManager.instance.OnWhiteBlueButton -= OnOffClick_Right;
        if (GotoTitle.Instance != null)
            GotoTitle.Instance.StopTimerWithMessage(wanttoClick, firstDangerText, secondDangerText);
    }
    IEnumerator DelayBtn()
    {
        yield return CoroutineData.GetWaitForSeconds(1f);
        ButtonManager.instance.OnWhiteBlueButton += OnOffClick_Right;
    }

    void OnOffClick_Right()
    {
        if (isComplete) return;

        btn_yes_right.OnNoClicked();
        GotoTitle.Instance.ResetTimerWithMessage(wanttoClick, firstDangerText, secondDangerText);
        StartCoroutine(BtnRoutine());
        if (!isclick)
        {
            first_Img.texture = whiteBlueTexture;
            btnClick.Play();
            isclick = true;
        }
        else
        {
            second_Img.texture = whiteBlueTexture;
            isComplete = true;
            btnClick.Play();
            btn_yes_right.SetComplete();
            
        }
    }

 
    IEnumerator BtnRoutine()
    {
        btn_on.texture = btnOffTexture;
        yield return CoroutineData.GetWaitForSeconds(0.5f);
        btn_on.texture = btnOnTexture;
    }
    public void OnYesClicked()
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
        StartCoroutine(networkManager.UpdateQuestion(questionId, "right", "0"));
        questions_right.SetActive(false);
    }
}
