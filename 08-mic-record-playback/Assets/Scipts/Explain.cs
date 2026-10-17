using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explain : MonoBehaviour
{
    [SerializeField] GameObject explain, selectVideoPage,title;
    [SerializeField] SelectVideo selectVideo; // 추가

    private void OnEnable()
    {
        StartCoroutine(ReturntoTitle());
        //selectVideo.PreloadAllVideos(); // 추가 - explain 켜지자마자 미리 Preload 시작
    }

    IEnumerator ReturntoTitle()
    {
        ArduinoManager.Instance.SendMessage(JsonManager.poly.OffLamp);
        yield return CoroutineData.GetWaitForSeconds(JsonManager.poly.ReturnToTItle);
        CommonFunc.instance.ReturnToTitle();
    }

    public void NextBtn()
    {
        explain.SetActive(false);
        selectVideoPage.SetActive(true);
    }

    public void PrevBtn()
    {
        explain.SetActive(false);
        title.SetActive(true);
    }
}