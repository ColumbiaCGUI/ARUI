using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NotificationManager : Singleton<NotificationManager>
{
    ///****** Confirmation Notification
    private GameObject _confirmationWindowPrefab = null;
    private List<ConfirmationDialogue> _confirmationWindows = null;     /// <Reference to confirmation dialogue

    // Start is called before the first frame update
    private void Start()
    {
        //Load resources for UI elements
        _confirmationWindowPrefab = Resources.Load(StringResources.ConfNotification_path) as GameObject;
        _confirmationWindowPrefab.gameObject.name = "***ARUI-" + StringResources.confirmationWindow_name;

        _confirmationWindows = new List<ConfirmationDialogue>();
    }

    /// <summary>
    /// If confirmation action is set - SetUserIntentCallback(...) - and no confirmation window is active at the moment, the user is shown a 
    /// timed confirmation window. Recommended text: "Did you mean ...". If the user confirms the dialogue, the onUserIntentConfirmedAction action is invoked. 
    /// </summary>
    /// <param name="msg">Message that is shown in the Confirmation Dialogue</param>
    /// <param name="actionOnConfirmation">Action triggerd if the user confirms the dialogue</param>
    /// <param name="actionOnTimeOut">OPTIONAL - Action triggered if notification times out</param>
    public void TryGetUserConfirmation(string msg, UnityAction actionOnConfirmation, UnityAction actionOnTimeOut)
    {
        if (_confirmationWindowPrefab == null || !Utils.StringValid(msg)) return;

        GameObject window = Instantiate(_confirmationWindowPrefab, transform);
        window.gameObject.name = "***ARUI-Confirmation-" + msg;
        ConfirmationDialogue dialogue = window.AddComponent<ConfirmationDialogue>();
        _confirmationWindows.Add(dialogue);
        dialogue.InitializeConfirmationNotification(msg, actionOnConfirmation, actionOnTimeOut);
    }
}
