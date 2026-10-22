using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ForthPage : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(returnTimer());
    }

    IEnumerator returnTimer()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.buildwind.resetTimer);
        Logger.Instance.SendLog("move_idle_timeout");
        CommonFunc.instance.GotoTitleTimer(this.gameObject);
    }
}
