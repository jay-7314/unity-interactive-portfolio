using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explain1 : MonoBehaviour
{
    [SerializeField] GameObject title, explain, explain2;

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
        explain.SetActive(false);
        explain2.SetActive(true);
    }

    public void Previouspage()
    {
        explain.SetActive(false);
        title.SetActive(true);
    }
}
