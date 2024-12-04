using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a storable object that can be set to follow the virtual agent
/// </summary>
public class StorableObject : MonoBehaviour
{
    private int _id = -1;
    public int ID { get { return _id; } }

    private BoxCollider _collider;                  // BoxCollider for the GameObject

    private bool _hadCollider = false;

    private bool _isLookingAt = false;              // Tracks if the object is currently being looked at
    public bool IsLookingAtObj => _isLookingAt; 

    public OrbStorageBox CurrentStorage = null;     // The current storage associated with this object
    public string LabelMessage
    {
        get
        {
            if (CurrentStorage != null)
            {
                return OrbStorageManager.Instance.LabelMessageUntether;
            }
            return OrbStorageManager.Instance.LabelMessageTether;
        }
    }

    private float _followSpeed = 2f; // Delay factor for the object's following movement
    private float _moveDuration = 1f;

    private Vector3 _originalScale; // Original scale of the GameObject
    private Vector3 _originalPosition; // Original position of the GameObject
    private int _originalLayer;
    private Vector3 _originalBounds = new Vector3(1, 1, 1);
    public Vector3 OriginalBounds => _originalBounds;

    private StorableGrabbable _grabbable = null; // Reference to the StorableGrabbable component
    public StorableGrabbable Grabbable => _grabbable;

    private bool _droppableZone = false;
    public bool Droppable
    {
        get { return _droppableZone; }
        set { _droppableZone = value; }
    }

    /// <summary>
    /// Every frame, check if the object is being looked at or grabbed, and if not, 
    /// smoothly moves it towards its associated storage position if it is in storage.
    /// </summary>
    public void Update()
    {
        if (EyeGazeManager.Instance)
            _isLookingAt = EyeGazeManager.Instance.CurrentHitID == gameObject.GetInstanceID();

        if (CurrentStorage != null && !_isLookingAt && !_grabbable.IsDragged)
        {
            transform.position = Vector3.Lerp(transform.position, CurrentStorage.transform.position, _followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(transform.position-AngelARUI.Instance.ARCamera.transform.position);
        }
    }

    #region Registering and Deregistering Process
    /// <summary>
    /// Initializes the GameObject by storing its original position and scale, 
    /// ensuring it has a valid BoxCollider, and registering it with the EyeGazeManager.
    /// If no mesh is found, a default 0.5-meter cube collider is created. 
    /// Also sets the GameObject's layer and adds a StorableGrabbable component.
    /// </summary>
    public void Register(int id)
    {
        _id = id;
        _originalScale = transform.localScale;
        _originalPosition = transform.position;
        _originalLayer = gameObject.layer;

        // Set bounds and collider
        bool boundsAreSet = TryToGetBoundsFromCollider();
        if (!_hadCollider)
        {
            var center = Vector3.zero;
            if (!boundsAreSet)
                center = TryToGetBoundsFromMesh();

            // Add a new BoxCollider if none exists
            _collider = gameObject.AddComponent<BoxCollider>();
            _collider.center = center;    
            _collider.size = _originalBounds;
        }

        // Set the layer and register eye target
        gameObject.layer = StringResources.LayerToInt(StringResources.UI_layer);
        EyeGazeManager.Instance.RegisterEyeTargetID(gameObject);

        _grabbable = gameObject.AddComponent<StorableGrabbable>();
    }

    public void Deregister()
    {
        transform.position = _originalPosition;
        transform.localScale = _originalScale;
        gameObject.layer = _originalLayer;

        EyeGazeManager.Instance.DeRegisterEyeTarget(gameObject);

        if (!_hadCollider)
            Destroy(_collider);
        Destroy(_grabbable);
    }

    /// <summary>
    /// Try to get the extents of this mesh by checking canvas or meshfilter components in children and this one
    /// </summary>
    private Vector3 TryToGetBoundsFromMesh()
    {
        if (GetComponent<RectTransform>()) // check for canvas
        {
            // Get the mesh filter of the target GameObject
            RectTransform targetCanvas = GetComponentInChildren<RectTransform>();
            _originalBounds = new Vector3(targetCanvas.rect.width, targetCanvas.rect.height, 0.001f);
            return Vector3.zero;

        } else if (GetComponentsInChildren<MeshFilter>()!=null) //check for meshfilter components
        {
            Bounds combinedBounds = GetCombinedBounds();

            // Set the BoxCollider's center and size based on the combined bounds.
            // Account for the scaling of the parentObject itself.
            Vector3 adjustedCenter = transform.InverseTransformPoint(combinedBounds.center);
            Vector3 adjustedSize = combinedBounds.size;

            // Since the BoxCollider is attached to the parent, we need to divide the size by the parent's local scale to get the correct collider size.
            Vector3 parentScale = transform.lossyScale;
            adjustedSize = new Vector3(
                adjustedSize.x / Mathf.Abs(parentScale.x),
                adjustedSize.y / Mathf.Abs(parentScale.y),
                adjustedSize.z / Mathf.Abs(parentScale.z)
            );
            _originalBounds = adjustedSize;
            return adjustedCenter;
        }

        return Vector3.zero; 
    }

    private Bounds GetCombinedBounds()
    {
        // Initialize an empty bounds object.
        Bounds combinedBounds = new Bounds();
        bool boundsInitialized = false;

        // Iterate over all children and include the parent itself.
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                // Get the mesh and calculate the bounds in world space.
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh != null)
                {
                    Bounds meshBounds = mesh.bounds;

                    // Transform mesh bounds to world space using the child's transform.
                    Vector3[] vertices = mesh.vertices;
                    foreach (Vector3 vertex in vertices)
                    {
                        Vector3 worldVertex = child.TransformPoint(vertex);
                        if (!boundsInitialized)
                        {
                            combinedBounds = new Bounds(worldVertex, Vector3.zero);
                            boundsInitialized = true;
                        }
                        else
                        {
                            combinedBounds.Encapsulate(worldVertex);
                        }
                    }
                }
            }
        }

        return combinedBounds;
    }

    /// <summary>
    /// Tries to get a collider mesh from this object or its children. If yes, it will be used as bounds
    /// </summary>
    private bool TryToGetBoundsFromCollider()
    {
        // Set bounds
        if (GetComponentInChildren<Collider>()) // check if parent has mesh
        {
            var allColliders = GetComponentsInChildren<Collider>();
            float maxDim = -111;
            Collider selectedCollider = GetComponentInChildren<Collider>();
            foreach (Collider col in allColliders)
            {
                var currentMaxDim = Mathf.Max(col.bounds.size.x, col.bounds.size.y, col.bounds.size.z);
                if (currentMaxDim > maxDim)
                {
                    maxDim = currentMaxDim;
                    selectedCollider = col;
                }
            }

            if (selectedCollider is BoxCollider)
            {
                _collider = selectedCollider as BoxCollider;
                _originalBounds = _collider.bounds.size;
                _hadCollider = true;
            }

            if (maxDim<0)
                return false;

            _originalBounds = new Vector3(
    selectedCollider.bounds.size.x / transform.lossyScale.x,
    selectedCollider.bounds.size.y / transform.lossyScale.y,
    selectedCollider.bounds.size.z / transform.lossyScale.z
);
            return true;
        }

        return false;
    }

    #endregion

    #region Tether and Untethering

    /// <summary>
    /// Scale object to original size. This is called when the object is untethered
    /// </summary>
    public void ToOriginalPosScale(Vector3? newPos = null)
    {
        Vector3 targetPos = _originalPosition;
        if (newPos != null)
            targetPos = (Vector3)newPos;

        StartCoroutine(ToOriginalPosScale(targetPos));
    }

    /// <summary>
    /// Smoothly moves the GameObject back to its original position and scale over a duration of 1 second.
    /// Uses linear interpolation for the transition and ensures the final position and scale 
    /// are set accurately at the end of the movement.
    /// </summary>
    /// <param name="targetPos">new position after untethering</param>
    /// <returns></returns>
    private IEnumerator ToOriginalPosScale(Vector3 targetPos)
    {
        Vector3 targetScale = _originalScale;

        float elapsedTime = Time.deltaTime;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        while (Vector3.Distance(transform.localScale, targetScale) > 0.001f)
        {
            float t = (elapsedTime / _moveDuration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        transform.position = targetPos;
        transform.localScale = _originalScale;
    }

    /// <summary>
    /// Scales the current GameObject to fit within a bounding box of a specified radius.
    /// It calculates the uniform scale factor based on the mesh or UI element size to
    /// ensure the GameObject is resized proportionally within the given radius.
    /// </summary>
    /// <param name="dim">The dim of the bounding box within which the GameObject should fit.</param>
    public void ScaleToBoxSize(float dim)
    {
        Vector3 targetScale = new Vector3(dim, dim, dim);

        // Get the world size of the target GameObject's mesh
        Vector3 currentScale = Vector3.Scale(_originalBounds, transform.lossyScale);

        // Calculate the uniform scaling factor for the target GameObject to fit inside the current GameObject's dimensions
        float scaleFactorX = targetScale.x / currentScale.x;
        float scaleFactorY = targetScale.y / currentScale.y;
        float scaleFactorZ = targetScale.z / currentScale.z;
        float uniformScaleFactor = Mathf.Min(scaleFactorX, Mathf.Min(scaleFactorY, scaleFactorZ));

        // Apply the new uniform scale to the target GameObject
        transform.localScale = transform.localScale * uniformScaleFactor;
    }

    #endregion
}