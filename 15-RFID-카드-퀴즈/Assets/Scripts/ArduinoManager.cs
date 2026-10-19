using System.IO.Ports;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using DG.Tweening;

public class ArduinoManager : MonoBehaviour
{
    public enum Port
    {
        COM1, COM2, COM3, COM4, COM5, COM6, COM7, COM8, COM9, COM10, COM11, COM12, COM13, COM14, COM15, COM16,
    }

    SerialPort serial;

    [Header("시리얼 포트 (JSON에 PortName/BaudRate가 있으면 JSON 값이 우선)")]
    [SerializeField] Port portNumber = Port.COM1;
    [SerializeField] int baudRate = 9600;
    [SerializeField] private GameMove[] gameMoves;

    [Header("정답 확인 딜레이")]
    [SerializeField] private float answerCheckDelay = 2f;

    [Header("재연결 설정")]
    [SerializeField] float reconnectInterval = 3f;
    private float lastReconnectAttempt = -999f;
    private bool wasDisconnected = false;

    private void Start()
    {
        TryOpenPort();
    }

    // JSON의 PortName을 우선 사용하고, 없으면 인스펙터의 portNumber를 사용한다.
    private string ResolvePortName()
    {
        if (JsonManager.rfid != null && !string.IsNullOrWhiteSpace(JsonManager.rfid.PortName))
        {
            return JsonManager.rfid.PortName.Trim().ToUpper();
        }

        return portNumber.ToString();
    }

    // JSON의 BaudRate가 0보다 크면 사용하고, 아니면 인스펙터 값을 사용한다.
    private int ResolveBaudRate()
    {
        if (JsonManager.rfid != null && JsonManager.rfid.BaudRate > 0)
        {
            return JsonManager.rfid.BaudRate;
        }

        return baudRate;
    }

    private void Update()
    {
        if (serial == null || !serial.IsOpen)
        {
            if (!wasDisconnected)
            {
                Debug.LogWarning("[RFID] 연결 끊김 감지, 재연결 시도 시작");
                wasDisconnected = true;
            }

            if (Time.time - lastReconnectAttempt > reconnectInterval)
            {
                lastReconnectAttempt = Time.time;
                TryOpenPort();
            }
            return;
        }

        if (wasDisconnected)
        {
            Debug.Log("[RFID] 재연결 성공");
            wasDisconnected = false;
        }

        try
        {
            string rfidKey = serial.ReadLine();
            ReadRFID(rfidKey);
        }
        catch (System.TimeoutException)
        {
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[RFID] 통신 오류 발생, 재연결 시도: {e.Message}");
            CloseSafely();
        }
    }

    private void TryOpenPort()
    {
        string targetPort = ResolvePortName();
        int targetBaud = ResolveBaudRate();

        try
        {
            // JSON을 수정해서 포트/보레이트가 바뀐 경우, 기존 객체를 버리고 새로 만든다.
            if (serial != null && (serial.PortName != targetPort || serial.BaudRate != targetBaud))
            {
                CloseSafely();
                serial.Dispose();
                serial = null;
            }

            if (serial == null)
            {
                serial = new SerialPort(targetPort, targetBaud, Parity.None, 8, StopBits.One);
                serial.ReadTimeout = 100;
            }

            if (!serial.IsOpen)
            {
                serial.Open();
                Debug.Log($"[RFID] {targetPort} ({targetBaud}bps) 연결 성공");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[RFID] {targetPort} 연결 실패: {e.Message} (재시도 예정)");
        }
    }

    private void CloseSafely()
    {
        if (serial != null)
        {
            try
            {
                if (serial.IsOpen)
                    serial.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[RFID] 포트 닫기 중 오류: {e.Message}");
            }
        }
    }

    void ReadRFID(string rfidKey)
    {
        rfidKey = Regex.Replace(rfidKey, "[^a-zA-Z0-9]", "").ToUpper();

        if (string.IsNullOrEmpty(rfidKey)) return;

#if UNITY_EDITOR
        Debug.Log($"[디버그] 딕셔너리 개수: {JsonManager.rfidDict.Count}, 조회 키: '{rfidKey}' (길이: {rfidKey.Length})");
#endif

        // 현재 RFID_Reader가 활성화되어 있는 게임(Game1/2/3 중 하나)을 찾는다.
        GameMove activeGame = System.Array.Find(gameMoves, g => g.IsRFIDReaderActive());

        if (activeGame == null)
        {
            Debug.Log("[RFID] 활성화된 RFID_Reader가 있는 게임이 없어 태깅을 무시합니다.");
            return;
        }

        if (JsonManager.rfidDict.TryGetValue(rfidKey, out string userName))
        {
            Debug.Log($"인식된 사용자: {userName}");

            // 공용 userParent가 아니라, 지금 활성화된 그 Game 자신의 RFID_Image에서 이미지를 켠다.
            activeGame.ActivateRFIDImage(userName);

            // 이미지가 나온 뒤 2초 후 정답 판정
            // (그 사이 타임아웃 등으로 게임이 바뀌었을 수 있으므로, 실행 시점에 다시 확인한다)
            DOVirtual.DelayedCall(answerCheckDelay, () =>
            {
                if (activeGame != null && activeGame.IsRFIDReaderActive())
                {
                    activeGame.CheckRFIDAnswer(userName);
                }
                else
                {
                    Debug.Log("[RFID] 판정 대기 중 게임이 바뀌어 이번 판정은 무시합니다.");
                }
            });
        }
        else
        {
            Debug.LogWarning($"등록되지 않은 RFID 태그: {rfidKey}");
        }
    }

    private void OnApplicationQuit()
    {
        CloseSafely();
        if (serial != null)
        {
            serial.Dispose();
            serial = null;
        }
    }
}