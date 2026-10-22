using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class BuildWind
{
    public int inx;
    public string uid;
    public int resetTimer;
}

public class JsonManager : MonoBehaviour
{
    public static BuildWind buildwind;

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "buildwind.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            buildwind = JsonUtility.FromJson<BuildWind>(json);
        }
    }
}
