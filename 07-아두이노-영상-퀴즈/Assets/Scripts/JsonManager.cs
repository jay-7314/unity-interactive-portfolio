using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonManager : MonoBehaviour
{
    [System.Serializable]
    public class Uljin_Frozen
    {
        public string Port1_Frist;
        public string Port2_Second;
        public float IdleLimit;
    }

    public static Uljin_Frozen Frozen;

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Uljin_Frozen.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            Frozen = JsonUtility.FromJson<Uljin_Frozen>(json);
        }
    }
}
