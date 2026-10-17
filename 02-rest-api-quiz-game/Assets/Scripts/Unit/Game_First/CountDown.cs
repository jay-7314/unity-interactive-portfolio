using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour
{
    [SerializeField] Text countdown;
    [SerializeField] GameObject leftSpawn;
    [SerializeField] GameObject rightSpawn;
    [SerializeField] AudioSource countDownSound, gameStartSound;
    int count = 3;

    private void OnEnable()
    {
        leftSpawn.SetActive(false);
        rightSpawn.SetActive(false);
        count = 3;
        countdown.gameObject.SetActive(true); 
        countdown.fontSize = 192;
        StartCoroutine(CountDownFunc());

    }

    IEnumerator CountDownFunc()
    {
        while (count > 0)
        {
            countdown.text = count.ToString();
            countDownSound.Play();
            yield return CoroutineData.GetWaitForSeconds(1f);
            count--;
        }

        countdown.text = "시작!";
        countdown.fontSize = 113;
        countDownSound.Stop();
        gameStartSound.Play();
        yield return CoroutineData.GetWaitForSeconds(1f);
        countdown.gameObject.SetActive(false);
        leftSpawn.SetActive(true);
        rightSpawn.SetActive(true);
        gameObject.SetActive(false);
    }
}
