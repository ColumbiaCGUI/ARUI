using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    private Shapes.Polyline _connection;
    private float _originalThickness = 0.005f;

    private StorableObject _previewedObj = null;
    public StorableObject PreviewedObject
    {
        get { return _previewedObj; }
        set { _previewedObj = value; }
    }

    private DraggableHandle _draggingHandle = null;

    public void Initialize(int placeInList)
    {
        _placeInList = placeInList;
        _connection = GetComponent<Shapes.Polyline>();
        _connection.enabled = false;

        _draggingHandle = transform.GetChild(0).gameObject.AddComponent<DraggableHandle>();
    }

    public void Update()
    {
        _connection.enabled = false; 
        _connection.Thickness = _originalThickness;

        if (_previewedObj)
        {
            _previewedObj.Droppable = false;
            _previewedObj.SetPreview(false);
        }
        if (_storedItem)
        {
            _storedItem.SetPreview(false);
        }
            
       
        if (_storedItem)
        {
            UpdateLinePositions(_storedItem.transform.position, AngelARUI.Instance.GetAgentTransform().transform.position);
            _connection.enabled = true;

            if (_storedItem.Grabbable.IsDragged)
            {
                // Calculate the distance between the two transforms
                float dist = Vector3.Distance(_storedItem.transform.position, AngelARUI.Instance.GetAgentTransform().transform.position);

                float excessDistance = dist; // Calculate distance beyond 0.3f
                // Map the distance to thickness (linearly decrease thickness)
                _connection.Thickness = Mathf.Min(_originalThickness, _originalThickness - (excessDistance * 0.015f));

                if (_connection.Thickness<0)
                {
                    _storedItem.SetPreview(true);
                    _draggingHandle.SetHandleProgress(1);
                } else
                {
                    _draggingHandle.SetHandleProgress(0);
                }
            }

        } else
        {
            if (_previewedObj==null)
            {
                _draggingHandle.SetHandleProgress(0);
            } else
            {
                _connection.enabled = true;
                Vector3 startpoint = transform.InverseTransformPoint(_previewedObj.transform.position);
                Vector3 endPoint = transform.InverseTransformPoint(AngelARUI.Instance.GetAgentTransform().transform.position);
                UpdateLinePositions(startpoint, endPoint);

                // Calculate the distance between the two transforms
                float dist = Vector3.Distance(_previewedObj.transform.position, AngelARUI.Instance.GetAgentTransform().transform.position);

                if (_previewedObj.Grabbable.IsDragged && dist < 0.1f)
                {
                    _connection.Thickness = _originalThickness * 2;
                    _previewedObj.SetPreview(true);
                    _previewedObj.Droppable = true;
                }
                else
                {
                    _previewedObj.ToOriginalScale();
                }
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="startpoint"></param>
    /// <param name="endPoint"></param>
    private void UpdateLinePositions(Vector3 startpoint, Vector3 endPoint)
    {
        float threshold = 0.01f;
        if ((startpoint - _connection.points[0].point).x > threshold || (startpoint - _connection.points[0].point).y > threshold || (startpoint - _connection.points[0].point).z > threshold)
        {
            startpoint = transform.InverseTransformPoint(startpoint);
            _connection.SetPointPosition(0, startpoint);
        }
        else
            startpoint = _connection.points[0].point;

        if ((endPoint - _connection.points[2].point).x > threshold || (endPoint - _connection.points[2].point).y > threshold || (endPoint - _connection.points[2].point).z > threshold)
        {
            endPoint = transform.InverseTransformPoint(endPoint);
            _connection.SetPointPosition(2, endPoint);
        }
        else
            endPoint = _connection.points[2].point;

        Vector3 halfway = Vector3.Scale(endPoint + startpoint, new Vector3(0.5f, 0.5f, 0.5f));
        _connection.SetPointPosition(1, halfway);
    }

    public void StoreItem(StorableObject item)
    {
        item.CurrentStorage = this;
        _previewedObj = item;

        _storedItem = item;
        _storedItem.ToBoxScale();
        _storedItem.CurrentStorage = this;
        _storedItem.Grabbable.DraggableHandle = _draggingHandle;
        _draggingHandle.gameObject.SetActive(true);
        _draggingHandle.SetHandleProgress(0);

        _connection = GetComponent<Shapes.Polyline>();
        _connection.SetPointColor(0,new Color(0, 0, 0, 0));
        _connection.SetPointColor(1, Color.white);
        _connection.SetPointColor(2, new Color(0, 0, 0, 0));
    }

    /// <summary>
    /// Clears the storage box, removing the stored item and updating IsFull.
    /// </summary>
    public void ClearStorage(Vector3? position = null)
    {
        _previewedObj = null;

        _storedItem.ToOriginalPosScale(position);

        _storedItem.Grabbable.DraggableHandle = null;
        _draggingHandle.gameObject.SetActive(false);

        _storedItem.CurrentStorage = null;
        _storedItem = null;
    }

}
