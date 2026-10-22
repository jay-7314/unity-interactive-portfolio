using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    [SerializeField] GameObject title, secondPage;

    public void StartButton()
    {
        title.SetActive(false);
        secondPage.SetActive(true);
    }
}
