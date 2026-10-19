using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fanfare : MonoBehaviour
{
    [SerializeField] GameObject[] fanfareIndex;

    public void FanfareFunc()
    {
        StartCoroutine(FanfareCor());
    }

    IEnumerator FanfareCor()
    {
        for(int i = 0; i<fanfareIndex.Length; i++)
        {
            yield return CoroutineData.GetWaitForSeconds(0.05f);
            fanfareIndex[i].SetActive(true);
        }
        yield return CoroutineData.GetWaitForSeconds(0.3f); 

        for (int i = 0; i < fanfareIndex.Length; i++)
        {
            fanfareIndex[i].SetActive(false); 
        }

    }
}
