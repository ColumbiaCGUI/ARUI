using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogOption : MonoBehaviour
{
    private DwellButton _btn;
    public DwellButton Btn => _btn;

    private List<UnityAction> _onOptionSelectedAction = new List<UnityAction>();
    private List<UnityEvent> _onOptionSelectedEvent = new List<UnityEvent>();
    public List<UnityAction> OnOptionSelectedAction => _onOptionSelectedAction;
    public List<UnityEvent> OnOptionSelectedEvent => _onOptionSelectedEvent;

    private int _localIndex = 0;
    public int LocalID
    {
        get { return _localIndex; }
    }

    private AcceptedSpeechInput _speechInput;
    public AcceptedSpeechInput SpeechInput
    {
        get { return _speechInput; }
    }

    private TMPro.TextMeshProUGUI _optionLabel;

    public DwellButton Init(int localIndex, AcceptedSpeechInput speechInputID, TMPro.TextMeshProUGUI label = null)
    {
        _optionLabel = label;
        _speechInput = speechInputID;
        _localIndex = localIndex;

        _btn = gameObject.AddComponent<DwellButton>();
        return _btn;
    }

    public void SetOnSelectEvents() { }

    public void SetOnConfirmation(List<UnityAction> unityAction, string label = "")
    {
        for (int i = 0; i < unityAction.Count; i++)
        {
            _onOptionSelectedAction.Add(unityAction[i]);

            _onOptionSelectedEvent.Add(new UnityEvent());
            _onOptionSelectedEvent[i].AddListener(unityAction[i]);
        }

        if (_optionLabel != null && label.Length>0)
            _optionLabel.text = label;
    }
}
