using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using UnityEngine;

public class StorableGrabbable : Grabbable, IMixedRealityPointerHandler
{
    public bool _isGrabbed = false;
    public bool IsGrabbed => _isGrabbed;

    private bool _isDragged = false;
    public bool IsDragged => _isDragged;

    private StorableObject storableObject;

    private void Start()
    {
        base.Start();

        gameObject.AddComponent<NearInteractionGrabbable>();
        storableObject = GetComponentInChildren<StorableObject>();
    }

    private void Update()
    {
       if (DraggableHandle != null)
       {
            DraggableHandle.transform.position = transform.position + new Vector3(0,-0.02f,0);
       }
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveStart);
        _isGrabbed = true;
        _isDragged = false;
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        _isDragged = true;
        _isGrabbed = true;
    }


    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveEnd);

        if (DraggableHandle !=null && DraggableHandle.Progress>=1.0f)
        {
            OrbStorageManager.Instance.HandleUnstore(storableObject.ID, transform.position);
        }

        _isGrabbed = false;
        _isDragged = false;
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }


    public void OnDestroy()
    {
        if (gameObject.GetComponent<NearInteractionGrabbable>())
        {
            Destroy(gameObject.GetComponent<NearInteractionGrabbable>());
        }
        if (gameObject.GetComponent<ObjectManipulator>())
        {
            Destroy(gameObject.GetComponent<ObjectManipulator>());
        }
        if (gameObject.GetComponent<ConstraintManager>())
        {
            Destroy(gameObject.GetComponent<ConstraintManager>());
        }
    }
}