using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEditor;
using UnityEngine.Events;

public class ExampleScript : MonoBehaviour
{
    public bool Automate = true;

    private Dictionary<string, int> _currentStepMap;
    private string _currentTask = "";

    public bool multipleTasks = false;

    private void Start()
    {
        if (Automate)
        {
            if (multipleTasks)
            {
                StartCoroutine(RunAutomatedTestsRecipes());
            } else
            {
                StartCoroutine(RunAutomatedTestsMaintenance());
            }
        }
    }

    private IEnumerator RunAutomatedTestsRecipes()
    {
        yield return new WaitForSeconds(1f);

        AngelARUI.Instance.DebugShowEyeGazeTarget(true);
        AngelARUI.Instance.PrintVMDebug = false;

        //test with dummy data
        var taskIDs = new List<string> { "Pinwheels", "Coffee", "Oatmeal", "Quesadilla", "Tea" };
        _currentStepMap = new Dictionary<string, int> {
            { "Pinwheels", 0 }, { "Coffee", 0 },
            { "Oatmeal", 0 }, { "Quesadilla", 0 }, { "Tea", 0 }};
        _currentTask = "Pinwheels";

        var allJsonTasks = new Dictionary<string, string>();
        foreach (string taskID in taskIDs)
        {
            var jsonTextFile = Resources.Load<TextAsset>("Manuals/" + taskID);
            allJsonTasks.Add(taskID, jsonTextFile.text);
        }

        AngelARUI.Instance.SetManual(allJsonTasks);

        AngelARUI.Instance.SetAgentThinking(true);

        yield return new WaitForSeconds(4f);

        AngelARUI.Instance.PlayDialogueAtAgent
            ("What is this in front of me?", "A grinder.");

        yield return new WaitForSeconds(5f);

        AngelARUI.Instance.SetCurrentObservedTask("Tea");
        _currentTask = "Tea";

        yield return new WaitForSeconds(1f);

        _currentStepMap[_currentTask]++;
        AngelARUI.Instance.GoToStep(_currentTask, _currentStepMap[_currentTask]);

        yield return new WaitForSeconds(2f);

        AngelARUI.Instance.SetWarningMessage("You are skipping the this step.");

        _currentStepMap[_currentTask]++;
        AngelARUI.Instance.GoToStep(_currentTask, _currentStepMap[_currentTask]);

        yield return new WaitForSeconds(3f);

        _currentStepMap[_currentTask]++;
        AngelARUI.Instance.GoToStep(_currentTask, _currentStepMap[_currentTask]);

        yield return new WaitForSeconds(3f);

        _currentStepMap[_currentTask]++;
        AngelARUI.Instance.GoToStep(_currentTask, _currentStepMap[_currentTask]);

        yield return new WaitForSeconds(2f);

        _currentStepMap["Pinwheels"]++;
        AngelARUI.Instance.GoToStep("Pinwheels", _currentStepMap["Pinwheels"]);

        yield return new WaitForSeconds(3f);

        _currentStepMap[_currentTask]++;
        AngelARUI.Instance.GoToStep(_currentTask, _currentStepMap[_currentTask]);
        AngelARUI.Instance.RemoveWarningMessage();

        yield return new WaitForSeconds(2f);

        AngelARUI.Instance.SetCurrentObservedTask("Pinwheels");
    }

    #region Maintenance Tests
    private IEnumerator RunAutomatedTestsMaintenance()
    {
        yield return new WaitForSeconds(1f);

        AngelARUI.Instance.DebugShowEyeGazeTarget(true);

        AngelARUI.Instance.PrintVMDebug = false;

        AngelARUI.Instance.RegisterKeyword("Start Procedure", () => { StartCoroutine(SpeechCommandRegistrationTest()); });
        AngelARUI.Instance.RegisterKeyword("Toggle Manual", () => { AngelARUI.Instance.ToggleTaskOverview(); });
        AngelARUI.Instance.RegisterKeyword("Next Step", () => { GoToNextStepConfirmation(); });
        AngelARUI.Instance.RegisterKeyword("Previous Step", () => { GoToPreviousStepConfirmation(); });
        AngelARUI.Instance.RegisterKeyword("Coach", () => { AngelARUI.Instance.CallAgentToUser(); });

        AngelARUI.Instance.RegisterKeyword("Right", () => { StartCoroutine(ShowWarningDelayed()); });

        AngelARUI.Instance.RegisterKeyword("Left", () => { StartCoroutine(ShowTestMultipleChoiceDelayed()); });
        AngelARUI.Instance.RegisterKeyword("Automatic", () => { ShowTestMultipleChoice(); });
        AngelARUI.Instance.RegisterKeyword("toggle debug", () => { AngelARUI.Instance.SetLoggerVisible(!Logger.Instance.IsVisible); });

        AngelARUI.Instance.RegisterKeyword("Hello", () => { AngelARUI.Instance.PlayMessageAtAgent("How can I help you?"); });

        // AngelARUI.Instance.RegisterDetectedObject(transform.GetChild(0).gameObject, "test");
        // AngelARUI.Instance.RegisterDetectedObject(transform.GetChild(1).gameObject, "test1");
        //AngelARUI.Instance.RegisterDetectedObject(transform.GetChild(2).gameObject, "test2");
    }

    private IEnumerator ShowWarningDelayed()
    {
        yield return new WaitForSeconds(2f);

        AngelARUI.Instance.SetWarningMessage("I detected that you are not wearing gloves, this is recommended");

        yield return new WaitForSeconds(5f);

        AngelARUI.Instance.RemoveWarningMessage();
    }

    private IEnumerator ShowTestMultipleChoiceDelayed()
    {
        Debug.Log("here1");
        yield return new WaitForSeconds(2f);
        Debug.Log("here2");

        AngelARUI.Instance.TryGetUserMultipleChoice("We noticed you are taking a long time with the current step. Please select to see more information.",
    new List<string> { "I am fine.", "Video please.", "Show the manual." },
    new List<UnityAction>()
    {
                    () => { Debug.Log("First selecte"); },
                    () => { Debug.Log("Second selecte"); },
                    () => { Debug.Log("Third selecte"); },
    }, null, 20);

    }

    private void GoToNextStepConfirmation()
    {
        if (_currentStepMap == null)
        {
            AngelARUI.Instance.PlayMessageAtAgent("No manual is set yet.");
            return;
        }

        AngelARUI.Instance.TryGetUserConfirmation(
            "Please confirm if you are 100% confident that you want to go to the next step in the current task. We really need your confirmation.", 
            () => DialogueTestConfirmed(true), () => DialogueTestFailed());
    }

    private void GoToPreviousStepConfirmation()
    {
        if (_currentStepMap == null)
        {
            AngelARUI.Instance.PlayMessageAtAgent("No manual is set yet.");
            return;
        }

        AngelARUI.Instance.TryGetUserYesNoChoice(
            "Please confirm if you are 100% confident that you want to go to the next step in the current task. We really need your confirmation.", 
            () => DialogueTestConfirmed(false), () => DialogueTestFailed(), () => DialogueTestFailed());
    }

    private void DialogueTestConfirmed(bool forward)
    {
        if (_currentStepMap == null)
        {
            AngelARUI.Instance.PlayMessageAtAgent("No manual is set yet.");
            return;
        }

        if (forward)
        {
            _currentStepMap[_currentTask]++;
            AngelARUI.Instance.GoToStep(_currentTask, _currentStepMap[_currentTask]);
        } else
        {
            _currentStepMap[_currentTask]--;
            AngelARUI.Instance.GoToStep(_currentTask, _currentStepMap[_currentTask]);
        }
    }

    private void DialogueTestFailed() => AngelARUI.Instance.PlayMessageAtAgent("okay, i wont");

    private void ShowTestMultipleChoice()
    {
        AngelARUI.Instance.TryGetUserMultipleChoice("Please select your preferred instruction alignment:",
            new List<string> { "Right", "Left", "Automatic", "Automatic", "Automatic" },
            new List<UnityAction>()
            {
                    () => AngelARUI.Instance.SetAgentMessageAlignment(MessageAlignment.LockRight),
                    () => AngelARUI.Instance.SetAgentMessageAlignment(MessageAlignment.LockLeft),
                    () => AngelARUI.Instance.SetAgentMessageAlignment(MessageAlignment.Auto),
                    () => AngelARUI.Instance.SetAgentMessageAlignment(MessageAlignment.Auto),
                    () => AngelARUI.Instance.SetAgentMessageAlignment(MessageAlignment.Auto)
            }, null, 30);

    }

    private IEnumerator SpeechCommandRegistrationTest()
    {
        AngelARUI.Instance.DebugLogMessage("The keyword was triggered!", true);
        AngelARUI.Instance.SetAgentThinking(true);

        yield return new WaitForSeconds(2);

        AngelARUI.Instance.SetAgentThinking(false);

        //test with dummy data
        var taskIDs = new List<string> { "Filter Inspection" };
        _currentStepMap = new Dictionary<string, int> {
            { "Filter Inspection", 0 }};
        _currentTask = "Filter Inspection";

        var allJsonTasks = new Dictionary<string, string>();
        foreach (string taskID in taskIDs)
        {
            var jsonTextFile = Resources.Load<TextAsset>("Manuals/" + taskID);
            allJsonTasks.Add(taskID, jsonTextFile.text);
        }

        AngelARUI.Instance.SetManual(allJsonTasks);

        yield return new WaitForSeconds(2);

        AngelARUI.Instance.GoToStep("Filter Inspection", 1);

        yield return new WaitForSeconds(2);

        AngelARUI.Instance.ClearManual();

        yield return new WaitForSeconds(1);

    }
    #endregion

#if UNITY_EDITOR

    /// <summary>
    /// Listen to Keyevents for debugging(only in the Editor)
    /// </summary>
    public void Update()
    {
        //CheckForRecipeChange();

        if (Input.GetKeyUp(KeyCode.O))
        {
            var taskIDs = new List<string>();
            string key = "Recoil Starter Removal";
            if (multipleTasks)
            {
                //test with dummy data
                taskIDs = new List<string> { "Pinwheels", "Coffee", "Oatmeal", "Quesadilla", "Tea" };
                _currentStepMap = new Dictionary<string, int> {
            { "Pinwheels", 0 }, { "Coffee", 0 },
            { "Oatmeal", 0 }, { "Quesadilla", 0 }, { "Tea", 0 }};
                _currentTask = "Pinwheels";
            } else
            {
                if (new System.Random().Next(int.MaxValue) % 2==0)
                {
                    key = "Filter Inspection";
                }
                taskIDs = new List<string> { key };
                _currentStepMap = new Dictionary<string, int> {{ key, 0 }};
                _currentTask = key;
            }

            var allJsonTasks = new Dictionary<string, string>();
            foreach (string taskID in taskIDs)
            {
                var jsonTextFile = Resources.Load("Manuals/" + taskID) as TextAsset;
                allJsonTasks.Add(taskID, jsonTextFile.text);
            }

            AngelARUI.Instance.SetManual(allJsonTasks);
            AngelARUI.Instance.GoToStep(key, 0);
        }

        // Example how to step forward/backward in tasklist. 
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            _currentStepMap[_currentTask]++;
            AngelARUI.Instance.GoToStep(_currentTask, _currentStepMap[_currentTask]);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            _currentStepMap[_currentTask]--;
            AngelARUI.Instance.GoToStep(_currentTask, _currentStepMap[_currentTask]);
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            AngelARUI.Instance.SetViewManagement(!AngelARUI.Instance.IsVMActiv);
        }

        if (Input.GetKeyUp(KeyCode.M))
        {
            AngelARUI.Instance.PlayMessageAtAgent("Hello", 10);
        }

        if (Input.GetKeyUp(KeyCode.Delete))
        {
            AngelARUI.Instance.ClearManual();
        }

        if (Input.GetKeyUp(KeyCode.N))
        {
            ShowTestMultipleChoice();
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            AngelARUI.Instance.TryGetUserYesNoChoice("Are you done with the previous step?",
                null, () => { GoToPreviousStepConfirmation(); }, null, 30);
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            AngelARUI.Instance.DebugShowEyeGazeTarget(false);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            AngelARUI.Instance.PlaySoundAt(new Vector3(5, 5, 5), "test");
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            AngelARUI.Instance.PrintVMDebug = !AngelARUI.Instance.PrintVMDebug;
            AngelARUI.Instance.SetLoggerVisible(!Logger.Instance.IsVisible);
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            AngelARUI.Instance.PlayMessageAtAgent("This is a test");
        }

        if (Input.GetKeyUp(KeyCode.Alpha9))
        {
            AngelARUI.Instance.SetWarningMessage("You skipped the last step.");
        }
        if (Input.GetKeyUp(KeyCode.Alpha0))
        {
            AngelARUI.Instance.RemoveWarningMessage();
        }

        TestTethering();
    }

    private void TestTethering()
    {
        int tetherMode = -1;
        if (Input.GetKey(KeyCode.K))
        {
            tetherMode = 0;
        }
        if (Input.GetKey(KeyCode.L))
        {
            tetherMode = 1;
        }

        int id = -1;
        // Example how to use the NLI confirmation dialogue
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            id = 0;
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            id = 1;
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            id = 2;
        }

        if (id>=0)
        {
            if (tetherMode==-1)
            {
                if (!AngelARUI.Instance.RegisterTetheredObject(transform.GetChild(id).gameObject.GetInstanceID(), transform.GetChild(id).gameObject))
                {
                    AngelARUI.Instance.DeRegisterTetheredObject(transform.GetChild(id).gameObject.GetInstanceID());
                }
            } else if (tetherMode== 0) 
            {
                AngelARUI.Instance.Tether(transform.GetChild(id).gameObject.GetInstanceID());
            } else if (tetherMode == 1)
            {
                AngelARUI.Instance.Untether(transform.GetChild(id).gameObject.GetInstanceID());
            }
        }
    }


    private void CheckForRecipeChange()
    {
        if (!multipleTasks) { return; }

        // Example how to use the NLI confirmation dialogue
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            AngelARUI.Instance.SetCurrentObservedTask("Pinwheels");
            _currentTask = "Pinwheels";
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            AngelARUI.Instance.SetCurrentObservedTask("Coffee");
            _currentTask = "Coffee";
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            AngelARUI.Instance.SetCurrentObservedTask("Oatmeal");
            _currentTask = "Oatmeal";
        }

        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            AngelARUI.Instance.SetCurrentObservedTask("Tea");
            _currentTask = "Tea";
        }

        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            AngelARUI.Instance.SetCurrentObservedTask("Quesadilla");
            _currentTask = "Quesadilla";
        }
    }

#endif
}
