using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DataManager : Singleton<DataManager>
{
    private Dictionary<string, TaskList> _manual = new Dictionary<string, TaskList>();

    public Dictionary<string, TaskList> Manual => _manual;

    private string _currentTask = "";
    public string CurrentTask => _currentTask;


    private List<UnityEvent> subscribers = new List<UnityEvent>();

    //Converts the tasklist object with key recipename into a matrix of strings
    //The final matrix would be of size (number of steps and substeps x 2)
    //Each row element would be of the form [step description, 0] for main steps
    //and [step description, 1] for sub steps
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
    private void PublishToSubscribers()
    {
        foreach (var subscriber in subscribers)
            subscriber.Invoke();
    }

    public void AddDataSubscriber(UnityAction subscriberEvent) 
    { 
        UnityEvent newDataUpdateEvent = new UnityEvent();
        newDataUpdateEvent.AddListener(subscriberEvent);
        subscribers.Add(newDataUpdateEvent); 
    }

    #region Adding and Deleting Tasks

    //Go to the resources folder and load a new tasklist
    //json file. The name of the file should be in the form
    //(recipename).json
    public void LoadNewRecipe(string recipename)
    {
        var jsonTextFile = Resources.Load<TextAsset>("Text/" + recipename);
        AngelARUI.Instance.LogDebugMessage("Loaded task from json: "+jsonTextFile.text, true);
        LoadNewRecipeFromString(jsonTextFile.text);
    }

    //Take in a json of the class TaskList and add it as a recipe
    private void LoadNewRecipeFromString(string json)
    {
        TaskList currList = JsonUtility.FromJson<TaskList>(json);
        //If there already is a recipe with the same name, still add it to the main list
        //but add a number to its name (for example the second instance of the "Pinwheels"
        //recipe would be stored as "Pinwheels_2")
        if (!_manual.ContainsKey(currList.Name))
        {
            _manual.Add(currList.Name, currList);
        }
        else
        {
            for (int i = 2; i <= 5; i++)
            {
                if (!_manual.ContainsKey(currList.Name + "_" + i.ToString()))
                {
                    _manual.Add(currList.Name + "_" + i.ToString(), currList);
                    break;
                }
            }
        }

        PublishToSubscribers();
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

    #region Changes to current recipe and recipe steps

    /// <summary>
    /// Change which recipe shows up as the "current" one
    /// </summary>
    /// <param name="recipename"></param>
    public void SetCurrentActiveTask(string recipename)
    {
        _currentTask = recipename;
        TaskList CurrRecipeObj = _manual[_currentTask];
        int currStepIndex = _manual[_currentTask].CurrStepIndex;
        Orb.Instance.SetTaskMessage(CurrRecipeObj.Steps[currStepIndex].StepDesc, CurrRecipeObj.Steps[currStepIndex].StepDesc, CurrRecipeObj.Steps[currStepIndex].StepDesc);

        PublishToSubscribers();
    }

    public void SetCurrentStep(string recipeID, int taskID)
    {
        if (taskID <= 0)
            _manual[recipeID].PrevStepIndex = -1;
        else
            _manual[recipeID].PrevStepIndex = taskID-1;

        if (taskID >= _manual[recipeID].Steps.Count)
        {
            _manual[recipeID].CurrStepIndex = _manual[recipeID].Steps.Count-1;
            _manual[recipeID].NextStepIndex = _manual[recipeID].Steps.Count-1;
        } else
        {
            _manual[recipeID].CurrStepIndex = taskID;
            _manual[recipeID].NextStepIndex = taskID + 1;
        }

        PublishToSubscribers();
    }

    #endregion
}
