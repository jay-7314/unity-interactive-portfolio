using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CountryClick : MonoBehaviour, IPointerClickHandler
{

    private Button button;
    private Color normalColor;
    private Color pressedColor;

    private string countryId;

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        normalColor = button.colors.normalColor;
        pressedColor = button.colors.pressedColor;
    }

    public void SetId(string id)
    {
        countryId = id;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(ChangeClickColor());
    }
    IEnumerator ChangeClickColor()
    {
        button.targetGraphic.color = pressedColor;
        AudioManager.Instance.PlaySound(SelectJsonManager.line.Upline.items[0].btnSound);
        AudioManager.Instance.PlaySound(SelectJsonManager.line.Downline.items[0].btnSound);
        yield return new WaitForSeconds(0.1f);
        button.targetGraphic.color = normalColor;
        FindObjectOfType<MainDisplayManager_country>().ApplyCountry(countryId);
    }
}
