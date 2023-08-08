using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyARUI : MonoBehaviour
{
    List<TaskList> TaskLists = new List<TaskList>();
    int CurrTaskListIndex = 0;
    int SecondaryTaskListIndex1 = 0;
    int SecondaryTaskListIndex2 = 0;
    public SetupTaskOverview setupScript;

    void Start()
    {
        // TaskList newList = new TaskList();
        // Step step1 = new Step();
        // SubStep substep = new SubStep();
        // step1.StepDesc = "This is step 1";
        // step1.RequiredItems = new List<string>();
        // substep.StepDesc = "This is a substep 1";
        // substep.RequiredItems = new List<string>();
        // substep.RequiredItems.Add("Knife");
        // substep.RequiredItems.Add("Susage");
        // step1.SubSteps = new List<SubStep>();
        // step1.SubSteps.Add(substep);
        // newList.Steps = new List<Step>();
        // newList.Steps.Add(step1);
        // string json = JsonUtility.ToJson(newList);
        LoadNewTaskList("Text/Task1");
        Debug.Log(TaskLists[0].Steps[0].StepDesc);
        setupScript.SetupCurrentStep(TaskLists[0]);
    }

    public void LoadNewTaskList(string path)
    {
        var jsonTextFile = Resources.Load<TextAsset>(path);
        Debug.Log(jsonTextFile.text);
        TaskList currList = JsonUtility.FromJson<TaskList>(jsonTextFile.text);
        TaskLists.Add(currList);
    }

    public void UpdateCurrTaskListIndex(int index)
    {
        CurrTaskListIndex = index;
    }

    public void UpdateSecondaryTaskListIndex1(int index)
    {
        SecondaryTaskListIndex1 = index;
    }

    public void UpdateSecondaryTaskListIndex2(int index)
    {
        SecondaryTaskListIndex2 = index;
    }

    public void RefreshTaskList()
    {
        //Add refreshing code here
    }

    public void UpdateNextStepIndex(int index)
    {
        TaskLists[CurrTaskListIndex].NextStepIndex = index;
    }

    public void GoToNextSubStep()
    {
        TaskList currTaskList = TaskLists[CurrTaskListIndex];
        int currStepIndex = TaskLists[CurrTaskListIndex].CurrStepIndex;
        currTaskList.Steps[currStepIndex].CurrSubStepIndex++;
    }

    public void GoToNextStep()
    {
        TaskLists[CurrTaskListIndex].CurrStepIndex++;
    }
}
