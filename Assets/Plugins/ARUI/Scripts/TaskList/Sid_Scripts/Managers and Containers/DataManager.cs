using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    Dictionary<string, TaskList> TaskLists = new Dictionary<string, TaskList>();
    string CurrTaskList = "";
    public GameObject OverviewPrefab;

    void Start()
    {
        InitializeTaskOverview();
        StartCoroutine(ExampleScript());
    }

    //MOVE TO ARUI??
    public void InitializeTaskOverview()
    {
        GameObject overviewObj = Instantiate(OverviewPrefab);
        Center_of_Objs.Instance.SnapToCentroid();
    }

    //Converts the tasklist object with key taskname into a matrix of strings
    //The final matrix would be of size (number of steps and substeps x 2)
    //Each row element would be of the form [step description, 0] for main steps
    //and [step description, 1] for sub steps
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
        UnityEngine.Debug.Log(jsonTextFile.text);
        LoadNewTaskListFromString(jsonTextFile.text);
    }

    public void LoadNewTaskListFromString(string json)
    {
        TaskList currList = JsonUtility.FromJson<TaskList>(json);
        //If there already is a recipe with the same name, still add it to the main list
        //but add a number to its name (for example the second instance of the "Pinwheels"
        //recipe would be stored as "Pinwheels_2")
        if (!TaskLists.ContainsKey(currList.Name))
        {
            TaskLists.Add(currList.Name, currList);
        }
        else
        {
            for (int i = 2; i <= 5; i++)
            {
                if (!TaskLists.ContainsKey(currList.Name + "_" + i.ToString()))
                {
                    TaskLists.Add(currList.Name + "_" + i.ToString(), currList);
                    break;
                }
            }
        }
    }

    //After adding, removing or updating any of the recipe data
    //call this function to see changes reflected on task overview
    public void ReloadTaskList()
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

    //Change the next step index that the current task is pointing to
    public void UpdateCurrNextStepIndex(int index)
    {
        UpdateNextStepIndex(CurrTaskList, index);
    }

    //Change the next step index that the task with name "taskname" is pointing to
    public void UpdateNextStepIndex(string taskname, int index)
    {
        TaskLists[taskname].NextStepIndex = index;
    }

    //Change the previous step index that the current task is pointing to
    public void UpdateCurrPrevStepIndex(int index)
    {
        UpdatePrevStepIndex(CurrTaskList, index);
    }

    //Change the previous step index that the task with name "taskname" is pointing to
    public void UpdatePrevStepIndex(string taskname, int index)
    {
        TaskLists[taskname].PrevStepIndex = index;
    }

    //For the current recipe, have it go to the next substep
    public void GoToNextSubStepCurrRecipe()
    {
        GoToNextSubStep(CurrTaskList);
    }

    //For any recipe with key recipeName, have it go to the next substep
    public void GoToNextSubStep(string recipeName)
    {
        TaskList currTaskList = TaskLists[recipeName];
        int currStepIndex = TaskLists[recipeName].CurrStepIndex;
        currTaskList.Steps[currStepIndex].CurrSubStepIndex++;
    }

    //For the current recipe, have it go to the next step
    public void GoToNextStepCurrRecipe()
    {
        GoToNextStep(CurrTaskList);
    }

    //For the current recipe, have it go to the next step
    public void GoToPrevtStepCurrRecipe()
    {
        GoToPrevStep(CurrTaskList);
    }

    //For any recipe with key recipeName, have it go to the next step
    public void GoToNextStep(string recipeName)
    {
        if (TaskLists[recipeName].CurrStepIndex >= TaskLists[recipeName].Steps.Count - 1 || TaskLists[recipeName].CurrStepIndex == -1)
        {
            TaskLists[recipeName].CurrStepIndex = -1;
        }
        else if (TaskLists[recipeName].CurrStepIndex == TaskLists[recipeName].Steps.Count - 2)
        {
            TaskLists[recipeName].CurrStepIndex++;
            TaskLists[recipeName].NextStepIndex = -1;
            TaskLists[recipeName].PrevStepIndex++;
        }
        else
        {
            TaskLists[recipeName].CurrStepIndex++;
            TaskLists[recipeName].NextStepIndex++;
            TaskLists[recipeName].PrevStepIndex++;
        }
    }

    //For any recipe with key recipeName, have it go to the next step
    public void GoToPrevStep(string recipeName)
    {
        if (TaskLists[recipeName].CurrStepIndex <= 0 || TaskLists[recipeName].CurrStepIndex == -1)
        {
            TaskLists[recipeName].CurrStepIndex = -1;
        }
        else if (TaskLists[recipeName].CurrStepIndex == 1)
        {
            TaskLists[recipeName].CurrStepIndex--;
            TaskLists[recipeName].PrevStepIndex = -1;
            TaskLists[recipeName].NextStepIndex--;
        }
        else
        {
            TaskLists[recipeName].CurrStepIndex--;
            TaskLists[recipeName].NextStepIndex--;
            TaskLists[recipeName].PrevStepIndex--;
        }
    }

    IEnumerator ExampleScript()
    {
        yield return new WaitForSeconds(0.5f);
        LoadNewTaskList("Task1");
        LoadNewTaskList("Task1");
        LoadNewTaskList("Task1");
        UpdateCurrTaskList("Pinwheels");
        ReloadTaskList();
        yield return new WaitForSeconds(10.0f);
        GoToNextStep("Pinwheels_2");
        ReloadTaskList();
        yield return new WaitForSeconds(10.0f);
        GoToPrevStep("Pinwheels_3");
        ReloadTaskList();
        yield return new WaitForSeconds(10.0f);
        UpdateCurrTaskList("Pinwheels_2");
        ReloadTaskList();
    }
}
