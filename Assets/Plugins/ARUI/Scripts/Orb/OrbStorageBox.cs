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

        if (_storedItem != null)
        {
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
            } else
            {
                _connection.Start = transform.InverseTransformPoint(_isPreviewing.transform.position);
                _connection.End = transform.InverseTransformPoint(AngelARUI.Instance.GetAgentTransform().transform.position);
                _connection.enabled = true;

                _connection.Dashed = true;
            }

            _connection.Thickness = _originalThickness;
            _draggingHandle.SetHandleProgress(0);
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

        _connection = GetComponent<Shapes.Line>();
        _connection.ColorStart = Color.white;
        _connection.ColorEnd = new Color(1,1,1,0);
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

    internal void Preview(StorableObject lookingAt)
    {
        _isPreviewing = lookingAt;
    }

    internal void UnPreview()
    {
        _isPreviewing = null;
    }
}
