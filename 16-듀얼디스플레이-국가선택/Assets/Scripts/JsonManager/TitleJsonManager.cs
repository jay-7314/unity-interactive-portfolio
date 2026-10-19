using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class MainJsonList
{
    #region Title인스펙터
    public string mainBackground;                       //메인창 background
    public string titleCenter;                          //타이틀 센터 변경
    public string startBtnImg;                          //start버튼 이미지 변경
    public string startBtnAudio;                        //start버튼 오디오 변경
    public List<MainText> textList;                     //메인텍스트 폰트관련

    //위치 관련
    public Vector2 titleMessagePos;                     //타이틀 메세지 위치 변경
    public Vector2 startBtnPos;                         //start 버튼 위치 변경
    #endregion
}
[System.Serializable]
public class MainText : ITextData
{
    public string mainTextstring;                       //메인텍스트 내용 변경
    public int mainTextSize;                            //메인텍스트 폰트 변경

    public string GetText() => mainTextstring;
    public int GetFontSize() => mainTextSize;
}

public class TitleJsonManager : MonoBehaviour
{
    public static MainJsonList MainList;

    private void Awake()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Settings/Deagu.json").Replace("\\", "/");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            MainList = JsonUtility.FromJson<MainJsonList>(json);
        }

    }
}
