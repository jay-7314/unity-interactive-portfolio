using System;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager instance;
    public event Action OnRedButton, OnBlueButton, OnYellowButton, OnGreenButton, OnPuppleButton, OnBlackButton, OnOrangeButton, OnPinkButton, OnWhiteButton, OnWhiteBlueButton;
    private const float threshold = 0.04f;
    private float[] holdTimes = new float[10];
    private void Awake()
    {
        instance = this;
    }


    void Update()
    {
        CheckButton(0, ref holdTimes[0], OnRedButton);
        CheckButton(1, ref holdTimes[1], OnBlueButton);
        CheckButton(2, ref holdTimes[2], OnYellowButton);
        CheckButton(3, ref holdTimes[3], OnGreenButton);
        CheckButton(4, ref holdTimes[4], OnPuppleButton);
        CheckButton(5, ref holdTimes[5], OnBlackButton);
        CheckButton(6, ref holdTimes[6], OnOrangeButton);
        CheckButton(7, ref holdTimes[7], OnPinkButton);
        CheckButton(8, ref holdTimes[8], OnWhiteButton);
        CheckButton(9, ref holdTimes[9], OnWhiteBlueButton);

        // 키보드 테스트용 (나중에 삭제)
        if (Input.GetKeyDown(KeyCode.Q)) OnRedButton?.Invoke();
        else if (Input.GetKeyDown(KeyCode.W)) OnBlueButton?.Invoke();
        else if (Input.GetKeyDown(KeyCode.E)) OnYellowButton?.Invoke();
        else if (Input.GetKeyDown(KeyCode.R)) OnGreenButton?.Invoke();
        else if (Input.GetKeyDown(KeyCode.T)) OnPuppleButton?.Invoke();
        else if (Input.GetKeyDown(KeyCode.Y)) OnBlackButton?.Invoke();
        else if (Input.GetKeyDown(KeyCode.U)) OnOrangeButton?.Invoke();
        else if (Input.GetKeyDown(KeyCode.I)) OnPinkButton?.Invoke();
        else if (Input.GetKeyDown(KeyCode.O)) OnWhiteButton?.Invoke();
        else if (Input.GetKeyDown(KeyCode.P)) OnWhiteBlueButton?.Invoke();
    }

    void CheckButton(int index, ref float holdTime, Action buttonEvent)
    {
        string buttonName = "button" + index;

        if (Input.GetButton(buttonName))
        {
            holdTime += Time.deltaTime;

            if (holdTime >= threshold && holdTime - Time.deltaTime < threshold)
            {
                // threshold를 막 넘는 순간 딱 한번만 발동
                buttonEvent?.Invoke();
            }
        }
        else
        {
            holdTime = 0f;
        }
    }
}
