using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CheckPosition : MonoBehaviour, IPointerClickHandler
{
    public RawImage rawImage;

    public void OnPointerClick(PointerEventData eventData)
    {
        RectTransform rect = rawImage.rectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        Vector2 size = rect.rect.size;
        Vector2 topLeftPoint = new Vector2(localPoint.x + size.x / 2, size.y / 2 - localPoint.y);
        Debug.Log(topLeftPoint);

        WebCamTexture webTex = (WebCamTexture)rawImage.texture;
        int pixelX = (int)(topLeftPoint.x / size.x * webTex.width);
        int pixelY = webTex.height - (int)(topLeftPoint.y / size.y * webTex.height);
        Color color = webTex.GetPixel(pixelX, pixelY);
        Debug.Log($"RGB: ({(int)(color.r * 255)}, {(int)(color.g * 255)}, {(int)(color.b * 255)})");
    }
}