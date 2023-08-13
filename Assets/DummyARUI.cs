using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyARUI : MonoBehaviour
{
    Dictionary<string, TaskList> TaskLists = new Dictionary<string, TaskList>();
    string CurrTaskList = "";
    string SecondaryTaskList1 = "";
    string SecondaryTaskList2 = "";
    public SetupTaskOverview setupScript;

    void Start()
    {
        StartCoroutine(ExampleScript());
    }

    public void LoadNewTaskList(string taskname)
    {
        var jsonTextFile = Resources.Load<TextAsset>("Text/" + taskname);
        Debug.Log(jsonTextFile.text);
        TaskList currList = JsonUtility.FromJson<TaskList>(jsonTextFile.text);
        TaskLists.Add(taskname, currList);
    }

    public void ReloadCurrTaskList()
    {
        setupScript.SetupCurrentStep(TaskLists[CurrTaskList], CurrTaskList);
    }

    public void UpdateCurrTaskList(string taskname)
    {
        CurrTaskList = taskname;
    }

    public void UpdateSecondaryTaskListIndex1(string taskname)
    {
        SecondaryTaskList1 = taskname;
    }

    public void UpdateSecondaryTaskListIndex2(string taskname)
    {
        SecondaryTaskList2 = taskname;
    }

    public void RefreshTaskList()
    {
        //Add refreshing code here
    }

    public void UpdateNextStepIndex(int index)
    {
        TaskLists[CurrTaskList].NextStepIndex = index;
    }

    public void GoToNextSubStep()
    {
        TaskList currTaskList = TaskLists[CurrTaskList];
        int currStepIndex = TaskLists[CurrTaskList].CurrStepIndex;
        currTaskList.Steps[currStepIndex].CurrSubStepIndex++;
    }

    public void GoToNextStep()
    {
        TaskLists[CurrTaskList].CurrStepIndex++;
    }

    IEnumerator ExampleScript()
    {
        LoadNewTaskList("Task1");
        UpdateCurrTaskList("Task1");
        //TODO: change to map?
        setupScript.SetupCurrentStep(TaskLists["Task1"], "Task1");
        yield return new WaitForSeconds(20.0f);
        GoToNextSubStep();
        ReloadCurrTaskList();
        yield return new WaitForSeconds(20.0f);
        GoToNextSubStep();
        ReloadCurrTaskList();
    }
}
