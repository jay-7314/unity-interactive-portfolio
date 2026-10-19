using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//MessageSite관리
[System.Serializable]
public class MessageSite : ITextData
{
    public Vector2 messageSite;         //메세지 배경 위치
    public string messageSiteBackImage; //메세지 배경 변경
    public Vector2 messageSiteSize;     //메세지 배경 사이즈 변경
    public string messageString;        //메세지 내용 변경
    public int messageFontSize;         //메세지 폰트 변경

    public string GetText() => messageString;
    public int GetFontSize() => messageFontSize;
}

//줄을 어떻게 관리할 것인가
[System.Serializable]
public class LineObject
{
    public int ReturnTimer;             //두번째 페이지에서 초기로 돌아가는 시간설정
    public int maxPerRow;               //한줄에 최대 몇개까지 가능한가
    public LineConfig Upline;           //윗줄
    public LineConfig Downline;         //아랫줄
    
}
//각 줄의 특징들
[System.Serializable]
public class LineConfig
{
    public Vector2 padding;             //얼만큼 떨어져 있는가
    public int spacing;                 //각 요소별로 얼마나 떨어지는가
    public int rowY;                    //그림과 버튼간의 거리가 얼마나 떨어져있는가
    public List<ItemConfig> items;      //아이템별 특징
}

//json모두 관리
[System.Serializable]
public class JsonFileManager
{
    public List<MessageSite> site;
    public LineObject line;
}

//각 요소별 어떤게 들어가는가
[System.Serializable]
public class ItemConfig
{
    public string id;                   //나라별 id값 적기 -> "London"
    public string picture;              //나라별 그림모양
    public string btn;                  //해당하는 나라 버튼
    public string btnSound;             //버튼 소리 변경
}
public class SelectJsonManager : MonoBehaviour
{
    public static LineObject line;
    public static List<MessageSite> site;

    private void Awake()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Settings/Select.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            JsonFileManager jsonFile = JsonUtility.FromJson<JsonFileManager>(json);

            line = jsonFile.line;
            site = jsonFile.site;
        }
    }
}
