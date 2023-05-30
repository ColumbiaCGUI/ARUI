using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

/// <summary>
/// Catches pointer and dragging events at orb
/// </summary>
public class TaskListGrabbable : MonoBehaviour, IMixedRealityPointerHandler
{
    private ObjectManipulator _grabbable;

    public void Start()
    {
        _grabbable = gameObject.GetComponent<ObjectManipulator>();

        _grabbable.OnHoverEntered.AddListener(delegate { OnHoverStarted(); });
        _grabbable.OnHoverExited.AddListener(delegate { OnHoverExited(); });
    }

    private void OnHoverStarted() => TaskListManager.Instance.NearHovering = true;

    private void OnHoverExited() => TaskListManager.Instance.NearHovering = false;

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveStart);
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveEnd);
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
}