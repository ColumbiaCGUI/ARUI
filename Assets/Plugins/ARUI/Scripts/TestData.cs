using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TestData : MonoBehaviour
{
    public bool build = true;

    // Testing Task List
    string[,] tasks0 =
    {
        {"0", "Measure 12 ounces of water in the liquid measuring cup"},
        {"1", "Pour the water from the liquid measuring cup into  the electric kettle"},
        {"1", "Turn on the electric kettle by pushing the button underneath the handle"},
        {"1", "Boil the water. The water is done boiling when the button underneath the handle pops up"},
        {"1", "While the water is boiling, assemble the filter cone. Place the dripper on top of a coffee mug"}, //4
        {"0", "Prepare the filter insert by folding the paper filter in half to create a semi-circle, and in half again to create a quarter-circle. Place the paper filter in the dripper and spread open to create a cone."},
        {"1", "Take the coffee filter and fold it in half to create a semi-circle"},
        {"1", "Folder the filter in half again to create a quarter-circle"},
        {"1", "Place the folded filter into the dripper such that the the point of the quarter-circle rests in the center of the dripper"},
        {"1", "Spread the filter open to create a cone inside the dripper"},
        {"0", "Place the dripper on top of the mug"},//10
        {"0","Weigh the coffee beans and grind until the coffee grounds are the consistency of coarse sand, about 20 seconds. Transfer the grounds to the filter cone."},
        {"1","Turn on the kitchen scale"},
         {"0"," Turn on the thermometer"},
         {"1"," Place the end of the thermometer into the water. The temperature should read 195-205 degrees Fahrenheit or between 91-96 degrees Celsius."},
         {"0","Pour the water over the coffee grounds"},
         {"0","Clean up the paper filter and coffee grounds"}, //16
    };
    string[,] tasks1 =
    {
        {"0", "Place tortilla on cutting board."},
        {"0", "Use a butter knife to scoop about a tablespoon of nut butter from the jar."},
        {"1", "Spread nut butter onto tortilla, leaving 1/2-inch uncovered at the edges."},
        {"0", "Clean the knife by wiping with a paper towel."},
        {"0", "Use the knife to scoop about a tablespoon of jelly from the jar."},
        {"1", "Spread jelly over the nut butter."}, //4
        {"1", "Clean the knife by wiping with a paper towel."},
        {"0", "Roll the tortilla from one end to the other into a log shape, about 1.5 inches thick. Roll it tight enough to prevent gaps, but not so tight that the filling leaks."},
        {"0", "Secure the rolled tortilla by inserting 5 toothpicks about 1 inch apart."},
        {"0", "Trim the ends of the tortilla roll with the butter knife, leaving 1?2 inch margin between the last toothpick and the end of the roll. Discard ends."},
        {"0", "Slide floss under the tortilla, perpendicular to the length of the roll.Place the floss halfway between two toothpicks."},
        {"0", "Cross the two ends of the floss over the top of the tortilla roll." },
        {"1", "Holding one end of the floss in each hand, pull the floss ends in opposite directions to slice."},
        {"0", "Continue slicing with floss to create 5 pinwheels."},//12
    };

    private int currentTask = 0;

    private void Start()
    {
        if (!build)
            StartCoroutine(RunTasksAtRuntime());
    }

    private IEnumerator RunTasksAtRuntime()
    {
        yield return new WaitForSeconds(0.5f);

        AngelARUI.Instance.SetTasks(tasks0);

        yield return new WaitForSeconds(1f);

        AngelARUI.Instance.RegisterDetectedObject(transform.GetChild(0).gameObject, "test");

        yield return new WaitForSeconds(3f);

        AngelARUI.Instance.SetTasks(tasks1);

        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        yield return new WaitForSeconds(1f);

        AngelARUI.Instance.SetViewManagement(false);

        yield return new WaitForSeconds(4f);
        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);
        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);
        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        AngelARUI.Instance.MuteAudio(true);

        yield return new WaitForSeconds(2f);
        currentTask--;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        AngelARUI.Instance.SetViewManagement(true);

        yield return new WaitForSeconds(3f);
        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        yield return new WaitForSeconds(1f);
        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        AngelARUI.Instance.DeRegisterDetectedObject("test");

        AngelARUI.Instance.MuteAudio(false);

        yield return new WaitForSeconds(1f);

        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        yield return new WaitForSeconds(3f);
        AngelARUI.Instance.ShowSkipNotification(true);

        yield return new WaitForSeconds(5f);
        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        yield return new WaitForSeconds(2f);

        int next = currentTask + 1;
        //Set message (e.g. "Did you mean '{user intent}'?"
        string user_intent = "Did you mean 'Go to the next task'?";

        //Set event that should be triggered if user confirms
        AngelARUI.Instance.SetUserIntentCallback(() => { AngelARUI.Instance.SetCurrentTaskID(next); });

        //Show dialogue to user
        AngelARUI.Instance.TryGetUserFeedbackOnUserIntent(user_intent);

        yield return new WaitForSeconds(10f);

        next = currentTask - 1;
        //Set message (e.g. "Did you mean '{user intent}'?"
        user_intent = "Did you mean 'Go to the previous task'?";

        //Set event that should be triggered if user confirms
        AngelARUI.Instance.SetUserIntentCallback(() => { AngelARUI.Instance.SetCurrentTaskID(next); });

        //Show dialogue to user
        AngelARUI.Instance.TryGetUserFeedbackOnUserIntent(user_intent);
    }


#if UNITY_EDITOR

    /// <summary>
    /// Listen to Keyevents for debugging(only in the Editor)
    /// </summary>
    public void Update()
    {
        //Example how to set the recipe(task list in the ARUI) -example data see on top
        if (Input.GetKeyUp(KeyCode.O))
        {
            currentTask = 0;
            AngelARUI.Instance.SetCurrentTaskID(currentTask);

            int coin = Random.Range(0, 10);
            if (coin % 2 == 0)
            {
                AngelARUI.Instance.SetTasks(tasks0);
            }
            else
            {
                AngelARUI.Instance.SetTasks(tasks1);
            }
        }

        // Example how to use the NLI confirmation dialogue
        if (Input.GetKeyUp(KeyCode.P))
        {
            int next = currentTask++;
            //1) Set message (e.g. "Did you mean '{user intent}'?"
            string user_intent = "Did you mean 'Go to the next task'?";

            //2) Set event that should be triggered if user confirms
            AngelARUI.Instance.SetUserIntentCallback(() => { AngelARUI.Instance.SetCurrentTaskID(next); });

            //4) Show dialogue to user
            AngelARUI.Instance.TryGetUserFeedbackOnUserIntent(user_intent);
        }

        // Example how to use the NLI confirmation dialogue
        if (Input.GetKeyUp(KeyCode.I))
        {
            AngelARUI.Instance.RegisterDetectedObject(transform.GetChild(0).gameObject, "test");
        }

        // Example how to step forward/backward in tasklist. 
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            currentTask++;
            AngelARUI.Instance.SetCurrentTaskID(currentTask);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            currentTask--;
            AngelARUI.Instance.SetCurrentTaskID(currentTask);
        }

        // Example how to trigger a skip notification. 
        if (Input.GetKeyUp(KeyCode.M))
        {
            AngelARUI.Instance.ShowSkipNotification(true);
        }

        // Example how to disable skip notification (is disable if system sets new task, or if system disables task manually
        if (Input.GetKeyUp(KeyCode.B))
        {
            AngelARUI.Instance.ShowSkipNotification(false);
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            AngelARUI.Instance.SetViewManagement(!AngelARUI.Instance.IsVMActiv);
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            AngelARUI.Instance.ShowDebugEyeGazeTarget(false);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            AngelARUI.Instance.ShowDebugEyeGazeTarget(true);
        }
    }

#endif
}
