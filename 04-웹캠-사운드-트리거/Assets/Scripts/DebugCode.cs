using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DebugCode : MonoBehaviour
{
    [SerializeField] RawImage display;
    [SerializeField] RectTransform dotParent;
    [SerializeField] GameObject dotPrefab;
    WebCamTexture camTexture;
    int currentIndex = 0;
    Color32[] data;
    int frameCount = 0;
    List<RectTransform> dots = new List<RectTransform>();
    List<Text> dotTexts = new List<Text>();
    void Start()
    {
        WebCamDevice devices = WebCamTexture.devices[currentIndex];
        camTexture = new WebCamTexture(devices.name);
        display.texture = camTexture;
        camTexture.Play();
        data = new Color32[camTexture.width * camTexture.height];
        for (int i = 0; i < JsonManager.dorime.Position.Count; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotParent);
            RectTransform rt = dot.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0, 1);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            dots.Add(rt);
            Text txt = dot.GetComponentInChildren<Text>();
            dotTexts.Add(txt);
        }
        UpdateDotPositions();
    }
    void UpdateDotPositions()
    {
        float scaleX = dotParent.rect.width / 1920f;
        float scaleY = dotParent.rect.height / 1080f;
        for (int i = 0; i < JsonManager.dorime.Position.Count; i++)
        {
            Vector2 pixelPos = JsonManager.dorime.Position[i].Pos;
            dots[i].anchoredPosition = new Vector2(pixelPos.x * scaleX, pixelPos.y * scaleY);
        }
    }
    private void Update()
    {
        if (camTexture.didUpdateThisFrame)
        {
            camTexture.GetPixels32(data);
        }
        frameCount++;
        if (frameCount % 30 != 0) return;
        for (int i = 0; i < JsonManager.dorime.Position.Count; i++)
        {
            Vector2 pixelPos = JsonManager.dorime.Position[i].Pos;
            int x = Mathf.Clamp((int)(pixelPos.x / 1920f * camTexture.width), 0, camTexture.width - 1);
            int y = Mathf.Clamp((int)(-pixelPos.y / 1080f * camTexture.height), 0, camTexture.height - 1);
            y = camTexture.height - 1 - y;
            int index = y * camTexture.width + x;
            Color32 pixel = data[index];
            //각 prefab의 Text 업데이트
            if (dotTexts[i] != null)
            {
                dotTexts[i].text = $"픽셀({pixelPos.x},{pixelPos.y})\nR:{pixel.r} G:{pixel.g} B:{pixel.b}";
                dotTexts[i].horizontalOverflow = HorizontalWrapMode.Overflow;
                dotTexts[i].verticalOverflow = VerticalWrapMode.Overflow;
                dotTexts[i].color = Color.red;
                dotTexts[i].fontSize = 20;
            }
            //if (pixel.r <= 100 || pixel.g <= 100 || pixel.b <= 100)
            //{
            //    audios.play();
            //}
            Debug.Log($"[{i}] JSON({(int)pixelPos.x}, {(int)pixelPos.y}) | 픽셀({x}, {y}) - R:{pixel.r} G:{pixel.g} B:{pixel.b}");
        }
    }
}