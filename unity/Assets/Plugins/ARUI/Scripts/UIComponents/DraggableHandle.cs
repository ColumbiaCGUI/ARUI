using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableHandle : MonoBehaviour
{
    private Shapes.Triangle _indicator;
    private bool _isActive = false;

    private float _fixingProgress = 0.0f;
    public float Progress => _fixingProgress;

    public void SetHandleProgress(float progress)
    {
        if (_indicator==null)
            _indicator = gameObject.GetComponentInChildren<Shapes.Triangle>();
        _indicator.ColorA = Color.white * progress;
        _indicator.ColorB = Color.white * progress;
        _indicator.ColorC = Color.white * progress;
        _fixingProgress = progress;
    }

    public void SetInvisible(bool v)
    {
        if (_indicator)
            _indicator.enabled = !v;
    }

    public void Start()
    {
        _indicator = gameObject.GetComponentInChildren<Shapes.Triangle>();
        _indicator.gameObject.SetActive(false);
    }

    public void Update()
    {
        _isActive = _fixingProgress > 0.0f;
        _indicator.gameObject.SetActive(_isActive);
    }


}
