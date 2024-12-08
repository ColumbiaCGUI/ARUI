using Microsoft.MixedReality.Toolkit.Input;
using System.Diagnostics;

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
        //AngelARUI.Instance.DebugLogMessage(isUsedHandClosed(eventData).ToString(), true);
        //AngelARUI.Instance.DebugLogMessage("Right - " + eventData.Handedness.ToString() + " and " + HandPoseManager.Instance.rightPose, true);
        //if (!IsProcessingClosedHand && isUsedHandClosed(eventData))
        //{
        //    //start countdown for or fix
        //    IsProcessingClosedHand = true;
        //    StartCoroutine(TransitionToFullHandle(() => Orb.Instance.UpdateMovementbehavior(MovementBehavior.Fixed)));
        //} else if (IsProcessingClosedHand && !isUsedHandClosed(eventData))
        //{
        //    IsProcessingClosedHand = false;
        //    StopCoroutine(TransitionToFullHandle(() => Orb.Instance.UpdateMovementbehavior(MovementBehavior.Fixed)));

        //    Orb.Instance.UpdateMovementbehavior(MovementBehavior.Follow);

        //} else if (!IsProcessingClosedHand && !isUsedHandClosed(eventData))
        //{
        //    Orb.Instance.UpdateMovementbehavior(MovementBehavior.Follow);
        //}
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        Orb.Instance.SetIsDragging(false);
        AudioManager.Instance.PlaySound(transform.position, SoundType.moveEnd);
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) {}

}