using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionsPopup : MonoBehaviour
{
    [SerializeField] GameObject[] starImgPrefabs;
    GameObject questionsObj;

    public void Init(GameObject questionsObj)
    {
        this.questionsObj = questionsObj;
        ClearStars();
    }

    void ClearStars()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    public Texture ShowStar(int index)
    {
        ClearStars();
        if (index >= starImgPrefabs.Length) return null;
        GameObject obj = Instantiate(starImgPrefabs[index], transform);
        obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        RawImage prefabImage = starImgPrefabs[index].GetComponent<RawImage>();
        return prefabImage != null ? prefabImage.texture : null;
    }
}
