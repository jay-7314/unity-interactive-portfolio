using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;

public class ShowDisplay2 : MonoBehaviour
{
    [SerializeField] private float checkInterval = 1f;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    void Awake()
    {
        Screen.SetResolution(1080, 1920, true);
    }

    void Start()
    {
        // 이미 Display 2가 있으면 바로 Activate
        if (Display.displays.Length > 1)
        {
            RefreshRate refreshRate = new RefreshRate() { numerator = 60, denominator = 1 };
            Display.displays[1].Activate(1920, 1080, refreshRate);
            UnityEngine.Debug.Log("Display 2 activated!");
            return;
        }

        // 없으면 모니터 감지 시작
        StartCoroutine(WaitAndActivateDisplay2());
    }

    private IEnumerator WaitAndActivateDisplay2()
    {
        float elapsed = 0f;
        RefreshRate refreshRate = new RefreshRate() { numerator = 60, denominator = 1 };

        while (true)
        {
            int monitorCount = GetSystemMetrics(80);

            if (monitorCount > 1)
            {
                UnityEngine.Debug.Log($"새 모니터 감지 ({elapsed:F1}s), 재시작합니다.");

                string appPath = Path.Combine(
                    Directory.GetParent(Application.dataPath).FullName,
                    Application.productName + ".exe"
                );

                Process.Start(appPath);
                Application.Quit();
                yield break;
            }

            UnityEngine.Debug.Log($"Display 2 not found yet... ({elapsed:F1}s)");
            yield return new WaitForSeconds(checkInterval);
            elapsed += checkInterval;
        }
    }
}