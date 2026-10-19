using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommonFunc : MonoBehaviour
{
    public static CommonFunc instance;

    private void Awake()
    {
        instance = this;
    }
    public void ReturnToTitle()
    {
        SceneManager.LoadScene(0);
    }
}
