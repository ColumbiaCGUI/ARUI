using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Shapes;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Annotation : MonoBehaviour
{
    // ========================================================================
    // private properties
    // ========================================================================
    [SerializeField]
    private int id;
    [SerializeField]
    private float enableDelay = 1.0f;
    [SerializeField]
    private float disableDelay = 10.0f;
    [SerializeField]
    private float canvasPosOffset = 0.01f;
    [SerializeField]
    private float pointerResetTime = 1f;

    private float elapsed;

    private bool isGazing;
    private bool isFadingOut;
    private bool isVisible;
    private bool isInit;
    private bool isDisabling;

    private Line pointer;
    private Color pointerColor;
    private Coroutine fadeOutCoroutine;
    private Transform detectedObj;
    private GameObject mainCamera;
    private GameObject canvas;
    private GameObject anchor;
    private CanvasGroup canvasGroup;
    private AnnotationCanvasControl canvasCtl;

    // ========================================================================
    // Start()
    // ========================================================================
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

        isGazing = false;
        isFadingOut = false;
        isVisible = false;
        isInit = true;
        isDisabling = false;

        transform.localScale = anchor.transform.localScale / (detectedObj.localScale.x / 0.1f);

        // Put canvas into the right position
        Collider objCollider = detectedObj.GetComponent<BoxCollider>();
        Bounds objBounds = objCollider.bounds;

        Vector3 canvasPos = objBounds.center + new Vector3(0, objBounds.extents.y + canvasPosOffset, 0);
        canvas.transform.position = canvasPos;

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

        elapsed = 0;
    }

    // ========================================================================
    // Update()
    // ========================================================================
    void Update()
    {
        // TODO: Remove this as other scripts didn't need this
        if (!EyeGazeManager.Instance) return;

        if (isInit)
        {
            canvas.SetActive(false);
            // canvasCtl.canvasInitDone();
            isInit = false;
        }

        // Get eye gaze data
        EyeTarget currentHit = EyeGazeManager.Instance.CurrentHit;
        GameObject currentHitObject = EyeGazeManager.Instance.CurrentHitObj;
        EyeTarget prevHit = EyeGazeManager.Instance.PrevHit;
        GameObject prevHitObj = EyeGazeManager.Instance.PrevHitObj;

        Collider[] hitColliders = EyeGazeManager.Instance.hitColliders;
        Collider currentCollider = transform.Find("AnnotationCollider").GetComponent<Collider>();

        // If start looking at this object
        if (!isGazing
            && ((currentHit == EyeTarget.annotation && currentHitObject.transform.parent.parent.name == transform.parent.name)
                || EyeGazeManager.Instance.HitCollidersHave(currentCollider)))
        {
            isGazing = true;
            if (!isVisible)
            {
                StartCoroutine(EnableAnnotation());
            }
        }
        // If moved eye away from current object
        else if (isGazing
                 && ((currentHit != EyeTarget.annotation || currentHitObject.transform.parent.parent.name != transform.parent.name)
                     && !EyeGazeManager.Instance.HitCollidersHave(currentCollider)))
        {
            isGazing = false;
            StartCoroutine(DisableAnnotation());
        }

        if (isDisabling)
        {
            if (prevHit == EyeTarget.annotation && prevHitObj.transform.parent.parent.name != transform.parent.name)
            {
                StopCoroutine(DisableAnnotation());

                fadeOutCoroutine = StartCoroutine(FadeOut());
            }
        }

        // If look back when fading out
        if (isFadingOut && isGazing) 
        {
            // Stop fading out
            StopCoroutine(fadeOutCoroutine);
            isFadingOut = false;
            // Resume alpha
            canvasGroup.alpha = 1.0f;
            pointer.Color = pointerColor;
        }

        // Update collider position of canvas
        UpdateCanvasCollider();
    }

    // ========================================================================
    // LateUpdate()
    // ========================================================================
    private void LateUpdate()
    {
        /* ---- Anchor dot faces user ---- */
        anchor.transform.LookAt(mainCamera.transform);
        
        /* ---- Canvas faces user ---- */
        Vector3 diffVector = canvas.transform.position - mainCamera.transform.position;     // Get relative position between camera and canvas
        Vector3 offsetPos = canvas.transform.position + diffVector;                         // Find relative position on the other side of canvas
        canvas.transform.LookAt(offsetPos, Vector3.up);                                     // Let canvas face the other side - content shows backwards

        AnchorVisibility();
    }

    // ========================================================================
    // EnableAnnotation()
    // ========================================================================
    private IEnumerator EnableAnnotation()
    {
        bool success = false;

        canvas.SetActive(true);

        while (isGazing && elapsed < enableDelay)
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

            if (elapsed > enableDelay && isGazing)
                success = true;

            yield return null;
        }

        if (success)
        {
            if (canvasCtl.bHasName) canvas.transform.Find("Name").gameObject.SetActive(true);
            if (canvasCtl.bHasDescription) canvas.transform.Find("Description").gameObject.SetActive(true);
            if (canvasCtl.bHasImage) canvas.transform.Find("Image").gameObject.SetActive(true);
            if (canvasCtl.bHasVideo) canvas.transform.Find("Video").gameObject.SetActive(true);

            isVisible = true;
            elapsed = 0;
        }
        else
        {
            // Erase the pointer if user looked away in the progress
            pointer.Start = anchor.transform.localPosition;
            pointer.End = anchor.transform.localPosition;
            ResetPointer();
        }
    }

    // ========================================================================
    // DisableAnnotation()
    // ========================================================================
    private IEnumerator DisableAnnotation()
    {
        isDisabling = true;

        yield return new WaitForSeconds(disableDelay);

        isDisabling = false;

        fadeOutCoroutine = StartCoroutine(FadeOut());
    }

    // ========================================================================
    // UpdateCanvasCollider()
    // ========================================================================
    private void UpdateCanvasCollider()
    {
        // Get collider instance and recttransform
        BoxCollider collider = canvas.GetComponent<BoxCollider>();
        RectTransform rectTransform = canvas.GetComponent<RectTransform>();

        collider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 1);
    }

    // ========================================================================
    // Fadeout()
    // ========================================================================
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

        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            canvas.transform.GetChild(i).gameObject.SetActive(false);
        }

        pointer.Color = pointerColor;
        pointer.Start = anchor.transform.localPosition;
        pointer.End = anchor.transform.localPosition;

        isFadingOut = false;
        isVisible = false;
        elapsed = 0;
    }

    // ========================================================================
    // AnchorVisibility()
    // ========================================================================
    private void AnchorVisibility()
    {
        int objectColliderLayer = LayerMask.GetMask("ObjectCollider");
        Vector3 anchorToCamera = mainCamera.transform.position - anchor.transform.position;
        Ray ray = new Ray(anchor.transform.position, anchorToCamera);
        RaycastHit[] hits = Physics.RaycastAll(ray, anchorToCamera.magnitude, objectColliderLayer);

        if (hits.Length > 0)
        {
            anchor.SetActive(false);
        }
        else
        {
            anchor.SetActive(true);
        }
    }

    // ========================================================================
    // ResetPointer()
    // ========================================================================
    private IEnumerator ResetPointer()
    {
        yield return new WaitForSeconds(pointerResetTime);

        if (!isVisible) elapsed = 0;
    }
}
