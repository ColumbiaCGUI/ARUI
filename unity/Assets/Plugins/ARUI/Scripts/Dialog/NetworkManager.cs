using TMPro;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private PullClient _pullclient;
    private void Start()
    {
        _pullclient = new PullClient();
        _pullclient.Start();

        gameObject.AddComponent<UnityMainThreadDispatcher>();   
    }

    private void Update()
    {
        if (_pullclient.Text.Length > 0)
        {
            AngelARUI.Instance.DebugLogMessage(_pullclient.Text, true);
            AngelARUI.Instance.PlayMessageAtAgent(_pullclient.Text);   
            _pullclient.Text = "";
        }
    }

    private void OnDestroy()
    {
        _pullclient.CloseConnection();
        _pullclient.Stop();

    }
}