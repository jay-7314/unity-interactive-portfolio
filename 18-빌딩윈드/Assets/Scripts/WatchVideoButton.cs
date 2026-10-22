using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

// WatchVideo 버튼에 붙이는 스크립트
// 1) 버튼을 누르면 Videos를 활성화하고 영상을 처음부터 재생합니다.
// 2) 영상이 끝나면 ThirdPage를 끄고 ForthPage를 켭니다.
// 3) ThirdPage가 다시 켜질 때(다른 페이지에서 돌아올 때) Videos는 꺼진 상태로 초기화됩니다.
[RequireComponent(typeof(Button))]
public class WatchVideoButton : MonoBehaviour
{
    [Header("연결할 오브젝트")]
    [Tooltip("ThirdPage 아래의 Videos")]
    public GameObject videos;

    [Tooltip("현재 페이지 (ThirdPage)")]
    public GameObject thirdPage;

    [Tooltip("영상이 끝나면 넘어갈 페이지 (ForthPage)")]
    public GameObject forthPage;

    private VideoPlayer videoPlayer;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClickWatch);

        if (videos != null)
        {
            videoPlayer = videos.GetComponentInChildren<VideoPlayer>(true);
            if (videoPlayer != null)
            {
                videoPlayer.playOnAwake = false;   // 재생 시점은 스크립트에서 직접 제어
                videoPlayer.isLooping = false;
                videoPlayer.loopPointReached += OnVideoEnd;
            }
        }
    }

    // ThirdPage가 (다시) 켜질 때마다 영상 화면을 초기화
    private void OnEnable()
    {
        ResetVideo();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }

    private void OnClickWatch()
    {
        if (videos == null)
        {
            Debug.LogWarning("WatchVideoButton: Videos 오브젝트가 연결되지 않았습니다.");
            return;
        }

        videos.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.Stop();      // 처음 위치로 되돌림
            videoPlayer.time = 0;
            videoPlayer.Play();
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        ResetVideo();

        if (thirdPage != null) thirdPage.SetActive(false);
        if (forthPage != null) forthPage.SetActive(true);
    }

    private void ResetVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
        }

        if (videos != null)
            videos.SetActive(false);
    }
}