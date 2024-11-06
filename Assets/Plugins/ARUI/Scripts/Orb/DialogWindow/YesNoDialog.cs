using System.Collections.Generic;
using UnityEngine.Events;

public class YesNoDialog : DialogTemplate
{
    private string _subTitle = "</b><i><size=0.006><color=#d3d3d3>Confirm by saying 'Select Yes' or 'Select No'</color></size></i>";

    public void InitDialog(string dialogMessage, UnityAction onYes, UnityAction onNo, UnityAction onTimeOut, float timeOutInSeconds)
    {
        var allButtons = transform.GetChild(0);

        var onYesSelect = new UnityEvent();
        DialogOption yesOption = allButtons.GetChild(0).gameObject.AddComponent<DialogOption>();
        yesOption.gameObject.name = "Option - Yes";
        var dwell = yesOption.Init(0, AcceptedSpeechInput.SelectYes);
        // use localIndex 0 because the yes button is the first in the local _options list
        dwell.InitializeButton(dwell.gameObject, () => OnUserSelectedAnOption(yesOption), null, true, DwellButtonType.Select, true);
        
        var onNoSelect = new UnityEvent();
        DialogOption noOption = allButtons.GetChild(0).gameObject.AddComponent<DialogOption>();
        noOption.gameObject.name = "Option - No";
        dwell = noOption.Init(1, AcceptedSpeechInput.SelectNo);
        // use localIndex 1 because the no button is the second in the local _options list
        dwell.InitializeButton(dwell.gameObject, () => OnUserSelectedAnOption(noOption), null, true, DwellButtonType.Select, true);

        _options.Add(yesOption);
        yesOption.SetOnConfirmation(new List<UnityAction>() { onYes });

        _options.Add(noOption);
        noOption.SetOnConfirmation(new List<UnityAction>() { onNo });

        InitTemplate(
           OrbDialogType.YesNo,
           dialogMessage + "\n<b>", _subTitle,
           timeOutInSeconds,
           onTimeOut
           );
    }

}
