using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisPlaySetting : MonoBehaviour
{
    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.targetDisplay = 1;
        Debug.Log(" Canvas의 TargetDisplay를 Display 2로 설정 완료됨");
    }
}
