using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

/// <summary>
/// Catch pointer and dragging events at orb
/// </summary>
public class OrbGrabbable : MonoBehaviour, IMixedRealityPointerHandler
{
    private ObjectManipulator _grabbable;

    private void Start()
    {
        _grabbable = gameObject.GetComponent<ObjectManipulator>();

        _grabbable.OnHoverEntered.AddListener(delegate { OnHoverStarted(); });
        _grabbable.OnHoverExited.AddListener(delegate { OnHoverExited(); });
    }

    private void OnHoverStarted() => Orb.Instance.SetNearHover(true);

    private void OnHoverExited() => Orb.Instance.SetNearHover(false);

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        Orb.Instance.SetIsDragging(true);
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveStart);
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        Orb.Instance.SetIsDragging(false);
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveEnd);
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) {}
}