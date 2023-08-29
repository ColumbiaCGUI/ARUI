using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using Shapes;

public class MultipleListsContainer : MonoBehaviour
{
    public List<GameObject> itemsMenus;

    public GameObject Main_TaskOverview_Container;

    public GameObject TaskOverview_Container;

    public Line OverviewLine;

    public GameObject MenusContainer;

    int numSecondaryTasks = 0;

    //List of containers for each of the current lists
    public List<TaskOverviewContainerRepo> containers;

    //Prefab for additional tasklists
    public GameObject SecondaryListPrefab;

    public Transform currOverviewParent;

    int currIndex = 0;

    [SerializeField]
    bool isMenu = false;


    private float delta;

    [SerializeField]
    private float disableDelay = 1.0f;

    void Update()
    {
        if (isMenu)
        {
            //if eye gaze not on task objects then do fade out currentindex
            if (EyeGazeManager.Instance.CurrentHit != EyeTarget.listmenuButton_tasks)
            {
                if (delta > disableDelay)
                {
                    StartCoroutine(FadeOut());
                }
                else
                {
                    delta += Time.deltaTime;
                }

            }
            //else fade back in currentindex 
        }
    }

    #region Setting menu active and inactive
    public void SetMenuActive(int index)
    {
        currIndex = index;
        for(int i = 0; i < itemsMenus.Count; i++)
        {
            if(i == index)
            {
                itemsMenus[i].SetActive(true);
            } else
            {
                CanvasGroup canvasGroup = itemsMenus[i].GetComponent<CanvasGroup>();
                canvasGroup.alpha = 1.0f;
                itemsMenus[i].SetActive(false);
            }
        }

    }
    private IEnumerator FadeOut()
    {
        GameObject canvas = itemsMenus[currIndex];
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        float counter = 0f;
        float duration = 1.0f;
        float startAlpha = 1.0f;
        float targetAlpha = 0.0f;
        bool broken = false;
        while (counter < duration)
        {
            if (EyeGazeManager.Instance.CurrentHit == EyeTarget.listmenuButton_tasks)
            {
                broken = true;
                break;
            }
            counter += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, counter / duration);

            yield return null;
        }
        if (!broken)
        {
            canvas.SetActive(false);
            canvasGroup.alpha = 1.0f;
            ResetOrb();
        }
        else
        {
            delta = 0.0f;
            canvasGroup.alpha = 1.0f;
            canvas.SetActive(true);
        }

    }
    #endregion

    #region Managing Orb
    public void AttachOrb()
    {
        Orb currOrb = GameObject.FindObjectOfType<Orb>();
        //currOrb.enabled = false;
        GameObject orb = currOrb.gameObject;
        //TODO: Change to current task object's thingy
        orb.transform.parent = currOverviewParent;
        //Change child position + rotation
        Transform child = orb.transform.GetChild(0);
        child.gameObject.GetComponent<OrbFollowerSolver>().enabled = false;
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.Euler(0f, 0f, 0f);
        //Change position + rotation
        orb.transform.localPosition = Vector3.zero;
        orb.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        //Disable box colldider 
        child.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    public void ResetOrb()
    {
        Orb currOrb = GameObject.FindObjectOfType<Orb>();
        GameObject orb = currOrb.gameObject;
        //TODO: Change to current task object's thingy
        orb.transform.parent = AngelARUI.Instance.transform;
        //Change child position + rotation
        Transform child = orb.transform.GetChild(0);
        child.gameObject.GetComponent<OrbFollowerSolver>().enabled = true;
        //Disable box colldider 
        child.gameObject.GetComponent<BoxCollider>().enabled = true;
    }
    #endregion

    //INCOMPLETE!!
    #region Managing task overview steps and recipes
    //TODO: COMPLETE!!!
    public void UpdateAllSteps(Dictionary<string, TaskList> tasks, string currTask)
    {
        ResetAllTaskOverviews();
        int index = 1;
        foreach(KeyValuePair<string, TaskList> pair in tasks)
        {
            if (pair.Key == currTask)
            {
                containers[0].taskNameText.SetText(pair.Value.Name);
                SetupCurrTaskOverview currSetup = containers[0].setupInstance;
                if (pair.Value.CurrStepIndex != -1)
                {
                    currSetup.SetupCurrTask(pair.Value.Steps[pair.Value.CurrStepIndex], this.GetComponent<Center_of_Objs>());
                }
                if (pair.Value.NextStepIndex != -1)
                {
                    currSetup.SetupNextTask(pair.Value.Steps[pair.Value.NextStepIndex]);
                } else
                {
                    currSetup.DeactivateNextTask();
                }
                if (pair.Value.PrevStepIndex != -1)
                {
                    currSetup.SetupPrevTask(pair.Value.Steps[pair.Value.PrevStepIndex]);
                } else
                {
                    currSetup.DeactivatePrevTask();
                }
            }
            else
            {
                GameObject currOverview = AddNewTaskOverview();
                containers.Add(currOverview.GetComponent<TaskOverviewContainerRepo>());
                TaskOverviewContainerRepo curr = containers[containers.Count - 1];
                curr.multiListInstance.ListContainer = this.gameObject;
                curr.multiListInstance.index = index;
                curr.taskNameText.SetText(pair.Value.Name);
                itemsMenus.Add(curr.taskUI);
                SetupCurrTaskOverview currSetup = curr.setupInstance;
                if (pair.Value.CurrStepIndex != -1)
                {
                    currSetup.SetupCurrTask(pair.Value.Steps[pair.Value.CurrStepIndex]);
                }
                if (pair.Value.NextStepIndex != -1)
                {
                    currSetup.SetupNextTask(pair.Value.Steps[pair.Value.NextStepIndex]);
                } else
                {
                    currSetup.DeactivateNextTask();
                }
                if (pair.Value.PrevStepIndex != -1)
                {
                    currSetup.SetupPrevTask(pair.Value.Steps[pair.Value.PrevStepIndex]);
                } else
                {
                    currSetup.DeactivatePrevTask();
                }
                index++;
            }
        }
        //Clear all lists from currContainer (except for current one of course) 
        //For loop to go through all tasklists
        //Set up tasklist based on currTaskindex WE NEED A WAY TO ACCESS SetupCurrTaskOverview COMPONENTS!
        //IF CURRENTTASK INDEX ALSO UPDATE CENTER OBJECTS 
        //if not currtasklistindex then create a new object and set up based on task
    }

    public void ResetAllTaskOverviews()
    {
        TaskOverviewContainerRepo firstCont = containers[0];
        GameObject firstObj = itemsMenus[0];
        for(int i = 1; i < containers.Count; i++)
        {
            Destroy(containers[i].gameObject);
            OverviewLine.Start = new Vector3(OverviewLine.Start.x, OverviewLine.Start.y + 0.015f, OverviewLine.Start.z);
            MenusContainer.transform.localPosition = new Vector3(MenusContainer.transform.localPosition.x, MenusContainer.transform.localPosition.y - 0.025f, MenusContainer.transform.localPosition.z);
            numSecondaryTasks--;
        }
        itemsMenus.Clear();
        itemsMenus.Add(firstObj);
        containers.Clear();
        containers.Add(firstCont);
    }

    public GameObject AddNewTaskOverview()
    {
        numSecondaryTasks++;
        GameObject newOverview = Instantiate(SecondaryListPrefab, TaskOverview_Container.transform);
        newOverview.transform.localPosition = new Vector3(Main_TaskOverview_Container.transform.localPosition.x, Main_TaskOverview_Container.transform.localPosition.y - (0.07f * numSecondaryTasks), Main_TaskOverview_Container.transform.localPosition.z);
        OverviewLine.Start = new Vector3(OverviewLine.Start.x, OverviewLine.Start.y - 0.015f, OverviewLine.Start.z);
        MenusContainer.transform.localPosition = new Vector3(MenusContainer.transform.localPosition.x, MenusContainer.transform.localPosition.y + 0.025f, MenusContainer.transform.localPosition.z);
        return newOverview;
    }
    #endregion
}
