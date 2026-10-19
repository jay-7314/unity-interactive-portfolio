using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainDisplayManager_country : MonoBehaviour
{
    #region 인스펙터
    [Header("UI관련 컴포넌트")]
    [SerializeField] private UIJsonManager uiJson;
    [SerializeField] private RawImage countryBackgroundImage;

    [Header("text관련 UI인스펙터")]
    [SerializeField] private TextMeshProUGUI textDeagu;
    [SerializeField] private TextMeshProUGUI textTarget;
    [SerializeField] private TextMeshProUGUI textCal;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("페이지 오브젝트")]
    [SerializeField] private GameObject startPage;
    [SerializeField] private GameObject selectPage;
    [SerializeField] private GameObject selectCountry;

    [Header("버튼 오브젝트")]
    [SerializeField] private Button returnToHome;       //처음으로 돌아가기
    [SerializeField] private Button returnToBack;       //뒤로 돌아가기

    private string currentCountryId;
    #endregion

    #region 딕셔너리
    //딕셔너리관련(무엇을 선택했는지 확인)
    private Dictionary<string, string> displayName = new Dictionary<string, string>()
    {
        { "London", "영국 런던" },
        { "NewYork", "미국 뉴욕" },
        { "Paris", "프랑스 파리" },
        { "Cairo", "이집트 카이로" },
        { "Sydney", "호주 시드니" },
        { "Hanoi", "베트남 하노이" },
        { "Riopde", "브라질 리우데" }
    };

    private string[] Country = new string[]
    {
        "London", "NewYork", "Paris", "Cairo", "Sydney", "Hanoi", "Riopde"
    };

    private string[] KorCountry = new string[]
    {
         "영국 런던", "미국 뉴욕", "프랑스 파리", "이집트 카이로", "호주 시드니", "베트남 하노이", "브라질 리우데자네이루"
    };

    #endregion

    #region Func
    private void Start()
    {
        uiJson.LoadRawImage(CountryJsonManager.country.countryBackgroundImg, countryBackgroundImage);
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        ReturenOnClick();
        MainDisplayManager_select select = selectPage.GetComponent<MainDisplayManager_select>();
        select.StartReturnTimer();
    }

    private void TargetPage(GameObject targetPage)
    {
        startPage.SetActive(false);
        selectPage.SetActive(false);
        selectCountry.SetActive(false);

        targetPage.SetActive(true);
    }
   
    private string GetDisplayName(string id)
    {
        for (int i = 0; i < Country.Length; i++)
        {
            if(Country[i] == id)
            {
                return KorCountry[i];
            }
        }
        return null;
    }

    public void ApplyCountry(string id)
    {
        currentCountryId = id;
        string displayName = GetDisplayName(id);

        TargetPage(selectCountry);
        
        string info = CountryJsonManager.GetTimeInfo(id,displayName);
        string[] lines = info.Split('\n');

        if (lines.Length == 3)
        {

            textDeagu.text = lines[0];
            textTarget.text = lines[1];
            textCal.text = lines[2];
        }
        var (movie, _) = CountryJsonManager.CountryInfo(id);
        string path = Path.Combine(Application.streamingAssetsPath, movie).Replace("\\", "/");
        videoPlayer.url = path;
        videoPlayer.Play();

        SubDisplayManager.instance.PlaySubVideo(id);
    }
    #endregion

    #region Button
    public void HomeOnClick()
    {
        SubDisplayManager.instance.PlaySubVideo();
        
        TargetPage(startPage);
    }

    public void ReturenOnClick()
    {
        SubDisplayManager.instance.PlaySubVideo(currentCountryId, true);
        TargetPage(selectPage);
    }
    #endregion
}
