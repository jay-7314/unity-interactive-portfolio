using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PicturePage : MonoBehaviour
{
    Image worldImg, explainImg, cancelImg, returnToVideo;
    List<Image> selectObjects = new List<Image>();

    private void OnEnable()
    {
        LoadManager.Instance.ReturnToTitleTimer();
        CreateText();
        StartCoroutine(CreateImageUnit());
    }

    private void OnDisable()
    {
        LoadManager.Instance.StopReturnTimer();
        ClearUnit();
    }

    void ClearUnit()
    {
        if (returnToVideo != null)
        {
            Destroy(returnToVideo.gameObject);
            returnToVideo = null;
        }
        if (worldImg != null)
        {
            Destroy(worldImg.gameObject);
            worldImg = null;
        }

        if (explainImg != null)
        {
            Destroy(explainImg.gameObject);
            explainImg = null;
        }

        if (cancelImg != null)
        {
            Destroy(cancelImg.gameObject);
            cancelImg = null;
        }

        for (int i = selectObjects.Count - 1; i >= 0; i--)
        {
            if (selectObjects[i] != null)
            {
                Destroy(selectObjects[i].gameObject);
            }
        }

        selectObjects.Clear();
    }

    void CreateText()
    {
        var pictureData = JsonManager.uljin.PicturePage[0];
        var LM = LoadManager.Instance;

        returnToVideo = LM.CreateUIObject<ImageData, Image>("ReturnToVideo", pictureData.ReturnToVideo[0], LM.LoadImagePos, transform);
        Button returnVideoBtn = returnToVideo.AddComponent<Button>();
        returnVideoBtn.onClick.AddListener(ReturnToVideo);
        worldImg = LM.CreateUIObject<ImageData, Image>("MainSentance", pictureData.WorldImg[0], LM.LoadImagePos, transform);
        worldImg.transform.SetAsFirstSibling();

    }

    IEnumerator CreateImageUnit()
    {
        var pictureData = JsonManager.uljin.PicturePage[0];
        var LM = LoadManager.Instance;

        for (int i = 0; i < pictureData.Selects.Count; i++)
        {
            ImageData selectData = pictureData.Selects[i];
            Image img = LM.CreateUIObject<ImageData, Image>("Select" + selectData.id, selectData, LM.LoadImagePos, transform);
            selectObjects.Add(img);

            yield return LM.FadeUnit(img, 1.5f);
            yield return new WaitForSeconds(0.5f);
        }

        for (int i = 0; i < selectObjects.Count; i++)
        {
            int id = i;

            Button selectBtn = selectObjects[i].AddComponent<Button>();
            selectBtn.onClick.AddListener(() => SelectIdImage(id));
        }
    }

    void SelectIdImage(int id)
    {
        LoadManager.Instance.ReturnToTitleTimer();
        var pictureData = JsonManager.uljin.PicturePage[0];
        var LM = LoadManager.Instance;

        if (explainImg != null)
        {
            Destroy(explainImg.gameObject);
            explainImg = null;
        }
        if (cancelImg != null)
        {
            Destroy(cancelImg.gameObject);
            cancelImg = null;
        }

        for (int i = 0; i < selectObjects.Count; i++)
        {
            selectObjects[i].gameObject.SetActive(false);
        }

        ImageData explainData = null;

        for (int i = 0; i < pictureData.Select_Explain.Count; i++)
        {
            if (pictureData.Select_Explain[i].id == id)
            {
                explainData = pictureData.Select_Explain[i];
                break;
            }
        }

        if (explainData == null) return;

        explainImg = LM.CreateUIObject<ImageData, Image>("ExplainImg", explainData, LM.LoadImagePos, transform);
        cancelImg = LM.CreateUIObject<ImageData, Image>("CancleImg", pictureData.Select_Cancel[0], LM.LoadImagePos, transform);
        cancelImg.transform.SetParent(explainImg.transform, false);
        RectTransform cancelPos = cancelImg.GetComponent<RectTransform>();
        cancelPos.anchoredPosition = explainData.CencelPositions;
        Button cancelBtn = cancelImg.AddComponent<Button>();
        cancelBtn.onClick.AddListener(CancelExplain);
    }

    void CancelExplain()
    {
        LoadManager.Instance.ReturnToTitleTimer();
        if (explainImg != null)
        {
            Destroy(explainImg.gameObject);
            explainImg = null;
        }
        if (cancelImg != null)
        {
            Destroy(cancelImg.gameObject);
            cancelImg = null;
        }
        for (int i = 0; i < selectObjects.Count; i++)
        {
            selectObjects[i].gameObject.SetActive(true);
        }
    }

    void ReturnToVideo()
    {
        LoadManager.Instance.StopReturnTimer();
        ClientTCP.Instance.Send($"수렴 포인트 삭제");
        ClientTCP.Instance.Send($"수렴 동영상 비활성화");
        UIManager.Instance.TargetPage(UIManager.Pagelist.videoPage);
    }

}
