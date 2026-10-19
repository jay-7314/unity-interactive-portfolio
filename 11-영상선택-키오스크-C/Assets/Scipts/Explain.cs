using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Video;

public class Explain : MonoBehaviour
{
    [SerializeField] GameObject explainPage, selectPage, title;
    [SerializeField] VideoPlayer video;

    private void OnEnable()
    {
        StartCoroutine(ReturnToTitleCor());
        LoadManager.Instance.LoadVideo(JsonManager.busan.VideoDatas[0], video);
    }

    IEnumerator ReturnToTitleCor()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.busan.GotoTitleTimer);
        CommonFunc.instance.ReturnToTitle();

    }


    public void BtnToNext()
    {
        explainPage.SetActive(false);
        selectPage.SetActive(true);
    }

    public void BtnPrevious()
    {
        explainPage.SetActive(false);
        title.SetActive(true);
    }

}
