using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StartGame_Txt : MonoBehaviour
{
    [SerializeField] Text[] sg_texts;
    [SerializeField] GameObject startGame, FirstGame;
    [SerializeField] float[] textTimer;
    [SerializeField] GameObject countdown;
    [SerializeField] NetworkManager networkManager;
    Coroutine waitCor;

    private void OnEnable()
    {

        foreach (var txt in sg_texts)
            txt.gameObject.SetActive(false);

        if (waitCor != null) StopCoroutine(waitCor);
        waitCor = StartCoroutine(WaitforFrame());
    }

    IEnumerator WaitforFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);
        var jsonData = JsonManager.popup.StartGame_Texts[0];

        List<TextData> textDatas = new List<TextData>
        {
            jsonData.SG_text1[0],
            jsonData.SG_text2[0],
            jsonData.SG_text3[0],
            jsonData.SG_text4[0],
        };

        for (int i = 0; i < sg_texts.Length; i++)
        {
            if (i > 0) sg_texts[i - 1].gameObject.SetActive(false);

            sg_texts[i].gameObject.SetActive(false);
            LoadManager.Instance.LoadText(textDatas[i], sg_texts[i], networkManager.lang);
            sg_texts[i].gameObject.SetActive(true);

            if (i < textTimer.Length)
                yield return CoroutineData.GetWaitForSeconds(textTimer[i]);
        }
        FirstGame.SetActive(true);
        countdown.SetActive(true);
        gameObject.SetActive(false);
    }
}
