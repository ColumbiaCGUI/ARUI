using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DwellTracker : MonoBehaviour
{
    // True if user is gazing at this object
    public bool bGazed;

    // True if multiselection get this object
    public bool bMulti;

    // True if this object is triggered
    private bool bTriggered;

    // Time since user start focusing on this object
    private float timeGazed;

    // Time since user stop focusing on this object
    private float timeNotGazed;

    // Time threshold when we think this object is focused on
    [SerializeField]
    private float thresholdFocused;

    // The mesh render of this object
    private MeshRenderer meshRenderer;

    // The annotation gameobject
    private Transform annotation;

    private void Start()
    {
        bGazed = false;

        timeGazed = 0;
        timeNotGazed = 0;

        meshRenderer = GetComponent<MeshRenderer>();

        annotation = transform.GetChild(0);
    }

    private void Update()
    {
        // If user is currently looking at this object
        if (bGazed)
        {
            // Increase the time this object has been looked at
            timeGazed += Time.deltaTime;

            // If user is focusing on this object
            if (timeGazed > thresholdFocused)
            {
                // Update status of this object
                if (meshRenderer != null)
                {
                    bTriggered = true;
                    GameObject.Find("UserAttentionController").GetComponent<UserAttentionController>().bFocusing = true;
                }
            }
        }
        // If user no longer looking at this object
        else
        {
            // Update the not gazed time
            timeNotGazed += Time.deltaTime;

            // If this object is already trigger  
            if (bTriggered)
            {
                // Untrigger after 1 sec
                if (timeNotGazed > 1)
                {
                    bTriggered = false;
                    GameObject.Find("UserAttentionController").GetComponent<UserAttentionController>().bFocusing = false;
                }
            }
            // If the object is not triggered yet
            else
            {
                // Reset the dwell time after 0.5 sec
                if (timeNotGazed > 0.5f)
                {
                    timeGazed = 0;
                }
            }
        }

        // Update the color of the object
        GameObject annotationOverview = annotation.Find("Brief").gameObject;
        GameObject annotationDetail = annotation.Find("Detail").gameObject;
        if (!bTriggered)
        {
            annotationDetail.SetActive(false);
            if (bMulti && (!GameObject.Find("UserAttentionController").GetComponent<UserAttentionController>().bFocusing))
            {
                this.GetComponent<MeshRenderer>().material.color = Color.cyan;
                if (!annotationOverview.activeSelf)
                {
                    annotationOverview.SetActive(true);
                }
            }
            else
            {
                this.GetComponent<MeshRenderer>().material.color = Color.red;
                annotationOverview.SetActive(false);
            }
        }
        else
        {
            this.GetComponent<MeshRenderer>().material.color = Color.green;
            annotationOverview.SetActive(false);
            annotationDetail.SetActive(true);
        }
    }
}
