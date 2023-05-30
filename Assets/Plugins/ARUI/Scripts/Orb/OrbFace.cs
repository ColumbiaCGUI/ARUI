using System.Collections;
using UnityEngine;

public enum OrbStates
{
    Idle = 0,
    Loading = 1,
}

/// <summary>
/// Represents the visual representation of the orb (the disc)
/// </summary>
public class OrbFace : MonoBehaviour
{
    ///** Orb face parts and states
    private OrbStates _currentFaceState = OrbStates.Idle;
    private Shapes.Disc _face;
    private Shapes.Disc _eyes;
    private Shapes.Disc _mouth;

    ///** Colors of orb states
    private Color _faceColorInner = new Color(1, 1, 1, 1f);
    private Color _faceColorOuter = new Color(1, 1, 1, 1f);

    private GameObject _notificationIcon;
    private bool _notificationIconActive = false;
    public bool NotificationEnabled
    {
        get { return _notificationIconActive; } 
        set { 
            _notificationIconActive = value;
            if (_notificationIconActive) 
                _face.ColorOuter = Color.black;
            else
                _face.ColorOuter = _faceColorOuter;

            _notificationIcon.SetActive(value);
        }
    }

    private float _initialMouthScale;
    public float MouthScale { 
        get => _mouth.Radius;
        set
        {
            if (value<=0)
            {
                _mouth.Radius = _initialMouthScale;
                _mouth.gameObject.SetActive(false);
            } else
            {
                _mouth.gameObject.SetActive(true);
                _mouth.Radius = Mathf.Clamp(_initialMouthScale - value, 0.5f, _initialMouthScale);
            }
            
        }
    }

    private bool _userIsLooking = false;
    public bool UserIsLooking
    {
        get => _userIsLooking;
        set { _userIsLooking = value; }
    }
    
    private bool _userIsGrabbing = false;
    public bool UserIsGrabbing
    {
        get => _userIsGrabbing;
        set
        {
            _userIsGrabbing = value;
        }
    }

    private void Start()
    {
        Shapes.Disc[] allDiscs = GetComponentsInChildren<Shapes.Disc>();
        _face = allDiscs[0];
        _mouth = allDiscs[1];
        _eyes = allDiscs[2];
        _eyes.gameObject.SetActive(false);

        //Get notification object in orb prefab
        _notificationIcon = transform.GetChild(1).gameObject;
        _notificationIcon.SetActive(false);

        _faceColorOuter = _face.ColorOuter;
        _faceColorInner = _face.ColorInner;

        _initialMouthScale = _mouth.Radius;
        _mouth.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_userIsLooking || _userIsGrabbing && !_eyes.gameObject.activeSelf)
            _eyes.gameObject.SetActive(true);

        else if (!_userIsLooking && !_userIsGrabbing && _eyes.gameObject.activeSelf)
            _eyes.gameObject.SetActive(false);
    }

    public void SetOrbState(OrbStates newState)
    {
        if (newState.Equals(OrbStates.Loading) &&
            _currentFaceState!= OrbStates.Loading)
        {
            _face.Type = Shapes.DiscType.Arc;
            StartCoroutine(Rotating());

        } else if (newState.Equals(OrbStates.Idle) &&
            _currentFaceState != OrbStates.Idle)
        {
            _face.Type = Shapes.DiscType.Ring;
            StopCoroutine(Rotating());
        }

        _currentFaceState = newState;
    }

    /// <summary>
    /// For now, rotate the face while in loading state. 
    /// </summary>
    /// <returns></returns>
    private IEnumerator Rotating()
    {
        while (_currentFaceState == OrbStates.Loading)
        {
            _face.transform.Rotate(new Vector3(0,0,20f),Space.Self);
            yield return new WaitForEndOfFrame();
        }
    }

}