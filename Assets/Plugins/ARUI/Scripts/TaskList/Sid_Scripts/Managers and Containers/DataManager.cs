using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    Dictionary<string, TaskList> Recipes = new Dictionary<string, TaskList>();
    string CurrRecipe = "";
    public GameObject OverviewPrefab;

    void Start()
    {
        //Debugging purposes
        InitializeTaskOverview();
        StartCoroutine(ExampleScript());
    }

    //MOVE TO ARUI??
    //OVERVIEW REFERENCE
    public void InitializeTaskOverview()
    {
        GameObject overviewObj = Instantiate(OverviewPrefab);
        Center_of_Objs.Instance.SnapToCentroid();
    }

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

    #region Loading and deleting recipes
    //Go to the resources folder and load a new tasklist
    //json file. The name of the file should be in the form
    //(recipename).json
    public void LoadNewRecipe(string recipename)
    {
        var jsonTextFile = Resources.Load<TextAsset>("Text/" + recipename);
        UnityEngine.Debug.Log(jsonTextFile.text);
        LoadNewRecipeFromString(jsonTextFile.text);
    }

    //Take in a json of the class TaskList and add it as a recipe
    public void LoadNewRecipeFromString(string json)
    {
        TaskList currList = JsonUtility.FromJson<TaskList>(json);
        //If there already is a recipe with the same name, still add it to the main list
        //but add a number to its name (for example the second instance of the "Pinwheels"
        //recipe would be stored as "Pinwheels_2")
        if (!Recipes.ContainsKey(currList.Name))
        {
            Recipes.Add(currList.Name, currList);
        }
        else
        {
            for (int i = 2; i <= 5; i++)
            {
                if (!Recipes.ContainsKey(currList.Name + "_" + i.ToString()))
                {
                    Recipes.Add(currList.Name + "_" + i.ToString(), currList);
                    break;
                }
            }
        }
    }

    //Delete recipe with given recipe name
    //If it is the last recipe, then handle task overview and orb
    public void DeleteRecipe(string recipeName)
    {
        if (Recipes.ContainsKey(recipeName))
        {
            Recipes.Remove(recipeName);
        }

        if (Recipes.Count == 0)
        {
            Orb.Instance.SetTaskMessage("No pending tasks");
            //OVERVIEW REFERENCE
            MultipleListsContainer.Instance.gameObject.SetActive(false);
        }
    }

    //Delete the current recipe and replace it with a new current recipe
    //defined by newCurr. If that was the last recipe, then handle
    //task overview and orb
    public void DeleteCurrRecipe(string newCurr = "")
    {
        Recipes.Remove(CurrRecipe);

        if (Recipes.Count == 0)
        {
            Orb.Instance.SetTaskMessage("No pending tasks");
            //OVERVIEW REFERENCE
            MultipleListsContainer.Instance.gameObject.SetActive(false);
        } else
        {
            if (Recipes.ContainsKey(newCurr))
            {
                UpdateCurrRecipe(newCurr);
            }
        }
    }
    
    #endregion
    //After adding, removing or updating any of the recipe data
    //call this function to see changes reflected on task overview
    public void ReloadTaskList()
    {
        //OVERVIEW REFERENCE
        MultipleListsContainer.Instance.UpdateAllSteps(Recipes, CurrRecipe);
    }

    #region Changes to current recipe and recipe steps
    //Change which recipe shows up as the "current" one
    public void UpdateCurrRecipe(string recipename)
    {
        CurrRecipe = recipename;
        TaskList CurrRecipeObj = Recipes[CurrRecipe];
        int currStepIndex = Recipes[CurrRecipe].CurrStepIndex;
        Orb.Instance.SetTaskMessage(CurrRecipeObj.Steps[currStepIndex].StepDesc);
    }

    //Change the next step index that the current task is pointing to
    public void UpdateCurrNextStepIndex(int index)
    {
        UpdateNextStepIndex(CurrRecipe, index);
    }

    //Change the next step index that the task with name "recipename" is pointing to
    public void UpdateNextStepIndex(string recipename, int index)
    {
        Recipes[recipename].NextStepIndex = index;
    }

    //Change the previous step index that the current task is pointing to
    public void UpdateCurrPrevStepIndex(int index)
    {
        UpdatePrevStepIndex(CurrRecipe, index);
    }

    //Change the previous step index that the task with name "recipename" is pointing to
    public void UpdatePrevStepIndex(string recipename, int index)
    {
        Recipes[recipename].PrevStepIndex = index;
    }

    //For the current recipe, have it go to the next substep
    public void GoToNextSubStepCurrRecipe()
    {
        GoToNextSubStep(CurrRecipe);
    }

    //For any recipe with key recipeName, have it go to the next substep
    public void GoToNextSubStep(string recipeName)
    {
        TaskList CurrRecipe = Recipes[recipeName];
        int currStepIndex = Recipes[recipeName].CurrStepIndex;
        CurrRecipe.Steps[currStepIndex].CurrSubStepIndex++;
    }

    //For the current recipe, have it go to the next step
    public void GoToNextStepCurrRecipe()
    {
        GoToNextStep(CurrRecipe);
    }

    //For the current recipe, have it go to the next step
    public void GoToPrevStepCurrRecipe()
    {
        GoToPrevStep(CurrRecipe);
    }

    //For any recipe with key recipeName, have it go to the next step
    //while also updating the current and previous step 
    public void GoToNextStep(string recipeName)
    {
        if (Recipes[recipeName].CurrStepIndex >= Recipes[recipeName].Steps.Count - 1 || Recipes[recipeName].CurrStepIndex == -1)
        {
            Recipes[recipeName].CurrStepIndex = -1;
        }
        else if (Recipes[recipeName].CurrStepIndex == Recipes[recipeName].Steps.Count - 2)
        {
            Recipes[recipeName].CurrStepIndex++;
            Recipes[recipeName].NextStepIndex = -1;
            Recipes[recipeName].PrevStepIndex++;
        }
        else
        {
            Recipes[recipeName].CurrStepIndex++;
            Recipes[recipeName].NextStepIndex++;
            Recipes[recipeName].PrevStepIndex++;
        }
    }

    //For any recipe with key recipeName, have it go to the next step
    //while also updating the current and next step 
    public void GoToPrevStep(string recipeName)
    {
        if (Recipes[recipeName].CurrStepIndex <= 0 || Recipes[recipeName].CurrStepIndex == -1)
        {
            Recipes[recipeName].CurrStepIndex = -1;
        }
        else if (Recipes[recipeName].CurrStepIndex == 1)
        {
            Recipes[recipeName].CurrStepIndex--;
            Recipes[recipeName].PrevStepIndex = -1;
            Recipes[recipeName].NextStepIndex--;
        }
        else
        {
            Recipes[recipeName].CurrStepIndex--;
            Recipes[recipeName].NextStepIndex--;
            Recipes[recipeName].PrevStepIndex--;
        }
    }
    #endregion

    //Debugging purposes
    IEnumerator ExampleScript()
    {
        yield return new WaitForSeconds(0.5f);
        LoadNewRecipe("Task1");
        LoadNewRecipe("Task1");
        LoadNewRecipe("Task1");
        UpdateCurrRecipe("Pinwheels");
        ReloadTaskList();
        yield return new WaitForSeconds(5.0f);
        GoToNextStep("Pinwheels_2");
        ReloadTaskList();
        yield return new WaitForSeconds(5.0f);
        GoToPrevStep("Pinwheels_3");
        ReloadTaskList();
        yield return new WaitForSeconds(5.0f);
        UpdateCurrRecipe("Pinwheels_2");
        ReloadTaskList();
        yield return new WaitForSeconds(5.0f);
        DeleteCurrRecipe("Pinwheels");
        ReloadTaskList();
        yield return new WaitForSeconds(5.0f);
        DeleteRecipe("Pinwheels_3");
        ReloadTaskList();
        yield return new WaitForSeconds(5.0f);
        DeleteCurrRecipe();
    }
}
