using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PicturePage : MonoBehaviour
{
    [SerializeField] Texture[] imgs;
    [SerializeField] RawImage displayImgs;
    [SerializeField] GameObject picturePage, finalPage;

    private void OnEnable()
    {
        int idx = SaveStatus.Instance.imgIndex;
        if(idx >=0 && idx <imgs.Length) displayImgs.texture = imgs[idx];
        StartCoroutine(picturePageCor());
    }

    IEnumerator picturePageCor()
    {
        yield return CoroutineData.GetWaitForSeconds(15.5f);
        picturePage.SetActive(false);
        finalPage.SetActive(true);
    }
}
