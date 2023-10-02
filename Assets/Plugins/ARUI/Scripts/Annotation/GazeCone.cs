using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeCone : MonoBehaviour
{
    private IMixedRealityEyeGazeProvider m_EyeGazeProvider;
    private GameObject currentHit;
    private int hitNumber;

    // Start is called before the first frame update
    void Awake()
    {
        m_EyeGazeProvider = CoreServices.InputSystem.EyeGazeProvider;
        currentHit = null;
        hitNumber = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Attach gaze cone to eye gaze
        transform.position = m_EyeGazeProvider.GazeOrigin;
        transform.LookAt(m_EyeGazeProvider.GazeDirection.normalized + m_EyeGazeProvider.GazeDirection.normalized * transform.localScale.z);

        /*
        // If more than one objects are selected, shrink the gaze cone
        if (hitNumber > 1)
        {
            // Decrease the gaze cone radius to 0.75cm
            if (transform.localScale.x > 0.0075f)
            {
                // Calculate the new radius, maintance the length
                float newRadius = transform.localScale.x - 0.001f * Time.deltaTime;
                transform.localScale = new Vector3(newRadius, newRadius, transform.localScale.z);
            }
            // Shorten the gaze cone 
            else
            {
                float newDepth = transform.localScale.z - 0.1f * Time.deltaTime;
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, newDepth);
            }
        }
        // The remaining object is selected
        else if (hitNumber == 1)
        {
        }
        */
    }

    private void OnTriggerEnter(Collider other)
    {
        hitNumber += 1;
    }

    private void OnTriggerExit(Collider other)
    {
        hitNumber -= 1;
    }
}
