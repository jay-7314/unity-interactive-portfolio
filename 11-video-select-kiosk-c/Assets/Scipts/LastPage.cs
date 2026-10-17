using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastPage : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(ReturnToTitleCor());
    }

    IEnumerator ReturnToTitleCor()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.busan.GotoTitleTimer);
        CommonFunc.instance.ReturnToTitle();
    }
}
