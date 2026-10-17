using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Title : MonoBehaviour
{
    [SerializeField]Image title;
    bool IsGameStart = false;
    [SerializeField] GameObject bg;
    [SerializeField] VideoPlayer vp;

    bool empty1 = false;
    bool empty2 = false;

    private void Start()
    {
        title = GetComponent<Image>();
    }

    private void OnEnable()
    {
        ArduinoManager.OnArduinoMsg += OnMessageArrived;
        empty1 = false;
        empty2 = false;
        IsGameStart = false;
        Color coloralpha = title.color;
        coloralpha.a = 1f;
        title.color = coloralpha;

        vp.Stop();
        vp.time = 0;
        vp.frame = 0;

        vp.Prepare();
        vp.prepareCompleted += OnVideoPrepared;
        StartCoroutine(RequestEmpty());
    }

    private void OnDisable()
    {
        ArduinoManager.OnArduinoMsg -= OnMessageArrived;
        vp.prepareCompleted -= OnVideoPrepared;
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        source.frame = 0;
        source.Play();
        source.Pause();
    }

    IEnumerator RequestEmpty()
    {
        yield return CoroutineData.GetWaitForSeconds(2f);
        ArduinoManager.Instance.SendToArduinoA("CheckEmpty");
        ArduinoManager.Instance.SendToArduinoB("CheckEmpty");
        Debug.Log("아두이노에게 메세지를 보냈습니다.");
    }


    void OnMessageArrived(string msg)
    {
        if(msg.Equals("Empty1")) empty1 = true;
        if(msg.Equals("Empty2")) empty2 = true;

        if(empty1 && empty2 && !IsGameStart)
        {
            IsGameStart = true;
            StartCoroutine(FadeImage());
        }
    }

    IEnumerator FadeImage()
    {
        float alpha = 1f;
        while(alpha > 0)
        {
            alpha -= 0.1f;
            Color colorAlpha = title.color;
            colorAlpha.a = alpha;
            title.color = colorAlpha;
            yield return CoroutineData.GetWaitForSeconds(0.2f);
        }

        vp.time = 0;
        vp.frame = 0;
        vp.Play();

        yield return null;

        bg.SetActive(false);
        title.gameObject.SetActive(false);

        
    }
}
