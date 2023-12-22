using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    [SerializeField]
    float coneSize = 25;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent == null)
        {
            Debug.Log("I belive this only runs once.");
            GameObject mainCamera = GameObject.Find("MixedRealityPlayspace").GetNamedChild("Main Camera");
            if (mainCamera.transform.childCount != 0)
            {
                transform.parent = mainCamera.transform;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DwellTracker dwellTracker = other.GetComponent<DwellTracker>();

        if (dwellTracker == null) return;

        dwellTracker.bMulti = true; 
    }

    private void OnTriggerExit(Collider other)
    {
        DwellTracker dwellTracker = other.GetComponent<DwellTracker>();

        if (dwellTracker == null) return;

        dwellTracker.bMulti = false;
    }

    /*
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TestCone"))
        {
            MeshRenderer meshRenderer = other.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Transform mainCameraTransform = CameraCache.Main.transform;
                Vector3 cameraPosition = mainCameraTransform.position;
                Vector3 headDirection = mainCameraTransform.forward;

                Vector3 relativeVector = other.transform.position - cameraPosition;
                float angle = Vector3.Angle(headDirection, relativeVector);

                if (angle < coneSize)
                {
                    if (meshRenderer.material.color != Color.green)
                    {
                        meshRenderer.material.color = Color.red;
                    }
                }
                else
                {
                    meshRenderer.material.color = Color.white;
                }
            }
        }
    }
    

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("TestCone"))
        {
            MeshRenderer meshRenderer = other.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material.color = Color.white;
            }
        }
    }
    */
}
