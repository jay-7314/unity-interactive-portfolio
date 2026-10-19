using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Deagu_Insight
{
    public float ReturnToTitle;
}

[System.Serializable]
public class VideoData
{
    public int id;
    public string VideoPath;
}

public class JsonManager : MonoBehaviour
{
    public static Deagu_Insight insight;

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Busan.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            insight = JsonUtility.FromJson<Deagu_Insight>(json);
        }
    }
}
