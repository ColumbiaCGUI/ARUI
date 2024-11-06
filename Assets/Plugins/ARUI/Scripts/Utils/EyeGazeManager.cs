using Microsoft.MixedReality.Toolkit;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class EyeGazeManager : Singleton<EyeGazeManager>
{
    public int CurrentHitID = -1;

    private List<int> _eyeTargetIDs = new List<int>();

    /// ** Debug eye gaze target cube
    private MeshRenderer _eyeGazeTargetCube;
    private bool _showRayDebugCube = false;


    private GameObject _eyeGazeTargetLabel;
    private float gazeDuration = 0f;
    private int lastGazedObjectID = -1;
    private const float requiredGazeTime = 1.0f;

    private void Awake()
    {
        _eyeGazeTargetCube = gameObject.GetComponent<MeshRenderer>();

        _eyeGazeTargetLabel = transform.GetChild(0).gameObject;
        _eyeGazeTargetLabel.gameObject.SetActive(false);
    }

    private void Update()
    {
        var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
        if (eyeGazeProvider != null)
        {
            gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * 2.0f;
            _eyeGazeTargetCube.enabled = false;

            Ray rayToCenter = new Ray(eyeGazeProvider.GazeOrigin, eyeGazeProvider.GazeDirection);
            RaycastHit hitInfo;

            int layerMask = LayerMask.GetMask(StringResources.UI_layer, StringResources.VM_layer);
            UnityEngine.Physics.Raycast(rayToCenter, out hitInfo, 100f, layerMask);

            // Update GameObject to the current eye gaze position at a given distance
            if (hitInfo.collider != null)
            {
                float dist = (hitInfo.point - AngelARUI.Instance.ARCamera.transform.position).magnitude;
                gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * dist;

                //UnityEngine.Debug.Log("Currently looking at:" + hitInfo._collider.gameObject.name+" with ID"+ hitInfo._collider.gameObject.GetInstanceID());
                
                if (_eyeTargetIDs.Contains(hitInfo.collider.gameObject.GetInstanceID()))
                {
                    CurrentHitID = hitInfo.collider.gameObject.GetInstanceID();
                    if (_showRayDebugCube)
                    {
                        _eyeGazeTargetCube.enabled = true;
                    }

                    if (hitInfo.collider.gameObject.GetComponent<StorableObject>()!=null)
                    {
                        // Check if the user is gazing at the same object
                        if (CurrentHitID == lastGazedObjectID)
                        {
                            gazeDuration += Time.deltaTime; // Increment gaze duration if same object

                            if (gazeDuration >= requiredGazeTime && !_eyeGazeTargetLabel.activeSelf)
                            {
                                var storable = hitInfo.collider.gameObject.GetComponent<StorableObject>();

                                var TextContainer = _eyeGazeTargetLabel.GetComponentInChildren<TextMeshProUGUI>();
                                if (TextContainer != null)
                                    TextContainer.text = storable.LabelMessage;

                                _eyeGazeTargetLabel.gameObject.SetActive(true);
                            }
                        }
                        else
                        {
                            _eyeGazeTargetLabel.gameObject.SetActive(false);
                            gazeDuration = 0f;            // Reset gaze duration if object changes
                            lastGazedObjectID = CurrentHitID;    // Update last gazed object ID
                        }

                    } else
                    {
                        _eyeGazeTargetLabel.gameObject.SetActive(false);
                        gazeDuration = 0f;           
                        lastGazedObjectID = CurrentHitID;   
                    }

                } else
                {
                    CurrentHitID = -1;

                    _eyeGazeTargetLabel.gameObject.SetActive(false);
                    gazeDuration = 0f;
                    lastGazedObjectID = CurrentHitID;
                }
            }
            else
            {
                // If no target is hit, show the object at a default distance along the gaze ray.
                gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * 2.0f;
                CurrentHitID = -1;

                _eyeGazeTargetLabel.gameObject.SetActive(false);
                gazeDuration = 0f;
                lastGazedObjectID = CurrentHitID;
            }
        }
        else
        {
            CurrentHitID = -1;
        }

        // Update eye gaze label rot and scale
        float distance = Vector3.Distance(_eyeGazeTargetLabel.transform.position, AngelARUI.Instance.ARCamera.transform.position);
        float scaleValue = Mathf.Max(1f, distance * 1.2f);
        _eyeGazeTargetLabel.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        _eyeGazeTargetLabel.transform.rotation = Quaternion.LookRotation(transform.position - AngelARUI.Instance.ARCamera.transform.position, Vector3.up);
    }

    public void RegisterEyeTargetID(GameObject ob)
    {
        AngelARUI.Instance.DebugLogMessage("Registered Collision Events with "+ ob.name+" and ID "+ ob.GetInstanceID(), false);
        _eyeTargetIDs.Add(ob.GetInstanceID());
    }

    public void ShowDebugTarget(bool showEyeGazeTarget) => _showRayDebugCube = showEyeGazeTarget;
}
