using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SecondText : MonoBehaviour
{
    [SerializeField] Text text1, text2, text3;
    [SerializeField] GameObject SecondGame;
    [SerializeField] NetworkManager networkManager;

    private void OnEnable()
    {
        StartCoroutine(WaitforFrame());
    }

    IEnumerator WaitforFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);

        LoadManager.Instance.LoadText(JsonManager.popup.Second_Texts[0].Second_Text1[0], text1, networkManager.lang);
        LoadManager.Instance.LoadText(JsonManager.popup.Second_Texts[0].Second_Text2[0], text2, networkManager.lang);
        LoadManager.Instance.LoadText(JsonManager.popup.Second_Texts[0].Second_Text3[0], text3, networkManager.lang);

        text1.gameObject.SetActive(true);
        text2.gameObject.SetActive(false);
        text3.gameObject.SetActive(false);

        yield return CoroutineData.GetWaitForSeconds(2f);
        text1.gameObject.SetActive(false);
        text2.gameObject.SetActive(true);
        yield return CoroutineData.GetWaitForSeconds(3f);
        text2.gameObject.SetActive(false);
        text3.gameObject.SetActive(true);
        yield return CoroutineData.GetWaitForSeconds(3f);
        text3.gameObject.SetActive(false);
        SecondGame.SetActive(true);
    }
}