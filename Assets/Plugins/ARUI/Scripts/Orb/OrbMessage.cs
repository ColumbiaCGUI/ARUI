using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the message container next to the orb
/// </summary>
public class OrbMessage : MonoBehaviour
{
    public enum MessageAnchor
    {
        left = 1, //message is left from the orb
        right = 2, //message is right from the orb
    }

    public bool Active => _textContainer.gameObject.activeSelf;
    public BoxCollider Collider => _textContainer.MessageCollider;

    private MessageAnchor _currentAnchor = MessageAnchor.right;

    private Color32 _glowColor = Color.white;
    private float _maxglowAlpha = 0.3f;
    private Color _activeColorText = Color.white;

    //*** Prefabs for notifications
    private Notification _currentNote;
    private Notification _currentWarning;

    public bool IsNoteActive => _currentNote.IsSet;
    public bool IsWarningActive => _currentWarning.IsSet;


    private bool _userHasSeenNewTask = false;
    public bool UserHasSeenNewTask
    {
        get { return _userHasSeenNewTask; }
    }

    private bool _isLookingAtMessage = false;
    public bool IsLookingAtMessage
    {
        get { return _isLookingAtMessage; }
    }

    private bool _isMessageVisible = false;
    public bool IsMessageVisible
    {
        get { return _isMessageVisible; }
    }

    private bool _isMessageFading = false;
    public bool IsMessageFading
    {
        get { return _isMessageFading; }
    }

    private bool _messageIsLerping = false;

    private FlexibleTextContainer _textContainer;
    private GameObject _indicator;
    private Vector3 _initialIndicatorPos;
    private float _initialmessageYOffset;

    private TMPro.TextMeshProUGUI _progressText;

    private TMPro.TextMeshProUGUI _prevText;
    private TMPro.TextMeshProUGUI _nextText;

    private DwellButton _taskListbutton;                     /// <reference to dwell btn above orb ('tasklist button')
    public DwellButton TaskListToggle
    {
        get => _taskListbutton;
    }
    public List<BoxCollider> GetAllColliders
    {
        get => new List<BoxCollider> {_taskListbutton.Collider, _textContainer.MessageCollider};
    } 

    private void Start()
    {
        _textContainer = transform.GetChild(1).gameObject.AddComponent<FlexibleTextContainer>();
        _textContainer.gameObject.name += "_orb";

        TMPro.TextMeshProUGUI[] allText = _textContainer.AllTextMeshComponents;

        _progressText = allText[1].gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        _progressText.text = "";

        _prevText = _textContainer.VGroup.GetChild(1).gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        _prevText.text = "";
        _nextText = _textContainer.VGroup.GetChild(2).gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        _nextText.text = "";

        _currentWarning = _textContainer.VGroup.GetChild(3).gameObject.AddComponent<Notification>();
        _currentWarning.init(NotificationType.warning, "", _textContainer.TextRect.height);

        _currentNote = _textContainer.VGroup.GetChild(4).gameObject.AddComponent<Notification>();
        _currentNote.init(NotificationType.note, "", _textContainer.TextRect.height);

        _initialmessageYOffset = _textContainer.transform.position.x;

        //message direction indicator
        _indicator = gameObject.GetComponentInChildren<Shapes.Polyline>().gameObject;
        _initialIndicatorPos = _indicator.transform.position;

        _glowColor = _textContainer.GlowColor;


        // Init tasklist button
        GameObject taskListbtn = transform.GetChild(2).gameObject;
        _taskListbutton = taskListbtn.AddComponent<DwellButton>();
        _taskListbutton.gameObject.name += "FacetasklistButton";
        _taskListbutton.InitializeButton(EyeTarget.orbtasklistButton, () => ToggleOrbTaskList(),
            null, true, DwellButtonType.Select);

        SetIsActive(false, false);
    }

    private void Update()
    {
        // Update eye tracking flag
        if (_isLookingAtMessage && EyeGazeManager.Instance.CurrentHit != EyeTarget.orbMessage && EyeGazeManager.Instance.CurrentHit != EyeTarget.orbtasklistButton)
            _isLookingAtMessage = false;
        else if (!_isLookingAtMessage && (EyeGazeManager.Instance.CurrentHit == EyeTarget.orbMessage || EyeGazeManager.Instance.CurrentHit == EyeTarget.orbtasklistButton))
            _isLookingAtMessage = true;

        _currentNote.UpdateSize(_textContainer.TextRect.width / 2);
        _currentWarning.UpdateSize(_textContainer.TextRect.width / 2);

        _currentNote.UpdateYPos(_textContainer.TextRect.height, _prevText.gameObject.activeSelf);
        _currentWarning.UpdateYPos(_textContainer.TextRect.height, _prevText.gameObject.activeSelf);

        _prevText.gameObject.transform.SetLocalYPos(_textContainer.TextRect.height);
        _nextText.gameObject.transform.SetLocalYPos(-_textContainer.TextRect.height);

        if (!(_isMessageVisible && Active) || _messageIsLerping) return;

        // Update messagebox anchor
        if (ChangeMessageBoxToRight(100))
            UpdateAnchorLerp(MessageAnchor.right);

        else if (ChangeMessageBoxToLeft(100))
            UpdateAnchorLerp(MessageAnchor.left);
    }

    #region Message and Notification Updates

    public void AddNotification(NotificationType type, string message)
    {
        if (type.Equals(NotificationType.note))
            _currentNote.SetMessage(message, ARUISettings.OrbNoteMaxCharCountPerLine);

        else if (type.Equals(NotificationType.warning))
            _currentWarning.SetMessage(message, ARUISettings.OrbMessageMaxCharCountPerLine);

        SetOrbListActive(false);
        _taskListbutton.IsDisabled = true;
    }

    public void RemoveNotification(NotificationType type)
    {
        if (type.Equals(NotificationType.note))
            _currentNote.SetMessage("", ARUISettings.OrbMessageMaxCharCountPerLine);
        else if (type.Equals(NotificationType.warning))
            _currentWarning.SetMessage("", ARUISettings.OrbMessageMaxCharCountPerLine);

        SetOrbListActive(!(IsNoteActive || IsWarningActive));
        _taskListbutton.IsDisabled = (IsNoteActive || IsWarningActive);
    }

    public void RemoveAllNotifications()
    {
        _currentNote.SetMessage("", ARUISettings.OrbMessageMaxCharCountPerLine);
        _currentWarning.SetMessage("", ARUISettings.OrbMessageMaxCharCountPerLine);

        SetOrbListActive(true);
        _taskListbutton.IsDisabled = false;
    }

    /// <summary>
    /// Turn on or off message fading
    /// </summary>
    /// <param name="active"></param>
    public void SetFadeOutMessage(bool active)
    {
        if (active)
        {
            StartCoroutine(FadeOutMessage());
        } else
        {
            StopCoroutine(FadeOutMessage());
            _isMessageFading = false;
            _textContainer.BackgroundColor = ARUISettings.OrbMessageBGColor;

            SetTextAlpha(1f);
        }
    }

    /// <summary>
    /// Fade out message from the moment the user does not look at the message anymore
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOutMessage()
    {
        float fadeOutStep = 0.001f;
        _isMessageFading = true;

        yield return new WaitForSeconds(1.0f);

        float shade = ARUISettings.OrbMessageBGColor.r;
        float alpha = 1f;

        while (_isMessageFading && shade > 0)
        {
            alpha -= (fadeOutStep * 20);
            shade -= fadeOutStep;

            if (alpha < 0)
                alpha = 0;
            if (shade < 0)
                shade = 0;

            _textContainer.BackgroundColor = new Color(shade, shade, shade, shade);
            SetTextAlpha(alpha);

            yield return new WaitForEndOfFrame();
        }

        _isMessageFading = false;

        if (shade <= 0)
        {
            SetIsActive(false, false);
            _isMessageVisible = false;
        }
    }

    private IEnumerator FadeNewTaskGlow()
    {
        SetFadeOutMessage(false);

        _userHasSeenNewTask = false;

        _textContainer.GlowColor = new Color(_glowColor.r, _glowColor.g, _glowColor.b, _maxglowAlpha);

        while (!_isLookingAtMessage)
        {
            yield return new WaitForEndOfFrame();
        }

        float step = (_maxglowAlpha / 10);
        float current = _maxglowAlpha;
        while (current > 0)
        {
            current -= step;
            _textContainer.GlowColor = new Color(_glowColor.r, _glowColor.g, _glowColor.b, current);
            yield return new WaitForSeconds(0.1f);
        }

        _textContainer.GlowColor = new Color(_glowColor.r, _glowColor.g, _glowColor.b, 0f);

        _userHasSeenNewTask = true;
    }

    #endregion

    #region Position Updates

    /// <summary>
    /// Updates the anchor of the messagebox smoothly
    /// </summary>
    /// <param name="MessageAnchor">The new anchor</param>
    public void UpdateAnchorLerp(MessageAnchor newMessageAnchor)
    {
        if (_messageIsLerping) return;

        if (newMessageAnchor != _currentAnchor)
        {
            _messageIsLerping = true;
            _currentAnchor = newMessageAnchor;
            UpdateBoxIndicatorPos();

            StartCoroutine(MoveMessageBox(_initialmessageYOffset, newMessageAnchor != MessageAnchor.right, false));
        }
    }

    /// <summary>
    /// Updates the anchor of the messagebox instantly (still need to run coroutine to allow the Hgroup rect to update properly
    /// </summary>
    private void UpdateAnchorInstant()
    {
        _textContainer.UpdateAnchorInstant();

        bool isLeft = false;
        if (ChangeMessageBoxToLeft(0))
        {
            _currentAnchor = MessageAnchor.left;
            isLeft = true;
        }
        else
            _currentAnchor = MessageAnchor.right;

        UpdateBoxIndicatorPos();
        StartCoroutine(MoveMessageBox(_initialmessageYOffset, isLeft, true));
    }

    /// <summary>
    /// Updates the position and orientation of the messagebox indicator
    /// </summary>
    private void UpdateBoxIndicatorPos()
    {
        if (_currentAnchor == MessageAnchor.right)
        {
            _indicator.transform.localPosition = new Vector3(_initialIndicatorPos.x, 0, 0);
            _indicator.transform.localRotation = Quaternion.identity;
        }
        else
        {
            _indicator.transform.localPosition = new Vector3(-_initialIndicatorPos.x, 0, 0);
            _indicator.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
    }

    /// <summary>
    /// Check if message box should be anchored right
    /// </summary>
    /// <param name="offsetPaddingInPixel"></param>
    /// <returns></returns>
    private bool ChangeMessageBoxToRight(float offsetPaddingInPixel)
    {
        return (AngelARUI.Instance.ARCamera.WorldToScreenPoint(transform.position).x < ((AngelARUI.Instance.ARCamera.pixelWidth * 0.5f) - offsetPaddingInPixel));
    }

    /// <summary>
    /// Check if message box should be anchored left
    /// </summary>
    private bool ChangeMessageBoxToLeft(float offsetPaddingInPixel)
    {
        return (AngelARUI.Instance.ARCamera.WorldToScreenPoint(transform.position).x > ((AngelARUI.Instance.ARCamera.pixelWidth * 0.5f) + offsetPaddingInPixel));
    }

    /// <summary>
    /// Lerps the message box to the other side
    /// </summary>
    /// <param name="YOffset">y offset of the message box to the orb prefab</param>
    /// <param name="addWidth"> if messagebox on the left, change the signs</param>
    /// <param name="instant">if lerp should be almost instant (need to do this in a coroutine anyway, because we are waiting for the Hgroup to update properly</param>
    /// <returns></returns>
    IEnumerator MoveMessageBox(float YOffset, bool isLeft, bool instant)
    {
        float initialYOffset = YOffset;
        float step = 0.1f;

        if (instant)
            step = 0.5f;

        while (step < 1)
        {
            if (isLeft)
                YOffset = -initialYOffset - _textContainer.MessageCollider.size.x;

            _textContainer.transform.localPosition = Vector2.Lerp(_textContainer.transform.localPosition, new Vector3(YOffset, 0, 0), step += Time.deltaTime);
            step += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        if (isLeft)
            _taskListbutton.transform.SetLocalXPos(-0.043f);
        else
            _taskListbutton.transform.SetLocalXPos(0.043f);

        _messageIsLerping = false;
    }

    private void ToggleOrbTaskList()
    {
        SetOrbListActive(!_prevText.gameObject.activeSelf);
    }

    #endregion

    #region Getter and Setter

    /// <summary>
    /// Actives or disactivates the messagebox of the orb in the hierarchy
    /// </summary>
    /// <param name="active"></param>
    public void SetIsActive(bool active, bool newTask)
    {
        _textContainer.gameObject.SetActive(active);
        _indicator.SetActive(active);

        if (active)
        {
            UpdateAnchorInstant();
            _textContainer.BackgroundColor = ARUISettings.OrbMessageBGColor;
            SetTextAlpha(1f);
        }
        else
            _isMessageFading = false;

        _isMessageVisible = active;

        if (newTask)
        {
            StartCoroutine(FadeNewTaskGlow());
            RemoveAllNotifications();
        }

        _taskListbutton.gameObject.SetActive(active);
    }

    /// <summary>
    /// Sets the orb task message to the given message and adds line break based on maxCharCountPerLine
    /// </summary>
    /// <param name="message"></param>
    public void SetTaskMessage(string message, string previous, string next)
    {
        _textContainer.Text = message;
        _progressText.text = TaskListManager.Instance.GetCurrentTaskID() + "/" + TaskListManager.Instance.GetTaskCount();

        _prevText.text = "";
        _nextText.text = "";
        if (previous.Length > 0)
            _prevText.text = "<b>DONE:</b> " + Utils.SplitTextIntoLines(previous, ARUISettings.OrbMessageMaxCharCountPerLine);

        if (next.Length > 0)
            _nextText.text = "<b>Upcoming:</b> " + Utils.SplitTextIntoLines(next, ARUISettings.OrbNoteMaxCharCountPerLine);

        UpdateAnchorInstant();

        _progressText.gameObject.SetActive(!message.Contains("Done"));
    }

    private void SetOrbListActive(bool active)
    {
        _prevText.gameObject.SetActive(active);
        _nextText.gameObject.SetActive(active);

        if (active)
        {
            _textContainer.MessageCollider.size = new Vector3(_textContainer.MessageCollider.size.x, 0.08f, _textContainer.MessageCollider.size.z);
        } else
        {
            _textContainer.MessageCollider.size = new Vector3(_textContainer.MessageCollider.size.x, 0.05f, _textContainer.MessageCollider.size.z);
        }
    }

    /// <summary>
    /// Update the color of the text based on visibility
    /// </summary>
    /// <param name="alpha"></param>
    private void SetTextAlpha(float alpha)
    {
        if (alpha == 0)
            _textContainer.TextColor = new Color(0, 0, 0, 0);
        else
            _textContainer.TextColor = new Color(_activeColorText.r, _activeColorText.g, _activeColorText.b, alpha);
    }

    #endregion
}