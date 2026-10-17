using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class DrawingMatching : MonoBehaviour
{
    [Header("WaveSpeed API 설정")]
    public string apiKey = "YOUR_WAVESPEED_API_KEY";

    private const string ModelPath = "alibaba/wan-2.7/image-edit";
    private const string ApiBase = "https://api.wavespeed.ai/api/v3";
    private const string UploadEndpoint = ApiBase + "/media/upload/binary";
    private const string BgRemoverModel = "wavespeed-ai/image-background-remover";
    public int maxImageSize = 512;
    public bool useJpeg = false;
    public int jpegQuality = 75;

    [Header("로컬 서버 전송 설정")]
    public string serverHost;
    public int serverPort = 9999;
    public float pollIntervalSeconds = 1.5f;
    public int maxPollAttempts = 1000;

    public string generatedFolderName = "GeneratedCars";

    [Header("저장 개수 제한")]
    public int maxSavedImages = 5;

    [Header("출력 비율 설정")]
    public bool forceAspectRatio = true;
    public int targetAspectWidth = 3;
    public int targetAspectHeight = 2;

    [Serializable]
    public class GenerationResult
    {
        public string imagePath;
    }
    public IEnumerator GenerateFutureCar(string pngFilePath, Action<GenerationResult> onSuccess, Action<string> onError)
    {
        byte[] fileBytes = File.ReadAllBytes(pngFilePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileBytes);
        yield return GenerateFutureCar(tex, onSuccess, onError);
        Destroy(tex);
    }

    public IEnumerator GenerateFutureCar(Texture2D userDrawing, Action<GenerationResult> onSuccess, Action<string> onError)
    {
        // 로그: 전체 파이프라인 시작 시 1번만 call

        Texture2D resized = ResizeTexture(userDrawing, maxImageSize);
        byte[] imageBytes;
        string mimeType;
        string fileName;

        if (useJpeg)
        {
            imageBytes = resized.EncodeToJPG(jpegQuality);
            mimeType = "image/jpeg";
            fileName = "drawing.jpg";
        }
        else
        {
            imageBytes = resized.EncodeToPNG();
            mimeType = "image/png";
            fileName = "drawing.png";
        }

        if (resized != userDrawing) Destroy(resized);

        string uploadedImageUrl = null;
        string uploadError = null;

        yield return UploadImage(imageBytes, mimeType, fileName,
            onSuccess: url => uploadedImageUrl = url,
            onError: err => uploadError = err);

        if (!string.IsNullOrEmpty(uploadError) || string.IsNullOrEmpty(uploadedImageUrl))
        {
            Logger.Instance?.SendLog("Return fail");
            onError?.Invoke($"이미지 업로드 실패: {uploadError}");
            yield break;
        }

        string prompt =
            "참고 그림을 토대로 미래에 있을법한 자동차를 상상력이 있게 그려줘. " +
            "꼭 현대적인 자동차의 이미지가 아니어도 되고, 공기역학적인 구조를 반드시 지킬필요는 없어. " +
            "반드시 바퀴가 있거나 하지 않아도 되고, 있어도 되고 그건 자유야. " +
            "하지만 참조 이미지의 형태는 따라갔으면 좋겠어.";

        string genRequestId = null;
        string submitError = null;

        yield return SubmitEditTask(prompt, uploadedImageUrl,
            onSuccess: id => genRequestId = id,
            onError: err => submitError = err);

        if (!string.IsNullOrEmpty(submitError) || string.IsNullOrEmpty(genRequestId))
        {
            Logger.Instance?.SendLog("Return fail");
            onError?.Invoke($"이미지 생성 작업 제출 실패: {submitError}");
            yield break;
        }

        string generatedImageUrl = null;
        string pollError = null;

        yield return PollForResult(genRequestId,
            onSuccess: url => generatedImageUrl = url,
            onError: err => pollError = err);

        if (!string.IsNullOrEmpty(pollError) || string.IsNullOrEmpty(generatedImageUrl))
        {
            Logger.Instance?.SendLog("Return fail");
            onError?.Invoke($"이미지 생성 결과 수신 실패: {pollError}");
            yield break;
        }

        string bgRequestId = null;
        string bgSubmitError = null;

        yield return SubmitBgRemoveTask(generatedImageUrl,
            onSuccess: id => bgRequestId = id,
            onError: err => bgSubmitError = err);

        if (!string.IsNullOrEmpty(bgSubmitError) || string.IsNullOrEmpty(bgRequestId))
        {
            Logger.Instance?.SendLog("Return fail");
            onError?.Invoke($"배경제거 작업 제출 실패: {bgSubmitError}");
            yield break;
        }

        string bgRemovedImageUrl = null;
        string bgPollError = null;

        yield return PollForResult(bgRequestId,
            onSuccess: url => bgRemovedImageUrl = url,
            onError: err => bgPollError = err);

        if (!string.IsNullOrEmpty(bgPollError) || string.IsNullOrEmpty(bgRemovedImageUrl))
        {
            Logger.Instance?.SendLog("Return fail");
            onError?.Invoke($"배경제거 결과 수신 실패: {bgPollError}");
            yield break;
        }

        byte[] resultBytes = null;
        string downloadError = null;

        yield return DownloadBytes(bgRemovedImageUrl,
            onSuccess: bytes => resultBytes = bytes,
            onError: err => downloadError = err);

        if (resultBytes == null)
        {
            Logger.Instance?.SendLog("Return fail");
            onError?.Invoke($"결과 이미지 다운로드 실패: {downloadError}");
            yield break;
        }

        // ▼ 후처리: 3:2(또는 지정된) 비율 캔버스에 투명 배경으로 합성
        byte[] finalBytes = resultBytes;

        if (forceAspectRatio)
        {
            Texture2D bgRemovedTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            bgRemovedTex.LoadImage(resultBytes); // PNG 알파 채널 포함 로드

            Texture2D composited = CompositeOntoTransparentCanvas(
                bgRemovedTex, targetAspectWidth, targetAspectHeight);

            finalBytes = composited.EncodeToPNG();

            Destroy(bgRemovedTex);
            Destroy(composited);
        }

        string savedPath = SaveGeneratedImage(finalBytes, "image/png");
        yield return SendToServer(finalBytes);

        // 로그: 전체 파이프라인 성공 종료 시 1번만 return
        Logger.Instance?.SendLog("Return OK");

        onSuccess?.Invoke(new GenerationResult { imagePath = savedPath });
    }

    private IEnumerator UploadImage(byte[] imageBytes, string mimeType, string fileName,
        Action<string> onSuccess, Action<string> onError)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageBytes, fileName, mimeType);

        using (UnityWebRequest req = UnityWebRequest.Post(UploadEndpoint, form))
        {
            req.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"{req.responseCode} {req.error} / {req.downloadHandler.text}");
                yield break;
            }

            string url = ExtractJsonStringField(req.downloadHandler.text, "download_url");
            if (string.IsNullOrEmpty(url))
            {
                onError?.Invoke($"업로드 응답에서 download_url을 찾지 못했습니다: {req.downloadHandler.text}");
                yield break;
            }

            onSuccess?.Invoke(url);
        }
    }

    private IEnumerator SubmitEditTask(string prompt, string imageUrl,
     Action<string> onSuccess, Action<string> onError)
    {
        string jsonBody = "{"
            + $"\"prompt\": \"{EscapeJson(prompt)}\","
            + $"\"images\": [\"{EscapeJson(imageUrl)}\"],"
            + "\"seed\": -1"
            + "}";

        string url = $"{ApiBase}/{ModelPath}";

        yield return PostJson(url, jsonBody, onSuccess, onError);
    }

    private IEnumerator SubmitBgRemoveTask(string imageUrl,
        Action<string> onSuccess, Action<string> onError)
    {
        string jsonBody = "{"
            + $"\"image\": \"{EscapeJson(imageUrl)}\","
            + "\"enable_base64_output\": false"
            + "}";

        string url = $"{ApiBase}/{BgRemoverModel}";

        yield return PostJson(url, jsonBody, onSuccess, onError);
    }

    private IEnumerator PostJson(string url, string jsonBody,
        Action<string> onSuccess, Action<string> onError)
    {
        const int maxRetries = 3;
        const float rateLimitWait = 62f;

        for (int retry = 0; retry < maxRetries; retry++)
        {
            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                yield return req.SendWebRequest();

                if (req.responseCode == 429)
                {
                    Debug.LogWarning($"[DrawingMatching] Rate limit 도달. {rateLimitWait}초 후 재시도... ({retry + 1}/{maxRetries})");
                    yield return new WaitForSeconds(rateLimitWait);
                    continue;
                }

                if (req.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"{req.responseCode} {req.error} / {req.downloadHandler.text}");
                    yield break;
                }

                string id = ExtractJsonStringField(req.downloadHandler.text, "id");
                if (string.IsNullOrEmpty(id))
                {
                    onError?.Invoke($"작업 제출 응답에서 id를 찾지 못했습니다: {req.downloadHandler.text}");
                    yield break;
                }

                onSuccess?.Invoke(id);
                yield break;
            }
        }

        onError?.Invoke("Rate limit 재시도 횟수를 초과했습니다. 잠시 후 다시 시도해주세요.");
    }

    private IEnumerator PollForResult(string requestId,
        Action<string> onSuccess, Action<string> onError)
    {
        string resultUrl = $"{ApiBase}/predictions/{requestId}/result";

        for (int attempt = 0; attempt < maxPollAttempts; attempt++)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(resultUrl))
            {
                req.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"{req.responseCode} {req.error} / {req.downloadHandler.text}");
                    yield break;
                }

                string responseText = req.downloadHandler.text;
                string status = ExtractJsonStringField(responseText, "status");

                if (status == "completed" || status == "complete")
                {
                    string outputUrl = ExtractFirstOutputUrl(responseText);
                    if (string.IsNullOrEmpty(outputUrl))
                    {
                        onError?.Invoke($"완료됐지만 outputs를 찾지 못했습니다: {responseText}");
                        yield break;
                    }
                    onSuccess?.Invoke(outputUrl);
                    yield break;
                }

                if (status == "failed")
                {
                    string errorMsg = ExtractJsonStringField(responseText, "error");
                    onError?.Invoke($"작업 실패: {errorMsg}");
                    yield break;
                }
            }

            yield return new WaitForSeconds(pollIntervalSeconds);
        }

        onError?.Invoke("작업이 제한 시간 내에 완료되지 않았습니다 (타임아웃).");
    }

    private IEnumerator DownloadBytes(string url,
        Action<byte[]> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"{req.responseCode} {req.error}");
                yield break;
            }

            onSuccess?.Invoke(req.downloadHandler.data);
        }
    }

    public IEnumerator LoadGeneratedImage(string pathOrUrl,
        Action<Texture2D> onLoaded, Action<string> onError = null)
    {
        string url = pathOrUrl.Contains("://") ? pathOrUrl : "file://" + pathOrUrl;

        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(req);
                onLoaded?.Invoke(tex);
                yield break;
            }

            onError?.Invoke($"생성 이미지 로드 실패: {pathOrUrl} / {req.error}");
        }
    }

    private IEnumerator SendToServer(byte[] imageBytes)
    {
        bool done = false;
        string sendError = null;

        System.Threading.Thread thread = new System.Threading.Thread(() =>
        {
            try
            {
                using (System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient())
                {
                    client.Connect(serverHost, serverPort);
                    using (System.Net.Sockets.NetworkStream stream = client.GetStream())
                    {
                        byte[] lengthHeader = BitConverter.GetBytes(imageBytes.Length);
                        stream.Write(lengthHeader, 0, 4);
                        stream.Write(imageBytes, 0, imageBytes.Length);
                        stream.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                sendError = e.Message;
            }
            finally
            {
                done = true;
            }
        });

        thread.IsBackground = true;
        thread.Start();

        while (!done)
            yield return null;

        if (!string.IsNullOrEmpty(sendError))
            Debug.LogWarning($"[DrawingMatching] 서버 전송 실패 (이미지 생성은 정상): {sendError}");
        else
            Debug.Log($"[DrawingMatching] 서버 전송 완료 ({imageBytes.Length} bytes → {serverHost}:{serverPort})");
    }

    private void Start()
    {
        serverHost = JsonManager.makeCar.IPAddress;
        Debug.Log($"[DrawingMatching] 서버 IP: {serverHost}");
    }

    private string SaveGeneratedImage(byte[] imageBytes, string mimeType)
    {
        string ext = mimeType.Contains("jpeg") || mimeType.Contains("jpg") ? "jpg" : "png";

        string folderPath = Path.Combine(Application.persistentDataPath, generatedFolderName);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fileName = $"future_car_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.{ext}";
        string fullPath = Path.Combine(folderPath, fileName);

        File.WriteAllBytes(fullPath, imageBytes);
        Debug.Log($"생성된 미래 자동차 이미지 저장 완료: {fullPath}");

        EnforceMaxSavedFiles(folderPath, "future_car_*.*", maxSavedImages);

        return fullPath;
    }
    private void EnforceMaxSavedFiles(string folderPath, string searchPattern, int maxCount)
    {
        if (!Directory.Exists(folderPath))
            return;

        var files = new DirectoryInfo(folderPath)
            .GetFiles(searchPattern)
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .ToList();

        for (int i = maxCount; i < files.Count; i++)
        {
            try
            {
                files[i].Delete();
                Debug.Log($"[정리] 오래된 생성 이미지 삭제: {files[i].Name}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"파일 삭제 실패: {files[i].FullName}, {e.Message}");
            }
        }
    }
    private Texture2D ResizeTexture(Texture2D source, int maxSize)
    {
        if (source.width <= maxSize && source.height <= maxSize)
            return source;

        float scale = Mathf.Min((float)maxSize / source.width, (float)maxSize / source.height);
        int newWidth = Mathf.Max(1, Mathf.RoundToInt(source.width * scale));
        int newHeight = Mathf.Max(1, Mathf.RoundToInt(source.height * scale));

        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D resized = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        resized.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        resized.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return resized;
    }

    /// <summary>
    /// source 이미지를 지정한 aspectW:aspectH 비율의 캔버스 중앙에 배치합니다.
    /// 캔버스의 빈 영역은 완전 투명(alpha = 0)으로 채워지며,
    /// 원본 픽셀의 알파 채널은 블렌딩 없이 그대로 유지됩니다.
    /// </summary>
    private Texture2D CompositeOntoTransparentCanvas(Texture2D source, int aspectW, int aspectH)
    {
        float targetRatio = (float)aspectW / aspectH;
        float sourceRatio = (float)source.width / source.height;

        int canvasWidth, canvasHeight;

        if (sourceRatio > targetRatio)
        {
            canvasWidth = source.width;
            canvasHeight = Mathf.Max(1, Mathf.RoundToInt(source.width / targetRatio));
        }
        else
        {
            canvasHeight = source.height;
            canvasWidth = Mathf.Max(1, Mathf.RoundToInt(source.height * targetRatio));
        }

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color[] canvasPixels = new Color[canvasWidth * canvasHeight];
        for (int i = 0; i < canvasPixels.Length; i++)
            canvasPixels[i] = clear;

        int offsetX = (canvasWidth - source.width) / 2;
        int offsetY = (canvasHeight - source.height) / 2;

        Color[] sourcePixels = source.GetPixels();

        for (int y = 0; y < source.height; y++)
        {
            int canvasRowStart = (offsetY + y) * canvasWidth + offsetX;
            int sourceRowStart = y * source.width;

            for (int x = 0; x < source.width; x++)
            {
                canvasPixels[canvasRowStart + x] = sourcePixels[sourceRowStart + x];
            }
        }

        Texture2D canvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        canvas.SetPixels(canvasPixels);
        canvas.Apply();

        return canvas;
    }

    private static string ExtractJsonStringField(string json, string key)
    {
        try
        {
            string pattern = $"\"{key}\"";
            int keyIndex = json.IndexOf(pattern);
            if (keyIndex < 0) return null;

            int colonIndex = json.IndexOf(':', keyIndex);
            if (colonIndex < 0) return null;

            int firstQuote = json.IndexOf('"', colonIndex + 1);
            if (firstQuote < 0) return null;

            int cursor = firstQuote + 1;
            StringBuilder sb = new StringBuilder();

            while (cursor < json.Length)
            {
                char c = json[cursor];
                if (c == '\\' && cursor + 1 < json.Length)
                {
                    char next = json[cursor + 1];
                    switch (next)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        default: sb.Append(next); break;
                    }
                    cursor += 2;
                    continue;
                }
                if (c == '"') break;
                sb.Append(c);
                cursor++;
            }

            return sb.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON 필드({key}) 파싱 중 예외: {e.Message}");
            return null;
        }
    }

    private static string ExtractFirstOutputUrl(string json)
    {
        try
        {
            int outputsKeyIndex = json.IndexOf("\"outputs\"");
            if (outputsKeyIndex < 0) return null;

            int bracketIndex = json.IndexOf('[', outputsKeyIndex);
            if (bracketIndex < 0) return null;

            int firstQuote = json.IndexOf('"', bracketIndex);
            if (firstQuote < 0) return null;

            int cursor = firstQuote + 1;
            StringBuilder sb = new StringBuilder();

            while (cursor < json.Length)
            {
                char c = json[cursor];
                if (c == '\\' && cursor + 1 < json.Length)
                {
                    char next = json[cursor + 1];
                    sb.Append(next);
                    cursor += 2;
                    continue;
                }
                if (c == '"') break;
                sb.Append(c);
                cursor++;
            }

            return sb.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError($"outputs 파싱 중 예외: {e.Message}");
            return null;
        }
    }

    private static string EscapeJson(string s)
    {
        return s.Replace("\\", "\\\\")
                 .Replace("\"", "\\\"")
                 .Replace("\n", "\\n")
                 .Replace("\r", "\\r");
    }
}