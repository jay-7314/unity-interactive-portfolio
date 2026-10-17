using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecondCountDown : MonoBehaviour
{
    [SerializeField] Text countdown;
    [SerializeField] GameObject playGame;
    [SerializeField] AudioSource countdownAudio, startGameAudio;
    int count = 3;

    private void OnEnable()
    {
        count = 3;
        StartCoroutine(CountDownFunc());
    }

    IEnumerator CountDownFunc()
    {
        while (count > 0)
        {
            countdown.text = count.ToString();
            countdownAudio.Play();
            yield return CoroutineData.GetWaitForSeconds(1f);
            count--;
        }

        countdown.text = "시작!";
        countdown.fontSize = 113;
        countdownAudio.Stop();
        startGameAudio.Play();
        yield return CoroutineData.GetWaitForSeconds(1f);
        countdown.gameObject.SetActive(false);
        playGame.SetActive(true);
        gameObject.SetActive(false);
    }
}
