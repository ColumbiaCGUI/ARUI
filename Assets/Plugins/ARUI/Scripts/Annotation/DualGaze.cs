using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DualGaze : MonoBehaviour
{
    GameObject lastHitObject;
    Vector3 lastGazeDirection;

    [SerializeField]
    private Transform button;
    [SerializeField]
    private GameObject anno;
    [SerializeField]
    private float disableTimer = 3;

    private bool annoShown;

    // Start is called before the first frame update
    void Start()
    {
        annoShown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!EyeGazeManager.Instance) return;

        GameObject currentHitObject = EyeGazeManager.Instance.CurrentHitObj;
        RaycastHit hitInfo = EyeGazeManager.Instance.publicHitInfo;
        IMixedRealityEyeGazeProvider provider = EyeGazeManager.Instance.eyeGazeProvider;

        if (lastHitObject == null && currentHitObject == gameObject && annoShown == false)
        {
            Vector3 directionDiff = provider.GazeDirection - lastGazeDirection;
            directionDiff *= 100;

            float absX = Mathf.Abs(directionDiff.x);
            float absY = Mathf.Abs(directionDiff.y);
            float absZ = Mathf.Abs(directionDiff.z);

            float[] delta = { absX, absY, absZ };
            float deltaMax = delta.Max();
            if (deltaMax == absX)
            {
                if (directionDiff.x > 0) 
                {
                    button.position = new Vector3(transform.position.x - (transform.lossyScale.x / 2 + button.lossyScale.x), transform.position.y, transform.position.z);
                }
                else 
                { 
                    button.position = new Vector3(transform.position.x + (transform.lossyScale.x / 2 + button.lossyScale.x), transform.position.y, transform.position.z);
                }
            }
            else if (deltaMax == absY)
            {
                if (directionDiff.x > 0) 
                { 
                    button.position = new Vector3(transform.position.x, transform.position.y - (transform.lossyScale.y / 2 + button.lossyScale.y), transform.position.z);
                }
                else 
                { 
                    button.position = new Vector3(transform.position.x, transform.position.y + (transform.lossyScale.y / 2 + button.lossyScale.y), transform.position.z);
                }
            }
            else
            {
                if (directionDiff.z > 0) 
                { 
                    button.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - (transform.lossyScale.z / 2 + button.lossyScale.z));
                }
                else 
                { 
                    button.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + (transform.lossyScale.z / 2 + button.lossyScale.z));
                }
            }
        }

        if (currentHitObject == button.gameObject)
        {
            button.transform.localPosition = Vector3.zero;
            anno.SetActive(true);
            annoShown = true;
            StartCoroutine(CloseAnno());
        }

        lastHitObject = currentHitObject;
        lastGazeDirection = provider.GazeDirection;
    }

    IEnumerator CloseAnno()
    {
        yield return new WaitForSeconds(disableTimer);
        anno.SetActive(false);
        annoShown = false;
    }
}
