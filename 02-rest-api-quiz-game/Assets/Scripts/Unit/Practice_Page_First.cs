using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Practice_Page_First : MonoBehaviour
{
    [SerializeField] Image practiceBtn;
    [SerializeField] Text first_Txt, second_Txt, third_Txt;
    [SerializeField] GameObject tutorialBg_First, tutorialBg_Second, GameBg, firstTutorial, gameTutorial;
    [SerializeField] RawImage redBall, redBalltext;
    [SerializeField] NetworkManager networkManager;
    

    private void OnEnable()
    {
        first_Txt.gameObject.SetActive(true);
        second_Txt.gameObject.SetActive(false);
        third_Txt.gameObject.SetActive(false);
        practiceBtn.gameObject.SetActive(false);
        redBall.gameObject.SetActive(true);
        redBalltext.gameObject.SetActive(false);
        StartCoroutine(WaitforFrame());
    }
    IEnumerator WaitforFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);

        second_Txt.gameObject.SetActive(false);
        practiceBtn.gameObject.SetActive(false);
        third_Txt.gameObject.SetActive(false);

        var jsondata = JsonManager.popup.Practice_Page_First[0];
        LoadManager.Instance.LoadText(jsondata.FirstTxt[0], first_Txt, networkManager.lang);
        LoadManager.Instance.LoadText(jsondata.SecondTxt[0], second_Txt, networkManager.lang);
        LoadManager.Instance.LoadText(jsondata.ThirdTxt[0], third_Txt, networkManager.lang);

        yield return CoroutineData.GetWaitForSeconds(0.5f);
        redBalltext.gameObject.SetActive(true);

        yield return CoroutineData.GetWaitForSeconds(4f);
        first_Txt.gameObject.SetActive(false);
        second_Txt.gameObject.SetActive(true);
        yield return CoroutineData.GetWaitForSeconds(0.5f);
        practiceBtn.gameObject.SetActive(true);

        yield return CoroutineData.GetWaitForSeconds(4f);
        second_Txt.gameObject.SetActive(false);
        redBall.gameObject.SetActive(false);
        practiceBtn.gameObject.SetActive(false);
        third_Txt.gameObject.SetActive(true);

        yield return CoroutineData.GetWaitForSeconds(3f);
        tutorialBg_First.SetActive(true);
        GameBg.SetActive(true);
        gameTutorial.SetActive(true);
        firstTutorial.SetActive(true);
    }
}
