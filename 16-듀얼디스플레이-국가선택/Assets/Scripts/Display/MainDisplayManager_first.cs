using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainDisplayManager_first : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private UIJsonManager uiJson;

    [Header("배경")]
    [SerializeField] private RawImage backgroundImage;              //background변경
    [SerializeField] private RawImage titleCenterImage;             //Title화면 중앙변경

    [Header("내용변경")]
    [SerializeField] private TextMeshProUGUI titleMessage;

    [Header("버튼")]
    [SerializeField] private GameObject StartBtn;                       //start버튼

    [Header("페이지")]
    [SerializeField] private GameObject TitlePage;                  //타이틀 화면
    [SerializeField] private GameObject SelectPage;                 //select화면

    [Header("프로그램 종료")]
    float shutTimer = 0f;                      //0으로 초기값을 넣어진행
    int clickCount = 0;                        //누른 횟수 초기값으로 진행
    #endregion

    #region Func
    private void Start()
    {
        ApplyJsonData();
    }

    private void SetPage(GameObject targetPage)
    {
        TitlePage.SetActive(false);
        SelectPage.SetActive(false);

        targetPage.SetActive(true);
    }
    #endregion

    #region OnClick관련
    public void StartOnClick()
    {
        StartCoroutine(delayStartClick());

    }

    IEnumerator delayStartClick()
    {
        AudioManager.Instance.PlaySound(TitleJsonManager.MainList.startBtnAudio);
        yield return new WaitForSeconds(0.1f);
        SetPage(SelectPage);
    }

    public void ShutdownOnClick()
    {
        shutTimer += Time.deltaTime;
        if (shutTimer >= 3)
        {
            clickCount = 0;
            shutTimer = 0f;
        }
        else
        {
            clickCount++;
            if (clickCount >= 10)
            {
                Application.Quit();
            }
        }
    }
    #endregion

    #region Json관련
    private void ApplyJsonData()
    {
        //배경이미지 변경
        uiJson.LoadRawImage(TitleJsonManager.MainList.mainBackground, backgroundImage);
        //메인 텍스트 변경 및 폰트 사이즈 변경
        uiJson.LoadText(TitleJsonManager.MainList.textList[0], titleMessage);
        //메인 텍스트 위치 변경
        titleMessage.GetComponent<RectTransform>().anchoredPosition = TitleJsonManager.MainList.titleMessagePos;
        //maincenter Circle변경
        uiJson.LoadRawImage(TitleJsonManager.MainList.titleCenter, titleCenterImage);
        //start버튼 이미지 변경
        uiJson.LoadRawImage(TitleJsonManager.MainList.startBtnImg, StartBtn.GetComponent<RawImage>());
        //start버튼 위치 변경
        StartBtn.GetComponent<RectTransform>().anchoredPosition = TitleJsonManager.MainList.startBtnPos;
    }
    #endregion
}
