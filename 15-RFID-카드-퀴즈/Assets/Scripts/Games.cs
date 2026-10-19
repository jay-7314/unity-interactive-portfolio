using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Games : MonoBehaviour
{
    public GameObject[] games;

    private void Start()
    {
        ActivateRandomGame();
    }

    // 게임 시작 시 뿐 아니라, 타임아웃으로 인한 리셋 시에도 재사용할 수 있도록 public 메서드로 분리
    public void ActivateRandomGame()
    {
        // 전부 끄기
        for (int i = 0; i < games.Length; i++)
        {
            games[i].SetActive(false);
        }

        // 그중 하나만 무작위로 켜기
        int index = Random.Range(0, games.Length);
        games[index].SetActive(true);
    }
}