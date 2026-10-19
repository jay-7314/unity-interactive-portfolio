using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class BusanData
{
    public float GotoTitleTimer;
}

public class JsonManager : MonoBehaviour
{
    public static BusanData busan;

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Busan.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            busan = JsonUtility.FromJson<BusanData>(json);
        }
    }
}
