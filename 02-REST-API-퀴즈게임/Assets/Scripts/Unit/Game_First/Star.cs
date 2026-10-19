using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Star : MonoBehaviour
{
    [SerializeField] RawImage starImg;

    public void SetImage(Texture texture)
    {
        starImg.texture = texture;
    }
}
