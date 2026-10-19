using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainDisplayManager_select : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private UIJsonManager uiJson;
    public GameObject upLine;            //윗줄나라
    public GameObject downLine;          //아랫줄나라

    private List<RawImage> uplineImage = new List<RawImage>();
    private List<RawImage> downlineImage = new List<RawImage>();

    [Header("MessageSite")]
    [SerializeField] private GameObject messageSite;            //메세지창 위치
    [SerializeField] private TextMeshProUGUI messageText;       //메세지 내용변경

    private float returnTime;                                    //돌아가는 시간

    [SerializeField] private GameObject startPage;              //첫페이지로 돌아감.
    [SerializeField] private GameObject secondPage;             //두번째 페이지 꺼짐.
    #endregion

    #region Func
    private void Start()
    {
        returnTime = SelectJsonManager.line.ReturnTimer;
        ApplyJson();
        MessageJson();
    }

    private void OnEnable()
    {
        StartCoroutine(BackreturnTime());
    }
    IEnumerator BackreturnTime()
    {
        yield return new WaitForSeconds(returnTime);
        startPage.SetActive(true);
        SubDisplayManager.instance.PlaySubVideo();

        yield return null;
        secondPage.SetActive(false);
    }

    public void StartReturnTimer()
    {
        StartCoroutine(BackreturnTime());
    }

    #endregion

    #region json관련

    private void ApplyJson()
    {
        UpJson();
        DownJson();
    }

    private void UpJson()
    {
        List<ItemConfig> upItems = SelectJsonManager.line.Upline.items;
        Vector2 upPadding = SelectJsonManager.line.Upline.padding;
        int upSpacing = SelectJsonManager.line.Upline.spacing; //각 요소별 수평 간격
        int upRowY = SelectJsonManager.line.Upline.rowY;       //그림과 버튼 수직 간격
        for (int i = 0; i < upLine.transform.childCount; i++)
        {
            GameObject country = upLine.transform.GetChild(i).gameObject;

            if (i < upItems.Count)
            {
                country.SetActive(true);
                ItemConfig data = upItems[i];

                RawImage[] images = country.GetComponentsInChildren<RawImage>();
                if (images.Length >= 2)
                {
                    uiJson.LoadRawImage(data.picture, images[0]);
                    uiJson.LoadRawImage(data.btn, images[1]);

                    float x = upPadding.x + i * upSpacing;
                    images[0].rectTransform.localPosition = new Vector3(x, upPadding.y, 0);
                    images[1].rectTransform.localPosition = new Vector3(x, upPadding.y - upRowY, 0);
                }
                CountryClick[] clicks = country.GetComponentsInChildren<CountryClick>();
                for (int j = 0; j < clicks.Length; j++)
                {
                    clicks[j].SetId(data.id);
                }
            }
            else
            {
                country.SetActive(false);
            }
        }
    }

    private void DownJson()
    {
        List<ItemConfig> downItems = SelectJsonManager.line.Downline.items;
        Vector2 downPadding = SelectJsonManager.line.Downline.padding;
        int downSpacing = SelectJsonManager.line.Downline.spacing; //각 요소별 수평 간격
        int downRowY = SelectJsonManager.line.Downline.rowY;       //그림과 버튼 수직 간격
        for (int i = 0; i < downLine.transform.childCount; i++)
        {
            GameObject country = downLine.transform.GetChild(i).gameObject;

            if (i < downItems.Count)
            {
                country.SetActive(true);
                ItemConfig data = downItems[i];

                RawImage[] images = country.GetComponentsInChildren<RawImage>();
                if (images.Length >= 2)
                {
                    uiJson.LoadRawImage(data.picture, images[0]);
                    uiJson.LoadRawImage(data.btn, images[1]);

                    float x = downPadding.x + i * downSpacing;
                    images[0].rectTransform.localPosition = new Vector3(x, downPadding.y, 0);
                    images[1].rectTransform.localPosition = new Vector3(x, downPadding.y - downRowY, 0);
                }
                CountryClick[] clicks = country.GetComponentsInChildren<CountryClick>();
                for (int j = 0; j < clicks.Length; j++)
                {
                    clicks[j].SetId(data.id);
                }
            }
            else
            {
                country.SetActive(false);
            }
        }

    }
    private void MessageJson()
    {
        //메세지 내용 변경 및 폰트 사이즈 변경
        uiJson.LoadText(SelectJsonManager.site[0], messageText);
        //메세지 배경이미지 변경
        uiJson.LoadRawImage(SelectJsonManager.site[0].messageSiteBackImage, messageSite.GetComponent<RawImage>());
        //메세지 배경위치 변경
        messageSite.GetComponent<RectTransform>().anchoredPosition = SelectJsonManager.site[0].messageSite;
        //메세지 배경크기 변경
        messageSite.GetComponent<RectTransform>().sizeDelta = SelectJsonManager.site[0].messageSiteSize;
    }
    #endregion

}
    
