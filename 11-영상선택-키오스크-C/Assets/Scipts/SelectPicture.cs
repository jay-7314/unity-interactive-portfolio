using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPicture : MonoBehaviour
{
    public static SelectPicture instance;
    [SerializeField] GameObject explainPage, selectPage, anotherPage;
    [SerializeField] string[] buttonSave = { "btn1", "btn2", "btn3", "btn4", "btn5" };
    int selectedIndex = -1;

    private void Awake()
    {
        instance = this;
    }

    //버튼 클릭 후 저장하기
    public void OnButtonClicked(int index)
    {
        selectedIndex = index;
    }

    private void OnEnable()
    {
        selectedIndex = -1;
        StartCoroutine(ReturnToTitleCor());
    }

    IEnumerator ReturnToTitleCor()
    {
        yield return CoroutineData.GetWaitForSeconds(JsonManager.busan.GotoTitleTimer);
        CommonFunc.instance.ReturnToTitle();
    }

    public void BtnToNext()
    {
        if(selectedIndex != -1)
        {
            string key = buttonSave[selectedIndex];
            int current = PlayerPrefs.GetInt(key,0);
            PlayerPrefs.SetInt(key, current+1);
            PlayerPrefs.Save();
        }
        selectPage.SetActive(false);
        anotherPage.SetActive(true);

    }

    public void BtnPrevious()
    {
        selectPage.SetActive(false);
        explainPage.SetActive(true);
    }


}
