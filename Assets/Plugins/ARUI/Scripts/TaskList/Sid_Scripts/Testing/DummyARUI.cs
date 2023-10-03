using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DummyARUI : MonoBehaviour
{
    Dictionary<string, TaskList> TaskLists = new Dictionary<string, TaskList>();
    string CurrTaskList = "";
    public GameObject OverviewPrefab;

    void Start()
    {
        InitializeTaskOverview();
        StartCoroutine(ExampleScript());
    }

    public void InitializeTaskOverview()
    {
        GameObject overviewObj = Instantiate(OverviewPrefab);
        Center_of_Objs.Instance.SnapToCentroid();
    }

    public string[,] ConvertToStringList(string taskname)
    {
        var jsonTextFile = Resources.Load<TextAsset>("Text/" + taskname);
        TaskList currList = JsonUtility.FromJson<TaskList>(jsonTextFile.text);
        int currLength = currList.Steps.Count;
        foreach (var step in currList.Steps)
        {
            currLength += step.SubSteps.Count;
        }
        string[,] finalTaskList = new string[currLength, 2];
        int currRowIndex = 0;
        foreach (var step in currList.Steps)
        {
            finalTaskList[currRowIndex, 0] = "0";
            finalTaskList[currRowIndex, 1] = step.StepDesc;
            currRowIndex++;
            foreach (var substep in step.SubSteps)
            {
                finalTaskList[currRowIndex, 0] = "1";
                finalTaskList[currRowIndex, 1] = substep.StepDesc;
                currRowIndex++;
            }
        }
        return finalTaskList;
    }

    //Go to the resources folder and load a new tasklist
    //json file. The name of the file should be in the form
    //(taskname).json
    public void LoadNewTaskList(string taskname)
    {
        var jsonTextFile = Resources.Load<TextAsset>("Text/" + taskname);
        Debug.Log(jsonTextFile.text);
        LoadNewTaskListFromString(jsonTextFile.text);
    }

    public void LoadNewTaskListFromString(string json)
    {
        TaskList currList = JsonUtility.FromJson<TaskList>(json);
        if (!TaskLists.ContainsKey(currList.Name))
        {
            TaskLists.Add(currList.Name, currList);
        } else
        {
            for(int i = 2; i <=5; i++)
            {
                if (!TaskLists.ContainsKey(currList.Name + "_" + i.ToString()))
                {
                    TaskLists.Add(currList.Name + "_" + i.ToString(), currList);
                    break;
                }
            }
        }
    }

    //After adding, removing or updating any of the tasks
    //call this function to see changes reflected on task overview
    public void ReloadCurrTaskList()
    {
        MultipleListsContainer.Instance.UpdateAllSteps(TaskLists, CurrTaskList);
    }

    //Change which recipe shows up as the "current" one
    public void UpdateCurrTaskList(string taskname)
    {
        CurrTaskList = taskname;
        TaskList currTaskList = TaskLists[CurrTaskList];
        int currStepIndex = TaskLists[CurrTaskList].CurrStepIndex;
        Orb.Instance.SetTaskMessage(currTaskList.Steps[currStepIndex].StepDesc);
    }

    public void UpdateNextStepIndex(int index)
    {
        TaskLists[CurrTaskList].NextStepIndex = index;
    }

    //For the current recipe, have it go to the next substep
    public void GoToNextSubStep()
    {
        TaskList currTaskList = TaskLists[CurrTaskList];
        int currStepIndex = TaskLists[CurrTaskList].CurrStepIndex;
        currTaskList.Steps[currStepIndex].CurrSubStepIndex++;
    }
    //For the current recipe, have it go to the next step
    public void GoToNextStep()
    {
        TaskLists[CurrTaskList].CurrStepIndex++;
    }

    IEnumerator ExampleScript()
    {
        yield return new WaitForSeconds(0.5f);
        LoadNewTaskList("Task1");
        LoadNewTaskList("Task2");
        LoadNewTaskList("Task3");
        UpdateCurrTaskList("Pinwheels");
        ReloadCurrTaskList();
        yield return new WaitForSeconds(10.0f);
        TaskLists.Remove("Mug Cake");
        ReloadCurrTaskList();
        yield return new WaitForSeconds(10.0f);
        LoadNewTaskList("Task2");
        ReloadCurrTaskList();
        yield return new WaitForSeconds(10.0f);
        UpdateCurrTaskList("Mug Cake");
        ReloadCurrTaskList();
    }
}
