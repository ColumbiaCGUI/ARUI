using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UserAttentionController : MonoBehaviour
{
    // Head Gaze variables
    private Transform mainCamTransform;
    private Vector3 headPosition;
    private Vector3 headDirection;

    // Eye Gaze variables
    private IMixedRealityEyeGazeProvider eyeGazeProvider;
    private Vector3 gazeOrigin;
    private Vector3 gazeDirection;

    // Head + Eye Gaze Threshold
    [SerializeField]
    private float eyeHeadDiffThreshold = 5;

    // I-VT needed variables
    private Vector3 prevGazeDirection;
    private float currentFixationDuration;
    [SerializeField]
    private float angularVelocityThreshold = 100f;
    [SerializeField]
    private float minFixationDuration = 0.1f;

    [SerializeField]
    LayerMask virtualObjectLayer;

    // The current gazed object dwell tracker
    private DwellTracker currentDwellTracker;

    // Head gaze crosshair for demo
    // [SerializeField]
    // private Transform headGazeCrosshair;
    [SerializeField]
    private Transform eyeGazeCrosshair;

    // True if user is focusing on an object
    public bool bFocusing;

    // Start is called before the first frame update
    void Start()
    {
        // Get main camera transform
        mainCamTransform = CameraCache.Main.transform;
        // Get eye gaze information
        eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;

        // Default Dwell Tracker to null
        currentDwellTracker = null;
    }

    // Update is called once per frame
    void Update()
    {
        // Update head gaze data
        headPosition = mainCamTransform.position;
        headDirection = mainCamTransform.forward;

        // Update eye gaze data
        gazeOrigin = eyeGazeProvider.GazeOrigin;
        gazeDirection = eyeGazeProvider.GazeDirection;

        //headGazeCrosshair.position = headPosition + headDirection * 0.1f;

        float eyeHeadDirectionDiff = Vector3.Angle(headDirection, gazeDirection);

        // Debug.Log("Head Eye Diff: " + eyeHeadDirectionDiff);

        bool bFixation = IVT();

        if (eyeHeadDirectionDiff < eyeHeadDiffThreshold && bFixation) 
        {
            RaycastHit hit;
            if (Physics.Raycast(gazeOrigin, gazeDirection, out hit, 10, virtualObjectLayer))
            {
                eyeGazeCrosshair.position = hit.point;
                // Get the dwell tracker for the object we are looking at
                DwellTracker dwellTracker;
                if (hit.transform.name == "Canvas")
                {
                    dwellTracker = hit.transform.parent.parent.parent.GetComponent<DwellTracker>();
                }
                else
                {
                    dwellTracker = hit.transform.GetComponent<DwellTracker>();
                }

                if (dwellTracker != null)
                {
                    // If user starts gazing a new object, let the current tracker know
                    if (currentDwellTracker != dwellTracker && currentDwellTracker != null)
                    {
                        currentDwellTracker.bGazed = false;
                    }

                    // Update the tracker so that it knows we are looking at it
                    dwellTracker.bGazed = true;
                    currentDwellTracker = dwellTracker;
                }
            }
            // set the crosshair position to default if nothing was hit
            else
            {
                eyeGazeCrosshair.position = gazeOrigin + gazeDirection * 2f;
            }
        }
        // if user is not looking anything
        else
        {
            if (currentDwellTracker != null)
            {
                currentDwellTracker.bGazed = false;
                currentDwellTracker = null;
            }
        }

        prevGazeDirection = gazeDirection;
    }

    bool IVT()
    {
        float angularDiff = Vector3.Angle(gazeDirection, prevGazeDirection);
        float angularVelocity = angularDiff / Time.deltaTime;

        if (angularVelocity < angularVelocityThreshold)
        {
            currentFixationDuration += Time.deltaTime;
        }
        else
        {
            currentFixationDuration = 0;
        }

        if (currentFixationDuration >= minFixationDuration)
        {
            return true;
        }
        return false;
    }
}
