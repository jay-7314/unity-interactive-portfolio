using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MindPiece : MonoBehaviour
{
    [SerializeField] Text getPiece, getTotalPiece;
    [SerializeField] RawImage[] pieceGroup;
    [SerializeField] PointManager pointManager;
    [SerializeField] GameObject keyHoll, FinalBg_text;
    [SerializeField] GameObject FinalBg2;
    [SerializeField] NetworkManager networkManager;
    [SerializeField] AudioSource pieceSound;

    public int serverPiece = 0;

    private void Start()
    {
        serverPiece = SumMindCount();
        keyHoll.SetActive(false);
        FinalBg_text.SetActive(false);

        // 모든 조각을 낮은 alpha로 시작
        for (int i = 0; i < pieceGroup.Length; i++)
        {
            pieceGroup[i].gameObject.SetActive(true);
            AlphaManager.instance.SetAlpha(pieceGroup[i], 100f / 255f);
        }

        getPiece.gameObject.SetActive(false);
        getTotalPiece.gameObject.SetActive(false);
        StartCoroutine(textCutIn());
    }

    int SumMindCount()
    {
        int total = 0;
        total += ParseMind(networkManager.A1MindCount);
        total += ParseMind(networkManager.B1MindCount);
        total += ParseMind(networkManager.C1MindCount);
        total += ParseMind(networkManager.D1MindCount);
        total += ParseMind(networkManager.A2MindCount);
        total += ParseMind(networkManager.B2MindCount);
        total += ParseMind(networkManager.C2MindCount);
        total += ParseMind(networkManager.D2MindCount);
        total += ParseMind(networkManager.A3MindCount);
        total += ParseMind(networkManager.B3MindCount);
        total += ParseMind(networkManager.D3MindCount);
        return total;
    }

    int ParseMind(string value)
    {
        return int.TryParse(value, out int result) ? result : 0;
    }

    IEnumerator textCutIn()
    {
        yield return CoroutineData.GetWaitForSeconds(2f);

        // mindPoint 개수만큼 순차적으로 alpha 증가 + 소리
        for (int i = 0; i < pointManager.mindPoint && i < pieceGroup.Length; i++)
        {
            StartCoroutine(FadeInPiece(pieceGroup[i]));
            pieceSound?.PlayOneShot(pieceSound.clip);
            yield return CoroutineData.GetWaitForSeconds(0.5f);
        }

        yield return CoroutineData.GetWaitForSeconds(0.5f);

        getPiece.text = $"'우리는' 마음 조각 \n{pointManager.mindPoint}개를 받았습니다.";
        getTotalPiece.text = $"'우리'의 마음 조각 {serverPiece + pointManager.mindPoint}/20";
        getPiece.gameObject.SetActive(true);
        getTotalPiece.gameObject.SetActive(true);

        yield return CoroutineData.GetWaitForSeconds(2f);

        getPiece.gameObject.SetActive(false);
        getTotalPiece.gameObject.SetActive(false);
        FinalBg2.SetActive(true);
    }

    IEnumerator FadeInPiece(RawImage img)
    {
        float duration = 0.4f;
        float startAlpha = 100f / 255f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1f, elapsed / duration);
            AlphaManager.instance.SetAlpha(img, alpha);
            yield return null;
        }

        AlphaManager.instance.SetAlpha(img, 1f);
    }
}