using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIJsonManager : MonoBehaviour
{
    #region Func
    //이미지 받기
    public void LoadRawImage(string path, RawImage rawImage)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, path);
        if (File.Exists(filePath))
        {
            byte[] data = File.ReadAllBytes(filePath);
            Texture2D texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(data);
            rawImage.texture = texture2D;
        }
    }
    //텍스트 변경
    public void LoadText(ITextData textdata, TextMeshProUGUI text)
    {
        if (textdata != null && text != null)
        {
            text.text = textdata.GetText();
            text.fontSize = textdata.GetFontSize();
        }
    }


    #endregion
}
