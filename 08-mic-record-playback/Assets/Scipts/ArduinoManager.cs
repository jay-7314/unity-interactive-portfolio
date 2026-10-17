using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class ArduinoManager : MonoBehaviour
{
    public static ArduinoManager Instance;
    [SerializeField] string portName;
    [SerializeField] int baudRate = 9600;
    private SerialPort serialPort;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        portName = JsonManager.poly.Port;
        OpenPort();
    }

    private void OnApplicationQuit()
    {
        ClosePort();
    }

    public void OpenPort()
    {
        if (serialPort != null && serialPort.IsOpen) return;

        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            Debug.Log("아두이노가 열렸습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"포트 열기 실패: {e.Message}");
        }
    }

    public void ClosePort()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            Debug.Log("아두이노가 끊겼습니다.");
            serialPort.Close();
        }
    }

    public void SendString(string message)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.WriteLine(message);
        }
    }
}