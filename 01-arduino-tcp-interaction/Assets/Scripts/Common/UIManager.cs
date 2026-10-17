using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    CommonUI commonUI;

    public enum Pagelist
    {
        title,
        interaction,
        videoPage,
        inducePage,
        picturePage,
    }

    [System.Serializable]
    public class PageMapping
    {
        public Pagelist page;
        public GameObject pageObject;
    }

    [SerializeField] private List<PageMapping> pageMappings = new List<PageMapping>();
    Dictionary<Pagelist, GameObject> pageDic = new Dictionary<Pagelist, GameObject>();
    public Pagelist currentPage;

    private void Awake()
    {
        Instance = this;

        commonUI = FindAnyObjectByType<CommonUI>(FindObjectsInactive.Include);
        if (commonUI != null)
        {
            commonUI.gameObject.SetActive(false);
        }

        //리스트를 딕셔너리로 변환 직렬화
        foreach (var mapping in pageMappings)
        {
            if (mapping.pageObject != null && !pageDic.ContainsKey(mapping.page))
            {
                pageDic.Add(mapping.page, mapping.pageObject);
            }
        }
    }

    public void TargetPage(Pagelist target)
    {
        // title이 아닐 때만 CommonUI 활성화
        if (commonUI != null)
        {
            commonUI.gameObject.SetActive(target != Pagelist.title);
        }

        currentPage = target;
        foreach (var page in pageDic)
        {
            page.Value.SetActive(page.Key == target);
        }
    }
}

