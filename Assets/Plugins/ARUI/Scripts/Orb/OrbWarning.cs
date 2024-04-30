using UnityEngine;

public class OrbWarning : MonoBehaviour
{
    private bool _init = false;

    private TMPro.TextMeshProUGUI _textContent;
    public TMPro.TextMeshProUGUI Text {  get { return _textContent; } }

    public bool IsSet => _textContent.text.Length > 0;

    private float _xOffset = 0;
    public float XOffset
    {
        get { return _xOffset; }
    }

    /// <summary>
    /// Ini
    /// </summary>
    /// <param name="message"></param>
    public void Init(string message)
    {
        if (!_init)
        {
            //init notification message group
            _textContent = gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();

            _textContent.text = Utils.SplitTextIntoLines(message, ARUISettings.OrbMessageMaxCharCountPerLine);
            _xOffset = transform.localPosition.x;
            _init = true;
        }
    }

    /// <summary>
    /// Set the notification of the notification
    /// </summary>
    /// <param name="message"></param>
    /// <param name="charPerLine"></param>
    public void SetMessage(string message, int charPerLine) => _textContent.text = Utils.SplitTextIntoLines(message, charPerLine);

}
