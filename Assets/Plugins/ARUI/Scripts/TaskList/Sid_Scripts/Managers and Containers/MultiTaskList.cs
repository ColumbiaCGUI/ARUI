using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class MultiTaskList : Singleton<MultiTaskList>
{
    private List<GameObject> _allTasklists = new List<GameObject>();
    private Line _overviewHandle;
    private GameObject _followCameraContainer;
    private GameObject _taskOverviewContainer;
    private GameObject _mainTaskContainer;

    private int _numSecondaryTasks = 0;

    //List of containers for each of the current lists
    private List<TaskOverviewContainerRepo> _containers = new List<TaskOverviewContainerRepo>();

    private int _currIndex = 0;
    public int CurrentIndex => _currIndex;

    [SerializeField]
    private bool isMenu = false;

    private float delta;

    [SerializeField]
    private float disableDelay = 1.0f;

    public void Start()
    {
        //DataManager.Instance.AddDataSubscriber(() => HandleDataUpdateEvent());

        _overviewHandle = transform.GetChild(0).gameObject.GetComponent<Line>();
        _followCameraContainer = transform.GetChild(1).gameObject;

        _taskOverviewContainer = _followCameraContainer.transform.GetChild(0).gameObject;
        //_mainTaskContainer = _taskOverviewContainer.transform.GetChild(0).gameObject;
    }

    public void HandleDataUpdateEvent()
    {
        MultiTaskList.Instance.UpdateAllSteps(DataProvider.Instance.CurrentSelectedTasks, DataProvider.Instance.CurrentTask);
    }

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
                        delta += Time.deltaTime;

                }
            }
        }
    }

    #region Managing the main task line 
    public void SetLineEnd(Vector3 EndCords)
    {
        Vector3 finalCords = _overviewHandle.transform.InverseTransformPoint(EndCords);
        //OverviewLine.End = new Vector3(OverviewLine.End.x, finalCords.y, OverviewLine.End.z);
        _overviewHandle.End = finalCords;
    }

    public void SetLineStart(Vector3 EndCords)
    {
        Vector3 finalCords = _overviewHandle.transform.InverseTransformPoint(EndCords);
        //OverviewLine.End = new Vector3(OverviewLine.End.x, finalCords.y, OverviewLine.End.z);
        _overviewHandle.Start = finalCords;
    }
    #endregion

    #region Setting inidvidual recipe menus active/inative

    public void SetMenuActive(int index)
    {
        this.GetComponent<Center_of_Objs>().SetIsLooking(true);
        _currIndex = index;
        for(int i = 0; i < _allTasklists.Count; i++)
        {
            if(i == index)
            {
                _allTasklists[i].SetActive(true);
            } else
            {
                CanvasGroup canvasGroup = _allTasklists[i].GetComponent<CanvasGroup>();
                canvasGroup.alpha = 1.0f;
                _allTasklists[i].SetActive(false);
            }
        }

    }
    private IEnumerator FadeOut()
    {
        GameObject canvas = _allTasklists[_currIndex];
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
                _containers[0].taskNameText.SetText(pair.Key);
                SetupCurrTaskOverview currSetup = _containers[0].setupInstance;
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
                if (_allTasklists.Contains(_containers[_containers.Count - 1].gameObject))
                    return;
                GameObject currOverview = AddNewTaskOverview();
                _containers.Add(currOverview.GetComponent<TaskOverviewContainerRepo>());
                TaskOverviewContainerRepo curr = _containers[_containers.Count - 1];
                curr.multiListInstance.ListContainer = this.gameObject;
                curr.multiListInstance.index = index;
                curr.taskNameText.SetText(pair.Key);
                _allTasklists.Add(curr.taskUI);
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
        if (_containers.Count == 0) return;
        
        TaskOverviewContainerRepo firstCont = _containers[0];
        for(int i = 1; i < _containers.Count; i++)
        {
            Destroy(_containers[i].gameObject);
            _overviewHandle.Start = new Vector3(_overviewHandle.Start.x, _overviewHandle.Start.y + 0.015f, _overviewHandle.Start.z);
            _followCameraContainer.transform.localPosition = new Vector3(_followCameraContainer.transform.localPosition.x, _followCameraContainer.transform.localPosition.y - 0.025f, _followCameraContainer.transform.localPosition.z);
            _numSecondaryTasks--;
        }
        _containers.Clear();
        _containers.Add(firstCont);
    }

    public GameObject AddNewTaskOverview()
    {
        _numSecondaryTasks++;
        GameObject newOverview = Instantiate(Resources.Load(StringResources.Sid_TaskOverview_Container_path) as GameObject, _taskOverviewContainer.transform) ;
        newOverview.transform.localPosition = new Vector3(_taskOverviewContainer.transform.localPosition.x, _taskOverviewContainer.transform.localPosition.y - (0.07f * _numSecondaryTasks), _taskOverviewContainer.transform.localPosition.z);
        _overviewHandle.Start = new Vector3(_overviewHandle.Start.x, _overviewHandle.Start.y - 0.015f, _overviewHandle.Start.z);
        _followCameraContainer.transform.localPosition = new Vector3(_followCameraContainer.transform.localPosition.x, _followCameraContainer.transform.localPosition.y + 0.025f, _followCameraContainer.transform.localPosition.z);
        return newOverview;
    }
    #endregion

    #region Setting task overview active and inactive
    public void ToggleOverview()
    {
        if (!_followCameraContainer.activeSelf)
        {
            _overviewHandle.gameObject.SetActive(true);
            _followCameraContainer.SetActive(true);
            Center_of_Objs.Instance.SnapToCentroid();
        } else
        {
            _overviewHandle.gameObject.SetActive(false);
            _followCameraContainer.SetActive(false);
        }
    }
    #endregion
}
