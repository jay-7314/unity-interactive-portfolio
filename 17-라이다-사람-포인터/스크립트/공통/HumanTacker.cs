using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanTacker : MonoBehaviour
{
    [SerializeField] Transform pointerW;                            //내가 사람이라고 생각할 부분
    [SerializeField] float clusterCircle = 10f;                    //사람의 덩어리 사이즈
    [SerializeField] int minClusterPoints = 10;                     //사람의 덩어리 클러스터 갯수
    [SerializeField] int lostFrame = 20;                            //20프레임이 없어져도 사람이라고 판단

    Vector3 trackingCenter;                                         //사람의 중심좌표
    bool isTracking;                                                //추적중인지 판단
    int lostFrameCount = 0;                                         //사람인지 아닌지 판단할때 사용되는 프레임 카운트 초기화
    
    public void ProcessWorldPoints(List<Vector3> points)
    {
        if(points.Count == 0)
        {
            if (isTracking)
            {
                lostFrameCount++;
                if (lostFrameCount < lostFrame) return;
                isTracking = false;
            }
            return;
        }
        //클러스터는 사람 1개만 들어오니까 그냥 상관 ㄴㄴ
        Vector3 bestCenter = Vector3.zero;                              //단순히 초기값 넣은거임

        int bestCount = 0;

        bool[] used = new bool[points.Count];

        for(int i = 0; i< points.Count; i++)
        {
            if (used[i]) continue;
            Vector3 sum = Vector3.zero;
            int count = 0;

            for(int j = 0; j<points.Count; j++)
            {
                if (used[j]) continue;
                float dist = Vector3.Distance(points[i], points[j]);

                if (dist < clusterCircle)
                {
                    sum += points[j];
                    count++;
                    used[j] = true;
                }
            }

            if (count > minClusterPoints)
            {
                Vector3 center = sum / count;
                if (count > bestCount)
                {
                    bestCount = count;
                    bestCenter = center;
                }
            }
        }

        if(bestCount > 0)
        {
            isTracking = true;
            trackingCenter = bestCenter;
            pointerW.position = trackingCenter;
            lostFrameCount = 0;
        }

        else
        {
            if (isTracking)
            {
                lostFrameCount++;
                if (lostFrameCount < lostFrame)
                {
                    pointerW.position = trackingCenter;
                }
                else
                {
                    isTracking = false;
                }
            }
        }
    }
}
