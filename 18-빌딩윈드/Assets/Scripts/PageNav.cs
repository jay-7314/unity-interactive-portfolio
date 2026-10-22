using System.Collections;
using UnityEngine;

public class PageNav : MonoBehaviour
{
    [SerializeField] GameObject prevPage;
    [SerializeField] GameObject nextPage;

    public void Prev() { Go(prevPage); }
    public void Next() { Go(nextPage); }

    private void OnEnable()
    {
        StartCoroutine(returnTimer());
    }

    void Go(GameObject target)
    {
        if (target == null) return;          // 비어 있으면 아무것도 안 함 (현재 페이지도 안 꺼짐)
        gameObject.SetActive(false);
        target.SetActive(true);
    }

    IEnumerator returnTimer()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.buildwind.resetTimer);
        Logger.Instance.SendLog("move_idle_timeout");
        CommonFunc.instance.GotoTitleTimer(this.gameObject);
    }
}