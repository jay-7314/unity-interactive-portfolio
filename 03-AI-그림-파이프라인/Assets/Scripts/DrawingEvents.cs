using System;
using UnityEngine;

public static class DrawingEvents
{
    public static event Action OnSubmitStarted;

    // 생성 실패(네트워크 오류, 응답 파싱 실패 등) 시 발생. 이름은 기존과 동일하게 유지.
    public static event Action<string> OnMatchFailed;

    // 이미지 생성 + 로드까지 전부 끝났을 때 딱 한 번 발생
    // (image: 생성된 미래형 자동차 텍스처, imagePath: 저장된 경로/URL)
    public static event Action<Texture2D, string> OnResultReady;

    public static void RaiseSubmitStarted() => OnSubmitStarted?.Invoke();
    public static void RaiseMatchFailed(string error) => OnMatchFailed?.Invoke(error);
    public static void RaiseResultReady(Texture2D image, string imagePath)
        => OnResultReady?.Invoke(image, imagePath);
}