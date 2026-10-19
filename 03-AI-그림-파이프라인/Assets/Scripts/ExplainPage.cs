using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ExplainPage : MonoBehaviour
{
    [SerializeField] Image[] explainImgs;
    [SerializeField] GameObject explainPage, drawingPage, nextBtn;
    [SerializeField] private float imageFadeDuration = 0.5f; // 각 설명 이미지 페이드인 지속시간

    private void OnEnable()
    {
        StartCoroutine(ExplainShowNBtn());
        nextBtn.SetActive(false);
        StartCoroutine(returntoTitle());
    }

    private void OnDisable()
    {
        for (int i = 0; i < explainImgs.Length; i++)
        {
            explainImgs[i].gameObject.SetActive(false);
        }
        nextBtn.SetActive(false);
    }

    IEnumerator ExplainShowNBtn()
    {
        for (int i = 0; i < explainImgs.Length; i++)
        {
            yield return CoroutineData.GetWaitForSeconds(1f);
            yield return StartCoroutine(FadeInImage(explainImgs[i], imageFadeDuration));
            yield return CoroutineData.GetWaitForSeconds(1f);
        }
        nextBtn.SetActive(true);
    }

    private IEnumerator FadeInImage(Image img, float duration)
    {
        CanvasGroup cg = img.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = img.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        img.gameObject.SetActive(true);

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(time / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    public void GotoHome()
    {
        CommonFunc.instance.ReturnBtnFunc(this.gameObject);
        Logger.Instance.SendLog("move_idle");
    }

    public void NextPage()
    {
        CommonFunc.instance.ButtonFunc(explainPage, drawingPage);
    }

    IEnumerator returntoTitle()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.makeCar.returnTime);
        LogSender.Instance.SendLog("move_idle_timeout");
        GotoHome();
    }
}