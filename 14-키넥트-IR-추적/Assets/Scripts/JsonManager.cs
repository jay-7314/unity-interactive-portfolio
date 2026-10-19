using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PositionList
{
    public List<StickerPos> pos;
    public int checkPosTimer;
}

[System.Serializable]
public class StickerPos
{
    public string stickerName;
    public float stickerRadius;
    public string video;
    public Vector2 stickerposition;
}

public class JsonManager : MonoBehaviour
{
    public static PositionList PosList;

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Position.json").Replace("\\", "/");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PosList = JsonUtility.FromJson<PositionList>(json);
        }
    }
}
