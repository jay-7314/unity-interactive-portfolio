using UnityEngine;

public class FailPage : MonoBehaviour
{
    [SerializeField] private GameObject paintingPage;

    // "다시 그리기" 버튼
    public void RetryBtn()
    {
        CommonFunc.instance.ButtonFunc(this.gameObject, paintingPage);
    }

    // "처음으로" 버튼
    public void GotoTitlePageBtn()
    {
        CommonFunc.instance.ReturnBtnFunc(this.gameObject);
        LogSender.Instance.SendLog("move_idle");
    }
}