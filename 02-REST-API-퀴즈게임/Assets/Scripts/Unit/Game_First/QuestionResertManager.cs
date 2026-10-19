using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionResertManager : MonoBehaviour
{
    [SerializeField] GameObject leftWaitting, rightWaitting;
    [SerializeField] GameObject leftComplete, righrComplete;
    [SerializeField] GameObject firstGame,secondGameStart;
    [SerializeField] LeftBallManager leftBallManager;
    [SerializeField] RightBallManager rightBallManager;

    bool isLeftComplete, isRightComplete;

    private void OnEnable()
    {
        leftWaitting.SetActive(false);
        rightWaitting.SetActive(false);
        leftComplete.SetActive(false);
        righrComplete.SetActive(false);
        isLeftComplete = false;
        isRightComplete = false;
    }

    public void OnLeftWaitting()
    {
        leftWaitting.SetActive(true);
        isLeftComplete = true;
        AllCompleteImg();
    }

    public void OnRightWaitting()
    {
        rightWaitting.SetActive(true);
        isRightComplete = true;
        AllCompleteImg();
    }

    public void AllCompleteImg()
    {
        if (isLeftComplete && isRightComplete)
        {
            leftComplete.SetActive(true);
            righrComplete.SetActive(true);
            StartCoroutine(ResetAfterDelay());
        }
    }

    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        leftWaitting.SetActive(false);
        rightWaitting.SetActive(false);
        leftComplete.SetActive(false);
        righrComplete.SetActive(false);

        if (leftBallManager.RoundIndexLeft >= JsonManager.popup.GameFirstCount &&
            rightBallManager.RoundIndexRight >= JsonManager.popup.GameFirstCount)
        {
            firstGame.SetActive(false);
            secondGameStart.SetActive(true);     // 다음 이미지
            yield break;
        }

        isLeftComplete = false;
        isRightComplete = false;

        leftBallManager.RestartRound();
        rightBallManager.RestartRound();
    }
}
