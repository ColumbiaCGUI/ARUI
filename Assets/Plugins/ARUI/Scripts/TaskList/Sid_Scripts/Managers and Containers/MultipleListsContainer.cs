using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using Shapes;

public class MultipleListsContainer : Singleton<MultipleListsContainer>
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

    public int currIndex = 0;

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
            if (EyeGazeManager.Instance != null)
            {
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
            }
        }
    }
    #region Managing the main task line 
    public void SetLineEnd(Vector3 EndCords)
    {
        Vector3 finalCords = OverviewLine.transform.InverseTransformPoint(EndCords);
        //OverviewLine.End = new Vector3(OverviewLine.End.x, finalCords.y, OverviewLine.End.z);
        OverviewLine.End = finalCords;
    }

    public void SetLineStart(Vector3 EndCords)
    {
        Vector3 finalCords = OverviewLine.transform.InverseTransformPoint(EndCords);
        //OverviewLine.End = new Vector3(OverviewLine.End.x, finalCords.y, OverviewLine.End.z);
        OverviewLine.Start = finalCords;
    }
    #endregion

    #region Setting inidvidual recipe menus active/inative
    public void SetMenuActive(int index)
    {
        this.GetComponent<Center_of_Objs>().SetIsLooking(true);
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
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, counter / duration);
            }

            yield return null;
        }
        if (!broken)
        {
            if (canvas != null)
            {
                canvas.SetActive(false);
            }
            this.GetComponent<Center_of_Objs>().SetIsLooking(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1.0f;
            }
            this.GetComponent<Center_of_Objs>().DeactivateLines();
        }
        else
        {
            delta = 0.0f;
            canvasGroup.alpha = 1.0f;
            canvas.SetActive(true);
        }

    }
    #endregion

    #region Managing task overview steps and recipes
    public void UpdateAllSteps(Dictionary<string, TaskList> tasks, string currTask)
    {
        ResetAllTaskOverviews();
        int index = 1;
        foreach(KeyValuePair<string, TaskList> pair in tasks)
        {
            if (pair.Key == currTask)
            {
                containers[0].taskNameText.SetText(pair.Key);
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
                curr.taskNameText.SetText(pair.Key);
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

    #region Setting task overview active and inactive
    public void ToggleOverview()
    {
        if (!MenusContainer.activeSelf)
        {
            OverviewLine.gameObject.SetActive(true);
            MenusContainer.SetActive(true);
            Center_of_Objs.Instance.SnapToCentroid();
        } else
        {
            OverviewLine.gameObject.SetActive(false);
            MenusContainer.SetActive(false);
        }
    }
    #endregion
}
