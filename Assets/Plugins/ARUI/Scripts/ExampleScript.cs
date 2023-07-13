using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem.HID;

public class ExampleScript : MonoBehaviour
{
    public bool Automate = false;

    private bool cubeSpawned;
    private bool kettleSpawned;
    private bool potSpawned;

    // Testing Task List
    string[,] _tasks0 =
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

    private string[,] _tasks1 =
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

    string[,] _tasks2 =
    {
        {"0", "Remove the nut and the air cleaner cover"},
        {"0", "Remove the wing nut and air filter assembly"},
        {"0", "Separate the inner paper filter from the outer foam filter. Carefully check both filters for holes or tears and replace if damaged."},
        {"0", "Separate the inner paper filter from the outer foam filter."},
        {"0", "Remove the wing nut and air filter assembly."},
        {"0", "Remove the nut and the air cleaner cover." },

    };

    private int currentTask = 0;
    private Transform itemList;
    private int id;

    private void Start()
    {
        cubeSpawned = false;
        kettleSpawned = false;
        potSpawned = false;

        itemList = GameObject.Find("FakeItem").transform;

        id = 0;

        //if (Automate)
        //StartCoroutine(RunTasksAtRuntime());
        //StartCoroutine(AnnotationAtRuntime());
    }

    private IEnumerator RunTasksAtRuntime()
    {
        yield return new WaitForSeconds(0.5f); //Wait a few frames, so everything is initialized

        AngelARUI.Instance.SetTasklistEyeEventsActive(false);

        AngelARUI.Instance.SetTasks(_tasks0);

        yield return new WaitForSeconds(1f);

        AngelARUI.Instance.RegisterDetectedObject(transform.GetChild(0).gameObject, "test");

        yield return new WaitForSeconds(3f);

        AngelARUI.Instance.SetTasks(_tasks2);

        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        yield return new WaitForSeconds(1f);

        yield return new WaitForSeconds(4f);
        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        AngelARUI.Instance.MuteAudio(true);

        yield return new WaitForSeconds(2f);
        currentTask--;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        yield return new WaitForSeconds(3f);
        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        yield return new WaitForSeconds(1f);
        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

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
        string user_intent = "Go to the next task";

        //Set event that should be triggered if user confirms
        AngelARUI.Instance.SetUserIntentCallback(() => { AngelARUI.Instance.SetCurrentTaskID(next); });

        //Show dialogue to user
        AngelARUI.Instance.TryGetUserFeedbackOnUserIntent(user_intent);

        AngelARUI.Instance.RegisterDetectedObject(transform.GetChild(0).gameObject, "test");

        yield return new WaitForSeconds(10f);

        next = currentTask - 1;
        //Set message (e.g. "Did you mean '{user intent}'?"
        user_intent = "Go to the previous task";

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
        // Simulate a Cube is detected
        if (Input.GetKeyUp(KeyCode.C) && !cubeSpawned)
        {
            Instantiate(Resources.Load<GameObject>("Prefabs/TestObject/Cube"), itemList);
        }
        if (Input.GetKeyUp(KeyCode.K) && !kettleSpawned)
        {
            Instantiate(Resources.Load<GameObject>("Prefabs/TestObject/Kettle"), itemList);
        }
        if (Input.GetKeyUp(KeyCode.P) && !potSpawned)
        {
            Instantiate(Resources.Load<GameObject>("Prefabs/TestObject/Pot"), itemList);
        }

        if (itemList != null)
        {
            foreach (Transform child in itemList.transform)
            {
                if (!child.gameObject.activeSelf) continue;

                if (child.Find("Annotation(Clone)") != null) continue;

                if (child.gameObject.name.Contains("Cube") && !cubeSpawned)
                {
                    cubeSpawned = true;

                    StartCoroutine(
                        AngelARUI.Instance.AttachAnnotation(id, child.gameObject,
                        true, "Cube",
                        false, "A cube is used to be placed around.",
                        true, "Images/cubeUnity",
                        false, "cubeVideo")
                    );

                    id++;
                }

                if (child.gameObject.name.Contains("Kettle") && !kettleSpawned)
                {
                    string kettleDesc = "A kettle is a kitchen appliance used to heat water, often for making hot beverages like tea and coffee. They can be traditional, using a stovetop, or electric with an automatic shutoff.";

                    kettleSpawned = true;

                    StartCoroutine(
                        AngelARUI.Instance.AttachAnnotation(id, child.gameObject,
                        true, "Kettle",
                        false, kettleDesc,
                        false, "cube",
                        true, "Videos/kettle")
                    );

                    id++;
                }

                if (child.gameObject.name.Contains("Pot") && !potSpawned)
                {
                    string potDesc = "A pot is a round kitchen utensil used for cooking food.";
                    potSpawned = true;

                    StartCoroutine(
                        AngelARUI.Instance.AttachAnnotation(id, child.gameObject,
                        true, "Pot",
                        true, potDesc,
                        false, "cube",
                        false, "cubeVideo")
                    );

                    id++;
                }
            }
        }
        /* Input from Bettina's test 
        //Example how to set the recipe(task list in the ARUI) -example data see on top
        if (Input.GetKeyUp(KeyCode.O))
        {
            currentTask = 0;
            AngelARUI.Instance.SetTasks(_tasks2);
            AngelARUI.Instance.SetCurrentTaskID(currentTask);
        }

        // Example how to use the NLI confirmation dialogue
        if (Input.GetKeyUp(KeyCode.P))
        {
            int next = currentTask++;
            //1) Set message (e.g. "Did you mean '{user intent}'?"
            string user_intent = "Go to the next task";

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

        if (Input.GetKeyUp(KeyCode.K))
        {
            AngelARUI.Instance.DeRegisterDetectedObject("test");
        }

        if (Input.GetKeyUp(KeyCode.Y))
        {
            AngelARUI.Instance.SetTasklistEyeEventsActive(false);
        }

        if (Input.GetKeyUp(KeyCode.U))
        {
            AngelARUI.Instance.SetTasklistEyeEventsActive(true);
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
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            AngelARUI.Instance.ShowSkipNotification(true);
        }

        // Example how to disable skip notification (is disable if system sets new task, or if system disables task manually
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            AngelARUI.Instance.ShowSkipNotification(false);
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            AngelARUI.Instance.SetViewManagement(!AngelARUI.Instance.IsVMActiv);
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            AngelARUI.Instance.ShowDebugEyeGazeTarget(false);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            AngelARUI.Instance.ShowDebugEyeGazeTarget(true);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            AngelARUI.Instance.PrintVMDebug = true;
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            AngelARUI.Instance.PrintVMDebug = false;
        }
        */
    }

#endif
}
