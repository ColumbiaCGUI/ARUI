using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public enum DwellButtonType
{
    Toggle=0,
    Select =1
}

/// <summary>
/// Button that can be triggered using touch or eye-gaze dwelling
/// </summary>
public class DwellButton : MonoBehaviour, IMixedRealityTouchHandler
{
    public bool IsInteractingWithBtn = false;
    public float Width => _btnCollider.size.y;

    private bool _isLookingAtBtn = false;
    public bool GetIsLookingAtBtn => _isLookingAtBtn;

    private bool _isTouchingBtn = false;
    private bool _touchable = false;

    private bool _uniqueObj = false;

    private EyeTarget _target;
    private UnityEvent _selectEvent;
    private UnityEvent _quarterSelectEvent;
    private BoxCollider _btnCollider;
    public BoxCollider Collider => _btnCollider;
    private GameObject _btnmesh;

    private DwellButtonType _type = DwellButtonType.Toggle;
    private bool _toggled = false;
    public bool Toggled { 
        set { 
            _toggled = value; 
            SetSelected(value);
        } 
    }

    //*** Btn Dwelling Feedback 
    private Shapes.Disc _loadingDisc;
    private float _startingAngle;

    //*** Btn Push Feedback
    private Shapes.Disc _pushConfiromationDisc;

    //*** Btn Design
    private Material _btnBGMat;
   
    private void Awake()
    {
        Shapes.Disc[] discs = GetComponentsInChildren<Shapes.Disc>(true);
        _loadingDisc = discs[0];
        _pushConfiromationDisc = discs[1];
        _pushConfiromationDisc.enabled = false;

        _startingAngle = _loadingDisc.AngRadiansStart;

        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        _btnBGMat = new Material(mr.material);
        _btnBGMat.color = ARUISettings.BtnBaseColor;
        mr.material = _btnBGMat;

        _selectEvent = new UnityEvent();
        _quarterSelectEvent = new UnityEvent();

        _btnCollider = GetComponentInChildren<BoxCollider>(true);
        _btnmesh = transform.GetChild(0).gameObject;
    }

    public void InitializeButton(EyeTarget target, UnityAction btnSelectEvent, UnityAction btnHalfSelect, 
        bool touchable, DwellButtonType type, bool isUnique = false)
    {
        _uniqueObj = isUnique;
        //TODO: FIGURE OUT HOW TO GET RID OF THIS
        _selectEvent = new UnityEvent();
        _quarterSelectEvent = new UnityEvent();
        this._target = target;
        _selectEvent.AddListener(btnSelectEvent);

        if (btnHalfSelect != null)
            _quarterSelectEvent.AddListener(btnHalfSelect);

        this._touchable = touchable;
        this._type = type;

        if (touchable)
            gameObject.AddComponent<NearInteractionTouchable>();

    }

    private void Update()
    {
        UpdateCurrentlyLooking();
        IsInteractingWithBtn = _isTouchingBtn || _isLookingAtBtn || EyeGazeManager.Instance.CurrentHit.Equals(EyeTarget.textConfirmationWindow);
    }

    private void UpdateCurrentlyLooking()
    {
        bool currentLooking = false;

        if (_uniqueObj)
        {
            if (EyeGazeManager.Instance.CurrentHitObj != null)
            {
                currentLooking = EyeGazeManager.Instance.CurrentHitObj.GetInstanceID() == this.gameObject.GetInstanceID();
            }
        }
        else
        {
            currentLooking = EyeGazeManager.Instance.CurrentHit == _target;
        }

        if (currentLooking && !_isLookingAtBtn && !_isTouchingBtn)
        {
            _isLookingAtBtn = true;
            StartCoroutine(Dwelling());
        }  

        if (!currentLooking || _isTouchingBtn)
        {
            _isLookingAtBtn = false;
            StopCoroutine(Dwelling());
            _btnBGMat.color = ARUISettings.BtnBaseColor;
        }

        _isLookingAtBtn = currentLooking;
    }
    
    private IEnumerator Dwelling()
    {
        AudioManager.Instance.PlaySound(transform.position, SoundType.confirmation);

        _btnBGMat.color = ARUISettings.BtnActiveColor;

        bool halfEventEvoked = false;
        bool success = false;
        float duration = 6.24f / ARUISettings.EyeDwellTime; //full circle in radians

        float elapsed = 0f;
        while (!_isTouchingBtn && _isLookingAtBtn && elapsed < duration)
        {
            if (CoreServices.InputSystem.EyeGazeProvider.GazeTarget == null)
                break;

            elapsed += Time.deltaTime;
            _loadingDisc.AngRadiansEnd = elapsed * ARUISettings.EyeDwellTime;
            _loadingDisc.Color = Color.white;

            if (!halfEventEvoked && _isLookingAtBtn && _quarterSelectEvent != null && elapsed > (duration / 4))
            {
                halfEventEvoked = true;
                _quarterSelectEvent.Invoke();
            }
                
            if (elapsed>duration && _isLookingAtBtn)
                success = true;

            yield return null;
        }

        if (success)
        {
            _selectEvent.Invoke();
            if (_type == DwellButtonType.Toggle)
            {
                _toggled = !_toggled;
                SetSelected(_toggled);
            } else
            {
                _toggled = false;
                SetSelected(false);
            }
        } else
        {
            _btnBGMat.color = ARUISettings.BtnBaseColor;

            if (_type != DwellButtonType.Toggle || (_type == DwellButtonType.Toggle && !_toggled))
                SetSelected(false);
            else if (_type == DwellButtonType.Toggle && _toggled)
                SetSelected(true);
        }
    }

    /// <summary>
    /// Detect the user touching the button
    /// </summary>
    /// <param name="eventData"></param>
    public void OnTouchStarted(HandTrackingInputEventData eventData)
    {
        if (!_touchable) return;
        _isTouchingBtn = true;
        _btnBGMat.color = ARUISettings.BtnActiveColor;
        _pushConfiromationDisc.enabled = true;
    }

    public void OnTouchCompleted(HandTrackingInputEventData eventData)
    {
        if (!_touchable) return;
        _isTouchingBtn = false;

        _btnBGMat.color = ARUISettings.BtnBaseColor;
        _btnmesh.transform.localPosition = Vector3.zero;
        _pushConfiromationDisc.enabled = false;
    }

    public void OnTouchUpdated(HandTrackingInputEventData eventData) 
    {
        if (!_touchable) return;
        _btnmesh.transform.position = eventData.InputData;

        if (_btnmesh.transform.localPosition.z > _pushConfiromationDisc.transform.localPosition.z)
            _pushConfiromationDisc.Color = Color.cyan;
        else _pushConfiromationDisc.Color = Color.white;

        if (_btnmesh.transform.localPosition.z > _pushConfiromationDisc.transform.localPosition.z+0.01f)
            _selectEvent.Invoke();
    }

    private void SetSelected(bool selected)
    {
        if (selected)
        {
            _loadingDisc.AngRadiansEnd = 6.24f;
            _loadingDisc.Color = ARUISettings.BtnLoadingDiscColor;
            _btnBGMat.color = ARUISettings.BtnActiveColor;
        }
        else
        {
            _loadingDisc.AngRadiansEnd = _startingAngle;
            _loadingDisc.Color = Color.white;
            _btnBGMat.color = ARUISettings.BtnBaseColor;
        }
    }
}