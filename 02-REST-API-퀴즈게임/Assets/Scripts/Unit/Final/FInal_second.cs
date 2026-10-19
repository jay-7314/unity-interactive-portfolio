using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FInal_second : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] Text isNexttext, finishGame;

    private void OnEnable()
    {
        isNexttext.gameObject.SetActive(false);
        finishGame.gameObject.SetActive(false);

        StartCoroutine(networkManager.UpdateEnd());

        int finishCount = networkManager.GetFinishCount();
        if (finishCount >= 3)
            finishGame.gameObject.SetActive(true);
        else
            isNexttext.gameObject.SetActive(true);

        StartCoroutine(returnToTitle());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator returnToTitle()
    {
        yield return CoroutineData.GetWaitForSeconds(3f);
        StartCoroutine(networkManager.UpdateEnd());
        //StartCoroutine(networkManager.UpdateRestart());
        StartCoroutine(networkManager.ClearRoomInsertPerson());
        SceneManager.LoadScene(0);
    }
}
