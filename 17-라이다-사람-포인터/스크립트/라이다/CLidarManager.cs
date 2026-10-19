using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CLidarManager : MonoBehaviour
{
    #region private 변수
    [SerializeField] HumanTacker humanTacker;
    [SerializeField] private GameObject[] oArrPointPrefabs;
    [SerializeField] private CLidarSensor[] arrLidarSensors;
    [SerializeField] private Vector3 v3AreaLeftTop;
    [SerializeField] private Vector3 v3AreaRightBottom;

    private CancellationTokenSource _cts;

    private Thread[] _threadArrLidar = new Thread[1];
    private CLidarSerialController[] _arrLidar = new CLidarSerialController[1];
    private ConcurrentQueue<LaserScan>[] _qArrLaserScan = new ConcurrentQueue<LaserScan>[1];
    private Queue<GameObject>[] _qArrLaserScanPoints = new Queue<GameObject>[1];
    public Queue<GameObject>[] _qArrLaserScanPointsUsed = new Queue<GameObject>[1];

    public float maxAngle;
    public float minAngle;

    #endregion

    private void Awake()
    {
        // 라이다 os 초기화 (이거 Console용이라는 말이 있어서 초기화를 안해도 사용 가능함!)
        ydlidar.os_init();

        // 레이저 스캔 관련 초기화
        for (int i = 0; i < _qArrLaserScanPoints.Length; i++)
        {
            _qArrLaserScanPoints[i] = new Queue<GameObject>(500);
            _qArrLaserScanPointsUsed[i] = new Queue<GameObject>(500);
            _qArrLaserScan[i] = new ConcurrentQueue<LaserScan>();

            // 디버그용 레이저 포인트 생성 (오브젝트 풀링)
            for (int j = 0; j < _qArrLaserScanPoints[i].Count; j++)
            {
                GameObject point = Instantiate(oArrPointPrefabs[i], transform);
                point.SetActive(false);

                _qArrLaserScanPoints[i].Enqueue(point);
            }
        }

        // Thread 제어 토큰
        _cts = new CancellationTokenSource();
        var jsonData = JsonManager.math.Commons[0];

        // 라이다 센서 세팅값 초기화
        _arrLidar[0] = new CLidarSerialController(
            jsonData.PortName, 230400, (int)LidarTypeID.TYPE_TRIANGLE, 5, 8, 5,
            false, maxAngle, minAngle, true, false, _qArrLaserScan[0], _cts, jsonData.minRange, jsonData.maxRange);


        // 라이다 센서를 제어할 Thread 초기화
        for (int i = 0; i < _threadArrLidar.Length; i++)
        {
            _threadArrLidar[i] = new Thread(_arrLidar[i].SerialThread)
            { IsBackground = true, Name = "YDLidarWorker" + (i + 1) };
        }

        // Thread 시작 (라이다 센서 시작)
        for (int i = 0; i < _threadArrLidar.Length; i++)
        {
            _threadArrLidar[i].Start();
        }
    }

    private void Update()
    {
        // 라이다 1번
        if (_qArrLaserScan[0].TryDequeue(out var scan1))
        {
            DrawPoints(scan1, 0);
        }


        // (디버그용) 실행중인 Thread 및 라이다 강제 종료
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _cts?.Cancel();

            for (int i = 0; i < _threadArrLidar.Length; i++)
            {
                _threadArrLidar[i]?.Join();
            }

            ydlidar.os_shutdown();
        }
    }

    private void OnApplicationQuit()
    {
        // 종료 토큰 요청
        _cts?.Cancel();
    }

    private void OnDestroy()
    {
        // 스레드 종료 요청
        for (int i = 0; i < _threadArrLidar.Length; i++)
        {
            _threadArrLidar[i]?.Join();
        }

        ydlidar.os_shutdown();
    }

    /// <summary>
    /// (디버그) 화면에 센서 포인트를 찍는다.
    /// </summary>
    /// <param name="scan">Lidar Laser 스캔값</param>
    /// <param name="index">Lidar번호</param>
    private void DrawPoints(LaserScan scan, int index)
    {
        // 전에 찍었던 Points들 비활성화 및 반환
        while (_qArrLaserScanPointsUsed[index].Count > 0)
        {
            _qArrLaserScanPointsUsed[index].Peek().SetActive(false);
            _qArrLaserScanPoints[index].Enqueue(_qArrLaserScanPointsUsed[index].Dequeue());
        }
        List<Vector3> worldPoints = new List<Vector3>();

        // Points 그리기
        for (int i = 0; i < scan.points.Count; i++)
        {
            // 인식된 Point의 각도 및 거리값
            float angleRad = scan.points[i].angle;
            float distance = scan.points[i].range;

            // Unity 좌표로 변환
            float x = distance * Mathf.Cos(angleRad);
            float z = distance * Mathf.Sin(angleRad);

            // 좌표값 저장
            Vector3 point = new Vector3
            (
                x * 10 + arrLidarSensors[index].transform.position.x,
                0.0f,
                z * 10 + arrLidarSensors[index].transform.position.z
            );

            // 영역 필터링
            if (CheckFilterArea(point))
            {
                continue;
            }
            worldPoints.Add(point);

            // Pool에 Point 오브젝트가 부족할 경우 생성
            if (_qArrLaserScanPoints[index].Count <= 0)
            {
                GameObject oPoint = Instantiate(oArrPointPrefabs[index], transform);

                _qArrLaserScanPoints[index].Enqueue(oPoint);
            }

            // 찍힌 Point 활성화 및 Used 풀에 저장
            _qArrLaserScanPoints[index].Peek().SetActive(true);
            _qArrLaserScanPoints[index].Peek().transform.position = point;
            _qArrLaserScanPointsUsed[index].Enqueue(_qArrLaserScanPoints[index].Dequeue());
        }
        humanTacker.ProcessWorldPoints(worldPoints);
    }

    /// <summary>
    /// 필터 영역 안에 들어오는 Point인지 확인
    /// </summary>
    /// <returns>(true : 필터링 처리), (false : Point Draw)</returns>
    private bool CheckFilterArea(Vector3 point)
    {
        // 왼쪽 위 x좌표보다 작을 경우 그리지 않음
        if (point.x < v3AreaLeftTop.x)
        {
            return true;
        }
        // 오른쪽 아래 x좌표보다 클 경우 그리지 않음
        if (point.x > v3AreaRightBottom.x)
        {
            return true;
        }
        // 왼쪽 위 z좌표보다 클 경우 그리지 않음
        if (point.z > v3AreaLeftTop.z)
        {
            return true;
        }
        // 오른쪽 아래 z좌표보다 작을 경우 그리지 않음
        if (point.z < v3AreaRightBottom.z)
        {
            return true;
        }

        return false;
    }
}