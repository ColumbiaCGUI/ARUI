using System.Collections.Generic;
using UnityEngine.Events;

public class MultipleChoiceDialog : DialogTemplate
{
    private List<string> _ids = new List<string> { "A", "B", "C", "D", "E" };
    private List<AcceptedSpeechInput> _speechInput = new List<AcceptedSpeechInput> { AcceptedSpeechInput.SelectA, AcceptedSpeechInput.SelectB, AcceptedSpeechInput.SelectC, AcceptedSpeechInput.SelectD, AcceptedSpeechInput.SelectE};
    private string _subTitle = "</b><i><size=0.006><color=#d3d3d3>Confirm by saying 'Select A', 'Select B',.. etc.</color></size></i>";

    public void InitDialog(string dialogMessage, List<string> optionLabels, List<UnityAction> onSelection, UnityAction onTimeOut, float timeOutInSeconds)
    {
        //init ui buttons
        var allButtons = transform.GetChild(0);
        for (int i = 0; i < _ids.Count; i++)
        {
            UnityEvent onSelect = new UnityEvent();

            DialogOption newOption = allButtons.GetChild(i).gameObject.AddComponent<DialogOption>();
            newOption.gameObject.name = "Option - " + _ids[i];
            var dwell = newOption.Init(i, _speechInput[i], allButtons.GetChild(i).GetChild(2).GetComponentInChildren<TMPro.TextMeshProUGUI>());
            dwell.InitializeButton(dwell.gameObject, () => OnUserSelectedAnOption(newOption), null, true, DwellButtonType.Select, true);
            dwell.gameObject.SetActive(false);

            _options.Add(newOption);
        }

        for (int i = 0; i < optionLabels.Count; i++)
        {
            _options[i].SetOnConfirmation(new List<UnityAction>() { onSelection[i] }, optionLabels[i]);
            _options[i].gameObject.SetActive(true);
        }

        if (optionLabels.Count < _ids.Count)
        {
            int j = optionLabels.Count;
            while (j < _ids.Count)
            {
                _options[j].gameObject.SetActive(false);
                j++;
            }
        }

        InitTemplate(
           OrbDialogType.MulitpleChoice,
           dialogMessage + "\n<b>", _subTitle,
           timeOutInSeconds,
           onTimeOut
           );
    }
}
