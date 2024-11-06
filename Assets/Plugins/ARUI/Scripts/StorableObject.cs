using Microsoft.MixedReality.OpenXR;
using System;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

public class StorableObject : MonoBehaviour
{
    private SphereCollider _collider;
    public SphereCollider Collider => _collider;

    private bool _isLookingAt = false;
    public bool IsLookingAtObj => _isLookingAt;

    // Store the original scale of the target GameObject
    private Vector3 _originalTargetScale;

    public bool IsStored = false;

    public string LabelMessage = "Say 'Follow'";

    public void Update()
    {
        _isLookingAt = EyeGazeManager.Instance.CurrentHitID == gameObject.GetInstanceID();
    }

    public void Initialize()
    {
        _originalTargetScale = transform.localScale;

        if (gameObject.GetComponent<SphereCollider>() == null)
        {
            // Get the mesh renderer to calculate bounds
            var meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
            float largestDimension = 0.5f; //assuming half a meter is largest
            if (meshRenderer != null)
            {
                // Calculate the largest dimension of the bounds
                largestDimension = Mathf.Max(meshRenderer.bounds.max.x, meshRenderer.bounds.max.y, meshRenderer.bounds.max.z);
            }

            // Add a SphereCollider and set its radius
            _collider = gameObject.AddComponent<SphereCollider>();
            _collider.radius = largestDimension / 2f; // Radius is half the diameter
            _collider.center = Vector3.zero; // Center it, assuming no offset is needed

        }
        else
        {
            _collider = gameObject.GetComponent<SphereCollider>();
        }

        // Set the layer and register eye target
        gameObject.layer = StringResources.LayerToInt(StringResources.UI_layer);

        EyeGazeManager.Instance.RegisterEyeTargetID(gameObject);

        gameObject.AddComponent<StorableGrabbable>();
    }

    public void ScaleToOriginalSize()
    {
        transform.localScale = _originalTargetScale;
        LabelMessage = "Say 'Follow'";
    }

    public void ScaleToBoxSize(float radius)
    {
        // Get the mesh filter of the target GameObject
        Vector3 maxSize = new Vector3(1f,1f,1f); //fallback assumption is that the target object's max dimension is 1 meter

        var mesh = GetComponentInChildren<MeshFilter>();
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
}