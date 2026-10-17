using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Webcam : MonoBehaviour
{
    [SerializeField] RawImage display;                    //화면을 띄우기 위한 RawImage   
    [SerializeField] RectTransform dotParent;             //json의 기준을 잡기 위한 Parents -> 웹캠 해상도와 화면 해상도는 일치하지 않음
    [SerializeField] GameObject dotPrefab;                //해당하는 위치값과 rgb값을 알기 위한 prefabs

    WebCamTexture camTexture;                             //웹캠을 띄우기 위한 texture
    int currentIndex = 0;                                 //웹캠 디바이스 번호
    List<RectTransform> dots = new List<RectTransform>(); //점의 위치를 표현하기 위한 초기값
    List<AudioSource> audioSources = new List<AudioSource>();   //소리를 나타내기 위한 초기값

    bool[] wasPlaying;                                    //이전 프레임에 소리가 났는지 여부
    float[] cooldowns;                                    //쿨타임을 위한 시간 저장

    void Start()
    {
        //웹캠을 유니티에서 띄우기 위한 코드
        WebCamDevice devices = WebCamTexture.devices[currentIndex];
        camTexture = new WebCamTexture(devices.name);
        display.texture = camTexture;
        camTexture.Play();

        //json에서 지정한 위치를 반복문을 통해서 위치를 판단함
        for (int i = 0; i < JsonManager.dorime.Position.Count; i++)
        {
            //점을 그리고(실제로도 존재는 하나 알파값을 0으로 줘서 보이지는 않음) 기준점을 왼쪽 상단으로 보내기 위함
            GameObject dot = Instantiate(dotPrefab, dotParent);
            RectTransform rt = dot.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0, 1);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            dots.Add(rt);

            //오디오 초기값 설정
            AudioSource audioSource = dot.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSources.Add(audioSource);

            //소리파일 json에서 로드
            string soundPath = JsonManager.dorime.Position[i].Sound;
            StartCoroutine(LoadAudio(soundPath, audioSource));
        }

        //이전 프레임 상태 및 쿨타임 배열 초기화
        wasPlaying = new bool[JsonManager.dorime.Position.Count];
        cooldowns = new float[JsonManager.dorime.Position.Count];

        UpdateDotPositions();
    }

    //mp3파일을 불러오기 위한 Load파일 -> unitywebrequest방식, 평소에는 file을 사용하지 않지만 audio는 사용해야함.
    IEnumerator LoadAudio(string soundPath, AudioSource audioSource)
    {
        string fullPath = "file://" + Path.Combine(Application.streamingAssetsPath, soundPath);
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                audioSource.clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
            }
        }
    }

    //웹캠의 해상도와 유니티의 해상도가 다르기때문에 해상도변환이 필요함.
    void UpdateDotPositions()
    {
        float scaleX = dotParent.rect.width / 1920f;
        float scaleY = dotParent.rect.height / 1080f;

        for (int i = 0; i < JsonManager.dorime.Position.Count; i++)
        {
            Vector2 pixelPos = JsonManager.dorime.Position[i].Pos;
            dots[i].anchoredPosition = new Vector2(pixelPos.x * scaleX, pixelPos.y * scaleY);
        }
    }

    private void Update()
    {
        //이전프레임과 현재프레임 비교하기 위해서 didUpdateThisFrame사용 -> 웹캠 업데이트 없으면 스킵
        if (!camTexture.didUpdateThisFrame) return;

        for (int i = 0; i < JsonManager.dorime.Position.Count; i++)
        {
            Vector2 pixelPos = JsonManager.dorime.Position[i].Pos;

            //화면 해상도의 변화 때문에 설정함. 상하반전이 되어서 다시 조치함.
            int x = Mathf.Clamp((int)(pixelPos.x / 1920f * camTexture.width), 0, camTexture.width - 1);
            int y = Mathf.Clamp((int)(-pixelPos.y / 1080f * camTexture.height), 0, camTexture.height - 1);
            y = camTexture.height - 1 - y;

            // GetPixels32 대신 해당 좌표 픽셀만 읽기 -> 전체 배열 복사 없이 가볍게 처리
            Color32 pixel = camTexture.GetPixel(x, y);

            // 3채널 중 2개 이상 100 이하면 소리 재생
            int count = 0;
            if (pixel.r <= 100) count++;
            if (pixel.g <= 100) count++;
            if (pixel.b <= 100) count++;

            //해당하는 값의 count가 증가하면 그때서야 소리나오게 함.
            if (count >= 2)
            {
                if (audioSources[i] != null && audioSources[i].clip != null)
                {
                    if (!wasPlaying[i])
                    {
                        // 이전 프레임에 감지 안됐다가 새로 감지 → 악기처럼 즉각 재생
                        audioSources[i].PlayOneShot(audioSources[i].clip);
                        cooldowns[i] = Time.time + 0.2f;
                    }
                    else if (Time.time > cooldowns[i])
                    {
                        // 이전 프레임에도 감지 중 → 0.2초 쿨타임마다 재생
                        audioSources[i].PlayOneShot(audioSources[i].clip);
                        cooldowns[i] = Time.time + 0.2f;
                    }
                    wasPlaying[i] = true;
                }
            }
            else
            {
                // 감지 안되면 이전 프레임 상태 초기화
                wasPlaying[i] = false;
            }

            //이미지뿐만 아니라 console창에서 표현하기 위한 debug설정
            Debug.Log($"[{i}] ({(int)pixelPos.x}, {(int)pixelPos.y}) | R:{pixel.r} G:{pixel.g} B:{pixel.b}");
        }
    }
}