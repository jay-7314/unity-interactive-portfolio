using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// ForthPage(선택 화면) <-> FifthPage(상세 화면)를 연결하는 컨트롤러
// 흐름: Forth_X 클릭 -> FifthPage_X 표시 -> 확인(Check) 클릭 -> ForthPage 복귀,
//       Forth_X는 클릭 불가 + Off 이미지로 교체
// 타이틀 등으로 나가서 ForthPage/FifthPage가 모두 꺼지면 진행 상태를 자동 초기화

[System.Serializable]
public class MethodItem
{
    [Header("ForthPage 쪽")]
    public Button selectButton;      // Forth_First
    public RawImage selectImage;     // Forth_First의 Raw Image (비워두면 자동으로 찾음)
    public Texture doneTexture;      // 완료 후 바뀔 이미지 (First_Off 등)

    [Header("FifthPage 쪽")]
    public GameObject detailPage;    // FifthPage_First
    public Button confirmButton;     // FifthPage_First 아래의 Check(확인) 버튼

    [HideInInspector] public bool done;
    [HideInInspector] public Texture originalTexture;
}

public class MethodSelectController : MonoBehaviour
{
    [Header("페이지")]
    public GameObject forthPage;     // ForthPage
    public GameObject fifthPage;     // FifthPage

    [Header("항목 (1, 2, 3번 순서대로)")]
    public MethodItem[] items;

    [Header("세 항목을 모두 완료했을 때")]
    public UnityEvent onAllCompleted;

    private void Awake()
    {
        for (int i = 0; i < items.Length; i++)
        {
            int index = i; // 클로저용 복사

            MethodItem item = items[i];

            if (item.selectImage == null && item.selectButton != null)
                item.selectImage = item.selectButton.GetComponent<RawImage>();

            if (item.selectImage != null)
                item.originalTexture = item.selectImage.texture;

            item.selectButton.onClick.AddListener(() => OpenDetail(index));
            item.confirmButton.onClick.AddListener(() => Confirm(index));
        }
    }

    private void Update()
    {
        // ForthPage, FifthPage가 둘 다 꺼져 있으면 -> 선택 화면을 벗어난 것(타이틀 등)
        if (!forthPage.activeInHierarchy && !fifthPage.activeInHierarchy)
        {
            if (AnyDone()) ResetProgress();
        }
    }

    // Forth_X를 눌렀을 때: 해당 FifthPage_X만 보여줌
    private void OpenDetail(int index)
    {
        if (items[index].done) return;

        forthPage.SetActive(false);
        fifthPage.SetActive(true);

        for (int i = 0; i < items.Length; i++)
            items[i].detailPage.SetActive(i == index);
    }

    // 확인 버튼: 완료 처리 후 ForthPage로 복귀
    private void Confirm(int index)
    {
        MethodItem item = items[index];
        item.done = true;

        item.selectButton.interactable = false;
        if (item.selectImage != null && item.doneTexture != null)
            item.selectImage.texture = item.doneTexture;

        item.detailPage.SetActive(false);
        fifthPage.SetActive(false);
        forthPage.SetActive(true);

        if (AllDone())
            onAllCompleted.Invoke();
    }

    private bool AllDone()
    {
        foreach (var item in items)
            if (!item.done) return false;
        return true;
    }

    private bool AnyDone()
    {
        foreach (var item in items)
            if (item.done) return true;
        return false;
    }

    // 처음부터 다시 시작할 때 호출 (타이틀로 나갈 때 자동 호출됨)
    public void ResetProgress()
    {
        foreach (var item in items)
        {
            item.done = false;
            item.selectButton.interactable = true;
            if (item.selectImage != null && item.originalTexture != null)
                item.selectImage.texture = item.originalTexture;
            item.detailPage.SetActive(false);
        }
    }
}