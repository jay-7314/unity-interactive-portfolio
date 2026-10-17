using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] GameObject fanfareObj;
    [SerializeField] RawImage gameOverBG;
    [SerializeField] float duration = 3f;
    [SerializeField] GameObject gameoverText;
    [SerializeField] GameObject second;
    [SerializeField] GameObject finalImg;

    private void Start()
    {
        StartCoroutine(MoveImge(new Vector2(0, 1080), new Vector2(0, 0)));
    }

    IEnumerator MoveImge(Vector2 startPos, Vector2 endPos)
    {
        RectTransform rect = gameOverBG.GetComponent<RectTransform>();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }
        rect.anchoredPosition = endPos;
        gameoverText.SetActive(true);
        yield return CoroutineData.GetWaitForSeconds(1f);
        yield return StartCoroutine(AllmageFade());

    }

    public IEnumerator AllmageFade(bool isSuccess = false)
    {
        float fadeDuration = 2f;
        float elapsed = 0f;

        CanvasGroup secondCG = second.GetComponent<CanvasGroup>();
        if (secondCG == null) secondCG = second.AddComponent<CanvasGroup>();

        CanvasGroup finalCG = finalImg.GetComponent<CanvasGroup>();
        if (finalCG == null) finalCG = finalImg.AddComponent<CanvasGroup>();

        secondCG.alpha = 1f;
        finalCG.alpha = 0f;
        finalImg.SetActive(true);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            secondCG.alpha = Mathf.Lerp(1f, 0f, t);
            finalCG.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        secondCG.alpha = 0f;
        finalCG.alpha = 1f;
        second.SetActive(false);

        if (isSuccess && fanfareObj != null)
        {
            fanfareObj.GetComponent<Fanfare>()?.FanfareFunc();
        }
    }
}
