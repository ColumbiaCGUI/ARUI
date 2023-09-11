using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyARUI : MonoBehaviour
{
    Dictionary<string, TaskList> TaskLists = new Dictionary<string, TaskList>();
    string CurrTaskList = "";
    string SecondaryTaskList1 = "";
    string SecondaryTaskList2 = "";
    public MultipleListsContainer setupScript;

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
        setupScript.UpdateAllSteps(TaskLists, CurrTaskList);
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
        LoadNewTaskList("Task2");
        LoadNewTaskList("Task3");
        UpdateCurrTaskList("Task1");
        ReloadCurrTaskList();
        yield return new WaitForSeconds(30.0f);
/*        TaskLists.Remove("Task2");
        ReloadCurrTaskList();
        yield return new WaitForSeconds(30.0f);
        LoadNewTaskList("Task2");
        LoadNewTaskList("Task3");
        ReloadCurrTaskList();
        yield return new WaitForSeconds(30.0f);
        UpdateCurrTaskList("Task2");
        ReloadCurrTaskList();*/
    }
}
