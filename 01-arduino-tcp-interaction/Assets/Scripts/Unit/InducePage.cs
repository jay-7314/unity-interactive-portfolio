using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InducePage : MonoBehaviour
{
    Image gotoPicturePage, inducePageImg, returnToVideoPage;

    private void OnEnable()
    {
        LoadManager.Instance.ReturnToTitleTimer();
        CreateUnit();
    }

    private void OnDisable()
    {
        LoadManager.Instance.StopReturnTimer();
        ClearUnit();
    }

    void ClearUnit()
    {
        if (inducePageImg != null) Destroy(inducePageImg.gameObject);
        if (returnToVideoPage != null) Destroy(returnToVideoPage.gameObject);
        if (gotoPicturePage != null) Destroy(gotoPicturePage.gameObject);
    }

    void CreateUnit()
    {
        StartCoroutine(GotoNextPage(5f));
        var InduceData = JsonManager.uljin.InducePage[0];
        var LM = LoadManager.Instance;
        inducePageImg = LM.CreateUIObject<ImageData, Image>("InducePage", InduceData.InducePageImg[0], LM.LoadImagePos, transform);

        returnToVideoPage = LM.CreateUIObject<ImageData, Image>("ReturnTovideoPage", InduceData.ReturnToVideoPage[0], LM.LoadImagePos, transform);
        Button returnVideoPage = returnToVideoPage.AddComponent<Button>();
        returnVideoPage.onClick.AddListener(ReturnToVideo);

        gotoPicturePage = LM.CreateUIObject<ImageData, Image>("GotoImagePage", InduceData.GotoImagePage[0], LM.LoadImagePos, transform);
        Button goPicturePage = gotoPicturePage.AddComponent<Button>();
        goPicturePage.onClick.AddListener(PageNVideo);
    }

    IEnumerator GotoNextPage(float delay)
    {
        yield return new WaitForSeconds(delay);
        PageNVideo();
    }

    void PageNVideo()
    {
        UIManager.Instance.TargetPage(UIManager.Pagelist.picturePage);

    }

    void ReturnToVideo()
    {
        ClientTCP.Instance.Send($"수렴 포인트 삭제");
        ClientTCP.Instance.Send($"수렴 동영상 비활성화");
        UIManager.Instance.TargetPage(UIManager.Pagelist.videoPage);
    }


}
