using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Dialogue that asks for user confirmation of a given action. Used for the Natural Language Interface.
/// The user has timeInSeconds seconds to decide if the given action should be executed. Confirmation can be done by
/// looking at the button or touching it.
/// </summary>
public class SelectNotificationOrb : MonoBehaviour
{
    private bool _init = false;                      /// <true if dialogue was initialized (e.g. message, event)
    private bool _timerStarted = false;              /// <true if timer started already

    private FlexibleTextContainer _textContainer;
    private List<DwellButton> _buttons = new List<DwellButton>();
    private List<UnityEvent> _choiceEvents = new List<UnityEvent>();
    private List<string> _choiceLabels;

    private UnityEvent _timeOutEvent;                /// <Event that will be invoked if the notification timesout
    private UnityEvent _selfDestruct;

    private Shapes.Line _time;                       /// <Line that shows the user how much time is left to make a decision

    private float _timeOutInSeconds = 10;
    private int _maxChoices = 5;
    private void Awake()
    {
        _textContainer = transform.GetChild(1).GetChild(0).gameObject.AddComponent<FlexibleTextContainer>();

        var allButtons = transform.GetChild(0);
        int i = 0;
        for (i = 0; i<_maxChoices; i++)
        {
            UnityEvent current = new UnityEvent();
            _choiceEvents.Add(current);

            DwellButton dwell = allButtons.GetChild(i).gameObject.AddComponent<DwellButton>();
            dwell.InitializeButton(dwell.gameObject, () => Confirmed(true, current), null, true, DwellButtonType.Select, true);
            dwell.gameObject.SetActive(false);

            _buttons.Add(dwell);
        }
        
        _time = _textContainer.transform.GetComponentInChildren<Shapes.Line>();
        _time.enabled = false;

        _timeOutEvent = new UnityEvent();
        _selfDestruct = new UnityEvent();

        transform.SetLayerAllChildren(StringResources.LayerToInt(StringResources.UI_layer));

        _choiceLabels = new List<string> { "A", "B", "C", "D", "E"};
    }


    /// <summary>
    /// Start the timer if the dialogue is initialized and the timer is not running yet.
    /// </summary>
    private void Update()
    {
        if (_init & !_timerStarted && _textContainer.TextRect.width > 0.001f)
            StartCoroutine(DecreaseTime());
    }

    /// <summary>
    /// Initialize the dialgoue components - text and confirmation event
    /// </summary>
    /// <param name="intentMsg">Contains message that is shown to the user.</param>
    /// <param name="confirmedEvent">confirmation event, invoked when the user is triggering the okay button</param>
    public void InitNotification(string selectionMsg, List<string> choiceMsg, List<UnityAction> confirmedEventPerChoice, UnityAction actionOnTimeOut, UnityAction selfDestruct, float timeout)
    {
        if (choiceMsg.Count!=confirmedEventPerChoice.Count) return;

        _timeOutInSeconds = timeout;
        string dialogText = selectionMsg + "\n";
        int i = 0;
        foreach (string choice in choiceMsg)
        {
            dialogText += _choiceLabels[i]+" : "+ choice + ", ";
            _choiceEvents[i].AddListener(confirmedEventPerChoice[i]);
            _buttons[i].gameObject.SetActive(true);
            i++;
        }
        _textContainer.Text = dialogText;

        if (i<_maxChoices)
        {
            int j = i;
            while (j < _maxChoices)
            {
                _buttons[j].gameObject.SetActive(false);
                j++;
            }
        }

        _selfDestruct.AddListener(selfDestruct);
        if (actionOnTimeOut != null)
            _timeOutEvent.AddListener(actionOnTimeOut);

        _init = true;
    }

    /// <summary>
    /// Called if the user either actively confirmed, or passively did not confirm the dialogue.
    /// if isConfirmed is true, the event assigned to the dialogue during initialization is triggered
    /// </summary>
    /// <param name="isConfirmed">true if confirmed by user, else false</param>
    private void Confirmed(bool isConfirmed, UnityEvent confirmationEvent)
    {
        if (isConfirmed)
        {
            AngelARUI.Instance.DebugLogMessage("The user selected.", true);
            confirmationEvent.Invoke();
        }
        else
        {
            AngelARUI.Instance.DebugLogMessage("The user did not confirm the mulitple choice", true);

            if (_timeOutEvent != null)
                _timeOutEvent.Invoke();
        }

        StopCoroutine(DecreaseTime());
        _selfDestruct.Invoke();
    }

    private IEnumerator DecreaseTime()
    {
        AudioManager.Instance.PlaySound(transform.position, SoundType.select);

        _timerStarted = true;

        _time.enabled = true;

        //_okBtn.transform.localPosition = _textContainer.transform.localPosition + new Vector3(_textContainer.TextRect.width + _okBtn.Width/2, 0, 0);

        _time.Start = new Vector3(0, _textContainer.TextRect.height/2, 0);
        _time.End = new Vector3(_textContainer.TextRect.width, _textContainer.TextRect.height / 2, 0);
        Vector3 xEnd = _time.End;

        yield return new WaitForFixedUpdate();
        float timeElapsed= 0.00001f;
        float lerpDuration = _timeOutInSeconds;

        while (timeElapsed < lerpDuration)
        {
            yield return new WaitForEndOfFrame();

            bool isInteractingWithAnyChoice = false;

            foreach (var btn in _buttons)
            {
                isInteractingWithAnyChoice = btn.IsInteractingWithBtn || isInteractingWithAnyChoice;
            }

            if (!isInteractingWithAnyChoice)
            {
                _time.End = Vector3.Lerp(_time.Start, xEnd, 1 - (timeElapsed / lerpDuration));
                timeElapsed += Time.deltaTime;
            }
        }

        Confirmed(false, null);
    }
}
