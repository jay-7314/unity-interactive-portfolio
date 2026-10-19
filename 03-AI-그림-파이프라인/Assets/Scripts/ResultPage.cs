using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class ResultPage : MonoBehaviour
{
    [SerializeField] private Image resultImage;
    [SerializeField] GameObject gotoFinal, resultPage, finalPage;

    private Sprite currentSprite;
    private Texture2D currentTexture;

    private void OnEnable()
    {
        gotoFinal.SetActive(true);
        StartCoroutine(GotoFinalCor());
    }

    private void OnDisable()
    {
        gotoFinal.SetActive(false);
    }

    public void ShowResult(Sprite sprite, Texture2D sourceTexture)
    {
        // 이전 결과 정리
        if (currentSprite != null) Destroy(currentSprite);
        if (currentTexture != null) Destroy(currentTexture);

        currentSprite = sprite;
        currentTexture = sourceTexture;

        if (resultImage != null)
            resultImage.sprite = sprite;

    }

    public void GotoFinalPage()
    {
        CommonFunc.instance.ButtonFunc(resultPage, finalPage);
    }

    public void GotoTitlePageBtn()
    {
        CommonFunc.instance.ReturnBtnFunc(this.gameObject);
        LogSender.Instance.SendLog("move_idle");
    }

    IEnumerator GotoFinalCor()
    {
        yield return CoroutineData.GetWaitForSeconds(10f);
        GotoFinalPage();
    }
}