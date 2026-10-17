using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Ball : MonoBehaviour
{
    int ballId, factoryIndex;
    IBallManager manager;
    Vector3 moveDir;
    float stepSize, moveTimer;
    bool isReturn, isPopped, canUpdateSpeed = false;
    RectTransform rect, destroyLine;
    GameObject currentPopObj;
    AudioSource popSound;
    public int FactoryIndex => factoryIndex;

    //public System.Action OnStepFinished;
    public System.Action<int> OnMoveComplete;
    public int BallId => ballId;
    public void Init(int ballId, int factoryIndex, IBallManager manager, float moveTimer, Vector3 moveDir, float stepSize, RectTransform destroyLine, AudioSource popSound = null, bool canUpdateSpeed = false)
    {
        isPopped = false;
        isReturn = false;
        this.ballId = ballId;
        this.factoryIndex = factoryIndex;
        this.manager = manager;
        this.moveTimer = moveTimer;
        this.moveDir = moveDir;
        this.stepSize = stepSize;
        this.destroyLine = destroyLine;
        this.popSound = popSound;
        this.canUpdateSpeed = canUpdateSpeed;
        rect = GetComponent<RectTransform>();
        StartCoroutine(MoveStep());
    }
    public void UpdateMoveTimer(float newMoveTimer)
    {
        if (!canUpdateSpeed) return;
        moveTimer = newMoveTimer;
    }

    IEnumerator MoveStep()
    {
        int stepPoints = 6;
        for (int i = 0; i < stepPoints; i++)
        {
            Vector3 startPos = rect.position;
            Vector3 targetPos = rect.position + moveDir * stepSize;
            float elapsed = 0f;
            while (elapsed < moveTimer)
            {
                elapsed += Time.deltaTime;
                rect.position = Vector3.Lerp(startPos, targetPos, elapsed / moveTimer);
                if (IsOverlapping(rect, destroyLine))
                {
                    manager.StartWarning();
                    if (manager.IsFailed)
                    {
                        rect.position = new Vector3(startPos.x, startPos.y - 36f, startPos.z);
                        yield break;
                    }
                }
                yield return null;
            }
            rect.position = targetPos;
            OnMoveComplete?.Invoke(factoryIndex);

            //yield return null;
        }
        ReturnToPool();
    }

    bool IsOverlapping(RectTransform a, RectTransform b)
    {
        Vector3[] cornersA = new Vector3[4];
        Vector3[] cornersB = new Vector3[4];

        a.GetWorldCorners(cornersA);
        b.GetWorldCorners(cornersB);

        Rect rectA = new Rect(cornersA[0], cornersA[2] - cornersA[0]);
        Rect rectB = new Rect(cornersB[0], cornersB[2] - cornersB[0]);

        return rectA.Overlaps(rectB);
    }

    public void TryPop(GameObject popObj, float fadeDuration = 0.5f, System.Action onFadeComplete = null, System.Action onPopReturn = null)
    {
        if (isPopped || isReturn) return;
        isPopped = true;
        StopAllCoroutines();
        if (popSound != null)
            popSound.Play();
        StartCoroutine(PopAndFade(popObj, fadeDuration, onFadeComplete, onPopReturn));
    }
    IEnumerator PopAndFade(GameObject popObj, float fadeDuration, System.Action onFadeComplete, System.Action onPopReturn)
    {
        currentPopObj = popObj;
        rect.position = new Vector3(-9999, -9999, 0);
        CanvasGroup canvasGroup = popObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = popObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        onFadeComplete?.Invoke(); 
        yield return null;
        onPopReturn?.Invoke();  
        currentPopObj = null;
        ReturnToPool();
    }
    void ReturnToPool()
    {
        if (isReturn) return;
        isReturn = true;
        manager.ReturnBall(ballId, factoryIndex, this);
    }

    public void ChangeTexture(Texture newTexture)
    {
        RawImage img = GetComponent<RawImage>();
        if (img != null) img.texture = newTexture;
    }


    void OnDestroy()
    {
        StopAllCoroutines();
    }
}