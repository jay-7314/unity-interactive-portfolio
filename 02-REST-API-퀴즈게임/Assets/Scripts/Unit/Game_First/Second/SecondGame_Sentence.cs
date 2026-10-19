using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecondGame_Sentence : MonoBehaviour
{
    [SerializeField] GameObject second_TextBG, circle_Practice;
    [SerializeField] Text second_Text;

    private void OnEnable()
    {
        StartCoroutine(WaitForCor());
        second_TextBG.SetActive(false);
        second_Text.gameObject.SetActive(false);
    }

    IEnumerator WaitForCor()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);
        second_TextBG.SetActive(true);
        second_Text.gameObject.SetActive(true);
        yield return CoroutineData.GetWaitForSeconds(1f);
        circle_Practice.SetActive(true);
        second_TextBG.SetActive(false);
        second_Text.gameObject.SetActive(false);
    }
}
