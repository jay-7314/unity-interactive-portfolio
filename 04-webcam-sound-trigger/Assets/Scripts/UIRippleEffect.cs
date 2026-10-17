using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRippleEffect : MonoBehaviour, IPointerClickHandler
{
    [Header("Ripple Setup")]
    public Sprite m_EffectSprite;       // 클릭시 퍼져나갈 효과의 모양
    public Color rippleColor;           // 클릭시 퍼져나갈 효과의 색상
    public float maxPower = 0.25f;      // 처음 Alpha값
    public float duration = 0.25f;      // Effect 지속시간
    public Vector2 rippleSize = Vector2.one; // Effect 최대크기
    public AudioSource audio;

    bool m_IsInitialized = false;
    RectMask2D m_RectMask;

    private void Start()
    {
        SetupRippleContainer();
    }

    void SetupRippleContainer()
    {
        m_RectMask = gameObject.GetComponent<RectMask2D>();
        if (m_RectMask == null)
            m_RectMask = gameObject.AddComponent<RectMask2D>();

        m_RectMask.padding = new Vector4(5, 5, 5, 5);
        m_RectMask.softness = new Vector2Int(20, 20);
        m_IsInitialized = true;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (audio.isPlaying) return;
        audio.Play();
        if (!m_IsInitialized) return;

        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        GameObject rippleObject = new GameObject("_ripple_");
        Image currentRippleImage = rippleObject.AddComponent<Image>();
        currentRippleImage.sprite = m_EffectSprite;
        currentRippleImage.transform.SetParent(rootCanvas.transform);
        currentRippleImage.transform.SetAsLastSibling();
        currentRippleImage.color = new Color(rippleColor.r, rippleColor.g, rippleColor.b, 0f);
        currentRippleImage.raycastTarget = false;

        RectTransform rt = rippleObject.GetComponent<RectTransform>();
        rt.sizeDelta = Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        rt.anchoredPosition = localPoint;

        StartCoroutine(AnimateRipple(rt, currentRippleImage, () =>
        {
            Destroy(rippleObject);
        }));
    }

    IEnumerator AnimateRipple(RectTransform rippleTransform, Image rippleImage, Action OnComplete)
    {
        Vector2 initialSize = Vector2.zero;
        Vector2 targetSize = new Vector2(rippleSize.x, rippleSize.x); // 원형 유지
        Color initialColor = new Color(rippleColor.r, rippleColor.g, rippleColor.b, maxPower);
        Color targetColor = new Color(rippleColor.r, rippleColor.g, rippleColor.b, 0f);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            rippleTransform.sizeDelta = Vector2.Lerp(initialSize, targetSize, t);
            rippleImage.color = Color.Lerp(initialColor, targetColor, t);
            yield return null;
        }

        OnComplete?.Invoke();
    }
}