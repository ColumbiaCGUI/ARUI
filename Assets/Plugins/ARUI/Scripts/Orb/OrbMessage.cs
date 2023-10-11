using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OrbMessageType
{
    single = 0,
    multiple = 1
}

public enum MessageAnchor
{
    left = 1, //message is left from the orb
    right = 2, //message is right from the orb
}

public abstract class OrbMessage : MonoBehaviour
{
    protected OrbMessageType messageType;

    private MessageAnchor _currentAnchor = MessageAnchor.right;
    public MessageAnchor CurrentAnchor {
        get => _currentAnchor;
        set
        {
            _currentAnchor = value;
        }
    }

    private bool _isLookingAtMessage = false;
    public bool IsLookingAtMessage
    {
        get { return _isLookingAtMessage; }
    }

    private bool _userHasSeenNewStep = false;
    public bool UserHasSeenNewStep
    {
        get { return _userHasSeenNewStep; }
        set { _userHasSeenNewStep = value; }
    }

    private bool _isMessageVisible = false;
    public bool IsMessageVisible
    {
        get { return _isMessageVisible; }
        set { _isMessageVisible = value; }
    }

    private bool _isMessageFading = false;
    public bool IsMessageFading
    {
        get { return _isMessageFading; }
        set { _isMessageFading = value; }
    }

    private bool _messageIsLerping = false;
    protected bool IsMessageLerping
    {
        get { return _messageIsLerping; }
        set { _messageIsLerping = value; }
    }

    public void Update()
    {
        // Update eye tracking flag
        if (_isLookingAtMessage && EyeGazeManager.Instance.CurrentHit != EyeTarget.orbMessage && EyeGazeManager.Instance.CurrentHit != EyeTarget.orbtasklistButton)
            _isLookingAtMessage = false;
        else if (!_isLookingAtMessage && (EyeGazeManager.Instance.CurrentHit == EyeTarget.orbMessage || EyeGazeManager.Instance.CurrentHit == EyeTarget.orbtasklistButton))
            _isLookingAtMessage = true;
    }

    /// <summary>
    /// Check if message box should be anchored right
    /// </summary>
    /// <param name="offsetPaddingInPixel"></param>
    /// <returns></returns>
    protected bool ChangeMessageBoxToRight(float offsetPaddingInPixel)
    {
        return (AngelARUI.Instance.ARCamera.WorldToScreenPoint(transform.position).x < ((AngelARUI.Instance.ARCamera.pixelWidth * 0.5f) - offsetPaddingInPixel));
    }

    /// <summary>
    /// Check if message box should be anchored left
    /// </summary>
    protected bool ChangeMessageBoxToLeft(float offsetPaddingInPixel)
    {
        return (AngelARUI.Instance.ARCamera.WorldToScreenPoint(transform.position).x > ((AngelARUI.Instance.ARCamera.pixelWidth * 0.5f) + offsetPaddingInPixel));
    }

    public abstract void SetEnabled(bool enabled);

    public abstract void AddNotification(NotificationType type, string message, OrbFace face);
    public abstract void RemoveNotification(NotificationType type, OrbFace face);

    public abstract bool IsInteractingWithBtn();
    public abstract void SetIsActive(bool active, bool newTask);
    public abstract List<BoxCollider> GetAllColliders();
    public abstract string SetTaskMessage(TaskList currentTask);

    public abstract void UpdateTaskList(Dictionary<string, TaskList> currentSelectedTasks);

    public abstract void RemoveAllNotifications();

    public abstract void SetFadeOutMessage(bool fade);
}
