using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class UIJsonManager : MonoBehaviour
{
  public void LoadVideo(string path, VideoPlayer videoPlayer)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, path);
        if (File.Exists(filePath))
        {
            videoPlayer.url = filePath;
            videoPlayer.Play();
        }
    }
}
