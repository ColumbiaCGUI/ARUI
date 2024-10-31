using System;
using System.Collections.Generic;
using UnityEngine;

public enum StorageObjectType
{
    Twin = 0,
    Panel = 1
}

public class OrbStorageManager : MonoBehaviour
{
    private List<OrbStorageBox> _allStorageBoxes; 
    public List<OrbStorageBox> OrbStorage => _allStorageBoxes;

    private bool _isLookingAtStorageArea = false;                       // true if the user is looking at the storage box area or any of the storageboxes.
    public bool IsLookingAtStorageArea => _isLookingAtStorageArea;    


    public void Awake()
    {
        _allStorageBoxes = new List<OrbStorageBox>();
        foreach (var item in gameObject.GetComponentsInChildren<Transform>())
        {
            if (item.gameObject.GetInstanceID()!=gameObject.GetInstanceID())
            {
                _allStorageBoxes.Add(item.gameObject.AddComponent<OrbStorageBox>());
            }
        }

        SetAllVisible(false); // Example to set them inactive during Awake

        EyeGazeManager.Instance.RegisterEyeTargetID(gameObject);
    }

    public void Update()
    {
        foreach (var item in OrbStorage)
        {
            _isLookingAtStorageArea = item.IsLookingAtBox;
        }
        _isLookingAtStorageArea = _isLookingAtStorageArea || EyeGazeManager.Instance.CurrentHitID == gameObject.GetInstanceID();
    }

    /// <summary>
    /// Sets all OrbStorageBoxes visible or not based on the provided boolean.
    /// </summary>
    /// <param name="isVisible">True to set visible, false to set not visible.</param>
    public void SetAllVisible(bool isVisible)
    {
        foreach (var storageBox in _allStorageBoxes)
        {
            storageBox.gameObject.SetActive(isVisible);
        }
    }

    /// <summary>
    /// Stores the provided GameObject in the given OrbStorageBox if it is not full.
    /// A Twin should always be stored in the center box, whereas a Panel can be stored in any available box.
    /// </summary>
    /// <param name="item">The GameObject to store.</param>
    /// <param name="storageType">The storage type will define where it will show up.</param>
    /// <returns>True if the item was stored successfully, false otherwise.</returns>
    public bool StoreItemInBox(GameObject item, StorageObjectType storageType)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot store a null item.");
            return false;
        }

        switch (storageType)
        {
            case StorageObjectType.Twin:
                // Always store Twin in the center box (assuming center box is index 1).
                int centerIndex = 1;
                if (_allStorageBoxes.Count > centerIndex && !_allStorageBoxes[centerIndex].IsFull)
                {
                    _allStorageBoxes[centerIndex].StoredItem = item;
                    return true;
                }
                Debug.LogWarning("Center box is already full. Cannot store Twin.");
                return false;

            case StorageObjectType.Panel:
                // Store Panel in any available box.
                foreach (var box in _allStorageBoxes)
                {
                    if (!box.IsFull)
                    {
                        box.StoredItem = item;
                        return true;
                    }
                }
                Debug.LogWarning("All storage boxes are full. Cannot store Panel.");
                return false;

            default:
                Debug.LogWarning("Invalid storage type provided.");
                return false;
        }
    }
}
