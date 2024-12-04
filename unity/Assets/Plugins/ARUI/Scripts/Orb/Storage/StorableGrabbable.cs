using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using UnityEngine;

public class StorableGrabbable : Grabbable, IMixedRealityPointerHandler
{
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

        _isDragged = false;
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveStart);
        _isDragged = false;
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        _isDragged = true;
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveEnd);

        if (DraggableHandle !=null && DraggableHandle.Progress>=1.0f)
        {
            OrbStorageManager.Instance.HandleUnstore(storableObject.ID, transform.position);
        } else if (storableObject.Droppable)
        {
            OrbStorageManager.Instance.HandleStore(storableObject.ID);
        }

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