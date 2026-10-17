using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class modal : MonoBehaviour
{
    [SerializeField] GameObject modalpage, video1page, video1AnswerPage;
    [SerializeField] PageController pageController;

    public void Onclicked()
    {
        modalpage.SetActive(false);
    }

    private void OnEnable()
    {
        StartCoroutine(GotoTItleCor());
    }

    IEnumerator GotoTItleCor()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.insight.ReturnToTitle);
        modalpage.SetActive(false);
        pageController.ReturnToTitle();
    }
}
