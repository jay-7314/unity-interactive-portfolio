using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    [SerializeField] GameObject pointerW;

    // Update is called once per frame
    private float minRange, maxRange;

    bool isClusterCaptured = false;

    private void Start()
    {
        var jsondata = JsonManager.math.Commons[0];
        minRange = jsondata.minRange;
        maxRange = jsondata.maxRange;
    }

    private void Update()
    {
        #region 원을 잡음
        Vector3 ballPos = pointerW.transform.position;
        RectTransform canvasRect = transform.parent.GetComponent<RectTransform>();

        float screenX = -(ballPos.z / maxRange/10) * (canvasRect.sizeDelta.x / 2f);
        float screenY = -(ballPos.x / maxRange/10) * (canvasRect.sizeDelta.y / 2f) + canvasRect.sizeDelta.y/2;

        transform.localPosition = new Vector3(screenX, screenY, 0);
        #endregion
    }
}
