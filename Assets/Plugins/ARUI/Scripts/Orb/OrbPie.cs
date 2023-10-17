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

    private GameObject _pieSlice;
    private Shapes.Disc _pie;
    private Shapes.Disc _pieProgress;
    private TextMeshProUGUI _progressText;
    
    private GameObject _pieText;
    private TextMeshProUGUI _currentStepText;

    private float _minRadius = 0.0175f;
    private float _minThick = 0.005f;
    private float _maxRadius = 0.027f;
    private float _maxThick = 0.02f;
    private float _maxRadiusActive = 0.032f;
    private float _maxThickActive = 0.03f;

    private float _startDegRight = 23;
    private float _startDegLeft = 180;

    public void Start()
    {
        //init pie slice and components
        _pieSlice = transform.GetChild(0).gameObject;
        _pie = _pieSlice.GetComponent<Shapes.Disc>();
        _pieProgress = _pie.transform.GetChild(0).GetComponent<Shapes.Disc>();
        _pieProgress.Radius = 0;
        _pieProgress.Thickness = 0;

        float deg = _startDegRight;
        _pie.AngRadiansEnd = (deg) * Mathf.Deg2Rad;
        _pie.AngRadiansStart = (deg - 21) * Mathf.Deg2Rad;
        _pieProgress.AngRadiansEnd = (deg) * Mathf.Deg2Rad;
        _pieProgress.AngRadiansStart = (deg - 5) * Mathf.Deg2Rad;
        deg += -23;

        _progressText = _pieProgress.GetComponentInChildren<TextMeshProUGUI>();
        _progressText.gameObject.SetActive(false);

        //init pie text
        _pieText = transform.GetChild(1).gameObject;
        _currentStepText = _pieText.GetComponentInChildren<TextMeshProUGUI>();
        _currentStepText.text = "";
        _pieText.SetActive(false);
    }

    public void SetActive(bool active)
    {
        _pieSlice.SetActive(active);
        _pieText.SetActive(active);
    }

    public void SetTaskMessage(string message)
    {
        _currentStepText.text = message;
    }

    public void SetMessageActive(bool active)
    {
        _pieText.SetActive(active);
    }

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
}