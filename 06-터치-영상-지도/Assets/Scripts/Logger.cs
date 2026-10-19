using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Logger : MonoBehaviour
{
    public static Logger Instance;

    private string baseUrl = "https://logs.example.com/api/content_log";

    [Header("발급받은 값 (지금 확인한 값)")]
    public int idx;
    public string uid;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        idx = JsonManager.inx;
        uid = JsonManager.uid;
        SendLog("start");
    }

    public void SendLog(string message)
    {
        StartCoroutine(SendLogCoroutine(message));
    }

    private IEnumerator SendLogCoroutine(string message)
    {
        string url = $"{baseUrl}?idx_content_device={idx}&uid={uid}&message={UnityWebRequest.EscapeURL(message)}";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[LogSender] 성공: {message} / 응답: {req.downloadHandler.text}");
            }
            else
            {
                Debug.LogWarning($"[LogSender] 실패: {message} / 에러: {req.error}");
            }
        }
    }
}
