using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenBall : MonoBehaviour
{
    public void PlaySound()
    {
        GetComponent<AudioSource>().Play();
    }
}
