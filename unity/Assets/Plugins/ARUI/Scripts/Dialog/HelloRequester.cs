using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Example of requester who only sends Hello. Very nice guy.
///     You can copy this class and modify Run() to suits your needs.
///     To use this class, you just instantiate, call Start() when you want to start and Stop() when you want to stop.
/// </summary>
public class HelloRequester : RunAbleThread
{
    private string port = "tcp://localhost:5556";

    public string Text = "";

    private bool _clientCancelled = false;

    /// <summary>
    ///     Request Hello message to server and receive message back. Do it 10 times.
    ///     Stop requesting when Running=false.
    /// </summary>
    /// 
    protected override void Run()
    {
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
        using (SubscriberSocket client = new SubscriberSocket())
        {
            client.Connect(port);
            client.SubscribeToAnyTopic();

            Debug.Log("Connection to server is started - " + port);
            while (!_clientCancelled)
            {
                try
                {
                    if (!client.TryReceiveFrameString(out var message)) continue;

                    // Handle message processing on the background thread
                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        switch (message.Substring(0, 3))
                        {
                            case "400":
                                AngelARUI.Instance.SetAgentThinking(true);
                                break;
                            case "888":
                                AngelARUI.Instance.SetAgentThinking(false);
                                AngelARUI.Instance.DebugLogMessage("Incoming assembly task",true);
                                AngelARUI.Instance.SetManual("Assemble T-Rex", message.Substring(4));
                                AngelARUI.Instance.SetAgentMessageAlignment(MessageAlignment.LockRight);
                                break;
                            case "001":
                                AngelARUI.Instance.SetAgentThinking(false);
                                AngelARUI.Instance.GoToStep("Assemble T-Rex", Int32.Parse(message.Substring(4)));
                                break;
                            case "100":
                                AngelARUI.Instance.SetAgentThinking(false);
                                AngelARUI.Instance.GoToStep("Assemble T-Rex", Int32.Parse(message.Substring(4)));
                                break;
                            case "010":
                                GuidanceMaterialManager.Instance.TaskImage.Tether();
                                GuidanceMaterialManager.Instance.OverviewImage.Tether();
                                AngelARUI.Instance.SetAgentThinking(false);
                                break;
                            case "111":
                                string base64image = message.Substring(4);
                                AngelARUI.Instance.DebugLogMessage("got overview image:" + base64image, true);
                                GuidanceMaterialManager.Instance.UpdateImage(base64image, GuidanceMaterialType.overview);
                                AngelARUI.Instance.SetAgentThinking(false);
                                break;
                            case "---":
                                AngelARUI.Instance.SetAgentThinking(false);
                                break;
                            default:
                                Text = message;
                                Debug.Log("Received " + message);
                                AngelARUI.Instance.SetAgentThinking(false);
                                break;
                        }
                    });
                }
                catch (Exception E)
                {
                    Debug.Log("Connection to server ended unexpectedly: " + E.Message);
                }
            }
            client.Close();
        }
        NetMQConfig.Cleanup();
    }
    public void CloseConnection()
    {
        _clientCancelled = true;
        NetMQConfig.Cleanup();
        Stop();
    }

    void OnDestroy()
    {
        _clientCancelled = true;
        NetMQConfig.Cleanup();
        Stop();
    }
}