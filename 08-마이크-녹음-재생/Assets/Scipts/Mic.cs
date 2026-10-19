using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Mic : MonoBehaviour
{
    AudioSource audioSource;
    private VideoPlayer secondVp; // 인스펙터 대신 코드로 받음
    string micDevice;
    float recordTime = 4.5f;

    void Start()
    {
        micDevice = Microphone.devices[0];
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void SetVideoPlayer(VideoPlayer vp)
    {
        secondVp = vp;
        secondVp.time = 0;
        secondVp.Stop();
    }

    // Record 버튼에 연결
    public void Recod()
    {
        if (secondVp == null) return;

        if (Microphone.IsRecording(micDevice))
            Microphone.End(micDevice);

        secondVp.time = 0;
        secondVp.Play();

        audioSource.clip = Microphone.Start(micDevice, false, (int)recordTime, 44100);
        StartCoroutine(StopAfterRecord());
    }

    IEnumerator StopAfterRecord()
    {
        yield return new WaitForSeconds(recordTime);
        Microphone.End(micDevice);
        secondVp.Pause();
    }

    // Play 버튼에 연결
    public void Play()
    {
        if (secondVp == null) return;
        if (audioSource.clip == null) return;

        if (Microphone.IsRecording(micDevice))
            Microphone.End(micDevice);

        secondVp.time = 0;
        secondVp.Play();
        audioSource.Play();
        StartCoroutine(StopAfterPlay());
    }

    IEnumerator StopAfterPlay()
    {
        yield return new WaitForSeconds(recordTime);
        audioSource.Stop();
        secondVp.Pause();
    }
}