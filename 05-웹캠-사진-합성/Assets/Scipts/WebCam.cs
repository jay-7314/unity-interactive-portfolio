using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WebCam : MonoBehaviour
{
    public RawImage display;
    WebCamTexture camTexture;
    RenderTexture rotatedRT;

    private void OnEnable()
    {
        StartCoroutine(StartCam());
    }

    private IEnumerator StartCam()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (camTexture == null)
        {
            camTexture = new WebCamTexture(WebCamTexture.devices[0].name);
        }
        camTexture.Play();
        yield return new WaitUntil(() => camTexture.width > 16);

        rotatedRT = new RenderTexture(camTexture.height, camTexture.width, 0);
        display.texture = rotatedRT;

        // 좌우 반전 (uvRect X축 반전)
        display.uvRect = new Rect(1f, 0f, -1f, 1f);
    }

    private void Update()
    {
        if (camTexture == null || !camTexture.isPlaying || rotatedRT == null) return;

        Texture2D src = new Texture2D(camTexture.width, camTexture.height, TextureFormat.RGBA32, false);
        src.SetPixels(camTexture.GetPixels());
        src.Apply();
        Texture2D rotated = Rotate90CW(src, camTexture.width, camTexture.height);
        RenderTexture prev = RenderTexture.active;
        Graphics.Blit(rotated, rotatedRT);
        RenderTexture.active = prev;
        Destroy(src);
        Destroy(rotated);
    }

    Texture2D Rotate90CW(Texture2D src, int w, int h)
    {
        Texture2D result = new Texture2D(h, w, TextureFormat.RGBA32, false);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result.SetPixel(h - 1 - y, x, src.GetPixel(x, y));
        result.Apply();
        return result;
    }

    private void OnDisable()
    {
        if (camTexture != null && camTexture.isPlaying) camTexture.Stop();
        if (rotatedRT != null) rotatedRT.Release();
    }
}