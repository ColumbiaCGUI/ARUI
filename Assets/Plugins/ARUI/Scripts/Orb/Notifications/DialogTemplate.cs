using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum OrbDialogType
{
    MulitpleChoice = 0,
    OkayChoice = 1,
    YesNo = 2
}
public abstract class DialogTemplate : MonoBehaviour
{
    protected bool _init = false;                      /// <true if dialogue was initialized (e.g. message, event)
    protected bool _selected = false;

    public OrbDialogType Type = OrbDialogType.OkayChoice;
    protected FlexibleTextContainer _dialogMsgContainer;

    protected Shapes.Line _timeLine;
    protected bool _timerStarted = false;              /// <true if timer started already
    
    protected float _timeOutInSeconds = 10;
    protected UnityEvent _timeOutEvent;                /// <Event that will be invoked if the notification timesout

    protected List<DialogOption> _options = new List<DialogOption>();

    public void InitTemplate(
        OrbDialogType type, 
        string dialogMessage, string subTitle, float timeOutInSeconds,
        UnityAction onTimeOut)
    {
        Type = type;
        _dialogMsgContainer = transform.GetChild(1).GetChild(0).gameObject.AddComponent<FlexibleTextContainer>();
        _dialogMsgContainer.Text = dialogMessage;
        _dialogMsgContainer.AddShortLineToText(subTitle);

        _timeLine = transform.GetChild(1).GetComponentInChildren<Shapes.Line>();
        _timeLine.gameObject.SetActive(false);

        _timeOutInSeconds = timeOutInSeconds;
        _timeOutEvent = new UnityEvent();
        if (onTimeOut != null)
            _timeOutEvent.AddListener(onTimeOut);

        transform.SetLayerAllChildren(StringResources.LayerToInt(StringResources.UI_layer));

        _init = true;
    }

    public void ShowDialog() => StartCoroutine(DecreaseTime());

    public void ConfirmedViaSpeech(AcceptedSpeechInput input)
    {
        foreach (var option in _options)
        {
            if (option.SpeechInput.Equals(input))
            {
                OnUserSelectedAnOption(option);
                break;
            }
        }
    }

    public void OnUserSelectedAnOption(DialogOption selected)
    {
        bool validInvoke = false;

        foreach (var onConfirmationEvent in selected.OnOptionSelectedEvent)
        {
            onConfirmationEvent.Invoke();
            validInvoke = true;
        }

        if (validInvoke)
        {
            AngelARUI.Instance.DebugLogMessage("The user selected " 
                + selected.gameObject.name + " at "
                +_dialogMsgContainer.Text, true);

            AudioManager.Instance.PlaySound(transform.position, SoundType.actionConfirmation);
            _selected = true;
            DialogManager.Instance.DestructDialog(this);
        }
    }

    private IEnumerator DecreaseTime()
    {
        AudioManager.Instance.PlaySound(transform.position, SoundType.select);

        while (_dialogMsgContainer.TextRect.width < 0.0001f)
        {
            yield return new WaitForFixedUpdate();
        }

        _timeLine.gameObject.SetActive(true);

        _timerStarted = true;
        _timeLine.enabled = true;

        _timeLine.Start = new Vector3(0, 0, 0);
        _timeLine.End = new Vector3(_dialogMsgContainer.TextRect.width, 0, 0);
        Vector3 xEnd = _timeLine.End;

        yield return new WaitForFixedUpdate();
        float timeElapsed = 0.00001f;
        float lerpDuration = _timeOutInSeconds;

        while (timeElapsed < lerpDuration && !_selected)
        {
            yield return new WaitForEndOfFrame();

            bool interacting = false;
            foreach(var option in _options) { 
                interacting = interacting || option.Btn.IsInteractingWithBtn;
            }

            if (!interacting && !_dialogMsgContainer.IsLookingAtText)
            {
                _timeLine.End = Vector3.Lerp(_timeLine.Start, xEnd, 1 - (timeElapsed / lerpDuration));
                timeElapsed += Time.deltaTime;
            }
        }

        if (!_selected) //time ran out!
        {
            if (_timeOutEvent != null)
                _timeOutEvent.Invoke();

            AngelARUI.Instance.DebugLogMessage("Timeout for dialog "
                + _dialogMsgContainer.Text, true);

            yield return new WaitForEndOfFrame();
            DialogManager.Instance.DestructDialog(this);
        }
    }
}
