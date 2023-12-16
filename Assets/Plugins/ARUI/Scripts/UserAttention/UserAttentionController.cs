using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserAttentionController : MonoBehaviour
{
    // Head Gaze variables
    Transform mainCamTransform;
    Vector3 headPosition;
    Vector3 headDirection;

    // Eye Gaze variables
    IMixedRealityEyeGazeProvider eyeGazeProvider;
    Vector3 gazeOrigin;
    Vector3 gazeDirection;

    [SerializeField]
    float eyeHeadDiffThreshold = 5;

    // Start is called before the first frame update
    void Start()
    {
        // Get main camera transform
        mainCamTransform = CameraCache.Main.transform;
        // Get eye gaze information
        eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
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

        float eyeHeadDirectionDiff = Vector3.Angle(headDirection, gazeDirection);

        // Debug.Log("Head Eye Diff: " + eyeHeadDirectionDiff);

        if (eyeHeadDirectionDiff < eyeHeadDiffThreshold)
        {
            RaycastHit hit;
            if (Physics.Raycast(gazeOrigin, gazeDirection, out hit))
            {
                MeshRenderer hitMeshRenderer = hit.collider.gameObject.GetComponent<MeshRenderer>();
                if (hitMeshRenderer != null)
                {
                    hitMeshRenderer.material.color = Color.green;
                }
            }
        }
    }
}
