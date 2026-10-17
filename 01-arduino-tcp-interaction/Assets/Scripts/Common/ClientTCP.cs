using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ClientTCP : MonoBehaviour
{
    public static ClientTCP Instance;
    string clientName = "울진상설 : 수렴";

    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;
    Thread receiveThread;
    bool serverRun = true;
    bool isconnect = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(ConnectLoop());
    }

    IEnumerator ConnectLoop()
    {
        while (serverRun)
        {
            if (!isconnect)
            {
                TryConnect();
            }
            yield return new WaitForSeconds(2f);
        }
    }

    void TryConnect()
    {
        string ip = JsonManager.uljin.Common[0].IPAddress;
        int port = 7777;

        //소켓 생성
        try
        {
            socket = new TcpClient(ip, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream) { AutoFlush = true };
            reader = new StreamReader(stream);

            isconnect = true;

            receiveThread = new Thread(ReceiveLoop);
            receiveThread.Start();
            Debug.Log("서버와 연결되었습니다.");
        }
        catch (Exception e)
        {
            Debug.Log($"소켓에러 : {e.Message}");
            isconnect = false;
        }
    }

    void ReceiveLoop()
    {
        try
        {
            while (serverRun && isconnect)
            {
                string data = reader.ReadLine();
                if (data == null) break;

                OnIncomingData(data);
            }
        }
        catch
        {

        }
        Debug.Log("서버연결 끊김");
        CloseSkcket();

    }

    public void Send(string msg)
    {
        if (!isconnect)
        {
            Debug.LogWarning("[CLIENT] 소켓 연결 안됨");
            return;
        }

        Debug.Log($"[CLIENT - SERVER] {msg}");
        writer.WriteLine(msg);
    }


    void OnIncomingData(string data)
    {
        if (data == "%NAME")
        {
            Send($"&NAME|{clientName}");
        }
    }

    private void OnApplicationQuit()
    {
        serverRun = false;
        CloseSkcket();
    }

    void CloseSkcket()
    {
        isconnect = false;

        try { writer?.Close(); } catch { }
        try { reader?.Close(); } catch { }
        try { socket?.Close(); } catch { }
    }
}
