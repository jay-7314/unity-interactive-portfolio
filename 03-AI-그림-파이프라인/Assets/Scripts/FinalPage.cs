using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalPage : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(ReturnToTitleCor());   
    }

    IEnumerator ReturnToTitleCor()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.makeCar.returnTime);
        Logger.Instance.SendLog("move_idle_timeout");
        CommonFunc.instance.ReturnBtnFunc(this.gameObject);
    }

    public void ReturnToTitleBtn()
    {
        CommonFunc.instance.ReturnBtnFunc(this.gameObject);
        Logger.Instance.SendLog("move_idle");
    }
}
