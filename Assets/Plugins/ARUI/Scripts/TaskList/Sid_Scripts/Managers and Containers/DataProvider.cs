using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum SusbcriberType
{
    AddTask, RemoveTask, UpdateStep, UpdateTask
}

public class DataProvider : Singleton<DataProvider>
{
    private Dictionary<string, TaskList> _currentSelectedTasks = new Dictionary<string, TaskList>();
    public Dictionary<string, TaskList> CurrentSelectedTasks => _currentSelectedTasks;
    public void SetSelectedTasks(List<string> tasks)
    {
        Dictionary<string, TaskList> tmp = new Dictionary<string, TaskList>();
        foreach (string key in tasks)
        {
            if (_manual.Keys.Contains(key))
                tmp.Add(key, _manual[key]);
        }
            
        _currentSelectedTasks = tmp;

        PublishToSubscribers(SusbcriberType.AddTask);
    }

    private Dictionary<string, TaskList> _manual = new Dictionary<string, TaskList>();

    private string _currentTask = "";
    public string CurrentTask => _currentTask;

    private List<UnityEvent> AddTaskSubscribers = new List<UnityEvent>();
    private List<UnityEvent> RemoveTaskSubscribers = new List<UnityEvent>();
    private List<UnityEvent> UpdateStepSubscribers = new List<UnityEvent>();
    private List<UnityEvent> UpdateActiveTaskSubscribers = new List<UnityEvent>();

    /// <summary>
    /// Converts the tasklist object with key recipename into a matrix of strings
    /// The final matrix would be of size (number of steps and substeps x 2)
    /// Each row element would be of the form [step description, 0] for main steps
    /// and [step description, 1] for sub steps
    /// </summary>
    /// <param name="recipename"></param>
    /// <returns></returns>
    public string[,] ConvertToStringMatrix(string recipename)
    {
        var jsonTextFile = Resources.Load<TextAsset>("Text/" + recipename);
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

    /// <summary>
    /// After adding, removing or updating any of the recipe data
    /// call this function to see changes reflected on task overview
    /// </summary>
    private void PublishToSubscribers(SusbcriberType type)
    {
        if (type.Equals(SusbcriberType.AddTask))
        {
            foreach (var subscriber in AddTaskSubscribers)
                subscriber.Invoke();
        } else if (type.Equals(SusbcriberType.RemoveTask))
        {
            foreach (var subscriber in RemoveTaskSubscribers)
                subscriber.Invoke();
        } else if (type.Equals(SusbcriberType.UpdateTask))
        {
            foreach (var subscriber in UpdateActiveTaskSubscribers)
                subscriber.Invoke();
        } else
        {
            foreach (var subscriber in UpdateStepSubscribers)
                subscriber.Invoke();
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="subscriberEvent"></param>
    public void RegisterDataSubscriber(UnityAction subscriberEvent, SusbcriberType type) 
    { 
        UnityEvent newDataUpdateEvent = new UnityEvent();
        newDataUpdateEvent.AddListener(subscriberEvent);

        if (type.Equals(SusbcriberType.AddTask))
            AddTaskSubscribers.Add(newDataUpdateEvent);

        else if (type.Equals(SusbcriberType.RemoveTask))
            RemoveTaskSubscribers.Add(newDataUpdateEvent);

        else if (type.Equals(SusbcriberType.UpdateTask))
            UpdateActiveTaskSubscribers.Add(newDataUpdateEvent);

        else
            UpdateStepSubscribers.Add(newDataUpdateEvent);
    }

    #region Adding and Deleting Tasks

    /// <summary>
    /// //Go to the resources folder and load a new tasklist
    /// json file. The name of the file should be in the form
    /// (recipename).json and we will look in 'Resoureces/Text'
    /// </summary>
    /// <param name="filenamesWithoutExtension"></param>
    public void InitManual(List<string> filenamesWithoutExtension)
    {
        foreach (string filename in filenamesWithoutExtension)
        {
            var jsonTextFile = Resources.Load<TextAsset>("Text/" + filename);
            AngelARUI.Instance.LogDebugMessage("Loaded task from json: " + jsonTextFile.text, true);
            LoadTaskFromString(jsonTextFile.text);
        }
    }

    /// <summary>
    /// Take in a json of the class TaskList and add it as a recipe
    /// </summary>
    /// <param name="json"></param>
    private void LoadTaskFromString(string json)
    {
        TaskList currList = JsonUtility.FromJson<TaskList>(json);
        if (!_manual.ContainsKey(currList.Name))
        {
            _manual.Add(currList.Name, currList);
            //TODO: only add tasks selected by the user
            _currentSelectedTasks.Add(currList.Name, currList);
        }

        if (_currentTask.Equals(""))
            _currentTask = currList.Name;
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void SetAllTasksDone()
    {
        throw new NotImplementedException();
    }



    //Delete recipe with given recipe name
    //If it is the last recipe, then handle task overview and orb
    //public void DeleteRecipe(string recipeName)
    //{
    //    if (_manual.ContainsKey(recipeName))
    //    {
    //        _manual.Remove(recipeName);
    //    }

    //    if (_manual.Count == 0)
    //    {
    //        Orb.Instance.SetTaskMessage("No pending tasks", "", "");
    //        //OVERVIEW REFERENCE
    //        MultiTaskList.Instance.gameObject.SetActive(false);
    //    }

    //    PublishToSubscribers();
    //}

    //Delete the current recipe and replace it with a new current recipe
    //defined by newCurr. If that was the last recipe, then handle
    //task overview and orb
    //public void DeleteCurrRecipe(string newCurr = "")
    //{
    //    Recipes.Remove(CurrRecipe);

    //    if (Recipes.Count == 0)
    //    {
    //        Orb.Instance.SetTaskMessage("No pending tasks", "", "");
    //        //OVERVIEW REFERENCE
    //        MultipleListsContainer.Instance.gameObject.SetActive(false);
    //    } else
    //    {
    //        if (Recipes.ContainsKey(newCurr))
    //        {
    //            SetCurrentActiveTask(newCurr);
    //        }
    //    }
    //}

    #endregion

    #region Set current active task or step

    /// <summary>
    /// Change which recipe shows up as the "current" one
    /// </summary>
    /// <param name="recipeID"></param>
    public void SetCurrentActiveTask(string recipeID)
    {
        if (!_manual.ContainsKey(recipeID)) return;

        _currentTask = recipeID;
        TaskList CurrRecipeObj = _manual[_currentTask];
        int currStepIndex = _manual[_currentTask].CurrStepIndex;
        //Orb.Instance.SetTaskMessage(CurrRecipeObj.Steps[currStepIndex].StepDesc, CurrRecipeObj.Steps[currStepIndex].StepDesc, CurrRecipeObj.Steps[currStepIndex].StepDesc);

        PublishToSubscribers(SusbcriberType.UpdateTask);
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="recipeID"></param>
    /// <param name="taskID"></param>
    public void SetCurrentActiveStep(string recipeID, int taskID)
    {
        if (!_manual.ContainsKey(recipeID)) return;

        if (taskID <= 0)
        {
            _manual[recipeID].PrevStepIndex = -1;
            _manual[recipeID].CurrStepIndex = 0;
            _manual[recipeID].NextStepIndex = 1;

        } else
        {
            _manual[recipeID].PrevStepIndex = taskID-1;
            _manual[recipeID].CurrStepIndex = taskID;
            _manual[recipeID].NextStepIndex = taskID + 1;
        }

        PublishToSubscribers(SusbcriberType.UpdateStep);
    }

    #endregion
}
