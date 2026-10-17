using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class RFIDReader
{
    public List<RFID> RFIDList;
    public float ReturnTimer;
    public int inx;
    public string uid;

    // 아두이노 시리얼 포트 설정 (JSON에 없으면 ArduinoManager 인스펙터 값을 사용)
    public string PortName;   // 예: "COM3"
    public int BaudRate;      // 예: 9600
}

[System.Serializable]
public class RFID
{
    public string RFIDKey;
    public string RFIDValue;
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
    public static RFIDReader rfid;
    public static Dictionary<string, string> rfidDict = new Dictionary<string, string>();

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "RFIDReader.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            rfid = JsonUtility.FromJson<RFIDReader>(json);

            rfidDict.Clear();
            foreach (var item in rfid.RFIDList)
            {
                string key = item.RFIDKey.Trim().ToUpper();
                if (!rfidDict.ContainsKey(key))
                {
                    rfidDict.Add(key, item.RFIDValue);
                }
                else
                {
                    Debug.LogWarning($"중복된 RFID 키가 있습니다: {key}");
                }
            }

            Debug.Log($"RFID 데이터 {rfidDict.Count}개 로드 완료");
        }
        else
        {
            Debug.LogError("RFIDReader.json 파일을 찾을 수 없습니다: " + path);
        }
    }
}