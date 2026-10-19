using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RightCircle : MonoBehaviour
{
    [SerializeField] Animator anim_Circle;

    
    public void StopCircle()
    {
        anim_Circle.speed = 0;
        anim_Circle.SetTrigger("Popup");
    }
}