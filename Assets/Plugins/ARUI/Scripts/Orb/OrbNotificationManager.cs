using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OrbNotificationManager : MonoBehaviour
{
    ///****** Confirmation Notification
    private GameObject _confirmationNotificationPrefab = null;
    private Dictionary<int, GameObject> _allNotificationDialog = null;     /// <Reference to confirmation dialogue

    private GameObject _selectNotificationPrefab = null;

    // Start is called before the first frame update
    private void Start()
    {
        //Load resources for UI elements
        _confirmationNotificationPrefab = Resources.Load(StringResources.ConfNotificationOrb_path) as GameObject;
        _confirmationNotificationPrefab.gameObject.name = "***ARUI-ConfirmationNotification";

        _allNotificationDialog = new Dictionary<int, GameObject>();

        //Load resources for UI elements
        _selectNotificationPrefab = Resources.Load(StringResources.MultiSelectNotificationOrb_path) as GameObject;
        _selectNotificationPrefab.gameObject.name = "***ARUI-SelectNotification";

    }

    /// <summary>
    /// If confirmation action is set - SetUserIntentCallback(...) - and no confirmation window is active at the moment, the user is shown a 
    /// timed confirmation window. Recommended text: "Did you mean ...". If the user confirms the dialogue, the onUserIntentConfirmedAction action is invoked. 
    /// </summary>
    /// <param name="msg">Message that is shown in the Confirmation Dialogue</param>
    /// <param name="actionOnConfirmation">Action triggerd if the user confirms the dialogue</param>
    /// <param name="actionOnTimeOut">OPTIONAL - Action triggered if notification times out</param>
    public void TryGetUserConfirmation(string msg, List<UnityAction> actionOnConfirmation, UnityAction actionOnTimeOut, float timeOut)
    {
        if (_confirmationNotificationPrefab == null || !Utils.StringValid(msg)) return;

        GameObject window = Instantiate(_confirmationNotificationPrefab, transform);
        window.gameObject.name = "***ARUI-Confirmation-" + msg;
        window.transform.parent = transform;
        ConfirmationNotificationOrb dialogue = window.AddComponent<ConfirmationNotificationOrb>();
        _allNotificationDialog.Add(window.gameObject.GetInstanceID(), dialogue.gameObject);
        dialogue.InitNotification(msg, actionOnConfirmation, actionOnTimeOut, () => { DestroyWindow(window.gameObject.GetInstanceID()); }, timeOut) ;
    }

    public void TryGetUserChoice(string selectionMsg, List<string> choices, List<UnityAction> actionOnSelection, UnityAction actionOnTimeOut, float timeout)
    {
        if (choices.Count!= actionOnSelection.Count) return;

        GameObject window = Instantiate(_selectNotificationPrefab, transform);
        window.gameObject.name = "***ARUI-Multiselect-" + selectionMsg;
        window.transform.parent = transform;
        SelectNotificationOrb dialogue = window.AddComponent<SelectNotificationOrb>();
        _allNotificationDialog.Add(window.gameObject.GetInstanceID(), dialogue.gameObject);
        dialogue.InitNotification(selectionMsg, choices, actionOnSelection, actionOnTimeOut, () => { DestroyWindow(window.gameObject.GetInstanceID()); }, timeout);
    }

    private void DestroyWindow(int ID)
    {
        Destroy(_allNotificationDialog[ID]);
        _allNotificationDialog.Remove(ID);
    }
}
