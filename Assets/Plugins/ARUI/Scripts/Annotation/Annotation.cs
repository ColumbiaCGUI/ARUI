using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public struct AnnotationSignal
{

}

public class Annotation : MonoBehaviour
{
    [SerializeField]
    private int id;

    private Transform detectedObj;

    private GameObject mainCamera;
    private GameObject canvas;
    private GameObject anchor;

    AnnotationCanvasControl canvasCtl;

    private bool isLookingAtDot;
    private bool isFadingOut;
    private bool isVisible;
    private bool isInit;

    private Line pointer;
    private CanvasGroup canvasGroup;
    private Color pointerColor;

    private Coroutine fadeOutCoroutine;

    [SerializeField]
    private float enableDelay = 1.0f;
    [SerializeField]
    private float disableDelay = 1.0f;
    [SerializeField]
    private float canvasPosOffset = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        detectedObj = transform.parent;

        mainCamera = GameObject.Find("Main Camera");
        canvas = transform.Find("AnnotationCanvas").gameObject;
        anchor = transform.Find("Anchor").gameObject;
        pointer = transform.Find("AnnotationPointer").gameObject.GetComponent<Line>();

        canvasCtl = canvas.GetComponent<AnnotationCanvasControl>();

        canvasGroup = canvas.GetComponent<CanvasGroup>();
        pointerColor = pointer.Color;

        pointer.Thickness = 0.0125f;

        isLookingAtDot = false;
        isFadingOut = false;
        isVisible = false;
        isInit = true;

        transform.localScale = anchor.transform.localScale / (detectedObj.localScale.x / 0.1f);

        // Put canvas into the right position
        Collider objCollider = detectedObj.GetComponent<BoxCollider>();
        Bounds objBounds = objCollider.bounds;

        Vector3 canvasPos = objBounds.center + new Vector3(0, objBounds.extents.y + canvasPosOffset, 0);
        canvas.transform.position = canvasPos;

        /* Adjust anchor based on the object detected */
        // anchor.transform.localPosition = detectedObj.GetComponent<BoxCollider>().center;

        /* Adjust the eye gaze collider size */
        Transform annotationCollider = transform.Find("AnnotationCollider");
        if (annotationCollider != null ) 
        {
            BoxCollider bc = annotationCollider.GetComponent<BoxCollider>();
            if (bc != null)
            {
                BoxCollider actualCollider = detectedObj.GetComponent<BoxCollider>();
                bc.center = actualCollider.center;
                bc.size = actualCollider.size * (detectedObj.localScale.x / 0.1f);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Remove this as other scripts didn't need this
        if (!EyeGazeManager.Instance) 
        {
            return;
        }

        if (isInit)
        {
            canvas.SetActive(false);
            canvasCtl.canvasInitDone();
            isInit = false;
        }

        EyeTarget currentHit = EyeGazeManager.Instance.CurrentHit;
        GameObject currentHitObject = EyeGazeManager.Instance.CurrentHitObj;

        if (isLookingAtDot && (currentHit != EyeTarget.annotation || currentHitObject.transform.parent.parent.gameObject.name != gameObject.transform.parent.name))
        {
            isLookingAtDot = false;
            StartCoroutine(DisableAnnotation());
        }
        else if (!isLookingAtDot && currentHit == EyeTarget.annotation && currentHitObject.transform.parent.parent.gameObject.name == gameObject.transform.parent.name)
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
        
        /* ---- Annotation canvas always faces user ---- */
        Vector3 diffVector = canvas.transform.position - mainCamera.transform.position;     // Get relative position between camera and canvas
        Vector3 offsetPos = canvas.transform.position + diffVector;                         // Find relative position on the other side of canvas
        canvas.transform.LookAt(offsetPos, Vector3.up);                                     // Let canvas face the other side - content shows backwards
    }

    private IEnumerator EnableAnnotation()
    {
        float elapsed = 0f;
        bool success = false;

        while (isLookingAtDot && elapsed < enableDelay)
        {
            elapsed += Time.deltaTime;

            /* ---- Annotation Pointer ---- */
            Vector3 targetPos, diffVec, diffDir;
            float diffSize;

            // Decide where to stop the pointer
            if (canvasCtl.bHasName)
            {
                if (canvasCtl.bHasImage || canvasCtl.bHasVideo)
                {
                    BoxCollider boxCollider = canvas.GetComponent<BoxCollider>();
                    if (boxCollider)
                    {
                        float halfHeight = boxCollider.size.y * 0.5f * boxCollider.transform.lossyScale.y;
                        Vector3 bottomCenter = new Vector3(boxCollider.transform.position.x,
                            boxCollider.transform.position.y - halfHeight,
                            boxCollider.transform.position.z);
                        targetPos = bottomCenter;
                    }
                    else
                    {
                        targetPos = canvas.transform.position;
                    }

                    diffVec = targetPos - anchor.transform.position;
                    diffDir = diffVec.normalized;
                    // diffSize = diffVec.magnitude * (1 / detectedObj.localScale.y);
                    diffSize = diffVec.magnitude * (1 / 0.1f);

                    pointer.Start = anchor.transform.localPosition;
                }
                else if (canvasCtl.bHasDescription)
                {
                    Transform descObj = canvas.transform.Find("Description");
                    RectTransform rt = descObj.GetComponent<RectTransform>();
                    float nameHeight = rt.rect.height * 0.5f * rt.lossyScale.y;
                    Vector3 namePos = new Vector3(descObj.position.x,
                        descObj.position.y - nameHeight,
                        descObj.position.z);
                    targetPos = namePos;
                    diffVec = targetPos - anchor.transform.position;
                    diffDir = diffVec.normalized;
                    //diffSize = diffVec.magnitude * (1 / detectedObj.localScale.y);
                    diffSize = diffVec.magnitude * (1 / 0.1f);

                    pointer.Start = anchor.transform.localPosition;
                }
                else
                {
                    Transform nameObj = canvas.transform.Find("Name");
                    RectTransform rt = nameObj.GetComponent<RectTransform>();
                    float nameHeight = rt.rect.height * 0.5f * rt.lossyScale.y;
                    Vector3 namePos = new Vector3(nameObj.position.x,
                        nameObj.position.y - nameHeight,
                        nameObj.position.z);
                    targetPos = namePos;
                    diffVec = targetPos - anchor.transform.position;
                    diffDir = diffVec.normalized;
                    //diffSize = diffVec.magnitude * (1 / detectedObj.localScale.y);
                    diffSize = diffVec.magnitude * (1 / 0.1f);

                    pointer.Start = anchor.transform.localPosition;
                }
            }
            else
            {
                targetPos = canvas.transform.localPosition;
                diffVec = targetPos - anchor.transform.localPosition;
                diffDir = diffVec.normalized;
                diffSize = diffVec.magnitude;

                pointer.Start = anchor.transform.localPosition;
            }

            // Calculate the end position of the pointer
            float percent = (elapsed / enableDelay);
            Vector3 endpos = pointer.Start + diffDir * (diffSize * percent);

            pointer.End = endpos;

            // Control the capsule collider's size - it is used to make sure that user can look at the pointer when it grows
            CapsuleCollider capsule = pointer.GetComponent<CapsuleCollider>();
            Vector3 capsuleCenter = (pointer.Start + pointer.End) / 2;
            capsule.center = capsuleCenter;
            float capsuleHeight = (pointer.Start - pointer.End).magnitude;
            capsule.height = capsuleHeight;

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
