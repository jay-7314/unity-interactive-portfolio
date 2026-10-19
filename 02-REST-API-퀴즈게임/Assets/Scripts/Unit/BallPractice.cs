using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallPractice : MonoBehaviour
{
    [SerializeField] Text ballpractice_Text;
    [SerializeField] Animator ani_Yellow;
    [SerializeField] Image green, red;
    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameObject practicePage_first;

    private void OnEnable()
    {
        StartCoroutine(WaitforFrame());
    }

    IEnumerator WaitforFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);
        LoadManager.Instance.LoadText(JsonManager.popup.BallPractice[0].BallText[0], ballpractice_Text, networkManager.lang);
        green.gameObject.SetActive(false);
        ani_Yellow.Play("YellowAni");
        StartCoroutine(ActiveGreenBall());
    }

    IEnumerator ActiveGreenBall()
    {
        yield return CoroutineData.GetWaitForSeconds(0.1f);
        green.gameObject.SetActive(true);
        StartCoroutine(RemoveRedBall());
    }

    IEnumerator RemoveRedBall()
    {
        yield return CoroutineData.GetWaitForSeconds(3f);
        practicePage_first.SetActive(true);
        gameObject.SetActive(false);
    }
}
