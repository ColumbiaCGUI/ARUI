using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    private ObjectManipulator _grabbable;
    public ObjectManipulator Grab => _grabbable;

    private bool _grabbingAllowed = true;
    public bool IsGrabbingAllowed
    {
        get { return _grabbingAllowed; }
        set { _grabbable.enabled = value; }
    }

    private bool _isProcessingClosedHand = false;
    public bool IsProcessingClosedHand
    {
        get { return _isProcessingClosedHand;}
        set { _isProcessingClosedHand = value;}
    }

    private DraggableHandle _draggingHandle;
    public DraggableHandle DraggableHandle
    {
        get { return _draggingHandle; }
        set { _draggingHandle = value; }
    }

    public void Start()
    {
        _grabbable = gameObject.GetComponent<ObjectManipulator>();
        if (_grabbable == null )
        {
            _grabbable = gameObject.AddComponent<ObjectManipulator>();
        }
    }

    public IEnumerator TransitionToFullHandle(System.Action? action =null)
    {
        if (_draggingHandle !=null)
        {
            if (action != null)
                action.Invoke();

            float duration = 2f;
            float pastSeconds = 0;
            while (_isProcessingClosedHand && pastSeconds < duration)
            {
                pastSeconds += Time.deltaTime;
                _draggingHandle.SetHandleProgress(pastSeconds / duration);
                yield return new WaitForEndOfFrame();
            }

            _isProcessingClosedHand = false;
        }
        _isProcessingClosedHand = false;
    }

    protected bool isUsedHandClosed(MixedRealityPointerEventData eventData)
    { 
        return (
            Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right == eventData.Handedness && HandPoseManager.Instance.rightPose == Holofunk.HandPose.HandPose.Closed)
        || (Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left == eventData.Handedness && HandPoseManager.Instance.leftPose == Holofunk.HandPose.HandPose.Closed);
    }
}
