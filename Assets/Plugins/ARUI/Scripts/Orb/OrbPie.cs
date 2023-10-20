using System;
using TMPro;
using UnityEngine;

public class OrbPie : MonoBehaviour
{
    private string _taskname;
    public string TaskName {
        get { return _taskname; }
        set { _taskname = value; }
    }

    private bool _isMessageVisible = false;
    public bool IsMessageVisible
    {
        get { return _isMessageVisible; }
    }

    private GameObject _pieSlice;
    private Shapes.Disc _pie;
    private Shapes.Disc _pieProgress;
    private TextMeshProUGUI _progressText;
    
    private float _minRadius = 0.0175f;
    private float _minThick = 0.005f;
    private float _maxRadius = 0.027f;
    private float _maxThick = 0.02f;
    private float _maxRadiusActive = 0.032f;
    private float _maxThickActive = 0.03f;

    private float _rDeg = 0;
    private float _lDeg = 0;

    private FlexibleTextContainer _textContainer;
    public FlexibleTextContainer Text
    {
        get => _textContainer;
    }
    private float _initialmessageYOffset;

    private GameObject _pieText;
    private TextMeshProUGUI _currentStepText;

    private Color _activeColorText = Color.white;

    public void InitializeComponents(float rDeg, float lDeg)
    {
        _rDeg = rDeg;
        _lDeg = lDeg;

        //init pie slice and components
        _pieSlice = transform.GetChild(0).gameObject;
        _pie = _pieSlice.GetComponent<Shapes.Disc>();
        _pieProgress = _pie.transform.GetChild(0).GetComponent<Shapes.Disc>();
        _pieProgress.Radius = 0;
        _pieProgress.Thickness = 0;
        _progressText = _pieProgress.GetComponentInChildren<TextMeshProUGUI>();
        _progressText.gameObject.SetActive(false);

        _textContainer = transform.GetChild(1).GetChild(0).gameObject.AddComponent<FlexibleTextContainer>();
        _initialmessageYOffset = _textContainer.transform.position.x;

        UpdateAnchor(MessageAnchor.right);

        //init pie text
        _pieText = transform.GetChild(1).gameObject;
        var allChildren = _pieText.transform.GetAllDescendents();
        foreach (var child in allChildren)
        {
            if (child.GetComponent<TextMeshProUGUI>() != null)
            {
                _currentStepText = child.GetComponent<TextMeshProUGUI>();
                _currentStepText.text = "";
                break;
            }
        }

        _taskname = gameObject.name;

        SetTaskActive(false);
    }

    public void SetTaskActive(bool active)
    {
        _pieSlice.SetActive(active && _currentStepText.text.Length > 0);
        _pieText.SetActive(active && _currentStepText.text.Length > 0);
    }

    public void SetTaskMessage(string message)
    {
        AngelARUI.Instance.LogDebugMessage("Set step message: '" + message + "' for task: " + gameObject.name, true);
        _currentStepText.text = message;
    }

    //public void SetMessageActive(bool active)
    //{
    //    _pieText.SetActive(active);
    //}

    public void UpdateSlice(string activeTaskID)
    {
        if (_taskname.Equals(activeTaskID))
        {
            _pie.Radius = 0.032f;
            _pie.Thickness = 0.03f;
        }
        else
        {
            _pie.Radius = 0.027f;
            _pie.Thickness = 0.02f;
        }
    }

    public void UpdateAnchor(MessageAnchor anchor)
    {
        _textContainer.UpdateAnchorInstant();

        float deg = _rDeg;
        if (anchor.Equals(MessageAnchor.left))
        {
            deg = _lDeg;
        }

        _pie.AngRadiansEnd = deg * Mathf.Deg2Rad;
        _pie.AngRadiansStart = (deg -21) * Mathf.Deg2Rad;

        _pieProgress.AngRadiansEnd = deg * Mathf.Deg2Rad;
        _pieProgress.AngRadiansStart = (deg -5) * Mathf.Deg2Rad;
    }

    public void UpdateProgressbar(float ratio, string activeTaskID)
    {
        if (ratio == 0)
        {
            _pieProgress.Radius = 0;
            _pieProgress.Thickness = 0;
        }
        else
        {
            if (_taskname.Equals(activeTaskID))
            {
                _pieProgress.Radius = _minRadius + (ratio * (_maxRadiusActive - _minRadius));
                _pieProgress.Thickness = _minThick + (ratio * (_maxThickActive - _minThick));
            }
            else
            {
                _pieProgress.Radius = _minRadius + (ratio * (_maxRadius - _minRadius));
                _pieProgress.Thickness = _minThick + (ratio * (_maxThick - _minThick));
            }
        }
    }

    /// <summary>
    /// Update the color of the text based on visibility
    /// </summary>
    /// <param name="alpha"></param>
    public void SetTextAlpha(float alpha)
    {
        if (alpha == 0)
            _textContainer.TextColor = new Color(0, 0, 0, 0);
        else
            _textContainer.TextColor = new Color(_activeColorText.r, _activeColorText.g, _activeColorText.b, alpha);
    }
}