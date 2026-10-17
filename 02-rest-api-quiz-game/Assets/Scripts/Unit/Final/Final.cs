using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Final : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] Text finalBg_Text;
    [SerializeField] GameObject mindPiece, fanfare;
    [SerializeField] PointManager pointmanager;
    private void OnEnable()
    {
        StartCoroutine(WaitForSecond());
        finalBg_Text.gameObject.SetActive(false);
    }

    IEnumerator WaitForSecond()
    {
        yield return CoroutineData.GetWaitForSeconds(2f);
        finalBg_Text.gameObject.SetActive(true);
        finalBg_Text.text = $"우리는 함께 \n{pointmanager.popupPoint}개를 터트렸어요!";
        StartCoroutine(networkManager.UpdateMindCount(pointmanager.mindPoint));
        yield return CoroutineData.GetWaitForSeconds(2f);
        mindPiece.SetActive(true);

    }
}
