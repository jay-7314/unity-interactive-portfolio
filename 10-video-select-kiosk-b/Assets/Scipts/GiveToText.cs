using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GiveToText : MonoBehaviour
{
    [SerializeField] Text sendText, gaveText;

    private void OnEnable()
    {
        gaveText.text = sendText.text;
    }
}
