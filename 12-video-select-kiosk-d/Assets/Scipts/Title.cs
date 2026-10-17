using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] GameObject title, explain;

    private void Start()
    {
        for (int i = 0; i < JsonManager.poly.VideoList.Count; i++)
        {
            VideoData videoData = JsonManager.poly.VideoList[i];
            LoadManager.Instance.PreloadVideo(videoData);
        }
    }

    public void GotoSelectPage()
    {
        title.SetActive(false);
        explain.SetActive(true);
    }
}