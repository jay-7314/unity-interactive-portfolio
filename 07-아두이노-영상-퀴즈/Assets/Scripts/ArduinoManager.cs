using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ArduinoManager : MonoBehaviour
{
    public static ArduinoManager Instance;

    SerialPort port1;
    SerialPort port2;

    public bool IsReady
    {
        get
        {
            return port1 != null && port1.IsOpen &&
                   port2 != null && port2.IsOpen;
        }
    }
    bool IsRunning = false;
    Thread readThread;
    ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    public static event Action<string> OnArduinoMsg;

    private void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        port1 = StartPort(JsonManager.Frozen.Port1_Frist);
        port2 = StartPort(JsonManager.Frozen.Port2_Second);

        IsRunning = true;
        readThread = new Thread(ReadLoop);
        readThread.IsBackground = true;
        readThread.Start();
    }
    SerialPort StartPort(string portName)
    {
        try
        {
            SerialPort port = new SerialPort(portName, 9600);
            port.ReadTimeout = 1000;
            port.WriteTimeout = 1000;

            //연결시 비트를 보내기 위한 중요 설정
            port.DtrEnable = true;
            port.RtsEnable = true;

            port.Open();

            return port;

        }
        catch (Exception e)
        {
            Debug.Log("포트가 연결되지 않았습니다." + e);
            return null;
        }
    }

    void ReadLoop()
    {
        while (IsRunning)
        {
            ReadPort(port1);
            ReadPort(port2);
            Thread.Sleep(50);

        }
    }

    void ReadPort(SerialPort port)
    {
        try
        {
            if (port.BytesToRead > 0)
            {
                string msg = port.ReadLine().Trim();
                if (!string.IsNullOrEmpty(msg) && msg.Length > 0)
                {
                    messageQueue.Enqueue(msg);
                }
            }
        }
        catch (TimeoutException) { }
    }

    public void Update()
    {
        while (messageQueue.TryDequeue(out string msg))
        {
            Debug.Log("Arduino 수신: " + msg);
            OnArduinoMsg?.Invoke(msg);
        }
    }

    public void SendToArduinoA(string msg)
    {
        Send(port1, msg);
    }

    public void SendToArduinoB(string msg)
    {
        Send(port2, msg);
    }

    void Send(SerialPort port, string msg)
    {
        try
        {
            port.WriteLine(msg);
        }
        catch(Exception e)
        {
            Debug.LogError("메세지를 보내지 못했습니다." + e);
        }
    }

    private void OnApplicationQuit()
    {
        IsRunning = false;
        port1.Close();
        port2.Close();
    }
}
