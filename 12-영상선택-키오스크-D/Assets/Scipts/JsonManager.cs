using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Deagu_Poly
{
    public List<VideoData> VideoList;
    public float SelectPageToTitleTimer;
    public float FinishPageToTitleTimer;
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
    public static Deagu_Poly poly;

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Poly.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            poly = JsonUtility.FromJson<Deagu_Poly>(json);
        }
    }
}
