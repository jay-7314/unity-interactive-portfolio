using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Question_Btn_Yes_Left : MonoBehaviour
{
    [SerializeField] Question_Btn_No_Left btn_no_left;
    [SerializeField] QuestionResertManager resertManager;
    [SerializeField] GameObject questions_left;
    [SerializeField] RawImage first_Img, second_Img, btn_on;
    [SerializeField] Texture emptyTexture, redTexture, btnOnTexture, btnOffTexture;
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
        StartCoroutine(Delaybtn());
        GotoTitle.Instance.StartTimerWithMessage(wanttoClick,firstDangerText, secondDangerText);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (ButtonManager.instance != null)
            ButtonManager.instance.OnRedButton -= OnOffClick_Left;
        if (GotoTitle.Instance != null)
            GotoTitle.Instance.StopTimerWithMessage(wanttoClick, firstDangerText, secondDangerText);
    }
    IEnumerator Delaybtn()
    {
        yield return CoroutineData.GetWaitForSeconds(1f);
        ButtonManager.instance.OnRedButton += OnOffClick_Left;
    }
    void OnOffClick_Left()
    {
        if (isComplete) return;
        btn_no_left.OnYesClicked();
        GotoTitle.Instance.ResetTimerWithMessage(wanttoClick, firstDangerText, secondDangerText);
        StartCoroutine(BtnRoutine());

        if (!isclick)
        {
            first_Img.texture = redTexture;
            btnClick.Play();
            isclick = true;
        }
        else
        {
            second_Img.texture = redTexture;
            isComplete = true;
            btnClick.Play();
            btn_no_left.SetComplete();
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
        int questionId = questions_Txt.selectedId + 1;
        StartCoroutine(networkManager.UpdateQuestion(questionId, "left", "1"));
        resertManager.OnLeftWaitting();
        questions_left.SetActive(false);
    }
}
