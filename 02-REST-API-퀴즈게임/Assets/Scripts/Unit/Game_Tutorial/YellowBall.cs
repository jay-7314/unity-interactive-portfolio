using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YellowBall : MonoBehaviour
{
    public void PlaySound()
    {
        GetComponent<AudioSource>().Play();
    }
}
