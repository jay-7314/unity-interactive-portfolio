using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PointManager : MonoBehaviour
{
    [SerializeField] Text points, get_Mind;
    public int popupPoint, mindPoint = 0;
    public bool IsCleared => popupPoint >= 60;


    public void AddPoints()
    {
        popupPoint = Mathf.Min(popupPoint + 1, 60);
        if (popupPoint % 12 == 0)
        {
            mindPoint = Mathf.Min(mindPoint + 1, 5);
        }

        if (points != null) points.text = $"성공 :{popupPoint}/60";
        if (get_Mind != null) get_Mind.text = $"마음조각 x{mindPoint}";
    }
}
