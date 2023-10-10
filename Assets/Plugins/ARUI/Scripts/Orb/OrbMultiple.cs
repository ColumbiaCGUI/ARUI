using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OrbMultiple : OrbMessage
{
    private List<Shapes.Disc> allPies = new List<Shapes.Disc>();
    private List<Shapes.Disc> allProgress = new List<Shapes.Disc>();
    private List<TextMeshProUGUI> allCurrentStepText = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> allCurrentStepTextProgress = new List<TextMeshProUGUI>();

    private string[] tasknameToIndex = new string[5] { "", "", "", "", "" };

    private float _minRadius = 0.0175f;
    private float _minThick = 0.005f;
    private float _maxRadius = 0.027f;
    private float _maxThick = 0.02f;
    private float _maxRadiusActive = 0.032f;
    private float _maxThickActive = 0.03f;

    private float _startDegRight = 23;
    private float _startDegLeft = 180;

    public void Awake()
    {
        messageType = OrbMessageType.multiple;
    }

    /// <summary>
    /// Init component, get reference to gameobjects from children
    /// </summary>
    void Start()
    {
        for (int i = 0; i < 5; i++)
            allPies.Add(transform.GetChild(0).GetChild(0).GetChild(0).GetChild(i).GetComponent<Shapes.Disc>());

        float deg = _startDegRight;
        foreach (Shapes.Disc ob in allPies)
        {
            ob.gameObject.SetActive(false);
            Shapes.Disc progressDisc = ob.transform.GetChild(0).GetComponent<Shapes.Disc>();
            allProgress.Add(progressDisc);
            progressDisc.Radius = 0;
            progressDisc.Thickness = 0;

            TextMeshProUGUI tm = ob.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            allCurrentStepText.Add(tm);
            allCurrentStepTextProgress.Add(progressDisc.transform.GetChild(0).GetComponent<TextMeshProUGUI>());

            ob.AngRadiansEnd = (deg) * Mathf.Deg2Rad;
            ob.AngRadiansStart = (deg - 21) * Mathf.Deg2Rad;
            progressDisc.AngRadiansEnd = (deg) * Mathf.Deg2Rad;
            progressDisc.AngRadiansStart = (deg - 5) * Mathf.Deg2Rad;
            deg += -23;

        }
    }

    public override void UpdateTaskList(Dictionary<string, TaskList> currentSelectedTasks)
    {
        if (allPies.Count == 0 || currentSelectedTasks.Count == 0 || currentSelectedTasks.Count > 5) return;

        int i = 0;
        foreach (string taskName in currentSelectedTasks.Keys)
        {
            int index = Utils.ArrayContainsKey(tasknameToIndex, taskName);
            if (index < 0)
            {
                tasknameToIndex[i] = taskName;
                allPies[i].gameObject.SetActive(true);
                allPies[i].gameObject.name = currentSelectedTasks[taskName].Name;

                TextMeshProUGUI tm = allPies[i].transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
                tm.text = currentSelectedTasks[taskName].Name + " : " +
                    currentSelectedTasks[taskName].Steps[currentSelectedTasks[taskName].CurrStepIndex].StepDesc;
            }

            i++;
        }

        for (int j = 0; j < tasknameToIndex.Length; i++)
        {
            if (tasknameToIndex[i].Equals(""))
                allPies[i].gameObject.SetActive(false);
            else
                allPies[i].gameObject.SetActive(true);
        }
    }

    public void Update()
    {
        UpdateAnchorInstant();
    }

    public void UpdateAnchorInstant()
    {
        float deg = _startDegRight;
        if (CurrentAnchor.Equals(MessageAnchor.left))
        {
            deg = _startDegLeft;
        }

        foreach (Shapes.Disc ob in allPies)
        {
            ob.AngRadiansEnd = (deg) * Mathf.Deg2Rad;
            ob.AngRadiansStart = (deg - 21) * Mathf.Deg2Rad;
            deg += -23;
        }

        foreach (Shapes.Disc ob in allProgress)
        {
            ob.AngRadiansEnd = (deg) * Mathf.Deg2Rad;
            ob.AngRadiansStart = (deg - 5) * Mathf.Deg2Rad;
            deg += -23;
        }
    }



    public void UpdateActiveStep(TaskList taskList)
    {

    }

    public void UpdateActiveTask(Dictionary<string, TaskList> manual, string activeTaskID)
    {
        for (int i = 0; i < 5; i++)
        {
            if (allPies[i].gameObject.name.Equals(activeTaskID))
            {
                allPies[i].Radius = 0.032f;
                allPies[i].Thickness = 0.03f;
            }
            else
            {
                allPies[i].Radius = 0.027f;
                allPies[i].Thickness = 0.02f;
            }
        }

        SetTaskMessage(manual[activeTaskID]);
    }

    public override List<BoxCollider> GetAllColliders()
    {
        throw new System.NotImplementedException();
    }

    public override string SetTaskMessage(TaskList currentTask)
    {
        //TextMeshProUGUI tm = allPies[i].transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        //tm.text = currentTask.Steps[currentTask.CurrStepIndex].StepDesc;

        //float ratio = (float)currentTask.CurrStepIndex / (float)(currentTask.Steps.Count - 1);

        //if (ratio == 0)
        //{
        //    allProgress[i].Radius = 0;
        //    allProgress[i].Thickness = 0;
        //}
        //else
        //{
        //    if (allPies[i].gameObject.name.Equals(activeTaskID))
        //    {
        //        allProgress[i].Radius = _minRadius + (ratio * (_maxRadiusActive - _minRadius));
        //        allProgress[i].Thickness = _minThick + (ratio * (_maxThickActive - _minThick));
        //    }
        //    else
        //    {
        //        allProgress[i].Radius = _minRadius + (ratio * (_maxRadius - _minRadius));
        //        allProgress[i].Thickness = _minThick + (ratio * (_maxThick - _minThick));
        //    }
        //}

        return "";
    }

    public override void AddNotification(NotificationType type, string message, OrbFace face) { }

    public override void RemoveNotification(NotificationType type, OrbFace face) { }

    public override void RemoveAllNotifications() { }

    public override void SetIsActive(bool active, bool newTask) { }

    public override bool IsInteractingWithBtn() => false;

    public override void SetFadeOutMessage(bool fade) { }
}
