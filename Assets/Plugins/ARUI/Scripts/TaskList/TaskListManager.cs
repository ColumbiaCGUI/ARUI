using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.UI;
using Shapes;

/// <summary>
/// Represents the task list in 3D
/// </summary>
public class TaskListManager : Singleton<TaskListManager>
{
    private VMNonControllable _vmnc;
    private string[,] _tasks;                   /// < Reference to all tasks and indication if maintask (0) or subtask (1)

    private bool _taskListGenerated = false;

    private Dictionary<int, int> _taskToParent;
    private Dictionary<int, TaskListElement> _taskToElement;

    private List<TaskListElement> _currentTasksListElements;
    private Dictionary<int, List<TaskListElement>> _mainToSubTasks;

    private bool _isProcessingOpening = false;
    private bool _isProcessingRepositioning = false;

    //Reference to background panel, list container and taskprefabs
    private GameObject _list;
    private RectTransform _taskContainer;
    private GameObject _taskPrefab;

    private int _currentTaskIDInList = -1;

    private Line _progressLine;
    private GameObject _topPointsParent;
    private GameObject _bottomPointsParent;
    
    // Eye-gaze based updates
    private bool _isLookingAtTaskList = false;
    private bool _isVisible = false;
    private bool _isFading = false;

    private Material _bgMat;
    private Color _activeColor = new Color(0.06f, 0.06f, 0.06f, 0.5f);
    private float _step = 0.001f;

    private BoxCollider _taskListCollider;
    private Vector3 _openCollidersize;
    private Vector3 _openColliderCenter;

    private Vector3 _closedCollidersize;
    private Vector3 _closedColliderCenter;

    private float _currentMaxDistance = ARUISettings.TasksMaxDistToUser;

    private ObjectIndicator _obIndicator;
    private Line _dragHandle;
    public bool NearHovering
    {
        set { _dragHandle.gameObject.SetActive(value); }
    }

    private bool _isDone = false;
    public bool IsDone => _isDone;

    private bool IsActive => _list.activeInHierarchy;

    private bool _handleEyeEvents = true;

    /// <summary>
    /// Initialize all components of the task list and get all references from the task list prefab
    /// </summary>
    private void Awake()
    {
        _list = transform.GetChild(0).gameObject;

        _taskContainer = GameObject.Find("TaskContainer").GetComponent<RectTransform>();
        _bgMat = new Material(_taskContainer.GetComponent<Image>().material);
        _taskPrefab = Resources.Load(StringResources.Taskprefab_path) as GameObject;

        Line[] allLines = GetComponentsInChildren<Shapes.Line>();
        _progressLine = allLines[0];
        _bottomPointsParent = GameObject.Find("BottomPoints");
        _bottomPointsParent.transform.parent = _progressLine.transform;
        _topPointsParent = GameObject.Find("TopPoints");
        _topPointsParent.transform.parent = _progressLine.transform;
        _bottomPointsParent.SetActive(false);
        _topPointsParent.SetActive(false);

        _dragHandle = allLines[1];
        _dragHandle.gameObject.SetActive(false);

        _list.SetActive(false);

        _taskListCollider = gameObject.GetComponent<BoxCollider>();
        _openCollidersize = _taskListCollider.size;
        _openColliderCenter = _taskListCollider.center;
        _closedCollidersize = new Vector3(0.1f, 0.3f, 0.03f);
        _closedColliderCenter = new Vector3(-0.21f, 0, 0);

        _obIndicator = GetComponentInChildren<ObjectIndicator>();
        _obIndicator.gameObject.SetActive(false);

        _vmnc = gameObject.AddComponent<VMNonControllable>();
        _vmnc.enabled = false;

    }


    #region Generates tasklist at runtime

    public void SetTasklist(string[,] tasks)
    {
        if (tasks != null)
        {
            this._tasks = tasks;
            _taskListGenerated = false;

            _taskToParent = new Dictionary<int, int>();

            int lastParent = 0;
            int lastGrandparent = 0;
            for (int i = 0; i < tasks.GetLength(0); i++)
            {
                if (tasks[i, 0].Equals("0"))
                {
                    lastGrandparent = i;
                }

                else if (tasks[i, 0].Equals("1"))
                {
                    _taskToParent.Add(i, lastGrandparent);
                    lastParent = i;
                }

                else if (tasks[i, 0].Equals("2"))
                    _taskToParent.Add(i, lastParent);
            }

            StartCoroutine(GenerateTaskListElementsAsync(tasks));

            if (tasks.GetLength(0) > ARUISettings.TaskMaxNumTasksOnList)
                _bottomPointsParent.SetActive(true);
            else
                _bottomPointsParent.SetActive(false);

        }
    }

    private IEnumerator GenerateTaskListElementsAsync(string[,] tasks)
    {
        AngelARUI.Instance.LogDebugMessage("Generate template for task list.", false);

        SetTaskListActive(false);

        _list.SetActive(false);
        _taskListCollider.enabled = false;

        if (_taskToElement != null)
        {
            for (int i = 0; i < _taskToElement.Count; i++)
                Destroy(_taskToElement[i].gameObject);

            _taskToElement = null;
        }

        _taskToElement = new Dictionary<int, TaskListElement>();
        _currentTasksListElements = new List<TaskListElement>();
        _mainToSubTasks = new Dictionary<int, List<TaskListElement>>();

        int lastMainIndex = 0;
        for (int i = 0; i < tasks.GetLength(0); i++)
        {
            GameObject task = Instantiate(_taskPrefab, _taskContainer.transform);
            TaskListElement t = task.gameObject.AddComponent<TaskListElement>();
            t.InitText(i, tasks[i, 1], Int32.Parse(tasks[i, 0]));
            t.SetIsDone(false);
            t.gameObject.SetActive(false);

            _taskToElement.Add(i, t);

            if (tasks[i, 0].Equals("0"))
                lastMainIndex = i;
            else if (tasks[i, 0].Equals("1"))
            {
                if (!_mainToSubTasks.ContainsKey(lastMainIndex))
                    _mainToSubTasks.Add(lastMainIndex, new List<TaskListElement>());
                _mainToSubTasks[lastMainIndex].Add(t);
            }

            if (i % 10 == 0)
                yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();

        _taskListGenerated = true;

        //Orb.Instance.SetTaskListButtonActive(true);
        AngelARUI.Instance.LogDebugMessage("Finished generating task list", false);
    }

    #endregion

    private void Update()
    {
        if ( (!_taskListGenerated || !IsActive)) return;

        if (_handleEyeEvents)
        {
            // Update eye tracking flag
            if (_isLookingAtTaskList && EyeGazeManager.Instance.CurrentHit != EyeTarget.tasklist)
            {
                _isLookingAtTaskList = false;
            }
            else if (!_isLookingAtTaskList && EyeGazeManager.Instance.CurrentHit == EyeTarget.tasklist)
            {
                _isLookingAtTaskList = true;
                Orb.Instance.Message.SetIsActive(false, false);
            }

            if (_isLookingAtTaskList && !_isVisible && !_isFading)
            {
                _isVisible = true;
                _taskListCollider.center = _openColliderCenter;
                _taskListCollider.size = _openCollidersize;

                _bgMat.color = _activeColor;
                for (int i = 0; i < _currentTasksListElements.Count; i++)
                    _currentTasksListElements[i].SetAlpha(1f);

                _taskContainer.gameObject.SetActive(true);
                _vmnc.enabled = true;
            }
            else if (_isLookingAtTaskList && _isVisible && _isFading)
            {
                StopCoroutine(FadeOut());

                _isFading = false;
                _taskListCollider.center = _openColliderCenter;
                _taskListCollider.size = _openCollidersize;

                _bgMat.color = _activeColor;
                for (int i = 0; i < _currentTasksListElements.Count; i++)
                    _currentTasksListElements[i].SetAlpha(1f);
            }
            else if (!_isLookingAtTaskList && _isVisible && !_isFading)
            {
                StartCoroutine(FadeOut());
            }
        }

        //if (!isLookingAtTaskList && !isProcessingOpening && !isDragging && !isProcessingOpening && !isProcessingRepositioning)
        //    UpdatePosition();

        transform.rotation = Quaternion.LookRotation(transform.position - AngelARUI.Instance.ARCamera.transform.position, Vector3.up);
    }

    public void ToggleTasklist()
    {
        if (_list != null)
            SetTaskListActive(!_list.activeInHierarchy);
    }

    /// <summary>
    /// Update the collider and the the extent of the task list based on the extent of all tasks.
    /// </summary>
    private void LateUpdate()
    {
        _progressLine.Start = new Vector3(0, ((_taskContainer.rect.y)) * -1, 0);
        _progressLine.End = new Vector3(0, ((_taskContainer.rect.y)), 0);

        _topPointsParent.transform.position = _progressLine.transform.TransformPoint(_progressLine.Start);
        _bottomPointsParent.transform.position = _progressLine.transform.TransformPoint(_progressLine.End);

        _taskListCollider.size = new Vector3(_taskListCollider.size.x, _taskContainer.rect.height, _taskListCollider.size.z);
    }

    #region Task updates
    /// <summary>
    /// Set the ID of the current task (in regard to the given task list) - starts with 0 - and the task list will update accordingly.
    /// Does not update if no tasks were set beforehand
    /// </summary>
    public void SetCurrentTask(int currentTaskID)
    {
        if (_tasks == null || currentTaskID == _currentTaskIDInList) return;

        if (currentTaskID < 0)
        {
            AngelARUI.Instance.LogDebugMessage("TaskID was invalid: id " + currentTaskID + ", task list length: " + _tasks.GetLength(0), false);
            return;
        }

        string orbTaskMessage = "All Done!";
        _isDone = true;
        if (currentTaskID < GetTaskCount())
        {
            _isDone = false;
            orbTaskMessage = _tasks[currentTaskID, 1];
        }

        _currentTaskIDInList = currentTaskID;
        StartCoroutine(SetCurrentTaskAsync(currentTaskID, orbTaskMessage));
    }

    /// <summary>
    /// Update the tasklist
    /// </summary>
    /// <param name="currentTaskID">The id of the current task list (from 0 to n-1)</param>
    /// <param name="orbMessage">The task message the orb should show</param>
    /// <returns></returns>
    private IEnumerator SetCurrentTaskAsync(int currentTaskID, string orbMessage)
    {
        int messageID = currentTaskID;

        while (!_taskListGenerated)
            yield return new WaitForEndOfFrame();

        bool allDone = false;
        if (currentTaskID >= _tasks.GetLength(0))
        {
            allDone = true;
            currentTaskID = _tasks.GetLength(0) - 1;
        }

        AngelARUI.Instance.LogDebugMessage("TaskID was valid: " + currentTaskID + ", task list length: " + _tasks.GetLength(0), true);

        bool isSubTask = false;
        if (_tasks[currentTaskID, 0].Equals("1"))
            isSubTask = true;

        bool isMainTaskAndHasChildren = false;
        if (_tasks[currentTaskID, 0].Equals("0") && currentTaskID + 1 < _tasks.GetLength(0) && _tasks[currentTaskID + 1, 0].Equals("1"))
        {
            currentTaskID += 1;
            isMainTaskAndHasChildren = true;
        }

        //Deactivate previous task list elements
        for (int i = 0; i < _taskToElement.Count; i++)
        {
            _taskToElement[i].Reset(_list.activeInHierarchy);
            _taskToElement[i].gameObject.SetActive(false);
            _currentTasksListElements.Remove(_taskToElement[i]);
        }

        //Adapt begin and end list index in the UI based on main/subtask relationship
        int startIndex = currentTaskID - (ARUISettings.TaskMaxNumTasksOnList + 1) / 2;
        if (startIndex < 0)
            startIndex = 0;

        if (startIndex > 0)
            _topPointsParent.SetActive(true);
        else
            _topPointsParent.SetActive(false);

        int endIndex = startIndex + ARUISettings.TaskMaxNumTasksOnList;
        if (endIndex > _tasks.GetLength(0))
            endIndex = _tasks.GetLength(0);

        if (allDone)
            _bottomPointsParent.SetActive(false);
        else
            _bottomPointsParent.SetActive(true);

        for (int i = startIndex; i < endIndex; i++)
        {
            TaskListElement current = _taskToElement[i];
            if ((isSubTask || isMainTaskAndHasChildren) && i == (currentTaskID - 1))
            {
                current = _taskToElement[_taskToParent[currentTaskID]];
                int subTasksDone = currentTaskID - current.ID - 1;
                current.SetAsCurrent(subTasksDone + "/" + _mainToSubTasks[_taskToParent[currentTaskID]].Count);

            }
            else
            {
                if (i < currentTaskID || allDone)
                    current.SetIsDone(true);

                else if (i == currentTaskID && !allDone)
                    current.SetAsCurrent("");
                else
                    current.SetIsDone(false);
            }

            current.gameObject.SetActive(true);
            _currentTasksListElements.Add(current);
        }

        string previousMessage = "";
        string nextMessage = "";

        if (messageID < GetTaskCount() && messageID>=0)
        {
            if (messageID >= 1)
                previousMessage = _tasks[messageID - 1, 1];

            if (messageID + 1 < GetTaskCount())
                nextMessage = _tasks[messageID + 1, 1];
        }

        Orb.Instance.SetTaskMessage(orbMessage, previousMessage, nextMessage);
        AudioManager.Instance.PlaySound(Orb.Instance.AllOrbColliders[0].transform.position, SoundType.taskDone);
    }
    #endregion

    #region Visibility updates

    /// <summary>
    /// Fades out the background and the text on the tasklist
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOut()
    {
        _isFading = true;

        yield return new WaitForSeconds(1.0f);

        float shade = _activeColor.r;
        float alpha = 1f;

        while (_isFading && shade > 0)
        {
            alpha -= (_step * 20);
            shade -= _step;

            _bgMat.color = new Color(shade, shade, shade);

            if (alpha >= 0)
            {
                for (int i = 0; i < _currentTasksListElements.Count; i++)
                    _currentTasksListElements[i].SetAlpha(Mathf.Max(0, alpha));
            }

            yield return new WaitForEndOfFrame();
        }

        if (_isFading && _handleEyeEvents)
        {
            _isFading = false;
            _isVisible = false;

            _vmnc.enabled = false;
            _taskContainer.gameObject.SetActive(false);

            _taskListCollider.center = _closedColliderCenter;
            _taskListCollider.size = _closedCollidersize;
        }
    }

    private IEnumerator ShowTaskList()
    {
        _isProcessingOpening = true;

        StartCoroutine(UpdatePosAndRot(false));

        while (_isProcessingRepositioning)
        {
            yield return new WaitForEndOfFrame();
        }

        foreach (var elem in _currentTasksListElements)
            elem.SetAlpha(1);

        _list.SetActive(true);
        _taskListCollider.enabled = true;

        if (!_handleEyeEvents)
        {
            StartCoroutine(MakeTaskListVisible());
        }
        
        AudioManager.Instance.PlaySound(transform.position, SoundType.notification);

        _isProcessingOpening = false;
    }
    
    #endregion

    #region Pose Update

    public void Reposition()
    {
        if (_list.activeSelf)
            UpdatePosition();
    }

    private void UpdateMaxDistance()
    {
        float dist = transform.position.GetCameraToPosDist();

        if (dist != -1)
            _currentMaxDistance = Mathf.Max(ARUISettings.TasksMinDistToUser + 0.02f, Mathf.Min(dist - 0.08f, ARUISettings.TasksMaxDistToUser));
    }

    private void UpdatePosition()
    {
        UpdateMaxDistance();

        if ((Vector3.Distance(AngelARUI.Instance.ARCamera.transform.position, transform.position) > _currentMaxDistance
            || Vector3.Angle(transform.position - AngelARUI.Instance.ARCamera.transform.position, AngelARUI.Instance.ARCamera.transform.forward) > 90f))
        {
            StartCoroutine(UpdatePosAndRot(true));
        }
    }

    private IEnumerator UpdatePosAndRot(bool slow)
    {
        _isProcessingRepositioning = true;

        Vector3 direction = AngelARUI.Instance.ARCamera.transform.forward;
        var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
        if (eyeGazeProvider != null && eyeGazeProvider.IsEyeTrackingEnabledAndValid && eyeGazeProvider.IsEyeCalibrationValid.Value)
            direction = eyeGazeProvider.GazeDirection;

        Vector3 targetPos = AngelARUI.Instance.ARCamera.transform.position + Vector3.Scale(
            direction,
            new Vector3(_currentMaxDistance, _currentMaxDistance, _currentMaxDistance));

        if (slow)
        {
            Vector3 startPos = transform.position;

            float timeElapsed = 0;
            float lerpDuration = 0.4f;
            while (timeElapsed < lerpDuration)
            {
                // Set our position as a fraction of the distance between the markers.
                transform.position = Vector3.Lerp(startPos, targetPos, timeElapsed / lerpDuration);
                transform.SetYPos(AngelARUI.Instance.ARCamera.transform.position.y);

                timeElapsed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            transform.position = targetPos;
            transform.SetYPos(AngelARUI.Instance.ARCamera.transform.position.y);

            StartCoroutine(ShowHalo());
        } 
        else
        {
            transform.position = targetPos;
            transform.SetYPos(AngelARUI.Instance.ARCamera.transform.position.y);
        }

        _isProcessingRepositioning = false;
    }

    #endregion

    #region Getter and Setter

    public int GetTaskCount()
    {
        if (_tasks != null)
            return _tasks.GetLength(0);
        else
            return 0;
    }

    public int GetCurrentTaskID() => _currentTaskIDInList + 1;

    public void SetTaskListActive(bool isActive)
    {
        if (_isProcessingOpening || !_taskListGenerated) return;
        AngelARUI.Instance.LogDebugMessage("Show Task list: " + isActive, true);

        if (isActive)
        {
            UpdateMaxDistance();
            StartCoroutine(ShowTaskList());
            StartCoroutine(ShowHalo());
        }
        else
        {
            _list.SetActive(false);
            _taskListCollider.enabled = false;
            _vmnc.enabled = false;

            AudioManager.Instance.PlaySound(transform.position, SoundType.notification);
        }
    }

    /// <summary>
    /// Show directional indicator to task list until the tasklist is in the FOV of the user
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowHalo()
    {
        _obIndicator.gameObject.SetActive(true);

        while (!transform.position.InFOV(AngelARUI.Instance.ARCamera) && _list.activeSelf)
        {
            yield return new WaitForEndOfFrame();   
        }

        _obIndicator.gameObject.SetActive(false);
    }

    public void SetAllTasksDone() => SetCurrentTask(GetTaskCount() + 2);

    public void SetEyeEventsActive(bool enable)
    {
        _handleEyeEvents = enable;

        if (!_handleEyeEvents &&  IsActive)
        {
            StopCoroutine(FadeOut());

            StartCoroutine(MakeTaskListVisible());
        }
    }

    private IEnumerator MakeTaskListVisible()
    {
        yield return new WaitForSeconds(0.2f);

        foreach (var elem in _currentTasksListElements)
            elem.SetAlpha(1);

        _isVisible = true;

        _taskListCollider.enabled = true;
        _taskListCollider.center = _openColliderCenter;
        _taskListCollider.size = _openCollidersize;

        _bgMat.color = _activeColor;
        for (int i = 0; i < _currentTasksListElements.Count; i++)
            _currentTasksListElements[i].SetAlpha(1f);

        _taskContainer.gameObject.SetActive(true);
        _vmnc.enabled = true;
    }

    #endregion
}
