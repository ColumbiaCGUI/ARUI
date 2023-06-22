using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class ObjectAnchor : MonoBehaviour
{
    private GameObject mainCamera;
    private GameObject canvas;
    private GameObject anchor;

    private bool isLookingAtDot;
    private bool isFadingOut;
    private bool isVisible;

    private Line pointer;
    private CanvasGroup canvasGroup;
    private Color pointerColor;

    private Coroutine fadeOutCoroutine;

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
        canvas = transform.Find("DetectedObjectCanvas").gameObject;

        canvasGroup = canvas.GetComponent<CanvasGroup>();
        pointerColor = pointer.Color;

        pointer.Thickness = 0.0125f;

        isLookingAtDot = false;
        isFadingOut = false;
        isVisible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Remove this as other scripts didn't need this
        if (!EyeGazeManager.Instance) 
        {
            return;
        }

        if (isLookingAtDot && EyeGazeManager.Instance.CurrentHit != EyeTarget.detectedObject)
        {
            isLookingAtDot = false;
            StartCoroutine(DisableAnnotation());
        }
        else if (!isLookingAtDot && EyeGazeManager.Instance.CurrentHit == EyeTarget.detectedObject)
        {
            isLookingAtDot = true;
            if (!isVisible)
            {
                StartCoroutine(EnableAnnotation());
            }
        }

        if (isFadingOut && isLookingAtDot) 
        {
            StopCoroutine(fadeOutCoroutine);

            isFadingOut = false;

            canvasGroup.alpha = 1.0f;
            pointer.Color = pointerColor;
        }

        UpdateCanvasCollider();
    }

    private void LateUpdate()
    {
        // The anchor dot should always face the user
        anchor.transform.LookAt(mainCamera.transform);
        // The canvas should always face the user
        canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.rotation * Vector3.forward, Vector3.up);
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
            isVisible = true;
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

        fadeOutCoroutine = StartCoroutine(FadeOut());
    }

    private void UpdateCanvasCollider()
    {
        // Get collider instance and recttransform
        BoxCollider collider = canvas.GetComponent<BoxCollider>();
        RectTransform rectTransform = canvas.GetComponent<RectTransform>();

        collider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 1);
    }

    private IEnumerator FadeOut()
    {
        isFadingOut = true;

        float counter = 0f;
        float duration = 1f;

        float startAlpha = canvasGroup.alpha;
        float targetAlpha = 0f;

        while (counter < duration) 
        {
            counter += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, counter/duration);
            Color newColor = new Color(pointerColor.r, pointerColor.g, pointerColor.b, canvasGroup.alpha);
            pointer.Color = newColor;

            yield return null;
        }

        canvasGroup.alpha = 1.0f;
        canvas.SetActive(false);

        pointer.Color = pointerColor;
        pointer.Start = anchor.transform.localPosition;
        pointer.End = anchor.transform.localPosition;

        isFadingOut = false;
        isVisible = false;
    }
}
