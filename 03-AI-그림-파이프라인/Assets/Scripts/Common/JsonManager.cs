using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class MakeCar
{
    public List<ImageData> TitleImg;
    public List<ImageData> ExplainImgs;
    public float FinalCount;
    public string IPAddress;
    public int returnTime;
    public int returnTimeforDrawing;
    public int inx;
    public string uid;
}

#region 기본 요소
[System.Serializable]
public class TextData
{
    public string TextSentance;
    public int TextSize;
    public Vector2 textPos;
}

[System.Serializable]
public class ImageData
{
    public int id;
    public string path;
    public Vector2 imgSize;
    public Vector2 imgPos;
    public Vector2 CencelPositions;
}

[System.Serializable]
public class VideoData
{
    public int id;
    public string VideoPath;
    public Vector2 VideoSize;
    public Vector2 VideoPos;
}
#endregion

public class JsonManager : MonoBehaviour
{
    public static MakeCar makeCar;

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "MakeCar.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            makeCar = JsonUtility.FromJson<MakeCar>(json);
        }
    }
}
