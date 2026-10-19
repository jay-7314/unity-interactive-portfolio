using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
//using System.Runtime.Remoting.Channels;
using System.Threading;
using UnityEngine;

public class CLidarSerialController
{
    #region private 변수

    private ConcurrentQueue<LaserScan> _qLaserScan;
    private CancellationTokenSource _cts;

    private string _strPort;
    private int _nBaudrate;
    private int _nType;
    private int _nSampleRate;
    private int _nIntensityBit;
    private int _nScanFrequency;
    private bool _isSingleChannel;
    private float _fMaxAngle;
    private float _fMinAngle;
    private bool _isReverse;
    private bool _isInvert;
    private float _minRange;
    private float _maxRange;

    #endregion

    /// <summary>
    /// Lidar SerialController 생성자
    /// </summary>
    /// <param name="port">포트 번호</param>
    /// <param name="baudrate">통신 속도 (G2의 경우 230400)</param>
    /// <param name="type">거리 측정 방식</param>
    /// <param name="sampleRate">표본화 속도 (공식 깃허브 문서에 K로 나와있어서 천의 자리로 계산)</param>
    /// <param name="intensityBit">반사 강도</param>
    /// <param name="scanFrequency">스캔 주파수</param>
    /// <param name="isSingleChannel">단일 채널 장치 여부</param>
    /// <param name="qLaserScan">센서 값을 담을 공유 Queue</param>
    /// <param name="cts">Thread 관리 공용 토큰</param>
    public CLidarSerialController(string port, int baudrate, int type, int sampleRate, int intensityBit,
        int scanFrequency, bool isSingleChannel, float maxAngle, float minAngle, bool isReverse, bool isInvert,
        ConcurrentQueue<LaserScan> qLaserScan, CancellationTokenSource cts, float minRange, float maxRange)
    {
        _strPort = port;
        _nBaudrate = baudrate;
        _nType = type;
        _nSampleRate = sampleRate;
        _nIntensityBit = intensityBit;
        _nScanFrequency = scanFrequency;
        _isSingleChannel = isSingleChannel;
        _qLaserScan = qLaserScan;
        _cts = cts;
        _fMaxAngle = maxAngle;
        _fMinAngle = minAngle;
        _isReverse = isReverse;
        _isInvert = isInvert;
        _minRange = minRange;
        _maxRange = maxRange;
    }

    /// <summary>
    /// Lidar Serial Thread 동작 (실시간 센서값을 받아온다.)
    /// </summary>
    public void SerialThread()
    {
        CancellationToken token = _cts.Token;

        Thread.Sleep(1000);

        CYdLidar _lidarClass = new CYdLidar();

        try
        {
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropSerialPort, _strPort);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropSerialBaudrate, _nBaudrate);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropLidarType, _nType);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropSampleRate, _nSampleRate);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropSingleChannel, _isSingleChannel);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropIntenstiy, true);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropIntenstiyBit, _nIntensityBit);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropScanFrequency, _nScanFrequency);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropDeviceType, (int)DeviceTypeID.YDLIDAR_TYPE_SERIAL);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropMaxAngle, _fMaxAngle);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropMinAngle, _fMinAngle);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropMaxRange, _maxRange);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropMinRange, _minRange);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropReversion, _isReverse);
            _lidarClass.setlidaropt((int)LidarProperty.LidarPropInverted, _isInvert);

            if (_lidarClass.initialize() == false)
            {
                Debug.LogError("Can't initialize");
                Debug.LogError(_lidarClass.DescribeError());

                return;
            }

            if (_lidarClass.turnOn() == false)
            {
                Debug.LogError("Can't Turn On");
                Debug.LogError(_lidarClass.DescribeError());
                Debug.LogError(_lidarClass.getDriverError());
            }

            Debug.Log("Lidar Start");

            LaserScan _laserScan = new LaserScan();

            while (token.IsCancellationRequested == false)
            {
                if (_lidarClass.doProcessSimple(_laserScan))
                {
                    // 레이저 포인터에 대한 계산을 여기서하고, 결과값만 반환하는 방식으로?
                    _qLaserScan.Enqueue(_laserScan);

                    if (_qLaserScan.Count > 0)
                    {
                        while (_qLaserScan.Count > 1 && _qLaserScan.TryDequeue(out _))
                        {
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            _lidarClass.turnOff();
            _lidarClass.disconnecting();
            _lidarClass.Dispose();
        }
    }
}
