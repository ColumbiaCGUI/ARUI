using TMPro;
using UnityEngine;

public class HelloClient : MonoBehaviour
{
    private HelloRequester _helloRequester;
    private void Start()
    {
        _helloRequester = new HelloRequester();
        _helloRequester.Start();

        gameObject.AddComponent<UnityMainThreadDispatcher>();   
    }

    private void Update()
    {
        if (_helloRequester.Text.Length > 0)
        {
            AngelARUI.Instance.DebugLogMessage(_helloRequester.Text, true);
            AngelARUI.Instance.PlayMessageAtAgent(_helloRequester.Text);   
            _helloRequester.Text = "";
        }
    }

    private void OnDestroy()
    {
        _helloRequester.CloseConnection();
        _helloRequester.Stop();

    }
}