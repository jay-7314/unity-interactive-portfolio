using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonFunc : MonoBehaviour
{
    [Header("프로그램 종료 버튼")]
    [SerializeField] private int requiredClicks = 10;   // 필요한 클릭 수
    [SerializeField] private float timeLimit = 3f;      // 제한 시간(초)

    private float firstClickTime = 0f;
    private int clickCount = 0;
 

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