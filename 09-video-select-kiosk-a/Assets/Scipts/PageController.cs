using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageController : MonoBehaviour
{
    [SerializeField] GameObject[] pages;
    public int currentIndex;
    Coroutine backcor;
    
    public void Nextpage()
    {
        if (currentIndex == 1 && SaveStatus.Instance.imgIndex == -1) return;
        pages[currentIndex].SetActive(false);
        currentIndex++;
        pages[currentIndex].SetActive(true);
        Debug.Log($"Nextpage → currentIndex: {currentIndex}");
        if (currentIndex != 0)
        {
            if (backcor != null) StopCoroutine(backcor);
            if (currentIndex != 5)
                backcor = StartCoroutine(GotoTItleCor());
        }
    }

    public void Prevpage()
    {
        pages[currentIndex].SetActive(false);
        currentIndex--;
        pages[currentIndex].SetActive(true);
        Debug.Log($"Prevpage → currentIndex: {currentIndex}");
        if (currentIndex != 0)
        {
            if (backcor != null) StopCoroutine(backcor);
            backcor = StartCoroutine(GotoTItleCor());
        }

    }

    IEnumerator GotoTItleCor()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.insight.ReturnToTitle);
        Debug.Log($"Prevpage → currentIndex: {currentIndex}");
        pages[currentIndex].SetActive(false);
        currentIndex = 0;
        pages[0].SetActive(true);
    }

    public void ReturnToTitle()
    {
        Debug.Log($"Prevpage → currentIndex: {currentIndex}");
        if (backcor != null) StopCoroutine(backcor);
        pages[currentIndex].SetActive(false);
        currentIndex = 0;
        pages[0].SetActive(true);
    }
}
