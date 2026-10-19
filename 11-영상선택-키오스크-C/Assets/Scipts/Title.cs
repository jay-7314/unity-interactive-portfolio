using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] GameObject title, explain;
    public void GotoSelectPage()
    {
        title.SetActive(false);
        explain.SetActive(true);
    }
}
