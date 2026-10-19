using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TItle : MonoBehaviour
{
    Image titleImg, backgroundImg;

    private void Start()
    {
        LoadManager.Instance.ReturnToTitleTimer();
        CreateBtn();
    }

    void CreateBtn()
    {
        var titleData = JsonManager.uljin.Title[0];
        var LM = LoadManager.Instance;

        backgroundImg = LM.CreateUIObject<ImageData, Image>("TitleBackground", titleData.TitleBackgroundImg[0], LM.LoadImagePos, transform);
        titleImg = LM.CreateUIObject<ImageData, Image>("TitleBtn", titleData.titleImage[0], LM.LoadImagePos, transform);

        Button titleBtn = titleImg.gameObject.AddComponent<Button>();
        titleBtn.onClick.AddListener(GameStartUI);
    }

    void GameStartUI()
    {
        UIManager.Instance.TargetPage(UIManager.Pagelist.interaction);
    }
}
