using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using System;

/// <summary>
/// Interface to the ARUI Components - a floating assistant in the shape as an orb and a task overview panel.
/// </summary>
public class AngelARUI : Singleton<AngelARUI>
{
    private Camera _arCamera;

    [HideInInspector]
    public Camera ARCamera => _arCamera;              /// <Reference to camera rendering the AR scene

    ///****** Debug Settings
    private bool _showARUIDebugMessages = true;       /// <If true, ARUI debug messages are shown in the unity console and scene Logger (if available)
    private bool _showEyeGazeTarget = false;          /// <If true, the eye gaze target is shown if the eye ray hits UI elements (white small cube), can be toggled on/off at runtime

    [HideInInspector]
    public bool PrintVMDebug = false;

    ///****** Guidance Settings
    private bool _useViewManagement = true;           /// <If true, the ARUI view mangement will run

    [HideInInspector]
    public bool IsVMActiv => ViewManagement.Instance != null && _useViewManagement;

    [Tooltip("Set a custom Skip Notification Message. Can not be empty.")]
    public string SkipNotificationMessage = "You are skipping the current task:";

    ///****** Confirmation Dialogue
    private UnityAction _onUserIntentConfirmedAction = null;     /// <Action invoked if the user accepts the confirmation dialogue
    private ConfirmationDialogue _confirmationWindow = null;     /// <Reference to confirmation dialogue
    private GameObject _confirmationWindowPrefab = null;

    private Dictionary<string, CVDetectedObj> DetectedObjects = new Dictionary<string, CVDetectedObj>();

    private void Awake() => StartCoroutine(InitProjectSettingsAndScene());

    private IEnumerator InitProjectSettingsAndScene()
    {
        List<string> layers = new List<string>()
       {
           StringResources.zBuffer_layer, StringResources.Hand_layer, StringResources.VM_layer,
           StringResources.UI_layer, StringResources.spatialAwareness_layer
       };

        StringResources.LayerToLayerInt = new Dictionary<string, int>
        {
            {  layers[0], 24 },
            {  layers[1], 25 },
            {  layers[2], 26 },
            {  layers[3], 5 },
            {  layers[4], 31 }
        };

#if UNITY_EDITOR
        foreach (string layer in layers)
            Utils.CreateLayer(layer, StringResources.LayerToLayerInt[layer]);
#endif

        yield return new WaitForEndOfFrame();

        //Get persistant reference to ar cam
        _arCamera = Camera.main;
        int oldMask = _arCamera.cullingMask;
        _arCamera.cullingMask = oldMask & ~(1 << (StringResources.LayerToLayerInt[StringResources.zBuffer_layer]));

        //Instantiate audio manager, for audio feedback
        DataProvider database = new GameObject("DataManager").AddComponent<DataProvider>();
        database.gameObject.name = "***ARUI-" + StringResources.dataManager_name;

        //Instantiate audio manager, for audio feedback
        AudioManager am = new GameObject("AudioManager").AddComponent<AudioManager>();
        am.gameObject.name = "***ARUI-" + StringResources.audioManager_name;

        //Instantiate eye gaze managing script
        GameObject eyeTarget = Instantiate(Resources.Load(StringResources.EyeTarget_path)) as GameObject;
        eyeTarget.gameObject.name = "***ARUI-" + StringResources.eyeGazeManager_name;
        eyeTarget.AddComponent<EyeGazeManager>();
        EyeGazeManager.Instance.ShowDebugTarget(_showEyeGazeTarget);

        GameObject handPoseManager = Instantiate(Resources.Load(StringResources.HandPoseManager_path)) as GameObject;
        handPoseManager.gameObject.name = "***ARUI-" + StringResources.HandPoseManager_name;
        yield return new WaitForEndOfFrame();

        //Instantiate the AI assistant - orb
        GameObject orb = Instantiate(Resources.Load(StringResources.Orb_path)) as GameObject;
        orb.gameObject.name = "***ARUI-" + StringResources.orb_name;
        orb.transform.parent = transform;
        orb.AddComponent<Orb>();

        //Instantiate the Task Overview
        GameObject TaskOverview = Instantiate(Resources.Load(StringResources.Sid_Tasklist_path)) as GameObject;
        TaskOverview.gameObject.name = "***ARUI-" + StringResources.tasklist_name;
        TaskOverview.GetComponent<Center_of_Objs>().SnapToCentroid();

        //Start View Management, if enabled
        if (_useViewManagement)
            StartCoroutine(TryStartVM());

        //Instantiate empty tasklist
        //GameObject taskList = Instantiate(Resources.Load(StringResources.TaskList_path)) as GameObject;
        //taskList.gameObject.name = "***ARUI-" + StringResources.tasklist_name;
        //taskList.AddComponent<TaskListManager>();

        //Instantiate empty multi tasklist
        //GameObject overviewObj = Instantiate(Resources.Load(StringResources.Sid_Tasklist_path)) as GameObject;
        //Center_of_Objs.Instance.SnapToCentroid();

        //Load resources for UI elements
        _confirmationWindowPrefab = Resources.Load(StringResources.ConfNotification_path) as GameObject;
        _confirmationWindowPrefab.gameObject.name = "***ARUI-" + StringResources.confirmationWindow_name;

        //Initialize components for the visibility computation of physical objects
        Camera zBufferCam = new GameObject("zBuffer").AddComponent<Camera>();
        zBufferCam.transform.parent = _arCamera.transform;
        zBufferCam.transform.position = Vector3.zero;
        zBufferCam.gameObject.AddComponent<ZBufferCamera>();
    }

    public void SetSelectedTasks(List<string> list) => DataProvider.Instance.SetSelectedTasks(list);

    #region Task Guidance

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="allTasks"></param>
    public void InitManual(List<string> allTasks) => DataProvider.Instance.InitManual(allTasks);

    /// <summary>
    /// Set the current task the user has to do.
    /// If taskID is >= 0 and < the number of tasks, the orb won't react.
    /// If taskID is the same as the current one, the ARUI won't react.
    /// If taskID has subtasks, the orb shows the first subtask as the current task
    /// TODO
    /// </summary>
    /// <param name="taskID">index of the current task that should be highlighted in the UI</param>
    public void AdvanceToStep(string recipeID, int taskID) => DataProvider.Instance.SetCurrentActiveStep(recipeID, taskID);

    /// <summary>
    /// TODO 
    /// </summary>
    /// <param name="taskID"></param>
    public void SetCurrentDetectedTask(string taskID) => DataProvider.Instance.SetCurrentActiveTask(taskID);

    /// <summary>
    /// TODO: Set all tasks in the tasklist as done. (or a specific task?)
    /// </summary>
    public void SetAllTasksDone(string key)
    {

    }

    /// <summary>
    /// TODO
    /// </summary>
    public void SetEyeDwellingAllowed(bool active) => ARUISettings.EyeDwellAllowed = active;

    /// <summary>
    /// Mute voice feedback for task guidance. ONLY influences task guidance.
    /// </summary>
    /// <param name="mute">if true, the user will hear the tasks, in addition to text.</param>
    public void MuteAudio(bool mute) => AudioManager.Instance.MuteAudio(mute);

    #endregion

    #region Notifications

    /// <summary>
    /// Set the callback function that is invoked if the user confirms the confirmation dialogue
    /// </summary>
    public void SetUserIntentCallback(UnityAction userIntentCallBack) => _onUserIntentConfirmedAction = userIntentCallBack;


    ///// <summary>
    ///// If confirmation action is set - SetUserIntentCallback(...) - and no confirmation window is active at the moment, the user is shown a 
    ///// timed confirmation window. Recommended text: "Did you mean ...". If the user confirms the dialogue, the onUserIntentConfirmedAction action is invoked. 
    ///// </summary>
    ///// <param name="msg">message that is shown in the confirmation dialogue</param>
    public void TryGetUserFeedbackOnUserIntent(string msg)
    {
        if (_onUserIntentConfirmedAction == null || _confirmationWindow != null || msg == null || msg.Length == 0) return;

        GameObject window = Instantiate(_confirmationWindowPrefab, transform);
        window.gameObject.name = "***ARUI-Confirmation-" + msg;
        _confirmationWindow = window.AddComponent<ConfirmationDialogue>();
        _confirmationWindow.InitializeConfirmationNotification(msg, _onUserIntentConfirmedAction);
    }

    /// <summary>
    /// If given paramter is true, the orb will show message to the user that the system detected an attempt to skip the current task.
    /// The message will disappear if "SetCurrentTaskID(..)" is called, or ShowSkipNotification(false)
    /// </summary>
    /// <param name="show">if true, the orb will show a skip notification, if false, the notification will disappear</param>
    public void SetNotification(NotificationType type, string message)
    {
        if (DataProvider.Instance.CurrentSelectedTasks.Count==0) return;

        Orb.Instance.AddNotification(type, message);
    }

    public void RemoveNotification(NotificationType type) => Orb.Instance.RemoveNotification(type);

    #endregion

    #region Detected Physical Object Registration

    /// <summary>
    /// Add a 3D mesh to view management. BBox should contain a mesh filter
    /// </summary>
    /// <param name="bbox">The position, rotation, scale and mesh of this object should be considered in view management</param>
    /// <param name="ID">ID to identify the gameobject that should be added</param>
    public void RegisterDetectedObject(GameObject bbox, string ID)
    {
        if (DetectedObjects.ContainsKey(ID)) return;

        GameObject copy = Instantiate(bbox);
        copy.gameObject.name = "***ARUI-CVDetected-" + ID;

        // destroy mesh renderer, if attached
        if (copy.GetComponent<MeshRenderer>()!=null)
            Destroy(copy.GetComponent<MeshRenderer>());

        CVDetectedObj ndetection = copy.AddComponent<CVDetectedObj>();
        DetectedObjects.Add(ID, ndetection);
    }

    /// <summary>
    /// Remove a 3D mesh from view management
    /// </summary>
    /// <param name="ID">ID to identify the gameobject that should be removed</param>
    public void DeRegisterDetectedObject(string ID)
    {
        if (!DetectedObjects.ContainsKey(ID)) return;

        StartCoroutine(LateDestroy(DetectedObjects[ID]));
        DetectedObjects.Remove(ID);
    }

    private IEnumerator LateDestroy(CVDetectedObj temp)
    {
        temp.IsDestroyed = true;

        yield return new WaitForSeconds(0.2f);

        Destroy(temp.gameObject);
    }

    #endregion

    #region View management

    /// <summary>
    /// Enable or disable view management. enabled by default 
    /// </summary>
    /// <param name="enabled"></param>
    public void SetViewManagement(bool enabled)
    {
        if (_useViewManagement != enabled)
        {
            if (enabled)
            {
                StartCoroutine(TryStartVM());
            }
            else if (ViewManagement.Instance != null)
            {
                Destroy(ARCamera.gameObject.GetComponent<ViewManagement>());
                Destroy(ARCamera.gameObject.GetComponent<SpaceManagement>());
                _useViewManagement = false;

                AngelARUI.Instance.LogDebugMessage("View Management is OFF",true);
            }
        }
    }

    /// <summary>
    /// Start view management if dll is available. If dll could not be loaded, view management is turned off.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TryStartVM()
    {
        SpaceManagement sm = ARCamera.gameObject.gameObject.AddComponent<SpaceManagement>();
        yield return new WaitForEndOfFrame();

        bool loaded = sm.CheckIfDllLoaded();

        if (loaded)
        {
            ARCamera.gameObject.AddComponent<ViewManagement>();
            AngelARUI.Instance.LogDebugMessage("View Management is ON", true);
        }
        else
        {
            Destroy(sm);
            LogDebugMessage("VM could not be loaded. Setting vm disabled.", true);
        }

        _useViewManagement = loaded;
    }
    #endregion

    #region Orb Behavior

   // public void S

    #endregion

    #region Logging and Debugging

    /// <summary>
    /// Set if debug information is shown in the logger window
    /// </summary>
    /// <param name="show">if true, ARUI debug messages are shown in the unity console and scene Logger (if available)</param>
    public void ShowDebugMessagesInLogger(bool show) => _showARUIDebugMessages = show;

    /// <summary>
    /// Set if debug information is shown about the users eye gaze, the user will see a small transparent sphere that represents the eye target
    /// </summary>
    /// <param name="show">if true and the user is looking at a virtual UI element, a small transparent sphere is shown </param>
    public void ShowDebugEyeGazeTarget(bool show)
    {
        _showEyeGazeTarget = show;
        EyeGazeManager.Instance.ShowDebugTarget(_showEyeGazeTarget);
    }

    /// <summary>
    /// ********FOR DEBUGGING ONLY, prints ARUI logging messages
    /// </summary>
    /// <param name="message"></param>
    /// <param name="showInLogger"></param>
    public void LogDebugMessage(string message, bool showInLogger)
    {
        if (_showARUIDebugMessages)
        {
            if (showInLogger && FindObjectOfType<Logger>() != null)
                Logger.Instance.LogInfo("***ARUI: " + message);
            Debug.Log("***ARUI: " + message);
        }
    }

    #endregion
}
