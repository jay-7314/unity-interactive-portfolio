using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] GameObject title, explainPage;
    [SerializeField] SelectVideo selectVideo;

    private void Start()
    {
        ArduinoManager.Instance.SendString(JsonManager.poly.OffLamp);

        // 씬 시작 시 모든 영상 프리로드
        for (int i = 0; i < JsonManager.poly.VideoList.Count; i++)
        {
            VideoData videoData = JsonManager.poly.VideoList[i];
            LoadManager.Instance.PreloadVideo(videoData, () =>
            {
                Debug.Log($"[Title] 프리로드 완료: {videoData.VideoPath}");
            });
        }
    }

    private void OnEnable()
    {
        ArduinoManager.Instance.SendString(JsonManager.poly.OffLamp);
    }

    public void GotoSelectPage()
    {
        title.SetActive(false);
        explainPage.SetActive(true);
        ArduinoManager.Instance.SendString(JsonManager.poly.OnLamp);
    }
}