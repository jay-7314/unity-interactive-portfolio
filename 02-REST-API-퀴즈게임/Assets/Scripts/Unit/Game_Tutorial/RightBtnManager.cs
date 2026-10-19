using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RightBtnManager : MonoBehaviour
{
    [SerializeField] RawImage blackbtn, orangebtn, pinkbtn, whitebtn, whiteBluebtn;
    [SerializeField] Texture blackbtn_Off, orangebtn_Off, pinkbtn_Off, whitebtn_Off, whiteBluebtn_Off;
    [SerializeField] Texture blackbtn_On, orangebtn_On, pinkbtn_On, whitebtn_On, whiteBluebtn_On;
    [SerializeField] AudioSource btnSound;

    [SerializeField] FirstTutorial firstTutorial;

 
    public void FirstTutorialRightBtn()
    {
        StartCoroutine(FirstTutoriaRightlBtnCor());
    }

    public IEnumerator FirstTutoriaRightlBtnCor()
    {
        btnSound.Play();
        blackbtn.texture = blackbtn_Off;
        yield return CoroutineData.GetWaitForSeconds(0.1f);
        blackbtn.texture = blackbtn_On;
    }
    public void PressRightBtn(int index)
    {
        StartCoroutine(PressRightBtnCor(index));
    }
    IEnumerator PressRightBtnCor(int index)
    {
        RawImage btn = null;
        Texture offTex = null;
        Texture onTex = null;

        switch (index)
        {
            case 0: btn = whitebtn; offTex = whitebtn_Off; onTex = whitebtn_On; break;
            case 1: btn = whitebtn; offTex = whitebtn_Off; onTex = whitebtn_On; break;
            case 2: btn = blackbtn; offTex = blackbtn_Off; onTex = blackbtn_On; break;
            case 3: btn = orangebtn; offTex = orangebtn_Off; onTex = orangebtn_On; break;
            case 4: btn = whiteBluebtn; offTex = whiteBluebtn_Off; onTex = whiteBluebtn_On; break;
        }

        if (btn == null) yield break;
        btnSound.Play();
        btn.texture = offTex;
        yield return CoroutineData.GetWaitForSeconds(0.1f);
        btn.texture = onTex;
    }

    public void PressRightBtnByGroup(int groupIndex)
    {
        StartCoroutine(PressRightBtnByGroupCor(groupIndex));
    }

    IEnumerator PressRightBtnByGroupCor(int groupIndex)
    {
        RawImage btn = null;
        Texture offTex = null;
        Texture onTex = null;

        switch (groupIndex)
        {
            case 0: btn = blackbtn; offTex = blackbtn_Off; onTex = blackbtn_On; break;
            case 1: btn = orangebtn; offTex = orangebtn_Off; onTex = orangebtn_On; break;
            case 2: btn = pinkbtn; offTex = pinkbtn_Off; onTex = pinkbtn_On; break;
            case 3: btn = whitebtn; offTex = whitebtn_Off; onTex = whitebtn_On; break;
            case 4: btn = whiteBluebtn; offTex = whiteBluebtn_Off; onTex = whiteBluebtn_On; break;
        }

        if (btn == null) yield break;
        btnSound.Play();
        btn.texture = offTex;
        yield return CoroutineData.GetWaitForSeconds(0.1f);
        btn.texture = onTex;
    }
}
