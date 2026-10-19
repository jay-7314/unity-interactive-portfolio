using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MindPiece_First : MonoBehaviour
{
    [SerializeField] Image mindImg;
    [SerializeField] Text mindTxt;
    [SerializeField] GameObject personcheck;
    [SerializeField] NetworkManager networkManager;
    void OnEnable()
    {
        StartCoroutine(WaitForFrame());
    }

    IEnumerator WaitForFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);
        var json = JsonManager.popup.Explain[0];
        LoadManager.Instance.LoadText(JsonManager.popup.Explain[0].MindPiece_Txt[0], mindTxt, networkManager.lang);
        yield return CoroutineData.GetWaitForSeconds(7f);
        personcheck.SetActive(true);
        gameObject.SetActive(false);

    }
}
