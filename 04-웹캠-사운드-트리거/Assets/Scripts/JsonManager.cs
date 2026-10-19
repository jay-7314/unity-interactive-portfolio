using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Doremi
{
    public List<Positions> Position;
}

[System.Serializable]
public class Positions
{
    public Vector2 Pos;
    public string Sound;
}

public class JsonManager : MonoBehaviour
{
    public static Doremi dorime;

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Doremi.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            dorime = JsonUtility.FromJson<Doremi>(json);
        }
    }
}
