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

    private Dictionary<int, StorableObject> _registeredObjects = new Dictionary<int, StorableObject>();

    private List<Vector2[]> _defaultLayout = new List<Vector2[]>()
    {
        new Vector2[] { new Vector2(-ARUISettings.SizeAtStorage,0) },
        new Vector2[]
        {
            new Vector2(-ARUISettings.SizeAtStorage/2, ARUISettings.SizeAtStorage/2),
            new Vector2(-ARUISettings.SizeAtStorage/2, -ARUISettings.SizeAtStorage/2)
        },
        new Vector2[]
        {
            new Vector2(-ARUISettings.SizeAtStorage/2, ARUISettings.SizeAtStorage/2), 
            new Vector2(-ARUISettings.SizeAtStorage/2, -ARUISettings.SizeAtStorage/2),
            new Vector2(-ARUISettings.SizeAtStorage/2, -ARUISettings.SizeAtStorage/2)
        }
    };

    public void Awake()
    {
        _allStorageBoxes = new List<OrbStorageBox>();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.GetInstanceID() != gameObject.GetInstanceID())
            {
                var boxObj = transform.GetChild(i).gameObject;
                boxObj.transform.position = _defaultLayout[transform.childCount - 1][i];
                var box = boxObj.AddComponent<OrbStorageBox>();
                _allStorageBoxes.Add(box);
                box.Initialize(i);
            }
        }

        EyeGazeManager.Instance.RegisterEyeTargetID(gameObject);
    }

    public void Update()
    {
        StorableObject lookingAt = null;
        foreach (StorableObject objs in _registeredObjects.Values)
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

    /// <summary>
    /// Register an object with the storage manager. This is generating a sphere collider with diameter of the largest 
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="toRegister"></param>
    public void RegisterStorableObject(int ID, GameObject toRegister)
    {
        // Add the StorableObject component
        var storable = toRegister.AddComponent<StorableObject>();
        storable.Initialize();
        _registeredObjects.Add(ID,storable);
    }

    public void DeRegisterStorableObject(int iD)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Stores the provided GameObject in the given OrbStorageBox if it is not full.
    /// A Twin should always be stored in the center box, whereas a Panel can be stored in any available box.
    /// </summary>
    /// <param name="item">The GameObject to store.</param>
    /// <param name="storageType">The storage type will define where it will show up.</param>
    /// <returns>True if the item was stored successfully, false otherwise.</returns>
    private OrbStorageBox StoreItemInBox(StorableObject item)
    {
        if (item == null) return null;

        // Store Panel in any available box.
        foreach (var box in _allStorageBoxes)
        {
            if (!box.IsFull)
            {
                AngelARUI.Instance.DebugLogMessage("Storing object " + item.name, true);
                box.StoreItem(item);
                UpdateIdealLayout();
                return box;
            }
        }

        AngelARUI.Instance.DebugLogMessage("Can't store item. All storage spots are full at orb.", true);
        return null;
    }

 
    private void Remove(StorableObject toBeRemoved)
    {
        foreach (var box in _allStorageBoxes)
        {
            if (box.IsFull && box.StoredItem.GetInstanceID()== toBeRemoved.GetInstanceID())
            {
                box.ClearStorage();
                UpdateIdealLayout();
                return;
            }
        }
    }

    private void UpdateIdealLayout()
    {
        int count = CountFull();
        int layoutIndex = 0;
        foreach (var box in _allStorageBoxes)
        {
            if (box.IsFull)
            {
                box.transform.localPosition = _defaultLayout[count - 1][layoutIndex];
                layoutIndex++;
            }
        }
    }

    private int CountFull()
    {
        int count = 0;
        foreach (var box in _allStorageBoxes)
        {
            if (box.IsFull) count++;
        }
        return count;
    }


    public void HandleStoreKeyword()
    {
        foreach (StorableObject objs in _registeredObjects.Values)
        {
            if (objs.CurrentStorage==null && objs.IsLookingAtObj)
            {
                objs.CurrentStorage = StoreItemInBox(objs);
                break;
            }
        }
    }

    public void HandleUnstoreKeyword()
    {
        foreach (StorableObject objs in _registeredObjects.Values)
        {
            if (objs.CurrentStorage!=null && objs.IsLookingAtObj)
            {
                Remove(objs);
                objs.CurrentStorage = null;
                break;
            } 
        }
    }


}
