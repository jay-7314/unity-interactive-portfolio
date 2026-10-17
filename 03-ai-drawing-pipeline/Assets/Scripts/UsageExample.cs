using UnityEngine;

public class UsageExample : MonoBehaviour
{
    public Drawing drawing;
    public DrawingMatching drawingMatcher;

    [Header("페이지 전환")]
    [SerializeField] private GameObject paintingPage;
    [SerializeField] private GameObject loadingPage;

    // 중복 제출 방지 플래그
    private bool isSubmitting = false;

    public void OnClick_SubmitDrawing()
    {
        // 이미 제출 중이면 무시 (연타 방지)
        if (isSubmitting) return;
        isSubmitting = true;

        // 1. 페이지 전환 (Painting 비활성화, 로딩 페이지 활성화)
        //    전환(페이드인까지)이 완전히 끝난 뒤 로딩 시작 신호를 보냄
        CommonFunc.instance.ButtonFunc(paintingPage, loadingPage, () =>
        {
            DrawingEvents.RaiseSubmitStarted();
        });

        // 2. 서버 전송 시작
        StartCoroutine(drawing.CaptureRoutine(onSaved: (savedFilePath) =>
        {
            Debug.Log($"그림 저장 완료, 미래형 자동차 생성 시작: {savedFilePath}");
            drawing.Clear();
            StartCoroutine(drawingMatcher.GenerateFutureCar(
                savedFilePath,
                onSuccess: (result) =>
                {
                    Debug.Log($"생성된 이미지 경로: {result.imagePath}");
                    StartCoroutine(drawingMatcher.LoadGeneratedImage(
                        result.imagePath,
                        onLoaded: (tex) =>
                        {
                            isSubmitting = false;
                            DrawingEvents.RaiseResultReady(tex, result.imagePath);
                        },
                        onError: (err) =>
                        {
                            Debug.LogError(err);
                            FailSubmit(err);
                        }
                    ));
                },
                onError: (err) =>
                {
                    Debug.LogWarning($"이미지 생성 실패: {err}");
                    FailSubmit(err);
                }
            ));
        }));
    }

    // 실패 처리 공통 함수: 플래그 해제 + 실패 이벤트 전달
    private void FailSubmit(string err)
    {
        isSubmitting = false;
        DrawingEvents.RaiseMatchFailed(err);
    }
}