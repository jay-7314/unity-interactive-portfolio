using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explain2 : MonoBehaviour
{
    [SerializeField] GameObject videoPage, explain1, explain2;

    private void OnEnable()
    {
        StartCoroutine(ReturnToTitleCor());
    }

    IEnumerator ReturnToTitleCor()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.poly.ReturnToTitle);
        CommonFunc.instance.ReturnToTitle();
    }

    public void NexttoPage()
    {
        explain2.SetActive(false);
        videoPage.SetActive(true);
    }

    public void Previouspage()
    {
        explain2.SetActive(false);
        explain1.SetActive(true);
    }
}
