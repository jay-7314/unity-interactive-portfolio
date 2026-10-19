using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveStatus : MonoBehaviour
{
    public static SaveStatus Instance;
    public int imgIndex = -1;

    private void Awake()
    {
        Instance = this;
    }

}
