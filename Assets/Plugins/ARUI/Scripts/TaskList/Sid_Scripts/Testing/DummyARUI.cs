using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyARUI : MonoBehaviour
{
    Dictionary<string, TaskList> TaskLists = new Dictionary<string, TaskList>();
    string CurrTaskList = "";
    public MultipleListsContainer setupScript;
    public GameObject OverviewPrefab;

    void Start()
    {
        InitializeTaskOverview();
        StartCoroutine(ExampleScript());
    }

    public void InitializeTaskOverview()
    {
        GameObject overviewObj = Instantiate(OverviewPrefab);
        setupScript = overviewObj.GetComponent<MultipleListsContainer>();
    }

    //Go to the resources folder and load a new tasklist
    //json file. The name of the file should be in the form
    //(taskname).json
    public void LoadNewTaskList(string taskname)
    {
        var jsonTextFile = Resources.Load<TextAsset>("Text/" + taskname);
        Debug.Log(jsonTextFile.text);
        TaskList currList = JsonUtility.FromJson<TaskList>(jsonTextFile.text);
        TaskLists.Add(taskname, currList);
    }
    //After adding, removing or updating any of the tasks
    //call this function to see changes reflected on task overview
    public void ReloadCurrTaskList()
    {
        setupScript.UpdateAllSteps(TaskLists, CurrTaskList);
    }
    //Change which recipe shows up as the "current" one
    public void UpdateCurrTaskList(string taskname)
    {
        CurrTaskList = taskname;
        setupScript.gameObject.GetComponent<Center_of_Objs>().SnapToCentroid();
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
        setupScript.gameObject.GetComponent<Center_of_Objs>().SnapToCentroid();
    }
    //For the current recipe, have it go to the next step
    public void GoToNextStep()
    {
        TaskLists[CurrTaskList].CurrStepIndex++;
        setupScript.gameObject.GetComponent<Center_of_Objs>().SnapToCentroid();
    }

    IEnumerator ExampleScript()
    {
        LoadNewTaskList("Task1");
        LoadNewTaskList("Task2");
        LoadNewTaskList("Task3");
        UpdateCurrTaskList("Task1");
        ReloadCurrTaskList();
        yield return new WaitForSeconds(10.0f);
/*        TaskLists.Remove("Task2");
        ReloadCurrTaskList();
        yield return new WaitForSeconds(10.0f);
        LoadNewTaskList("Task2");
        ReloadCurrTaskList();
        yield return new WaitForSeconds(10.0f);
        UpdateCurrTaskList("Task2");
        ReloadCurrTaskList();*/
    }
}
