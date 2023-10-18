using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class HeadEyeGaze : MonoBehaviour
{
    private GameObject currentHitObject;

    private Transform mainCameraTransform;

    LayerMask objectColliderMask;

    [SerializeField]
    float maxDistance = 2;

    [SerializeField]
    GameObject anno;

    // Start is called before the first frame update
    void Start()
    {
        objectColliderMask = LayerMask.GetMask("ObjectCollider");
    }

    // Update is called once per frame
    void Update()
    {
        if (!EyeGazeManager.Instance) return;

        currentHitObject = EyeGazeManager.Instance.CurrentHitObj;

        if (currentHitObject == gameObject)
        {
            // Get main camera information
            mainCameraTransform = CameraCache.Main.transform;
            Vector3 cameraPosition = mainCameraTransform.position;
            Vector3 headDirection = mainCameraTransform.forward;

            RaycastHit hitInfo;

            if (Physics.Raycast(cameraPosition, headDirection, out hitInfo, maxDistance, objectColliderMask))
            {
                GameObject headGazeObject = hitInfo.collider.gameObject;

                if (headGazeObject == currentHitObject)
                {
                    anno.SetActive(true);
                    StartCoroutine(CloseAnno());
                }
            }
        }
    }

    IEnumerator CloseAnno()
    {
        yield return new WaitForSeconds(3);
        anno.SetActive(false);
    }
}
