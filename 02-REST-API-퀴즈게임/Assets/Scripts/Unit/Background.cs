using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    Image backgroundImg;
    [SerializeField] PlayerGamer playerGamer;
    [SerializeField] PlayGamePlay playgamePlay;
    public GameObject gameStart, mindPiece;
    public Text titleMessage;
    bool isPlayer1, isPlayer2 = false;

    private void Awake()
    {
        backgroundImg = GetComponent<Image>();
    }

    void OnEnable()
    {
        isPlayer1 = false;
        isPlayer2 = false;
        playerGamer.ResetPlayer();
        playgamePlay.ResetPlayer();
        titleMessage.gameObject.SetActive(false);
        StartCoroutine(networkManager.UpdateRestart());
        StartCoroutine(networkManager.GetRoomCheckData());
        StartCoroutine(WaitForFrame());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator WaitForFrame()
    {
        yield return CoroutineData.GetWaitForSeconds(0.5f);
        LoadManager.Instance.LoadText(JsonManager.popup.Title[0].Title_Txt[0], titleMessage, networkManager.lang);
        StartCoroutine(TitleTimer());

        yield return new WaitUntil(() =>
            !string.IsNullOrEmpty(networkManager.leftUser) &&
            !string.IsNullOrEmpty(networkManager.rightUser));

        yield return StartCoroutine(networkManager.GetDetailAndStart(networkManager.leftUser, "left"));
        isPlayer1 = true;

        yield return StartCoroutine(networkManager.GetDetailAndStart(networkManager.rightUser, "right"));
        isPlayer2 = true;

        CheckComfirm();
    }

    void CheckComfirm()
    {
        if (isPlayer1 && isPlayer2)
        {
            titleMessage.gameObject.SetActive(false);
            gameStart.SetActive(true);
            mindPiece.SetActive(true);
            playerGamer.ResetPlayer();
            gameObject.SetActive(false);
        }
    }

    IEnumerator TitleTimer()
    {
        yield return CoroutineData.GetWaitForSeconds(5f);
        titleMessage.gameObject.SetActive(true);
        yield return CoroutineData.GetWaitForSeconds(10f);
        titleMessage.gameObject.SetActive(false);
    }
}