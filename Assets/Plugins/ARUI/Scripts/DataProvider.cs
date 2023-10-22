using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum SusbcriberType
{
    UpdateTask, UpdateStep, UpdateActiveTask
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

        PublishToSubscribers(SusbcriberType.UpdateTask);
    }

    private Dictionary<string, TaskList> _manual = new Dictionary<string, TaskList>();

    private string _currentTask = "";
    public string CurrentTask => _currentTask;

    private List<UnityEvent> UpdateTasksSubscribers = new List<UnityEvent>(); /// <Events are triggered if task list changed (add or removal)
    private List<UnityEvent> UpdateStepSubscribers = new List<UnityEvent>();  /// <Events are triggered if step changed at any task list
    private List<UnityEvent> UpdateActiveTaskSubscribers = new List<UnityEvent>();

    /// <summary>
    /// Converts the tasklist object with key taskname into a matrix of strings
    /// The final matrix would be of size (number of steps and substeps x 2)
    /// Each row element would be of the form [step description, 0] for main steps
    /// and [step description, 1] for sub steps
    /// </summary>
    /// <param name="taskname"></param>
    /// <returns></returns>
    public string[,] ConvertToStringMatrix(string taskname)
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

    /// <summary>
    /// After adding, removing or updating any of the recipe data
    /// call this function to see changes reflected on task overview
    /// </summary>
    private void PublishToSubscribers(SusbcriberType type)
    {
        if (type.Equals(SusbcriberType.UpdateTask))
        {
            foreach (var subscriber in UpdateTasksSubscribers)
            {
                subscriber.Invoke();
            }
        } else if (type.Equals(SusbcriberType.UpdateActiveTask))
        {
            foreach (var subscriber in UpdateActiveTaskSubscribers)
            {
                subscriber.Invoke();
            }
        } else
        {
            foreach (var subscriber in UpdateStepSubscribers)
            {
                subscriber.Invoke();
            }
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

        if (type.Equals(SusbcriberType.UpdateTask))
            UpdateTasksSubscribers.Add(newDataUpdateEvent);

        else if (type.Equals(SusbcriberType.UpdateActiveTask))
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
    /// Take in a json of the class TaskList and add it as a task
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
    /// TODO: Delete task from the currently Selected task (not the manual)
    /// </summary>
    /// <param name="taskname"></param>
    public void DeleteRecipe(string taskname)
    {

    }

    #endregion

    #region Set current active task or step

    /// <summary>
    /// Change which taskID shows up as the "current" one
    /// </summary>
    /// <param name="taskID"></param>
    public void SetCurrentActiveTask(string taskID)
    {
        if (!_manual.ContainsKey(taskID)) return;
        _currentTask = taskID;

        PublishToSubscribers(SusbcriberType.UpdateActiveTask);
    }

    /// <summary>
    /// Sets the active step of a given task id.
    /// If step index is out of bounds, adjust to first or last step
    /// </summary>
    /// <param name="taskID"></param>
    /// <param name="stepIndex"></param>
    public void SetCurrentActiveStep(string taskID, int stepIndex)
    {
        if (!_manual.ContainsKey(taskID)) return;

        if (stepIndex <= 0)
        {
            _manual[taskID].PrevStepIndex = -1;
            _manual[taskID].CurrStepIndex = 0;
            _manual[taskID].NextStepIndex = 1;

        } else if (stepIndex >= _manual[taskID].Steps.Count-1)
        {
            _manual[taskID].PrevStepIndex = _manual[taskID].Steps.Count-2;
            _manual[taskID].CurrStepIndex = _manual[taskID].Steps.Count-1;
            _manual[taskID].NextStepIndex = -1;
        }
        else
        {
            _manual[taskID].PrevStepIndex = stepIndex - 1;
            _manual[taskID].CurrStepIndex = stepIndex;
            _manual[taskID].NextStepIndex = stepIndex + 1;
        }

        PublishToSubscribers(SusbcriberType.UpdateStep);
    }

    #endregion
}
