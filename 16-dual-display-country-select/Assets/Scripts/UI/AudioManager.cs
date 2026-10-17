using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    #region 인스펙터
    public static AudioManager Instance;
    private AudioSource audioSource;
    private Dictionary<string, AudioClip> Clip = new Dictionary<string, AudioClip>();
    #endregion

    #region Func
    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(string fileName)
    {
        if (Clip.ContainsKey(fileName))
        {
            audioSource.PlayOneShot(Clip[fileName]);
            return;
        }
        StartCoroutine(LoadAndPlay(fileName));
    }

    IEnumerator LoadAndPlay(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        string url = "file://" + path;

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clipSound = DownloadHandlerAudioClip.GetContent(request);
                Clip[fileName] = clipSound;
                audioSource.PlayOneShot(clipSound);
            }
        }
    }
    #endregion
}

