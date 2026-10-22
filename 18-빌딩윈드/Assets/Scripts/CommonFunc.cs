using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonFunc : MonoBehaviour
{
    public static CommonFunc instance;
    [SerializeField] GameObject title;
    [SerializeField] private int requiredClicks = 10;   // 필요한 클릭 수
    [SerializeField] private float timeLimit = 3f;      // 제한 시간(초)

    private float firstClickTime = 0f;
    private int clickCount = 0;

    private void Awake()
    {
        instance = this;
    }

    public void GotoTitleTimer(GameObject currentPage)
    {
        currentPage.SetActive(false);
        title.SetActive(true);
    }

    public void GotoTitle(GameObject currentPage)
    {
        currentPage.SetActive(false);
        title.SetActive(true);
        Logger.Instance.SendLog("move_idle");
    }

    public void ShutdownBtn()
    {
        // 첫 클릭이거나, 첫 클릭 후 제한 시간이 지났으면 새로 시작
        if (clickCount == 0 || Time.unscaledTime - firstClickTime > timeLimit)
        {
            clickCount = 0;
            firstClickTime = Time.unscaledTime;
        }

        clickCount++;

        if (clickCount >= requiredClicks)
        {
            clickCount = 0;
            Logger.Instance.SendLog("end_kill");
            Application.Quit();
        }
    }
}
