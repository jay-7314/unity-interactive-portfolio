using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class UljinData
{
    public List<CommonData> Common;
    public List<TitleData> Title;
    public List<InteractionData> Interaction;
    public List<VideoPageData> VideoPage;
    public List<InducePageData> InducePage;
    public List<PicturePageData> PicturePage;
}

[System.Serializable]
public class CommonData
{
    public float returnTimer;
    public List<ImageData> TItleSentance;
    public List<ImageData> HomeBtn;
    public string IPAddress;
    public string Port1;
    public string Port2_Light;
    public int BaudRate;
    public List<ImageData> BackgroundImg;
}
//타이틀
[System.Serializable]
public class TitleData
{
    public List<ImageData> TitleBackgroundImg;
    public List<ImageData> titleImage;
}

//인터랙션
[System.Serializable]
public class InteractionData
{
    public List<ImageData> FirstSentance;
    public List<VideoData> SecondVideo;
    public List<ImageData> LastSentance;
    public List<VideoData> LastImage;
    public List<ImageData> FirstImage;
    public List<ImageData> SecondImage;
    public List<ImageData> ThirdImage;
}

//비디오 페이지
[System.Serializable]
public class VideoPageData
{
    public List<ImageData> SkipBtn;
    public List<VideoData> Video;
}

//시선유도 페이지
[System.Serializable]
public class InducePageData
{
    public List<ImageData> InducePageImg;
    public List<ImageData> ReturnToVideoPage;
    public List<ImageData> GotoImagePage;
}

//사진 페이지
[System.Serializable]
public class PicturePageData
{
    public List<ImageData> ReturnToVideo;
    public List<ImageData> WorldImg;
    public List<ImageData> Selects;
    public List<ImageData> Select_Explain;
    public List<ImageData> Select_Cancel;
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
    public static UljinData uljin;

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Su.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            uljin = JsonUtility.FromJson<UljinData>(json);
        }
    }
}
