using UnityEngine;

public enum ListPosition
{
    Top = 0,
    Bottom = 1,
    Middle =2,
}

public class TaskListElement : MonoBehaviour
{
    public int ID;

    private TMPro.TextMeshProUGUI _textComponent;
    private Shapes.Rectangle _checkBox;
    private Shapes.Cone _checkBoxCurrent;
    private Shapes.Line _subTaskIndicator;

    private int _taskLevel = 0;

    private float _currentAlpha = 1f;

    // Left, Top, Right, Bottom
    private Vector4 _prefabMargin;
    private Vector4 _subTaskMargin = new Vector4(0.01f, 0, 0, 0);

    private string _taskMessage = "";

    /// <summary>
    /// Get all reference from the task list element prefab
    /// </summary>
    private void InitIfNeeded()
    {
        if (_checkBox == null)
        {
            _checkBox = GetComponentInChildren<Shapes.Rectangle>();
            if (_checkBox == null) Debug.Log("Script could not be found: Shapes.Rectangle at " + gameObject.name);
            
            _checkBoxCurrent = GetComponentInChildren<Shapes.Cone>();
            if (_checkBoxCurrent == null) Debug.Log("Script could not be found: Shapes.Cone at " + gameObject.name);

            _subTaskIndicator = transform.GetComponentInChildren<Shapes.Line>();
            if (_subTaskIndicator == null) Debug.Log("Script could not be found: Shapes.Line at " + gameObject.name);

            _textComponent = GetComponent<TMPro.TextMeshProUGUI>();
            if (_textComponent == null) Debug.Log("Script could not be found: TMPro.TextMeshProUGUI at " + gameObject.name);

            _prefabMargin = new Vector4(_textComponent.margin.x, _textComponent.margin.y, _textComponent.margin.z, _textComponent.margin.w);
        }
    }

    /// <summary>
    /// Reset all values
    /// </summary>
    public void Reset(bool visible)
    {
        if (visible)
            _currentAlpha = 1f;
        _textComponent.text = _taskMessage;
        _checkBox.gameObject.SetActive(false);
        _checkBoxCurrent.gameObject.SetActive(false);
        UpdateColor(ARUISettings.TaskFutureColor);
    }

    /// <summary>
    /// Set text, ID and level in task hierarchy (0 for main task, 1 for subtask)
    /// </summary>
    /// <param name="taskID">id in the task list (starts with 0)</param>
    /// <param name="text">text of task message</param>
    /// <param name="taskLevel">0 or 1</param>
    public void InitText(int taskID, string text, int taskLevel)
    {
        InitIfNeeded();

        _textComponent.text = text;
        this._taskLevel = taskLevel;
        _taskMessage = text;
        ID = taskID;
        
        _checkBox.gameObject.SetActive(false);
        _checkBoxCurrent.gameObject.SetActive(false);

        UpdateColor(ARUISettings.TaskFutureColor);

        if (taskLevel == 0)
        {
            _textComponent.margin = _prefabMargin;
            _subTaskIndicator.gameObject.SetActive(false);
        }
        else
        {
            _textComponent.margin = _prefabMargin + _subTaskMargin;
            _subTaskIndicator.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Set the this task as done
    /// </summary>
    /// <param name="isDone"></param>
    public void SetIsDone(bool isDone)
    {
        InitIfNeeded();

        _checkBox.gameObject.SetActive(true);
        _checkBoxCurrent.gameObject.SetActive(false);

        //define color and alpha of element based on user attention and task state
        if (isDone)
        {
            UpdateColor(ARUISettings.TaskDoneColor);
            _checkBox.Type = Shapes.Rectangle.RectangleType.HardSolid;
        }
        else
        {
            UpdateColor(ARUISettings.TaskFutureColor);
            _checkBox.Type = Shapes.Rectangle.RectangleType.HardBorder;
        }
            
        if (_taskLevel == 0)
            _textComponent.text = _taskMessage;

    }

    /// <summary>
    /// Set this task as the one the user has to do
    /// </summary>
    /// <param name="postMessage"></param>
    public void SetAsCurrent(string postMessage)
    {
        InitIfNeeded();

        _checkBox.gameObject.SetActive(false);
        _checkBoxCurrent.gameObject.SetActive(true);

        UpdateColor(ARUISettings.TaskCurrentColor);

        if (_taskLevel==0 && postMessage.Length>0)
            _textComponent.text = _taskMessage + " - " +postMessage;

    }

    /// <summary>
    /// Update the color of the task message and icon depending on it's state
    /// </summary>
    /// <param name="newColor"></param>
    private void UpdateColor(Color newColor)
    {
        _textComponent.color = new Color(newColor.r, newColor.g, newColor.b, _currentAlpha);
        _checkBoxCurrent.Color = new Color(newColor.r, newColor.g, newColor.b, _currentAlpha);
        _checkBox.Color = new Color(newColor.r, newColor.g, newColor.b, _currentAlpha);
        _subTaskIndicator.Color = new Color(newColor.r, newColor.g, newColor.b, _currentAlpha);
    }

    /// <summary>
    ///  Set the alpha value of the task text, used for fading
    /// </summary>
    /// <param name="alpha"></param>
    public void SetAlpha(float alpha)
    {
        _textComponent.color = new Color(_textComponent.color.r, _textComponent.color.g, _textComponent.color.b, alpha);
        _checkBoxCurrent.Color = new Color(_checkBoxCurrent.Color.r, _checkBoxCurrent.Color.g, _checkBoxCurrent.Color.b, alpha);
        _subTaskIndicator.Color = new Color(_subTaskIndicator.Color.r, _subTaskIndicator.Color.g, _subTaskIndicator.Color.b, alpha);
        _checkBox.Color = new Color(_checkBox.Color.r, _checkBox.Color.g, _checkBox.Color.b, alpha);

        _currentAlpha = alpha;
    }


}
