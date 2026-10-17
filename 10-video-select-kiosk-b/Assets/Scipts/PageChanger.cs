using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PageChanger : MonoBehaviour
{
    [SerializeField] GameObject[] pages;
    [SerializeField] Text keypadPagesText;
    int currentIndex;
    Coroutine _backCoroutine;

    public void GotoNext()
    {
        if (keypadPagesText != null && keypadPagesText.gameObject.activeInHierarchy && keypadPagesText.text == "")
            return;
        pages[currentIndex].SetActive(false);
        currentIndex++;
        pages[currentIndex].SetActive(true);

        if (currentIndex != 0)
        {
            if (_backCoroutine != null) StopCoroutine(_backCoroutine);
            _backCoroutine = StartCoroutine(BackToTitle());
        }
    }

    public void GotoPrev()
    {
        pages[currentIndex].SetActive(false);
        currentIndex--;
        pages[currentIndex].SetActive(true);
        if (currentIndex != 0)
        {
            if (_backCoroutine != null) StopCoroutine(_backCoroutine);
            _backCoroutine = StartCoroutine(BackToTitle());
        }
    }

    IEnumerator BackToTitle()
    {
        int startIndex = currentIndex;

        if (startIndex == 10)
            yield return CoroutineData.GetWaitForSeconds(50f);
        else
            yield return CoroutineData.GetWaitForSeconds(JsonManager.busan.GotoTitleTimer);

        CommonFunc.instance.ReturnToTitle();
        _backCoroutine = null;
        currentIndex = 0;
    }
}
