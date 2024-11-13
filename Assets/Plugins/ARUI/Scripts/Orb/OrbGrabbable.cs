using Microsoft.MixedReality.Toolkit.Input;

/// <summary>
/// Catch pointer and dragging events at orb
/// </summary>
public class OrbGrabbable : Grabbable, IMixedRealityPointerHandler
{
    void Start()
    {
        base.Start();

        Grab.OnHoverEntered.AddListener(delegate { OnHoverStarted(); });
        Grab.OnHoverExited.AddListener(delegate { OnHoverExited(); });
    }

    private void OnHoverStarted() => Orb.Instance.SetNearHover(true);
    private void OnHoverExited() => Orb.Instance.SetNearHover(false);

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        Orb.Instance.SetIsDragging(true);
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveStart);
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        if (!IsProcessingClosedHand && isUsedHandClosed(eventData))
        {
            //start countdown for or fix
            IsProcessingClosedHand = true;
            StartCoroutine(TransitionToFullHandle(() => Orb.Instance.UpdateMovementbehavior(MovementBehavior.Fixed)));
        } else if (IsProcessingClosedHand && !isUsedHandClosed(eventData))
        {
            IsProcessingClosedHand = false;
            StopCoroutine(TransitionToFullHandle(() => Orb.Instance.UpdateMovementbehavior(MovementBehavior.Fixed)));

            Orb.Instance.UpdateMovementbehavior(MovementBehavior.Follow);

        } else if (!IsProcessingClosedHand && !isUsedHandClosed(eventData))
        {
            Orb.Instance.UpdateMovementbehavior(MovementBehavior.Follow);
        }
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        Orb.Instance.SetIsDragging(false);
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveEnd);
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) {}

}