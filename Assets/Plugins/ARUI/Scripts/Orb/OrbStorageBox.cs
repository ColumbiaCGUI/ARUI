using Microsoft.MixedReality.Toolkit.Input;
using System;
using UnityEngine;

public class OrbStorageBox : MonoBehaviour, IMixedRealityPointerHandler
{
    private GameObject _storedItem;         // Holds the stored GameObject.
    public GameObject StoredItem
    {
        get => _storedItem;
        set
        {
            _storedItem = value;
            _isFull = _storedItem != null;  // Update IsFull whenever StoredItem is set.
        }
    }

    private bool _isFull;                   // Stores whether the box is full or not.
    public bool IsFull => _isFull;

    private MeshRenderer _meshRenderer;     // To change the color of the storage box.

    // Define colors for different pointer states.
    private Color defaultColor = Color.white;
    private Color pointerHoverColor = Color.yellow;
    private Color pointerClickedColor = Color.green;

    private bool _isLookingAtBox = false;
    public bool IsLookingAtBox => _isLookingAtBox;


    public void Start()
    {
        // Initialize MeshRenderer and set default color
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer != null)
        {
            _meshRenderer.material.color = defaultColor;
        }

        EyeGazeManager.Instance.RegisterEyeTargetID(this.gameObject);
    }

    public void Update()
    {
        _isLookingAtBox = EyeGazeManager.Instance.CurrentHitID == gameObject.GetInstanceID();
    }

    /// <summary>
    /// Clears the storage box, removing the stored item and updating IsFull.
    /// </summary>
    public void ClearStorage()
    {
        StoredItem = null;  // This will also set _isFull to false.
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        // Change the color to indicate the pointer is pressed.
        if (_meshRenderer != null)
        {
            _meshRenderer.material.color = pointerClickedColor;
        }
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        // Reset the color to the hover color when the pointer is released.
        if (_meshRenderer != null)
        {
            _meshRenderer.material.color = pointerHoverColor;
        }
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        // Change the color back to the default after clicking.
        if (_meshRenderer != null)
        {
            _meshRenderer.material.color = defaultColor;
        }
    }

    internal bool StoreItem(GameObject item)
    {
        throw new NotImplementedException();
    }
}
