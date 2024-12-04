using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EyeGazeManager : Singleton<EyeGazeManager>
{
    private int _currentHitID = -1;                     // ID of the currently hit object by the eye gaze ray
    public int CurrentHitID => _currentHitID;

    private List<int> _registeredEyeTargetIDs = new List<int>();  // Stores unique IDs of registered eye target objects for quick lookup

    /// ** Debug eye gaze target cube
    private MeshRenderer _eyeGazeTargetCube;

    private bool _showEyeGazeTargetIndicator = false;             
    public bool ShowEyeGazeTargetIndicator                        // Toggles the visibility of the debug cube for eye gaze
    {   
        get { return _showEyeGazeTargetIndicator;}
        set {  _showEyeGazeTargetIndicator = value;}
    }

    /// ** Components for Eye Gaze Label
    private TextMeshProUGUI _eyeGazeTargetLabel;             // UI label displayed when gazing at objects
    private float _gazeDuration = 0f;                   // Tracks how long the user has gazed at an object
    private int _lastGazedObjectID = -1;                // Stores the ID of the last gazed object to detect changes

    private void Awake()
    {
        _eyeGazeTargetCube = gameObject.GetComponent<MeshRenderer>();
        _eyeGazeTargetLabel = transform.GetChild(0).gameObject.GetComponentInChildren<TextMeshProUGUI>();
        _eyeGazeTargetLabel.gameObject.SetActive(false);
    }

    private void Update()
    {
        var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
        _eyeGazeTargetCube.enabled = false;
        _eyeGazeTargetLabel.text = "";

        if (eyeGazeProvider == null)
        {
            ResetGaze();
        } else
        {
            // Try to check for object collision 
            Ray rayToCenter = new Ray(eyeGazeProvider.GazeOrigin, eyeGazeProvider.GazeDirection);

            RaycastHit hitInfo;
            int layerMask = LayerMask.GetMask(StringResources.UI_layer, StringResources.VM_layer); //valid layes atm are vm and UI
            Physics.Raycast(rayToCenter, out hitInfo, 100f, layerMask);                            // check up to 100 meter

            float eyeGazeIndicatorDistance = 2.0f;  //default eye gaze indicator is 2 meter
            int eyeGazeCollisionID = -1;
            if (hitInfo.collider != null)
            {
                eyeGazeIndicatorDistance = (hitInfo.point - AngelARUI.Instance.ARCamera.transform.position).magnitude;
                if (_registeredEyeTargetIDs.Contains(hitInfo.collider.gameObject.GetInstanceID()))
                {
                    eyeGazeCollisionID = HandleRegisteredEyeGazeTargetHit(hitInfo.collider);
                }
            }

            ResetGaze(eyeGazeCollisionID);

            gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * eyeGazeIndicatorDistance;
        }

        UpdateLabelPositionAndDirection();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitCollider">The collider of a registered eye gaze target</param>
    /// <returns></returns>
    private int HandleRegisteredEyeGazeTargetHit(Collider hitCollider)
    {
        if (hitCollider == null) return -1;

        _currentHitID = hitCollider.gameObject.GetInstanceID();
        _eyeGazeTargetCube.enabled = _showEyeGazeTargetIndicator;

        if (hitCollider.gameObject.GetComponent<StorableObject>() != null)
        {
            // Check if the user is gazing at the same object
            if (_currentHitID == _lastGazedObjectID)
            {
                _gazeDuration += Time.deltaTime; // Increment gaze duration if same object

                if (_gazeDuration >= ARUISettings.EyeGazeLabelDwelltime && !_eyeGazeTargetLabel.gameObject.activeSelf)
                {
                    var storable = hitCollider.gameObject.GetComponent<StorableObject>();
                    _eyeGazeTargetLabel.text = storable.LabelMessage;
                    Debug.Log(_eyeGazeTargetLabel.text);
                }
            }
        }

        return _currentHitID;
    }

    private void UpdateLabelPositionAndDirection()
    {
        float distance = Vector3.Distance(_eyeGazeTargetLabel.transform.position, AngelARUI.Instance.ARCamera.transform.position);
        float scaleValue = Mathf.Max(1f, distance * 1.2f);
        _eyeGazeTargetLabel.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        _eyeGazeTargetLabel.transform.rotation = Quaternion.LookRotation(transform.position - AngelARUI.Instance.ARCamera.transform.position, Vector3.up);
    }

    private void ResetGaze(int newGazeID = -1)
    {
        _currentHitID = newGazeID;

        _eyeGazeTargetLabel.gameObject.SetActive(_eyeGazeTargetLabel.text.Length > 0);
        _gazeDuration = 0f;
        _lastGazedObjectID = _currentHitID;
    }

    #region Eye Gaze Target Registration and Deregistration

    /// <summary>
    /// Registers a GameObject as an eye target by adding its unique ID to the eye target list.
    /// </summary>
    /// <param name="ob">The GameObject to be registered as an eye target.</param>
    public void RegisterEyeTargetID(GameObject ob)
    {
        if (ob == null)
        {
            AngelARUI.Instance.DebugLogMessage("Attempted to register a null GameObject as an eye target.", false);
            return;
        }

        if (!_registeredEyeTargetIDs.Contains(ob.GetInstanceID()))
        {
            AngelARUI.Instance.DebugLogMessage("Registered Collision Events with " + ob.name + " and ID " + ob.GetInstanceID(), false);
            _registeredEyeTargetIDs.Add(ob.GetInstanceID());
        } 
    }

    /// <summary>
    /// Deregisters a GameObject from the eye target list by removing its unique gameobject instance ID.
    /// </summary>
    /// <param name="ob">The GameObject to be deregistered as an eye target.</param>
    public void DeRegisterEyeTarget(GameObject ob)
    {
        AngelARUI.Instance.DebugLogMessage("Trying to deregister target " + ob.name + " and ID " + ob.GetInstanceID(), false);
        if (_registeredEyeTargetIDs.Contains(ob.GetInstanceID()))
        {
            _registeredEyeTargetIDs.Remove(ob.GetInstanceID());
        }
    }

    #endregion
}
