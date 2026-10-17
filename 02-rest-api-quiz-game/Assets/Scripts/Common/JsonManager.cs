using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class Popup
{
    public int GameFirstCount;
    public List<titleData> Title;
    public List<explainData> Explain;
    public List<PersonCheckData> PersonCheck;
    public List<BallPracticeData> BallPractice;
    public List<Practice_Page_FirstData> Practice_Page_First;
    public List<Tutorial_First_TextData> Tutorial_First_Text;
    public List<Tutorial_Second_TextData> Tutorial_Second_Text;
    public List<StartGame_TextData> StartGame_Texts;
    public List<Second_TextData> Second_Texts;
}

//타이틀
[System.Serializable]
public class titleData
{
    public List<TextData> Title_Txt;
}

//조각 텍스트
[System.Serializable]
public class explainData
{
    public List<TextData> MindPiece_Txt;
}

//사람 확인
[System.Serializable]
public class PersonCheckData
{
    public List<ImageCircle> PersonalCircle;
    public List<TextData> PersonCheckText, PersonelSiteText;
}

//볼터트리기 방법
[System.Serializable]
public class BallPracticeData
{
    public List<TextData> BallText;
}

//볼터트리기 문구
[System.Serializable]
public class Practice_Page_FirstData
{
    public List <TextData> FirstTxt, SecondTxt, ThirdTxt;
}

//튜토리얼 문구1
[System.Serializable]
public class Tutorial_First_TextData
{
    public List<TextData> Tutorial_first_Txt;
    public List<ImageSmallCircle> ImageSC;
    public List<TextData> NamePrep;
}

//튜토리얼 문구2
[System.Serializable]
public class Tutorial_Second_TextData
{
    public List<TextData> Tutorial_Second_Txt, Finish_Tutorial_Txt;
}

//튜토리얼 문구3
[System.Serializable]
public class StartGame_TextData
{
    public List<TextData> SG_text1, SG_text2, SG_text3, SG_text4;
}

//두번째 게임 문구1
[System.Serializable]
public class Second_TextData
{
    public List<TextData> Second_Text1, Second_Text2, Second_Text3;
}

#region 기본 요소
[System.Serializable]
public class TextData
{
    public string id;
    public string KorSentance;
    public string EngSentance;
    public string JapSentance;

    public string GetSentance(string languageId)
    {
        switch (languageId)
        {
            case "ko": return KorSentance;
            case "en": return EngSentance;
            case "jp": return JapSentance;
            default: return KorSentance;
        }
    }
}

[System.Serializable]
public class ImageCircle
{
    public string BlueImage;
    public string GreenImage;
    public string OrangeImage;
    public string PinkImage;
    public string RedImage;
    public string YellowImage;

    public string GetImageCircle(string ImageCircleId)
    {
        switch (ImageCircleId)
        {
            case "0": return BlueImage;
            case "1": return GreenImage;
            case "2": return OrangeImage;
            case "3": return PinkImage;
            case "4": return RedImage;
            case "5": return YellowImage;
            default: return BlueImage;
        }
    }
}

[System.Serializable]
public class ImageSmallCircle
{
    public string SmallBlueImage;
    public string SmallGreenImage;
    public string SmallOrangeImage;
    public string SmallPinkImage;
    public string SmallRedImage;
    public string SmallYellowImage;

    public string GetImageCircle(string ImageCircleId)
    {
        switch (ImageCircleId)
        {
            case "0": return SmallBlueImage;
            case "1": return SmallGreenImage;
            case "2": return SmallOrangeImage;
            case "3": return SmallPinkImage;
            case "4": return SmallRedImage;
            case "5": return SmallYellowImage;
            default: return SmallBlueImage;
        }
    }
}

[System.Serializable]
public class ImageData
{
    public int id;
    public string path;
    public Vector2 imgSize;
    public Vector2 imgPos;
}

[System.Serializable]
public class HeadsetData
{
    public int id;
    public List<ImageData> circle;
    public List<ImageData> name;

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
    public static Popup popup;

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Popup.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            popup = JsonUtility.FromJson<Popup>(json);
        }
    }
}
