using System.Collections;
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

    //*** Flexible Textbox for Notification Message
    private RectTransform _notificationMessageRect;
    private TMPro.TextMeshProUGUI _textNotification;

    private Color32 _glowColor = Color.white;
    private float _maxglowAlpha = 0.3f;
    private Color _activeColorText = Color.white;

    //Flags
    private bool _isNotificationActive = false;
    public bool IsNotificationActive
    {
        get => _isNotificationActive;
        set { SetNotificationTextActive(value); }
    }

    private bool _userHasNotSeenNewTask = false;
    public bool UserHasNotSeenNewTask
    {
        get { return _userHasNotSeenNewTask; }
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

    private void Start()
    {
        _textContainer = transform.GetChild(1).gameObject.AddComponent<FlexibleTextContainer>();
        _textContainer.gameObject.name += "_orb";

        TMPro.TextMeshProUGUI[] allText = _textContainer.AllTextMeshComponents;

        _progressText = allText[1].gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        _progressText.text = "";

        _initialmessageYOffset = _textContainer.transform.position.x;

        //init notification message group
        _notificationMessageRect = allText[2].gameObject.GetComponent<RectTransform>();
        _textNotification = _notificationMessageRect.gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        _textNotification.text = "";
        _notificationMessageRect.gameObject.SetActive(false);

        //message direction indicator
        _indicator = gameObject.GetComponentInChildren<Shapes.Polyline>().gameObject;
        _initialIndicatorPos = _indicator.transform.position;

        _glowColor = _textContainer.GlowColor;

        SetIsActive(false, false);
    }

    private void Update()
    {
        // Update eye tracking flag
        if (_isLookingAtMessage && EyeGazeManager.Instance.CurrentHit != EyeTarget.orbMessage)
            _isLookingAtMessage = false;
        else if (!_isLookingAtMessage && EyeGazeManager.Instance.CurrentHit == EyeTarget.orbMessage)
            _isLookingAtMessage = true;

        _notificationMessageRect.sizeDelta = new Vector2(_textContainer.TextRect.width / 2, _notificationMessageRect.rect.height);

        if (!(_isMessageVisible && Active) || _messageIsLerping) return;

        // Update messagebox anchor
        if (ChangeMessageBoxToRight(100))
            UpdateAnchorLerp(MessageAnchor.right);

        else if (ChangeMessageBoxToLeft(100))
            UpdateAnchorLerp(MessageAnchor.left);
    }

    #region Message and Notification Updates

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

        _userHasNotSeenNewTask = true;

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
        _userHasNotSeenNewTask = false;
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

        _messageIsLerping = false;
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
            if (_isNotificationActive)
                SetNotificationMessage("");
        }
    }

    /// <summary>
    /// Sets the orb task message to the given message and adds line break based on maxCharCountPerLine
    /// </summary>
    /// <param name="message"></param>
    public void SetTaskMessage(string message)
    {
        _textContainer.Text = message;
        _progressText.text = TaskListManager.Instance.GetCurrentTaskID() + "/" + TaskListManager.Instance.GetTaskCount();

        if (message.Contains("Done"))
        {
            _progressText.gameObject.SetActive(false);
        }
        else
        {
            _progressText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Sets the orb notification message to the given message and adds line break based on maxCharCountPerLine
    /// </summary>
    /// <param name="message"></param>
    public void SetNotificationMessage(string message) => _textNotification.text = Utils.SplitTextIntoLines(message, ARUISettings.OrbMessageMaxCharCountPerLine);

    /// <summary>
    /// Update the visibility of the notification message
    /// </summary>
    /// <param name="isActive"></param>
    private void SetNotificationTextActive(bool isActive)
    {
        _notificationMessageRect.gameObject.SetActive(isActive);
        _isNotificationActive = isActive;

        if (!isActive)
            _textNotification.text = "";

        if (isActive)
            _notificationMessageRect.transform.SetLocalYPos(_textContainer.TextRect.height / 2);
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