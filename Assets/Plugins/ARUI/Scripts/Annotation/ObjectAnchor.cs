using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class ObjectAnchor : MonoBehaviour
{
    private GameObject mainCamera;
    private GameObject canvas;
    private GameObject anchor;

    private Line pointer;

    private bool isLookingAtDot;

    [SerializeField]
    private float enableDelay = 1.0f;
    [SerializeField]
    private float disableDelay = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera");

        anchor = transform.Find("Anchor").gameObject;
        pointer = transform.Find("Pointer").GetComponent<Line>();
        canvas = transform.Find("Canvas").gameObject;

        pointer.Thickness = 0.0125f;

        isLookingAtDot = false;
    }

    // Update is called once per frame
    void Update()
    {
        // The anchor should always look at the user
        anchor.transform.LookAt(mainCamera.transform);
        //canvas.transform.LookAt(mainCamera.transform);

        // TODO: Remove this as other scripts didn't need this
        if (!EyeGazeManager.Instance) 
        {
            return;
        }

        if (isLookingAtDot && EyeGazeManager.Instance.CurrentHit != EyeTarget.objectAnchor)
        {
            isLookingAtDot = false;
            StartCoroutine(DisableAnnotation());
        }
        else if (!isLookingAtDot && EyeGazeManager.Instance.CurrentHit == EyeTarget.objectAnchor)
        {
            isLookingAtDot = true;
            StartCoroutine(EnableAnnotation());
        }
    }

    private IEnumerator EnableAnnotation()
    {
        float elapsed = 0f;
        bool success = false;

        while (isLookingAtDot && elapsed < enableDelay)
        {
            elapsed += Time.deltaTime;

            // Get relative info between the canvas and anchor
            Vector3 diffVec = canvas.transform.localPosition - anchor.transform.localPosition;
            Vector3 diffDir = diffVec.normalized;
            float diffSize = diffVec.magnitude;

            // Set the start of the pointer
            pointer.Start = anchor.transform.localPosition + diffDir * (diffSize * 0.2f);

            // Calculate the end position of the pointer
            float percent = (elapsed / enableDelay);
            Vector3 endPos = pointer.Start + (diffDir) * (diffSize * 0.6f * percent);

            pointer.End = endPos;

            if (elapsed > enableDelay && isLookingAtDot)
                success = true;

            yield return null;
        }

        if (success)
        {
            canvas.SetActive(true);
        }
        else
        {
            // Erase the pointer if user looked away in the progress
            pointer.Start = anchor.transform.localPosition;
            pointer.End = anchor.transform.localPosition;
        }
    }

    private IEnumerator DisableAnnotation()
    {

        yield return new WaitForSeconds(disableDelay);

        // TODO: Add fade out

        pointer.Start = anchor.transform.localPosition;
        pointer.End = anchor.transform.localPosition;

        canvas.SetActive(false);
    }
}
