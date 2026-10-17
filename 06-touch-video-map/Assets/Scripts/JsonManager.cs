using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Building
{
    public int id;
    public string hight;
    public string floor;

    // 영상 파일명 (StreamingAssets 기준 상대 경로)
    public string idleVideo;
    public string touchVideo;
    public string mainVideo;

    // 레이아웃 정보 (Building1~10 위치/크기)
    public float buildingposX;
    public float buildingposY;
    public float buildingwidth;
    public float buildingheight;
    public float buttonposX;
    public float buttonposY;
    public float buttonwidth;
    public float buttonheight;
}

// JsonUtility는 최상위가 배열이면 바로 못 읽어서 래퍼 클래스로 감싸줌
// buildings 배열 외에 최상위에 있는 inx, uid도 함께 파싱
[System.Serializable]
public class BuildingListWrapper
{
    public List<Building> buildings;
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
    // 전체 리스트 (10개 건물)
    public static List<Building> buildingList;

    // 필요하면 id로 바로 찾아쓸 수 있는 딕셔너리
    public static Dictionary<int, Building> buildingDict;

    // JSON 최상위에 있는 부가 정보
    public static int inx;
    public static string uid;

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Building.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            BuildingListWrapper wrapper = JsonUtility.FromJson<BuildingListWrapper>(json);

            buildingList = wrapper.buildings;
            inx = wrapper.inx;
            uid = wrapper.uid;

            buildingDict = new Dictionary<int, Building>();
            foreach (var b in buildingList)
            {
                buildingDict[b.id] = b;
            }
        }
        else
        {
            Debug.LogWarning("Building.json을 찾을 수 없습니다: " + path);
        }
    }

    // id로 특정 건물 데이터 가져오기 (레이아웃 + 영상 정보 포함)
    public static Building GetBuilding(int id)
    {
        if (buildingDict != null && buildingDict.ContainsKey(id))
        {
            return buildingDict[id];
        }
        Debug.LogWarning("해당 id의 건물 데이터가 없습니다: " + id);
        return null;
    }
}