using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownText : MonoBehaviour
{
    [SerializeField] Text countdownText;
    int count = 15;

    private void OnEnable()
    {
        count = 15;
        StartCoroutine(CountdownCor());
    }

    IEnumerator CountdownCor()
    {
        float startTime = Time.time;
        int lastCount = -1;

        while (count > 0)
        {
            // 경과 시간 기준으로 count 계산
            int elapsed = Mathf.FloorToInt(Time.time - startTime);
            count = Mathf.Max(0, 15 - elapsed);

            if (count != lastCount)
            {
                lastCount = count;
                if (count > 0)
                    countdownText.text = $"<color=#D24E59>{count}</color>초 후에 사진이 촬영합니다.";
            }

            yield return null; // 매 프레임 체크
        }
    }
}
