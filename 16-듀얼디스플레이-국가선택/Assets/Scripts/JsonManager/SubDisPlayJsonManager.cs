using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SubVideoItem
{
    public string id;                   //id값 받기
    public string subVideoTarget;       //video값 받기
    public string subVideoReturn;       //Return Video값 받기
}

[System.Serializable]
public class SubVideoData
{
    public string defultVideo;          //한국만 있는 동영상
    public List<SubVideoItem> videos;
}

public enum SubvideoType
{
    Enter,
    Return
}

public class SubDisPlayJsonManager : MonoBehaviour
{
    public static Dictionary<string, string> subVideoDic;
    public static Dictionary<string, string> returnsubVideoDic;
    public static string DefaultVideoPath;

    private void Awake()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Settings/SubVideo.json").Replace("\\","/");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SubVideoData data = JsonUtility.FromJson<SubVideoData>(json);

            subVideoDic = new Dictionary<string, string>();
            returnsubVideoDic = new Dictionary<string, string>();
            for (int i = 0; i < data.videos.Count; i++)
            {
                subVideoDic[data.videos[i].id] = data.videos[i].subVideoTarget;
                returnsubVideoDic[data.videos[i].id] = data.videos[i].subVideoReturn;
            }
            DefaultVideoPath = Path.Combine(Application.streamingAssetsPath, data.defultVideo).Replace("\\", "/");

        }
    }

    public static string SubMoviePath(string id, SubvideoType type)
    {
        Dictionary<string, string> targetDic = (type == SubvideoType.Enter) ? subVideoDic : returnsubVideoDic;

        if (subVideoDic != null && targetDic.ContainsKey(id))
        {
            return Path.Combine(Application.streamingAssetsPath, targetDic[id]).Replace("\\", "/");
        }

        if(type == SubvideoType.Enter)
        {
            return DefaultVideoPath;

        }
        return null;
        
    }
   

}
