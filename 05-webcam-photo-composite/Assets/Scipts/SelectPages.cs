using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPages : MonoBehaviour
{
    [SerializeField] Texture[] prevImg, clickedImgs;
    [SerializeField] RawImage[] buttons;
    int selectIndex = -1;

    void OnEnable()
    {
        SaveStatus.Instance.imgIndex = -1;
        selectIndex = -1;

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].texture = prevImg[i];
        }
    }

    public void clickedChanges(int index)
    {
        if (selectIndex != -1) buttons[selectIndex].texture = prevImg[selectIndex];
        selectIndex = index;
        buttons[index].texture = clickedImgs[index];
        SaveStatus.Instance.imgIndex = index;
    }
}