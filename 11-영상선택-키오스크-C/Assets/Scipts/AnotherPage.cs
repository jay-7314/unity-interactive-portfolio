using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnotherPage : MonoBehaviour
{
    [SerializeField] GameObject anotherPeoplePage, selectPage, lastPage;
    [SerializeField] Text[] precentText;
    string[] buttonSave = { "btn1", "btn2", "btn3", "btn4", "btn5" };

    private void OnEnable()
    {
        StartCoroutine(ReturnToTitleCor());
        PercentTextUI();
    }
    public void PercentTextUI()
    {
        int total = 0;
        int[] count = new int[buttonSave.Length];
        for (int i = 0; i < buttonSave.Length; i++)
        {
            count[i] = PlayerPrefs.GetInt(buttonSave[i], 0);
            total += count[i];
        }

        float[] targetPercents = new float[precentText.Length];
        for (int i = 0; i < precentText.Length; i++)
        {
            targetPercents[i] = total > 0 ? (count[i] / (float)total) * 100 : 0f;
        }

        for (int i = 0; i < precentText.Length; i++)
        {
            StartCoroutine(AnimatePercent(precentText[i], targetPercents[i]));
        }
    }

    IEnumerator AnimatePercent(Text text, float target)
    {
        float duration = 1.5f; // 애니메이션 총 시간
        float elapsed = 0f;
        float current = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            current = Mathf.Lerp(0f, target, elapsed / duration);
            text.text = $"{current:F0}%";
            yield return null;
        }

        text.text = $"{target:F0}%"; // 마지막은 정확한 값으로 고정
    }

    IEnumerator ReturnToTitleCor()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.busan.GotoTitleTimer);
        CommonFunc.instance.ReturnToTitle();
    }

    public void BtnToNext()
    {
        anotherPeoplePage.SetActive(false);
        lastPage.SetActive(true);
    }

    public void BtnPrevious()
    {
        anotherPeoplePage.SetActive(false);
        selectPage.SetActive(true);
    }
}
