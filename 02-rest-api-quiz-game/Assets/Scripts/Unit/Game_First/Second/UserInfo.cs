using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfo : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    public Text leftPerson_Text, rightPerson_Text;
    [SerializeField] Image leftPerson_Circle, rightPerson_Circle;
    string baseLeftText, baseRightText;


    private void OnEnable()
    {
        StartCoroutine(WaitForFrame());
    }

    IEnumerator WaitForFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(0.1f);
        var jsondata = JsonManager.popup.Tutorial_First_Text[0];
        LoadManager.Instance.LoadSmallCircleImage(jsondata.ImageSC[0], leftPerson_Circle, networkManager.leftColor);
        LoadManager.Instance.LoadSmallCircleImage(jsondata.ImageSC[0], rightPerson_Circle, networkManager.rightColor);

        LoadManager.Instance.LoadText(jsondata.NamePrep[0], leftPerson_Text, networkManager.lang);
        LoadManager.Instance.LoadText(jsondata.NamePrep[0], rightPerson_Text, networkManager.lang);
        baseLeftText = leftPerson_Text.text;
        baseRightText = rightPerson_Text.text;
        ChangeTextCount();
    }

    public void ChangeTextCount()
    {
        leftPerson_Text.text = $"{networkManager.leftName}{baseLeftText}";
        rightPerson_Text.text = $"{networkManager.rightName}{baseRightText}";
    }
}
