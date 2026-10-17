using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeftBtnManager : MonoBehaviour
{
    [SerializeField] RawImage redbtn, bluebtn, yellowbtn, greenbtn, brownbtn;
    [SerializeField] Texture redbtn_Off, bluebtn_Off, yellowbtn_Off, greenbtn_Off, brownbtn_Off;
    [SerializeField] Texture redbtn_On, bluebtn_On, yellowbtn_On, greenbtn_On, brownbtn_On;

    [SerializeField] FirstTutorial firstTutorial;
    [SerializeField] AudioSource btnSound;

    public void FirstTutorialLeftBtn()
    {
        StartCoroutine(FirstTutorialLeftBtnCor());
    }

    public IEnumerator FirstTutorialLeftBtnCor()
    {
        btnSound.Play();
        bluebtn.texture = bluebtn_Off;
        yield return CoroutineData.GetWaitForSeconds(0.1f);
        bluebtn.texture = bluebtn_On;
    }

    public void PressLeftBtn(int index)
    {
        StartCoroutine(PressLeftBtnCor(index));
    }

    public IEnumerator PressLeftBtnCor(int index)
    {
        RawImage btn = null;
        Texture offTex = null;
        Texture onTex = null;

        switch (index)
        {
            case 0: btn = yellowbtn; offTex = yellowbtn_Off; onTex = yellowbtn_On; break;
            case 1: btn = bluebtn; offTex = bluebtn_Off; onTex = bluebtn_On; break;
            case 2: btn = greenbtn; offTex = greenbtn_Off; onTex = greenbtn_On; break;
            case 3: btn = redbtn; offTex = redbtn_Off; onTex = redbtn_On; break;
            case 4: btn = redbtn; offTex = redbtn_Off; onTex = redbtn_On; break;
        }

        if (btn == null) yield break;
        btnSound.Play();
        btn.texture = offTex;
        yield return CoroutineData.GetWaitForSeconds(0.1f);
        btn.texture = onTex;
    }

    public void PressLeftBtnByGroup(int groupIndex)
    {
        StartCoroutine(PressLeftBtnByGroupCor(groupIndex));
    }

    public IEnumerator PressLeftBtnByGroupCor(int groupIndex)
    {
        RawImage btn = null;
        Texture offTex = null;
        Texture onTex = null;

        switch (groupIndex)
        {
            case 0: btn = redbtn; offTex = redbtn_Off; onTex = redbtn_On; break;
            case 1: btn = bluebtn; offTex = bluebtn_Off; onTex = bluebtn_On; break;
            case 2: btn = yellowbtn; offTex = yellowbtn_Off; onTex = yellowbtn_On; break;
            case 3: btn = greenbtn; offTex = greenbtn_Off; onTex = greenbtn_On; break;
            case 4: btn = brownbtn; offTex = brownbtn_Off; onTex = brownbtn_On; break;
        }

        if (btn == null) yield break;
        btnSound.Play();
        btn.texture = offTex;
        yield return CoroutineData.GetWaitForSeconds(0.1f);
        btn.texture = onTex;
    }
}
