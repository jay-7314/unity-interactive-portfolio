using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[System.Serializable]
public class Math
{
    public List<CommonData> Commons;
}

[System.Serializable]
public class CommonData
{
    public string PortName;
    public float minRange;
    public float maxRange;
}

public class JsonManager : MonoBehaviour
{
    public static Math math;

    private void Awake()
    {
        string file = Path.Combine(Application.streamingAssetsPath, "Math.json");
        if (File.Exists(file))
        {
            string json = File.ReadAllText(file, Encoding.UTF8);
            math = JsonUtility.FromJson<Math>(json);
        }
    }
}
