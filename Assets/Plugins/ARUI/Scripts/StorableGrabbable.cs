using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class StorableGrabbable : MonoBehaviour, IMixedRealityPointerHandler
{
    private ObjectManipulator _grabbable;
    private StorableObject storableObject;

    private bool _grabbingAllowed = true;
    public bool IsGrabbingAllowed
    {
        get { return _grabbingAllowed; }
        set { _grabbable.enabled = value; }
    }

    public bool _isGrabbed = false;

    public bool IsGrabbed => _isGrabbed;

    private void Start()
    {
        gameObject.AddComponent<NearInteractionGrabbable>();
        _grabbable = gameObject.AddComponent<ObjectManipulator>();

        storableObject = GetComponentInChildren<StorableObject>();

        _grabbable.OnHoverEntered.AddListener(delegate { OnHoverStarted(); });
        _grabbable.OnHoverExited.AddListener(delegate { OnHoverExited(); });
    }

    private void OnHoverStarted() { }

    private void OnHoverExited() { }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveStart);
        _isGrabbed = true;
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        _isGrabbed = true;
    }


    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveEnd);
        _isGrabbed = false;
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }

}