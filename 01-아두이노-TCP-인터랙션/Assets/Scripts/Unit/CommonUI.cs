using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CommonUI : MonoBehaviour
{
    Image backgroundImg, leftText, rightImg;

    private void Start()
    {
        UpsideUnit();
    }

    void UpsideUnit()
    {
        var commonData = JsonManager.uljin.Common[0];
        var LM = LoadManager.Instance;

        backgroundImg = LM.CreateUIObject<ImageData, Image>("CommonBackground", commonData.BackgroundImg[0], LM.LoadImagePos, transform);
        backgroundImg.transform.SetAsFirstSibling();
        Transform Top = transform.Find("Top");

        leftText = LM.CreateUIObject<ImageData, Image>("LeftImage", commonData.TItleSentance[0], LM.LoadImagePos, transform);

        GameObject commonRight = new GameObject("RightImg");
        commonRight.transform.SetParent(Top);
        rightImg = commonRight.AddComponent<Image>();
        LoadManager.Instance.LoadImagePos(commonData.HomeBtn[0], rightImg);
        Button returnTotitle = rightImg.AddComponent<Button>();
        returnTotitle.onClick.AddListener(GotoTitle);

        void GotoTitle()
        {
            LoadManager.Instance.ReturnToTitleBtnFuc();
        }
    }
}
