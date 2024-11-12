using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

public class StorableObject : VMControllable
{
    private BoxCollider _collider;
    public BoxCollider Collider => _collider;

    private bool _isLookingAt = false;
    public bool IsLookingAtObj => _isLookingAt;

    // Store the original scale of the target GameObject
    private Vector3 _originalScale;
    private Vector3 _originalPosition;

    public OrbStorageBox CurrentStorage = null;

    public string LabelMessage = "Say 'Follow'";

    private float _followSpeed = 2f;   // Delay factor for the following movement.

    private StorableGrabbable _grabbable= null;

    private bool _isMoving = false;

    public void Update()
    {
        base.Update();

        _isLookingAt = EyeGazeManager.Instance.CurrentHitID == gameObject.GetInstanceID();

        if (CurrentStorage != null && !_isLookingAt && !_grabbable.IsGrabbed && !_isMoving)
        {
            if (CurrentStorage.PlaceInList == 1)
                priority = 1;
            else 
                priority = 0;

            //Get default target pos (next to orb, storage box position
            DesiredPos = Vector3.Lerp(transform.position, CurrentStorage.transform.position, _followSpeed * Time.deltaTime);

            Vector3 vmOptimal = Vector3.zero;
            //if (AngelARUI.Instance.IsVMActiv)
            //    vmOptimal = GetOptimalPos();

            if (vmOptimal != Vector3.zero) 
            {
                StartCoroutine(MoveToPose());
            } else
            {
                transform.position = DesiredPos;
            }
        }
    }

    private IEnumerator MoveToPose()
    {
        _isMoving = true;

        Vector3 startPos = transform.position;
        float elapsedTime = Time.deltaTime;
        Vector3 vmOptimal = DesiredPos;
        while (Vector3.Distance(transform.position, vmOptimal) > 0.001f)
        {
            transform.position = Vector3.Lerp(startPos, DesiredPos, (elapsedTime / 1f));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        transform.position = DesiredPos;
        yield return new WaitForEndOfFrame();

        _isMoving = false;
    }

    private Vector3 GetOptimalPos()
    {
        Vector3 optimalPos = Vector3.zero;
        Rect getBest = ViewManagement.Instance.GetBestEmptyRect(this);
        if (getBest != Rect.zero)
        {
            float depth = (transform.position - AngelARUI.Instance.ARCamera.transform.position).magnitude;

            Vector3 pivot = new Vector3(getBest.x + getBest.width / 2, getBest.y + getBest.height / 2, depth);
            optimalPos = AngelARUI.Instance.ARCamera.ScreenToWorldPoint(pivot);
        }
        return optimalPos;
    }

    /// <summary>
    /// Prepare the storable object
    /// 
    /// </summary>
    public void Initialize()
    {
        _originalScale = transform.localScale;
        _originalPosition = transform.position;

        _collider = gameObject.GetComponent<BoxCollider>();
        if (_collider == null)
        {
            _collider = gameObject.AddComponent<BoxCollider>();

            // If no mesh is available, generate a 0.5 cube collider
            var meshProvider = gameObject.GetComponentInChildren<MeshFilter>();
            if (meshProvider == null)
            {
                Bounds bounds = new Bounds(Vector3.zero, new Vector3(0.5f, 0.5f, 0.5f)); // if no mesh can be found, assume a half meter cube
                _collider.center = Vector3.zero; // Center it, assuming no offset is needed
                _collider.size = bounds.extents;
            }
        }

        // Set the layer and register eye target
        gameObject.layer = StringResources.LayerToInt(StringResources.UI_layer);
        EyeGazeManager.Instance.RegisterEyeTargetID(gameObject);

        _grabbable =gameObject.AddComponent<StorableGrabbable>();

        IsVMReady = false; //turn off for now
    }

    #region Scaling

    /// <summary>
    /// Scale object to original size. This is called when the object is untethered
    /// </summary>
    public void ScaleToOriginalSize()
    {
        LabelMessage = "Say 'Follow'";

        StartCoroutine(MoveToOriginal());
    }

    private IEnumerator MoveToOriginal()
    {
        Vector3 targetPos = _originalPosition;
        Vector3 targetScale = _originalScale;

        float elapsedTime = Time.deltaTime;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        while (Vector3.Distance(transform.position, targetPos) > 0.001f)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, (elapsedTime / 1f));
            transform.localScale = Vector3.Lerp(startScale, targetScale, (elapsedTime / 1f));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        transform.position = _originalPosition;
        transform.localScale = _originalScale;
    }

    /// <summary>
    /// Scale object to tethered size. The largest dimension should fit in the tethered space
    /// </summary>
    /// <param name="radius"></param>
    public void ScaleToBoxSize(float radius)
    {
        // Get the mesh filter of the target GameObject
        Vector3 maxSize = new Vector3(1f,1f,1f); //fallback assumption is that the target object's max dimension is 1 meter

        var mesh = GetComponent<MeshFilter>();
        if (mesh == null || mesh.mesh == null)
        {
            // Get the mesh filter of the target GameObject
            RectTransform targetCanvas = GetComponentInChildren<RectTransform>();
            if (targetCanvas != null)
            {
                maxSize = new Vector3(targetCanvas.rect.width, targetCanvas.rect.height, 0.001f);
            }
        }
        else
        {
            maxSize = mesh.mesh.bounds.size;
        }


        // Get the world size of the current GameObject's mesh
        Vector3 currentMeshSize = Vector3.Scale(new Vector3(radius * 2, radius * 2, radius * 2), transform.lossyScale);

        // Get the world size of the target GameObject's mesh
        Vector3 targetMeshSize = Vector3.Scale(maxSize, transform.lossyScale);

        // Calculate the uniform scaling factor for the target GameObject to fit inside the current GameObject's dimensions
        float scaleFactorX = currentMeshSize.x / targetMeshSize.x;
        float scaleFactorY = currentMeshSize.y / targetMeshSize.y;
        float scaleFactorZ = currentMeshSize.z / targetMeshSize.z;
        float uniformScaleFactor = Mathf.Min(scaleFactorX, Mathf.Min(scaleFactorY, scaleFactorZ));

        // Apply the new uniform scale to the target GameObject
        transform.localScale = transform.localScale * uniformScaleFactor;

        LabelMessage = "Say 'Unfollow'";
    }

    #endregion
}