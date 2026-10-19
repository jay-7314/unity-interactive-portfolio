using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ArduinoManager : MonoBehaviour
{
    public static ArduinoManager Instance;
    public static event Action<string> OnArduinoMsg;

    [SerializeField] bool usePort1 = true;
    [SerializeField] bool usePort2 = true;
    [SerializeField] float keepAliveInterval = 5f;

    SerialPort port1;
    SerialPort port2;
    string port1Name;
    string port2Name;
    int baudRate;

    Thread readThread;
    bool isRunning;
    ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    float keepAliveTimer = 0;

    float port1RetryTimer = 0;
    float port2RetryTimer = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        baudRate = JsonManager.uljin.Common[0].BaudRate;
        port1Name = JsonManager.uljin.Common[0].Port1;
        port2Name = JsonManager.uljin.Common[0].Port2_Light;

        if (usePort1)
            port1 = InitializePort(port1Name, baudRate);
        if (usePort2)
            port2 = InitializePort(port2Name, baudRate);

        isRunning = true;
        readThread = new Thread(ReadLoop);
        readThread.IsBackground = true;
        readThread.Start();
    }

    SerialPort InitializePort(string portName, int baud)
    {
        try
        {
            SerialPort port = new SerialPort(portName, baud);
            port.ReadTimeout = 1000;
            port.WriteTimeout = 1000;
            port.DtrEnable = true;
            port.RtsEnable = true;
            port.Open();
            Debug.Log($"[포트 연결 성공] {portName}");
            return port;
        }
        catch (Exception e)
        {
            Debug.LogError($"[포트 연결 실패] {portName}: {e.Message}");
            return null;
        }
    }

    SerialPort TryReconnect(string portName, ref float retryTimer)
    {
        retryTimer += 0.05f;
        if (retryTimer < 3f) return null;
        retryTimer = 0;

        Debug.LogWarning($"[재연결 시도] {portName}");
        SerialPort newPort = InitializePort(portName, baudRate);

        if (newPort != null && newPort.IsOpen)
            Debug.Log($"[재연결 성공] {portName}");
        else
            Debug.LogWarning($"[재연결 실패, 재시도 대기] {portName}");

        return newPort;
    }

    void ReadLoop()
    {
        while (isRunning)
        {
            if (usePort1)
            {
                if (port1 == null || !port1.IsOpen)
                    port1 = TryReconnect(port1Name, ref port1RetryTimer);
                else
                    port1 = ReadFromPort(port1);
            }

            if (usePort2)
            {
                if (port2 == null || !port2.IsOpen)
                    port2 = TryReconnect(port2Name, ref port2RetryTimer);
                else
                    port2 = ReadFromPort(port2);
            }
            Thread.Sleep(50);
        }
    }

    SerialPort ReadFromPort(SerialPort port)
    {
        if (port == null || !port.IsOpen) return port;

        try
        {
            if (port.BytesToRead > 0)
            {
                string msg = port.ReadLine().Trim();
                if (!string.IsNullOrEmpty(msg))
                {
                    msg = msg.Replace("?", "").Trim();
                    if (!string.IsNullOrEmpty(msg))
                    {
                        messageQueue.Enqueue(msg);
                        Debug.Log($"[수신] {port.PortName}: {msg}");
                    }
                }
            }
        }
        catch (TimeoutException) { }
        catch (Exception e)
        {
            Debug.LogWarning($"[포트 읽기 에러] {port.PortName}: {e.Message}");
            try { port.Dispose(); } catch { }
            return null;
        }

        return port;
    }

    private void Update()
    {
        while (messageQueue.TryDequeue(out string msg))
            OnArduinoMsg?.Invoke(msg);

        keepAliveTimer += Time.deltaTime;
        if (keepAliveTimer >= keepAliveInterval)
        {
            keepAliveTimer = 0;
            SendKeepAlive();
        }
    }

    void SendKeepAlive()
    {
        SendToArduinoA("PING");
        SendToArduinoB("PING");
    }

    public void SendToArduinoA(string msg)
    {
        if (!usePort1) return;
        Send(port1, msg);
    }

    public void SendToArduinoB(string msg)
    {
        Send(port2, msg);
    }

    void Send(SerialPort port, string msg)
    {
        if (port == null || !port.IsOpen)
        {
            Debug.LogWarning($"[전송 실패] 포트가 열려있지 않음");
            return;
        }

        try
        {
            port.WriteLine(msg);
            Debug.Log($"[전송] {port.PortName}: {msg}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[전송 에러] {port.PortName}: {e.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        isRunning = false;

        if (readThread != null && readThread.IsAlive)
            readThread.Join(1000);

        if (usePort2) SendToArduinoB("Off");
        ClosePort(port1);
        ClosePort(port2);
    }

    void ClosePort(SerialPort port)
    {
        if (port != null && port.IsOpen)
        {
            try
            {
                port.Close();
                Debug.Log($"[포트 닫기] {port.PortName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[포트 닫기 실패] {e.Message}");
            }
        }
    }
}