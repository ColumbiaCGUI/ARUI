using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum AcceptedSpeechInput
{
    SelectA = 0, SelectB = 1, SelectC = 2, SelectD = 3, SelectE = 4,
    SelectYes = 5, SelectNo = 6,
    SelectOkay = 7,
}

/// <summary>
/// Manages the creation, lifecycle, and interactions of different types of dialogs in the AR UI system.
/// </summary>
/// <remarks>
/// This class serves as a singleton that facilitates the instantiation and management of multiple types of dialogs such as Confirmation, Yes/No, and Multiple Choice dialogs.
/// It keeps track of active dialogs and allows the UI to interact with users effectively.
/// </remarks>
public class DialogManager : Singleton<DialogManager>
{
    private GameObject _confirmationDialogPrefab;
    private GameObject _yesNoDialogPrefab;
    private GameObject _multipleChoiceDialogPrefab;

    // Dictionary to keep track of all the currently active dialogs
    private Queue<DialogTemplate> _currentActiveDialogs = new Queue<DialogTemplate>();
    public int CurrentActiveDialogs => _currentActiveDialogs.Count;

    public bool IsLookingAtAnyDialog
    {
        get
        {
           foreach (var dialog in _currentActiveDialogs)
                if (dialog.LookingAtDialog) return true;

            return false;
        }
    }

    protected UnityEvent _timeOutEvent;                /// <Event that will be invoked if the notification timesout
    protected UnityEvent _selfDestruct;

    protected Shapes.Line _time;                       /// <Line that shows the user how much time is left to make a decision

    protected float _timeOutInSeconds = 10;

    // Start is called before the first frame update
    private void Start()
    {
        _confirmationDialogPrefab = Resources.Load(StringResources.ConfDialogOrb_path) as GameObject;
        _confirmationDialogPrefab.gameObject.name = "***ARUI-ConfirmationNotification";

        _multipleChoiceDialogPrefab = Resources.Load(StringResources.MultiChoiceDialogOrb_path) as GameObject;
        _multipleChoiceDialogPrefab.gameObject.name = "***ARUI-SelectNotification";

        _yesNoDialogPrefab = Resources.Load(StringResources.YesNoDialogOrb_path) as GameObject;
        _yesNoDialogPrefab.gameObject.name = "***ARUI-YesNoNotification";

        RegisterSpeechKeywordsForDialogsystem();
    }

    private void RegisterSpeechKeywordsForDialogsystem()
    {
        AudioManager.Instance.RegisterKeyword("Select Okay", () => { HandleDialogSpeechInput(AcceptedSpeechInput.SelectOkay); });
        AudioManager.Instance.RegisterKeyword("Select A", () => { HandleDialogSpeechInput(AcceptedSpeechInput.SelectA); });
        AudioManager.Instance.RegisterKeyword("Select B", () => { HandleDialogSpeechInput(AcceptedSpeechInput.SelectB); });
        AudioManager.Instance.RegisterKeyword("Select C", () => { HandleDialogSpeechInput(AcceptedSpeechInput.SelectC); });
        AudioManager.Instance.RegisterKeyword("Select D", () => { HandleDialogSpeechInput(AcceptedSpeechInput.SelectD); });
        AudioManager.Instance.RegisterKeyword("Select E", () => { HandleDialogSpeechInput(AcceptedSpeechInput.SelectE); });
        AudioManager.Instance.RegisterKeyword("Select Yes", () => { HandleDialogSpeechInput(AcceptedSpeechInput.SelectYes); });
        AudioManager.Instance.RegisterKeyword("Select No", () => { HandleDialogSpeechInput(AcceptedSpeechInput.SelectNo); });
    }

    /// <summary>
    /// Handles the user's speech input and triggers corresponding dialog actions.
    /// </summary>
    /// <param name="input">The recognized speech input.</param>
    /// <remarks>
    /// Depending on the speech input, this method identifies the correct dialog type and invokes the corresponding confirmation or selection action.
    /// </remarks>
    private void HandleDialogSpeechInput(AcceptedSpeechInput input)
    {
        if (_currentActiveDialogs.Count == 0) return;

        // Handle only for the dialog in the front
        var currentDialog = _currentActiveDialogs.Peek();
        currentDialog.ConfirmedViaSpeech(input);
    }

    /// <summary>
    /// Adds a confirmation dialog request to the queue.
    /// </summary>
    /// <param name="message">The dialogMessage that will be displayed in the dialog.</param>
    /// <param name="onConfirm">The action to invoke if the user confirms by pressing "Okay".</param>
    /// <param name="onTimeout">The action to invoke if the dialog times out without user interaction.</param>
    /// <param name="timeout">The duration in seconds before the dialog times out.</param>
    public void TryGetUserConfirmation(string dialogMessage, List<UnityAction> onConfirmation, UnityAction onTimeOut, float timeOutInSeconds)
    {
        if (_confirmationDialogPrefab == null || !Utils.StringValid(dialogMessage)) return;

        GameObject window = Instantiate(_confirmationDialogPrefab, transform);
        window.gameObject.name = "***ARUI-Confirmation-" + dialogMessage;
        window.transform.parent = transform;
        ConfirmationDialog dialog = window.AddComponent<ConfirmationDialog>();

        dialog.InitDialog(dialogMessage, onConfirmation, onTimeOut, timeOutInSeconds);

        AddToQueue(dialog);
    }

    /// <summary>
    /// Displays a Yes/No dialog to the user, allowing them to choose between two options.
    /// </summary>
    /// <param name="dialogMessage">The dialogMessage that will be displayed in the dialog.</param>
    /// <param name="onYes">The action to invoke if the user selects "Yes".</param>
    /// <param name="onNo">The action to invoke if the user selects "No".</param>
    /// <param name="onTimeOut">The action to invoke if the dialog times out without user interaction.</param>
    /// <param name="timeOutInSeconds">The duration in seconds before the dialog times out.</param>
    public void TryGetUserYesNoChoice(string dialogMessage, UnityAction onYes, UnityAction onNo, UnityAction onTimeOut, float timeOutInSeconds)
    {
        if (_yesNoDialogPrefab == null || string.IsNullOrEmpty(dialogMessage)) return;

        GameObject window = Instantiate(_yesNoDialogPrefab, transform);
        window.gameObject.name = "***ARUI-YesNoDialog-" + dialogMessage;
        window.transform.parent = transform;
        YesNoDialog dialog = window.AddComponent<YesNoDialog>();

        dialog.InitDialog(dialogMessage, onYes, onNo, onTimeOut, timeOutInSeconds);

        AddToQueue(dialog);
    }

    /// <summary>
    /// Displays a multiple-choice dialog to the user with several options.
    /// </summary>
    /// <param name="dialogMessage">The dialogMessage that will be displayed in the dialog.</param>
    /// <param name="optionLabels">A list of optionLabels that the user can select from.</param>
    /// <param name="actions">A list of actions corresponding to each choice, which will be invoked if that choice is selected.</param>
    /// <param name="onTimeOut">The action to invoke if the dialog times out without user interaction.</param>
    /// <param name="timeOutInSeconds">The duration in seconds before the dialog times out.</param>
    public void TryGetUserChoice(string dialogMessage, List<string> optionLabels, List<UnityAction> onSelection, UnityAction onTimeOut, float timeOutInSeconds)
    {
        if (_multipleChoiceDialogPrefab == null || string.IsNullOrEmpty(dialogMessage) || optionLabels.Count != onSelection.Count) return;

        GameObject window = Instantiate(_multipleChoiceDialogPrefab, transform);
        window.gameObject.name = "***ARUI-Multiselect-" + dialogMessage;
        window.transform.parent = transform;
        MultipleChoiceDialog dialog = window.AddComponent<MultipleChoiceDialog>();

        dialog.InitDialog(dialogMessage, optionLabels, onSelection, onTimeOut, timeOutInSeconds);
        
        AddToQueue(dialog);
    }

    private void AddToQueue(DialogTemplate dialog)
    {
        //Check if there is one in the queue
        if (_currentActiveDialogs.Count > 0)
        {
            dialog.gameObject.SetActive(false);
        } else
        {
            dialog.gameObject.SetActive(true);
            dialog.ShowDialog();
        }

        _currentActiveDialogs.Enqueue(dialog);
    }

    public void DestructDialog(DialogTemplate dialogTemplate)
    {
        if (_currentActiveDialogs.Count == 0) return;

        var currentDialog = _currentActiveDialogs.Peek();

        if (currentDialog != null && currentDialog.gameObject.GetInstanceID() == dialogTemplate.gameObject.GetInstanceID())
        {
            Destroy(dialogTemplate.gameObject);
            _currentActiveDialogs.Dequeue();

            //check if there is one in the queue
            if (_currentActiveDialogs.Count==0)
                return;

            var upcomingDialog = _currentActiveDialogs.Peek();
            if (upcomingDialog!=null)
            {
                upcomingDialog.gameObject.SetActive(true);
                upcomingDialog.ShowDialog();
            }
        }
    }
}
