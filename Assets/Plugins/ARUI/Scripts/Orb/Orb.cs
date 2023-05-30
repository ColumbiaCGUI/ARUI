using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a virtual assistant in the shape of an orb, staying in the FOV of the user and
/// guiding the user through a sequence of tasks
/// </summary>
public class Orb : Singleton<Orb>
{
    ///** Reference to parts of the orb
    private OrbFace _face;                                   /// <the orb shape itself (part of prefab)
    public float MouthScale
    {
        get => _face.MouthScale;
        set => _face.MouthScale = value;
    }

    private OrbGrabbable _grabbable;                         /// <reference to grabbing behavior
    public OrbMessage _messageContainer;                     /// <reference to orb message container (part of prefab)
    private DwellButton _taskListbutton;                     /// <reference to dwell btn above orb ('tasklist button')

    private List<BoxCollider> _allOrbColliders;              /// <reference to all collider - will be merged for view management.
    public List<BoxCollider> AllOrbColliders => _allOrbColliders;

    ///** Placement behaviors - overall, orb stays in the FOV of the user
    private OrbFollowerSolver _followSolver;

    ///** Flags
    private bool _isLookingAtOrb = false;                    /// <true if the user is currently looking at the orb shape or orb message
    private bool _lazyLookAtRunning = false;                 /// <used for lazy look at disable
    private bool _lazyFollowStarted = false;                 /// <used for lazy following

    /// <summary>
    /// Get all orb references from prefab
    /// </summary>
    private void Awake()
    {
        gameObject.name = "***ARUI-Orb";
        _face = transform.GetChild(0).GetChild(0).gameObject.AddComponent<OrbFace>();
        
        // Get message object in orb prefab
        GameObject messageObj = transform.GetChild(0).GetChild(1).gameObject;
        _messageContainer = messageObj.AddComponent<OrbMessage>();

        // Get grabbable and following scripts
        _followSolver = gameObject.GetComponentInChildren<OrbFollowerSolver>();
        _grabbable = gameObject.GetComponentInChildren<OrbGrabbable>();

        // Init tasklist button
        GameObject taskListbtn = transform.GetChild(0).GetChild(2).gameObject;
        _taskListbutton = taskListbtn.AddComponent<DwellButton>();
        _taskListbutton.gameObject.name += "FacetasklistButton";
        _taskListbutton.InitializeButton(EyeTarget.orbtasklistButton, () => TaskListManager.Instance.ToggleTasklist(), 
            () => TaskListManager.Instance.Reposition(), false, DwellButtonType.Toggle);
        taskListbtn.SetActive(false);

        BoxCollider taskListBtnCol = transform.GetChild(0).GetComponent<BoxCollider>();

        // Collect all orb colliders
        _allOrbColliders = new List<BoxCollider>() { taskListBtnCol, _taskListbutton.Collider };
    }

    /// <summary>
    /// Update visibility of orb based on eye evets and task manager.
    /// </summary>
    private void Update()
    {
        // Update eye tracking flag
        if (_isLookingAtOrb && EyeGazeManager.Instance.CurrentHit != EyeTarget.orbFace)
            SetIsLookingAtFace(false);
        else if (!_isLookingAtOrb && EyeGazeManager.Instance.CurrentHit == EyeTarget.orbFace)
            SetIsLookingAtFace(true);

        if (_messageContainer.UserHasNotSeenNewTask && _messageContainer.IsLookingAtMessage)
            _face.NotificationEnabled = false;

        UpdateOrbVisibility();
    }


    #region Visibility, Position Updates and eye/collision event handler

    /// <summary>
    /// View management
    /// Update the visibility of the orb message based on eye gaze collisions with the orb collider 
    /// </summary>
    private void UpdateOrbVisibility()
    {
        if (TaskListManager.Instance.GetTaskCount() != 0)
        {
            if ((IsLookingAtOrb(false) && !_messageContainer.IsMessageVisible && !_messageContainer.IsMessageFading))
            { //Set the message visible!
                _messageContainer.SetIsActive(true, false);
            }
            else if (!_messageContainer.IsLookingAtMessage && !IsLookingAtOrb(false) && _followSolver.IsOutOfFOV)
            {
                _messageContainer.SetIsActive(false, false);
            }
            else if ((_messageContainer.IsLookingAtMessage || IsLookingAtOrb(false)) && _messageContainer.IsMessageVisible && _messageContainer.IsMessageFading)
            { //Stop Fading, set the message visible
                _messageContainer.SetFadeOutMessage(false);
            }
            else if (!IsLookingAtOrb(false) && _messageContainer.IsMessageVisible && !_messageContainer.IsMessageFading
                && !_messageContainer.IsLookingAtMessage && !_messageContainer.UserHasNotSeenNewTask && !_messageContainer.IsNotificationActive)
            { //Start Fading
                _messageContainer.SetFadeOutMessage(true);
            }
        } 
    } 

    /// <summary>
    /// If the user drags the orb, the orb will stay in place until it will be out of FOV
    /// </summary>
    private IEnumerator EnableLazyFollow()
    {
        _lazyFollowStarted = true;

        yield return new WaitForEndOfFrame();

        _followSolver.IsPaused = (true);

        while (_grabbable.transform.position.InFOV(AngelARUI.Instance.ARCamera))
            yield return new WaitForSeconds(0.1f);

        _followSolver.IsPaused = (false);
        _lazyFollowStarted = false;
    }

    /// <summary>
    /// Make sure that fast eye movements are not detected as dwelling
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartLazyLookAt()
    {
        yield return new WaitForSeconds(0.2f);

        if (_lazyLookAtRunning)
        {
            _isLookingAtOrb = true;
            _lazyLookAtRunning = false;
            _face.UserIsLooking = true;

            AudioManager.Instance.PlaySound(_face.transform.position, SoundType.select);
        }
    }

    /// <summary>
    /// Called if input events with hand collider are detected
    /// </summary>
    /// <param name="isDragging"></param>
    public void SetIsDragging(bool isDragging)
    {
        _face.UserIsGrabbing = isDragging;
        _followSolver.IsPaused = (isDragging);

        if (!isDragging && !_lazyFollowStarted)
        {
            StartCoroutine(EnableLazyFollow());
        }

        if (isDragging && _lazyFollowStarted)
        {
            StopCoroutine(EnableLazyFollow());

            _lazyFollowStarted = false;
            _followSolver.IsPaused = (false);
        }
    }

    /// <summary>
    /// Called if changes in eye events are detected
    /// </summary>
    /// <param name="isLooking"></param>
    private void SetIsLookingAtFace(bool isLooking)
    {
        if (isLooking && !_lazyLookAtRunning)
        {
            _lazyLookAtRunning = true;
            StartCoroutine(StartLazyLookAt());
        }
        else if (!isLooking)
        {
            if (_lazyLookAtRunning)
                StopCoroutine(StartLazyLookAt());

            _isLookingAtOrb = false;
            _lazyLookAtRunning = false;
            _face.UserIsLooking= false;
        }

    }

    #endregion

    #region Task Messages and Notifications

    /// <summary>
    /// Set the notification messages the orb communicates, if 'message' is less than 2 char, the message is deactivated
    /// </summary>
    /// <param name="message"></param>
    public void SetNotificationMessage(string message)
    {
        if (message.Length <= 1)
        {
            _messageContainer.IsNotificationActive = false ;
            _face.NotificationEnabled = false;
            _followSolver.MoveToCenter(false);
        }
        else
        {
            _messageContainer.SetIsActive(true, false);
            _messageContainer.IsNotificationActive = true;
            _face.NotificationEnabled = true;
            _followSolver.MoveToCenter(true);

            AudioManager.Instance.PlaySound(_face.transform.position, SoundType.warning);
        }

        _messageContainer.SetNotificationMessage(message);
    }

    /// <summary>
    /// Set the task messages the orb communicates, if 'message' is less than 2 char, the message is deactivated
    /// </summary>
    /// <param name="message"></param>
    public void SetTaskMessage(string message)
    {
        if (message.Length <= 1)
            _messageContainer.SetIsActive(false, false);
        else
        {
            _messageContainer.SetIsActive(true, true);
            _face.NotificationEnabled= true;

            AudioManager.Instance.PlayText(message);
        }

        _messageContainer.SetTaskMessage(message);

        SetNotificationMessage("");

        if (!_allOrbColliders.Contains(_messageContainer.Collider))
            _allOrbColliders.Add(_messageContainer.Collider);
    }

    public void ResetToggleBtn() => _taskListbutton.Toggled = false;

    #endregion

    #region Getter and Setter

    /// <summary>
    /// Access to collider of orb (including task message)
    /// </summary>
    /// <returns>The box collider of the orb message, if the message is not active, returns null</returns>
    public BoxCollider GetCurrentMessageCollider()
    {
        if (_messageContainer.Active)
            return _messageContainer.Collider;
        else
            return null;
    }

    /// <summary>
    /// Detect hand hovering events
    /// </summary>
    /// <param name="isHovering"></param>
    public void SetNearHover(bool isHovering) => _face.UserIsGrabbing = isHovering;

    /// <summary>
    /// Change the visibility of the tasklist button
    /// </summary>
    /// <param name="isActive"></param>
    public void SetTaskListButtonActive(bool isActive) => _taskListbutton.gameObject.SetActive(isActive);

    /// <summary>
    /// Update the position behavior of the orb
    /// </summary>
    /// <param name="isSticky"></param>
    public void SetSticky(bool isSticky)
    {
        _followSolver.SetSticky(isSticky);

        if (isSticky)
            _messageContainer.SetIsActive(false, false);
    }

    /// <summary>
    /// Check if user is looking at orb. - includes orb message and task list button if 'any' is true. else only orb face and message
    /// </summary>
    /// <param name="any">if true, subobjects of orb are inlcluded, else only face and message</param>
    /// <returns></returns>
    public bool IsLookingAtOrb(bool any)
    {
        if (any)
            return _isLookingAtOrb || _messageContainer.IsLookingAtMessage || _taskListbutton.IsInteractingWithBtn;
        else
            return _isLookingAtOrb || _messageContainer.IsLookingAtMessage;
    }

    #endregion

}
