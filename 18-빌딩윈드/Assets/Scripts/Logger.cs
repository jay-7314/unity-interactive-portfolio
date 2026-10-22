using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Logger : MonoBehaviour
{
    public static Logger Instance;

    private string baseUrl = "https://www.huliac.com/api/content_log.cfm";

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

    private IEnumerator Start()
    {
        // Paper가 채워질 때까지 매 프레임 대기
        yield return new WaitUntil(() => JsonManager.buildwind != null);

        idx = JsonManager.buildwind.inx;
        uid = JsonManager.buildwind.uid;
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
