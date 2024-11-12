using Microsoft.MixedReality.Toolkit.Input;
using System;
using UnityEditor;
using UnityEngine;

public class OrbStorageBox : MonoBehaviour
{
    private StorableObject _storedItem = null;         // Holds the stored GameObject.
    public StorableObject StoredItem
    {
        get => _storedItem;
    }

    public bool IsFull => _storedItem != null;

    private int _placeInList = 0;
    public int PlaceInList => _placeInList;

    private Shapes.Line _connection;

    private float _baseRadius = 0.1f;

    private StorableObject _isPreviewing = null;

    public void Initialize(int placeInList)
    {
        _placeInList = placeInList;
        _connection = GetComponent<Shapes.Line>();
        _connection.enabled = false;
    }

    public void Update()
    {
        if (_storedItem != null)
        {
            _connection.Start = transform.InverseTransformPoint(_storedItem.transform.position);
            _connection.End = transform.InverseTransformPoint(AngelARUI.Instance.GetAgentTransform().transform.position);
            _connection.enabled = true;
        } else
        {
            if (_isPreviewing==null)
            {
                _connection.enabled = false;
            } else
            {
                _connection.Start = transform.InverseTransformPoint(_isPreviewing.transform.position);
                _connection.End = transform.InverseTransformPoint(AngelARUI.Instance.GetAgentTransform().transform.position);
                _connection.enabled = true;
            }
        }
    }

    public void StoreItem(StorableObject item)
    {
        _isPreviewing = item;

        _storedItem = item;
        _storedItem.ScaleToBoxSize(ARUISettings.SizeAtStorage);
        _storedItem.CurrentStorage = this;

        _connection = GetComponent<Shapes.Line>();
        _connection.ColorStart = Color.white;
        _connection.ColorEnd = new Color(1,1,1,0);
    }

    /// <summary>
    /// Clears the storage box, removing the stored item and updating IsFull.
    /// </summary>
    public void ClearStorage()
    {
        _isPreviewing = null;

        _storedItem.ScaleToOriginalSize();

        // Move the stored item 1 meter back relative to the user
        _storedItem.transform.position = _storedItem.transform.position + AngelARUI.Instance.ARCamera.transform.forward;

        _storedItem.CurrentStorage = null;
        _storedItem = null;  
    }

    internal void Preview(StorableObject lookingAt)
    {
        _isPreviewing = lookingAt;
    }

    internal void UnPreview()
    {
        _isPreviewing = null;
    }
}
