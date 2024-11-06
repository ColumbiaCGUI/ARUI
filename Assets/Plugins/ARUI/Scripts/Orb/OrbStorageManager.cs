using System;
using System.Collections.Generic;
using UnityEngine;

public enum StorageObjectType
{
    Twin = 0,
    Panel = 1
}

public class OrbStorageManager : Singleton<OrbStorageManager>
{
    private List<OrbStorageBox> _allStorageBoxes; 

    private List<StorableObject> _registeredObjects = new List<StorableObject>();

    public void Awake()
    {
        _allStorageBoxes = new List<OrbStorageBox>();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.GetInstanceID() != gameObject.GetInstanceID())
            {
                var box = transform.GetChild(i).gameObject.AddComponent<OrbStorageBox>();
                _allStorageBoxes.Add(box);
                box.Initialize();
            }
        }

        EyeGazeManager.Instance.RegisterEyeTargetID(gameObject);
    }

    public void Update()
    {
        StorableObject lookingAt = null;
        foreach (StorableObject objs in _registeredObjects)
        {
            if (objs.IsLookingAtObj)
                lookingAt = objs;
        }

        if (lookingAt != null)
        {
            foreach (var box in _allStorageBoxes)
            {
                if (!box.IsFull)
                {
                    box.Preview(lookingAt);
                    break;
                }
            }
        } else
        {
            foreach (var box in _allStorageBoxes)
            {
                box.UnPreview();
            }
        }
    }

    public void RegisterStorableObject(GameObject toRegister)
    {
        // Add the StorableObject component
        var storable = toRegister.AddComponent<StorableObject>();
        storable.Initialize();
        _registeredObjects.Add(storable);
    }


    /// <summary>
    /// Stores the provided GameObject in the given OrbStorageBox if it is not full.
    /// A Twin should always be stored in the center box, whereas a Panel can be stored in any available box.
    /// </summary>
    /// <param name="item">The GameObject to store.</param>
    /// <param name="storageType">The storage type will define where it will show up.</param>
    /// <returns>True if the item was stored successfully, false otherwise.</returns>
    private bool StoreItemInBox(StorableObject item)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot store a null item.");
            return false;
        }

        AngelARUI.Instance.DebugLogMessage("Storing object " + item.name, true);
        // Store Panel in any available box.
        foreach (var box in _allStorageBoxes)
        {
            if (!box.IsFull)
            {
                box.StoreItem(item);
                return true;
            }
        }

        Debug.LogWarning("All storage boxes are full. Cannot store Panel.");
        return false;
    }

    private void Remove(StorableObject toBeRemoved)
    {
        foreach (var box in _allStorageBoxes)
        {
            if (box.IsFull && box.StoredItem.GetInstanceID()== toBeRemoved.GetInstanceID())
            {
                box.ClearStorage();
                return;
            }
        }
    }

    public void HandleStoreKeyword()
    {
        foreach (StorableObject objs in _registeredObjects)
        {
            if (!objs.IsStored && objs.IsLookingAtObj)
            {
                StoreItemInBox(objs);
            }
        }
    }

    public void HandleUnstoreKeyword()
    {
        foreach (StorableObject objs in _registeredObjects)
        {
            if (objs.IsStored && objs.IsLookingAtObj)
            {
                Remove(objs);
            }
        }
    }
}
