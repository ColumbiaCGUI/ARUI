using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections.Generic;
using UnityEngine;

public class OrbStorageManager : Singleton<OrbStorageManager>
{
    private List<OrbStorageBox> _allStorageAnchors;   // Keeps track of all storage anchors

    private Dictionary<int, StorableObject> _registeredObjects = new Dictionary<int, StorableObject>();  // Keeps track of all storable objects

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
            new Vector2(-ARUISettings.SizeAtStorage/2, ARUISettings.SizeAtStorage),
            new Vector2(-ARUISettings.SizeAtStorage,0),
            new Vector2(-ARUISettings.SizeAtStorage/2, -ARUISettings.SizeAtStorage)
        }
    };      // Default Layout for 1,2 or 3 stored objects

    private bool _isMessageRight = true;

    public readonly string LabelMessageTether = "Say '" + ARUISettings.TetherSpeechKeyword + "'";  
    public readonly string LabelMessageUntether = "Say '" + ARUISettings.UnTetherSpeechKeyword + "'";

    /// <summary>
    /// Initializes the storage anchors. There will be as many as the current default layout allows.
    /// Sets their default positions, adds necessary components, and registers the game object
    /// with the EyeGazeManager for interaction.
    /// </summary>
    public void Awake()
    {
        gameObject.name = StringResources.OrbStorageManager_name;
        _allStorageAnchors = new List<OrbStorageBox>();
  
        for (int i = 0; i < _defaultLayout.Count; i++)
        {
            GameObject newBox = Instantiate(Resources.Load(StringResources.OrbStorageBox_path) as GameObject);
            newBox.name = "Storagebox " + i;
            newBox.transform.parent = transform;
            newBox.transform.localPosition = _defaultLayout[transform.childCount - 1][i];

            var box = newBox.AddComponent<OrbStorageBox>();
            _allStorageAnchors.Add(box);
            box.Initialize(i);
        }
    }

    /// <summary>
    /// Updates the status of the storable object being looked at and previews it in the available storage anchor.
    /// If no storable object (stored or not stored) is being looked at, clear all line previews.
    /// </summary>
    public void Update()
    {
        StorableObject lookingAt = null;
        foreach (StorableObject objs in _registeredObjects.Values)
        {
            if (objs.IsLookingAtObj)
            {
                lookingAt = objs;
                break;
            }
        }

        if (lookingAt != null && lookingAt.CurrentStorage==null)
        {
            foreach (var box in _allStorageAnchors)
            {
                if (!box.IsFull)
                {
                    box.Preview(lookingAt);
                    return; // Exit method after previewing the object
                }
            }
        }

        // If no object is being previewed or no storage anchor is available, clear previews
        foreach (var box in _allStorageAnchors)
            box.UnPreview();

        if (_isMessageRight != Orb.Instance.IsMessageRight)
            UpdateIdealLayout();
    }


    #region Registration of Storables

    /// <summary>
    /// Registers a new storable object by adding the necessary component, initializing it,
    /// and storing it in the registered objects dictionary.
    /// </summary>
    /// <param name="ID">The unique identifier for the storable object.</param>
    /// <param name="toRegister">The GameObject to be registered as a storable object.</param>
    public bool RegisterStorableObject(int ID, GameObject toRegister)
    {
        if (toRegister == null)
        {
            AngelARUI.Instance.DebugLogMessage("Attempted to register a nullable as storable. Registration aborted.", true);
            return false;
        }

        if (_registeredObjects.ContainsKey(ID))
        {
            AngelARUI.Instance.DebugLogMessage($"Can not register, {_registeredObjects[ID].gameObject.name} already registered with ID {ID}", true);
            return false;
        }

        // Add the StorableObject component
        var storable = toRegister.AddComponent<StorableObject>();
        storable.Register(ID);
        _registeredObjects.Add(ID,storable);

        AngelARUI.Instance.DebugLogMessage($"Registered {toRegister.name} with ID {ID}", true);
        return true;
    }

    /// <summary>
    /// Deregisters a storable object identified by the given ID.
    /// Ensures the object is removed from storage and properly deregistered before being removed from the registry.
    /// </summary>
    /// <param name="ID">The unique identifier of the storable object to deregister.</param>
    public void DeRegisterStorableObject(int ID)
    {
        // Check if the ID already exists in the dictionary
        if (!_registeredObjects.ContainsKey(ID))
        {
            AngelARUI.Instance.DebugLogMessage($"Attempted to deregister an object with ID {ID} not known to the storage manager.", true);
            return;
        }

        Remove(_registeredObjects[ID]);

        _registeredObjects[ID].Deregister();
        Destroy(_registeredObjects[ID]);
        _registeredObjects.Remove(ID);

        AngelARUI.Instance.DebugLogMessage($"Successfully deregistered tethered object with ID {ID}.", true);
    }

    #endregion

    #region Tethering and Untethering

    /// <summary>
    /// Handles the storing speech command by checking if the user is looking at a storable object.
    /// If a storable object is found, it triggers the store operation; otherwise, logs a debug message.
    /// </summary>

    public void HandleStoreKeyword()
    {
        foreach (int ID in _registeredObjects.Keys)
        {
            if (_registeredObjects[ID].IsLookingAtObj)
            {
                HandleStore(ID);
                return;
            }
        }

        AngelARUI.Instance.DebugLogMessage($"User is not looking at storable object.", true);
    }

    /// <summary>
    /// Handles the "Unstore" speech command by checking if the user is looking at a storable object.
    /// If a storable object is found, it triggers the unstore operation; otherwise, logs a debug message.
    /// </summary>
    public void HandleUnstoreKeyword()
    {
        foreach (int ID in _registeredObjects.Keys)
        {
            if (_registeredObjects[ID].IsLookingAtObj)
            {
                HandleUnstore(ID);
                return;
            }
        }

        AngelARUI.Instance.DebugLogMessage($"User is not looking at storable object.", true);
    }


    /// <summary>
    /// Handles the process of storing an object by its ID.
    /// Validates if the object exists, ensures it's not already stored, and attempts to store it in an available storage.
    /// </summary>
    /// <param name="ID">The unique identifier of the object to be stored.</param>
    /// <returns>
    /// Returns true if the object is successfully stored; otherwise, false.
    /// </returns>
    public bool HandleStore(int ID)
    {
        if (!_registeredObjects.ContainsKey(ID))
        {
            AngelARUI.Instance.DebugLogMessage($"No object with {ID} is registered as storable object", true);
            return false;
        }

        if (_registeredObjects[ID].CurrentStorage != null)
        {
            AngelARUI.Instance.DebugLogMessage($"Object with ID {ID} is already tethered.", true);
            return false;
        }

        var storage = StoreItemInBox(_registeredObjects[ID]);
        if (storage == null)
        {
            AngelARUI.Instance.DebugLogMessage($"Storage is full", true);
            return false;
        }

        AngelARUI.Instance.DebugLogMessage($"Successfully stored {ID}.", true);
        return true;
    }

    /// <summary>
    /// Attempts to unstore an object with the given ID.
    /// Validates if the object is registered and currently tethered, 
    /// removes it from storage if valid, and logs relevant messages on failure.
    /// </summary>
    /// <param name="ID">The identifier of the object to unstore.</param>
    /// <returns>True if the object was successfully unstored; false otherwise.</returns>

    public bool HandleUnstore(int ID, Vector3? position = null)
    {
        if (!_registeredObjects.ContainsKey(ID))
        {
            AngelARUI.Instance.DebugLogMessage($"No object with {ID} is registered as storable object", true);
            return false;
        }

        if (_registeredObjects[ID].CurrentStorage == null)
        {
            AngelARUI.Instance.DebugLogMessage($"Object with ID {ID} is not tethered.", true);
            return false;
        }

        Remove(_registeredObjects[ID], position);

        return true;
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
        foreach (var box in _allStorageAnchors)
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

    /// <summary>
    /// Removes a storable object from its storage anchor, if found.
    /// Clears the storage of the corresponding anchor and updates the ideal layout.
    /// Logs a debug message if the object is not found.
    /// </summary>
    /// <param name="toBeRemoved">The storable object to be removed.</param>
    private void Remove(StorableObject toBeRemoved, Vector3? position = null)
    {
        foreach (var box in _allStorageAnchors)
        {
            if (box.IsFull && box.StoredItem.GetInstanceID() == toBeRemoved.GetInstanceID())
            {
                box.ClearStorage(position);
                UpdateIdealLayout();
                return;
            }
        }
    }

    #endregion

    /// <summary>
    /// Counts the number of storage anchors that are full.
    /// </summary>
    /// <returns>The number of full storage anchors.</returns>
    private int CountFull()
    {
        int count = 0;
        foreach (var box in _allStorageAnchors)
        {
            if (box.IsFull) count++;
        }
        return count;
    }

    /// <summary>
    /// Updates the layout of stored objects based on how many are currently stored.
    /// Adjusts the local position of each full storage anchor according to the default layout.
    /// </summary>
    private void UpdateIdealLayout()
    {
        int count = CountFull();
        int layoutIndex = 0;
        foreach (var box in _allStorageAnchors)
        {
            if (box.IsFull)
            {
                if (Orb.Instance.IsMessageRight)
                {
                    box.transform.localPosition = _defaultLayout[count - 1][layoutIndex];
                } else
                {
                    Vector3 layoutpos = _defaultLayout[count - 1][layoutIndex];
                    box.transform.localPosition = new Vector3(layoutpos.x*-1, layoutpos.y, layoutpos.z);    
                }
                
                layoutIndex++;
            }
        }

        _isMessageRight = Orb.Instance.IsMessageRight;
    }

}
