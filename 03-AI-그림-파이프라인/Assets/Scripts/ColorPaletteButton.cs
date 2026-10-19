using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ColorPaletteButton : MonoBehaviour
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();

        // Inspector에서 normalSprite를 안 채워놨다면 현재 Image에 걸린 sprite를 기본값으로 사용
        if (normalSprite == null)
            normalSprite = image.sprite;
    }

    public void SetSelected(bool selected)
    {
        if (image == null)
            image = GetComponent<Image>();

        image.sprite = selected ? selectedSprite : normalSprite;
        Debug.Log($"[{gameObject.name}] SetSelected({selected}) → sprite={image.sprite?.name}");
    }
}