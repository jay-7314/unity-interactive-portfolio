using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    [Header("프로그램 종료 버튼")]
    float shutTimer = 0f;                      //0으로 초기값을 넣어진행
    int clickCount = 0;

    private void Awake()
    {
        instance = this;
    }

    public void ReturnTItle()
    {
        SceneManager.LoadScene(0);
    }

    public void ShutdownOnClick()
    {
        shutTimer += Time.deltaTime;
        if (shutTimer >= 3)
        {
            clickCount = 0;
            shutTimer = 0f;
        }
        else
        {
            clickCount++;
            if (clickCount >= 10)
            {
                Application.Quit();
            }
        }


    }
}
