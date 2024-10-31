using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Dialogue that asks for user confirmation of a given action. Used for the Natural Language Interface.
/// The user has timeInSeconds seconds to decide if the given action should be executed. Confirmation can be done by
/// looking at the button or touching it.
/// </summary>
public class ConfirmationDialog : DialogTemplate
{
    private string _subTitle = "<i><size=0.006><color=#d3d3d3>Confirm by saying 'Select Okay'</color></size></i>";
    public void InitDialog(string dialogMessage, List<UnityAction> onConfirmation, UnityAction onTimeOut, float timeOutInSeconds)
    {
        var okaySelect = new UnityEvent();
        DialogOption okayOption = transform.GetChild(0).gameObject.AddComponent<DialogOption>();
        okayOption.gameObject.name = "Option - Okay";
        var dwell = okayOption.Init(0, AcceptedSpeechInput.SelectOkay);

        // use localIndex 0 because the confirmationdialog only has one option (okay btn)
        dwell.InitializeButton(dwell.gameObject, () => OnUserSelectedAnOption(okayOption), null, true, DwellButtonType.Select, true);

        _options.Add(okayOption);
        okayOption.SetOnConfirmation(onConfirmation);

        InitTemplate(
           OrbDialogType.OkayChoice,
           dialogMessage + "\n<b>", _subTitle,
           timeOutInSeconds,
           onTimeOut
           );
    }
}