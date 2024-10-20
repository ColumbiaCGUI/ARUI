using UnityEngine;

public enum NotificationUrgency
{
    warning,
    note
}

public class Notification : MonoBehaviour
{
    public bool userHasSeen = false;

    private bool _init = false;

    private NotificationUrgency _notificationUrgency;

    private RectTransform _notificationMessageRect;
    private TMPro.TextMeshProUGUI _textNotification;
    public string GetMessage => _textNotification.text;

    public bool IsSet => _textNotification.text.Length > 0;

    public void init(NotificationUrgency urgency, string message, float containerHeight)
    {
        if (!_init)
        {
            _notificationUrgency = urgency;

            //init notification message group
            _notificationMessageRect = gameObject.GetComponent<RectTransform>();
            _textNotification = _notificationMessageRect.gameObject.GetComponent<TMPro.TextMeshProUGUI>();

            _textNotification.text = Utils.SplitTextIntoLines(message, ARUISettings.OrbMessageMaxCharCountPerLine);

            _notificationMessageRect.rotation = Quaternion.identity;
            _notificationMessageRect.localRotation = Quaternion.identity;   
            _notificationMessageRect.SetLocalXPos(0);
            UpdateYPos(containerHeight, false);
            _notificationMessageRect.SetLocalZPos(0);

            _init = true;
        }
    }

    public void UpdateSize(float xSize)
    {
        if (!_init)
            _notificationMessageRect.sizeDelta = new Vector2(xSize, _notificationMessageRect.rect.height);
    }

    public void SetMessage(string message, int charPerLine)
    {
        _textNotification.text = Utils.SplitTextIntoLines(message, charPerLine);
    }

    public void UpdateYPos(float containerHeight, bool taskListIsActive)
    {
        if (_notificationUrgency == NotificationUrgency.note && !taskListIsActive)
            _notificationMessageRect.SetLocalYPos(-containerHeight);
        else if (_notificationUrgency == NotificationUrgency.note && taskListIsActive)
            _notificationMessageRect.SetLocalYPos(-containerHeight);
        else if (_notificationUrgency == NotificationUrgency.warning && !taskListIsActive)
            _notificationMessageRect.SetLocalYPos(containerHeight);
        else if (_notificationUrgency == NotificationUrgency.warning && taskListIsActive) 
            _notificationMessageRect.SetLocalYPos(containerHeight);
    }
}
