using UnityEngine;
public class LogToGUI : MonoBehaviour
{
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }
    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        AngelARUI.Instance.LogDebugMessage(logString, true);
    }
}