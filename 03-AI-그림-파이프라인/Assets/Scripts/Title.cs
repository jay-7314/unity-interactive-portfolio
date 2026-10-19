using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] GameObject title, explainPage;

    public void ClicktoNext()
    {
        CommonFunc.instance.ButtonFunc(title, explainPage);
    }
}
