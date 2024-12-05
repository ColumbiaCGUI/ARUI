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
    private float _originalThickness = 0.005f;

    private float _baseRadius = 0.1f;

    private StorableObject _isPreviewing = null;

    private DraggableHandle _draggingHandle = null;

    public void Initialize(int placeInList)
    {
        _placeInList = placeInList;
        _connection = GetComponent<Shapes.Line>();
        _connection.enabled = false;

        _draggingHandle = transform.GetChild(0).gameObject.AddComponent<DraggableHandle>();
    }

    public void Update()
    {
        _connection.Dashed = false;
        _connection.enabled = false;

        if (_storedItem != null)
        {
            _storedItem.Droppable = false;
            _connection.Start = transform.InverseTransformPoint(_storedItem.transform.position);
            _connection.End = transform.InverseTransformPoint(AngelARUI.Instance.GetAgentTransform().transform.position);
            _connection.enabled = true;

            if (_storedItem.Grabbable.IsDragged)
            {
                // Calculate the distance between the two transforms
                float dist = Vector3.Distance(_storedItem.transform.position, AngelARUI.Instance.GetAgentTransform().transform.position);

                float excessDistance = dist - 0.1f; // Calculate distance beyond 0.3f
                // Map the distance to thickness (linearly decrease thickness)
                _connection.Thickness = Mathf.Min(_originalThickness, _originalThickness - (excessDistance * 0.015f));

                if (_connection.Thickness<0)
                {
                    _draggingHandle.SetHandleProgress(1);
                } else
                {
                    _draggingHandle.SetHandleProgress(0);
                }
            }

        } else
        {
            if (_isPreviewing==null)
            {
                _connection.enabled = false;
                _connection.Thickness = _originalThickness;
                _draggingHandle.SetHandleProgress(0);

            } else
            {
                _connection.enabled = true;
                _connection.Start = transform.InverseTransformPoint(_isPreviewing.transform.position);
                _connection.End = transform.InverseTransformPoint(AngelARUI.Instance.GetAgentTransform().transform.position);

                // Calculate the distance between the two transforms
                float dist = Vector3.Distance(_isPreviewing.transform.position, AngelARUI.Instance.GetAgentTransform().transform.position);

                if (_isPreviewing.Grabbable.IsDragged && dist < 0.3f)
                {
                    _isPreviewing.Droppable = true;
                    _connection.Thickness = _originalThickness * 2;
                    _connection.Dashed = false;
                }
                else
                {
                    _isPreviewing.Droppable = false;
                    _connection.Thickness = _originalThickness;
                    _connection.Dashed = true;
                }
            }
        }
    }

    public void StoreItem(StorableObject item)
    {
        item.CurrentStorage = this;
        _isPreviewing = item;

        _storedItem = item;
        _storedItem.ScaleToBoxSize(ARUISettings.SizeAtStorage);
        _storedItem.CurrentStorage = this;
        _storedItem.Grabbable.DraggableHandle = _draggingHandle;
        _draggingHandle.gameObject.SetActive(true);
        _draggingHandle.SetHandleProgress(0);

        _connection = GetComponent<Shapes.Line>();
        _connection.ColorStart = Color.white;
        _connection.ColorEnd = new Color(0,0,0,0);
    }

    /// <summary>
    /// Clears the storage box, removing the stored item and updating IsFull.
    /// </summary>
    public void ClearStorage(Vector3? position = null)
    {
        _isPreviewing = null;

        _storedItem.ToOriginalPosScale(position);

        _storedItem.Grabbable.DraggableHandle = null;
        _draggingHandle.gameObject.SetActive(false);

        _storedItem.CurrentStorage = null;
        _storedItem = null;  
    }

    public void Preview(StorableObject lookingAt)
    {
        _isPreviewing = lookingAt;
    }

    public void UnPreview()
    {
        _isPreviewing = null;
    }
}
