using UnityEngine;
using System.IO;

public class LogToFile : MonoBehaviour
{
    private static string logFilePath;

    void Awake()
    {
        logFilePath = Application.persistentDataPath + "/GameLog.txt";
        Debug.Log("로그 파일 경로: " + logFilePath);

        // 기존 로그 파일 삭제 후 새로 생성
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        File.AppendAllText(logFilePath, logString + "\n");
    }
}
