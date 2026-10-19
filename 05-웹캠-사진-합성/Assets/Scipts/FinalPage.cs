using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalPage : MonoBehaviour
{
    [SerializeField] GameObject finalPage, titlePage;
    [SerializeField] RawImage resultImg2;
    [SerializeField] PageController pageController;
    public void notGoodBtn()
    {
        SeperatePicture.CroppedResult = null;
        finalPage.SetActive(false);
        titlePage.SetActive(true);
        pageController.currentIndex = 0;
    }

    public void GoodBtn()
    {
        if (SeperatePicture.CroppedResult != null && resultImg2 != null)
        {
            resultImg2.texture = SeperatePicture.CroppedResult;
            finalPage.SetActive(false);
            titlePage.SetActive(true);
            pageController.currentIndex = 0;

        }
    }
}
