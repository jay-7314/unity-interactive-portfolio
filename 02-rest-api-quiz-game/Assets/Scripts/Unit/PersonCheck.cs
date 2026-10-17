using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonCheck : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] Text leftperson_Text, rightperson_Text, personcheck_Text;
    [SerializeField] RawImage leftperson_Circle, rightperson_Circle, leftperson_Check, rightperson_Check, wanttoClick;
    [SerializeField] AudioSource checkSound;
    [SerializeField] Image leftArrow, rightArrow;
    bool isPlayer1, isPlayer2;
    [SerializeField] GameObject tutorialBg_first, tutorialBg_second;
    [SerializeField] RawImage[] rawImages;
    [SerializeField] BallPractice ballPractice;

    private void OnEnable()
    {
        isPlayer1 = false;
        isPlayer2 = false;
        leftperson_Check.gameObject.SetActive(false);
        rightperson_Check.gameObject.SetActive(false);
        leftArrow.gameObject.SetActive(false);
        rightArrow.gameObject.SetActive(false);
        wanttoClick.gameObject.SetActive(false);
        SetRawImagesVisible(false);
        StartCoroutine(WaitForFrame());
    }

    void OnDisable()
    {
        isPlayer1 = false;
        isPlayer2 = false;
        leftperson_Check.gameObject.SetActive(false);
        rightperson_Check.gameObject.SetActive(false);
        leftArrow.gameObject.SetActive(false); 
        rightArrow.gameObject.SetActive(false);
        ButtonManager.instance.OnRedButton -= CheckPlayer1;
        ButtonManager.instance.OnBlackButton -= CheckPlayer2;
        gameObject.SetActive(false);
    }


    IEnumerator WaitForFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);
        ButtonManager.instance.OnRedButton += CheckPlayer1;
        ButtonManager.instance.OnBlackButton += CheckPlayer2;

        var jsonData = JsonManager.popup.PersonCheck[0];
        LoadManager.Instance.LoadCircleImage(jsonData.PersonalCircle[0], leftperson_Circle, networkManager.leftColor);
        LoadManager.Instance.LoadCircleImage(jsonData.PersonalCircle[0], rightperson_Circle, networkManager.rightColor);
        LoadManager.Instance.LoadText(jsonData.PersonelSiteText[0], leftperson_Text, networkManager.lang);
        LoadManager.Instance.LoadText(jsonData.PersonelSiteText[0], rightperson_Text, networkManager.lang);
        LoadManager.Instance.LoadText(jsonData.PersonCheckText[0], personcheck_Text, networkManager.lang);
        leftperson_Text.text = $"{networkManager.leftName}{leftperson_Text.text}";
        rightperson_Text.text = $"{networkManager.rightName}{rightperson_Text.text}";
        SetRawImagesVisible(true);
        leftArrow.gameObject.SetActive(true);
        rightArrow.gameObject.SetActive(true);
        GotoTitle.Instance.StartTimer(wanttoClick);
    }

    void SetRawImagesVisible(bool visible)
    {
        if (rawImages == null) return;
        foreach (var img in rawImages)
        {
            if (img != null)
                img.gameObject.SetActive(visible);
        }
    }

    void CheckPlayer1()
    {
        Debug.Log("Player1가 확인을 눌렀습니다.");
        isPlayer1 = true;
        if (isPlayer1)
        {
            leftperson_Check.gameObject.SetActive(true);
        }
        checkSound.Play();
        GotoTitle.Instance.ResetTimer(wanttoClick);
        ButtonManager.instance.OnRedButton -= CheckPlayer1;
        Checkperson();
    }

    void CheckPlayer2()
    {
        Debug.Log("Player2가 확인을 눌렀습니다.");
        isPlayer2 = true;
        if (isPlayer2)
        {
            rightperson_Check.gameObject.SetActive(true);

        }
        checkSound.Play();
        GotoTitle.Instance.ResetTimer(wanttoClick);
        ButtonManager.instance.OnBlackButton -= CheckPlayer2;
        Checkperson();
    }

    void Checkperson()
    {
        if(isPlayer1 && isPlayer2)
        {
            GotoTitle.Instance.StopTimer(wanttoClick);
            StartCoroutine(PersonCheckPageEnd());
        }
    }

    IEnumerator PersonCheckPageEnd()
    {
        yield return CoroutineData.GetWaitForSeconds(1f);
        tutorialBg_first.SetActive(false);
        tutorialBg_second.SetActive(true);
        ballPractice.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
