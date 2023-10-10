using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbMessageContainer : MonoBehaviour
{
    private OrbMessage _currentActiveMessage;

    private OrbSingle _singleMessage;
    private OrbMultiple _multipleMessage;

    public bool IsLookingAtMessage => _currentActiveMessage.IsLookingAtMessage;

    public bool UserHasSeenNewStep => _currentActiveMessage.UserHasSeenNewStep;

    public bool IsMessageVisible => _currentActiveMessage.IsMessageVisible;
    public bool IsMessageFading => _currentActiveMessage.IsMessageFading;

    public IEnumerable<BoxCollider> GetAllColliders => _currentActiveMessage.GetAllColliders();

    public bool IsInteractingWithBtn => _currentActiveMessage.IsInteractingWithBtn();

    private void Awake()
    {
        // Get message object in orb prefab
        GameObject single = transform.GetChild(0).gameObject;
        _singleMessage = single.AddComponent<OrbSingle>();

        // Get message object in orb prefab
        GameObject multiple = transform.GetChild(0).gameObject;
        _multipleMessage = multiple.AddComponent<OrbMultiple>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _multipleMessage.gameObject.SetActive(false);
        _currentActiveMessage = _singleMessage;
    }

    public void HandleUpdateTaskListEvent(Dictionary<string, TaskList> currentSelectedTasks)
    {
        if (currentSelectedTasks.Keys.Count==1)
        {
            _singleMessage.gameObject.SetActive(true);
            _multipleMessage.gameObject.SetActive(false);

            _currentActiveMessage = _singleMessage;
        } else
        {
            _singleMessage.gameObject.SetActive(false);
            _multipleMessage.gameObject.SetActive(true);

            _currentActiveMessage = _multipleMessage;
        }

        _currentActiveMessage.UpdateTaskList(currentSelectedTasks);
    }

    public void SetIsActive(bool v1, bool v2) => _currentActiveMessage.SetIsActive(v1, v2);

    public void AddNotification(NotificationType type, string message, OrbFace face)
    {
        _currentActiveMessage.AddNotification(type, message, face);
        
    }

    public void RemoveNotification(NotificationType type, OrbFace face)
    {
        _currentActiveMessage.RemoveNotification(type, face);
    }

    public void RemoveAllNotifications()
    {
        _currentActiveMessage.RemoveAllNotifications();
    }

    public string SetTaskMessage(TaskList currentTask)
    {
        return _currentActiveMessage.SetTaskMessage(currentTask);
    }

    public void SetFadeOutMessage(bool fade) => _currentActiveMessage.SetFadeOutMessage(fade);
}
