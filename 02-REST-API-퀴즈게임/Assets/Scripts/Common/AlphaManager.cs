using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphaManager : MonoBehaviour
{
    public static AlphaManager instance;

    private void Awake()
    {
        instance = this;
    }
    public void SetAlpha(Graphic graphic, float a)
    {
        Color c;
        c = graphic.color;
        c.a = a;
        graphic.color = c;
    }
}
