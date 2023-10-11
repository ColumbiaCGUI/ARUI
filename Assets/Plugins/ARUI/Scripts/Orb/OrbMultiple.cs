using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Collections;

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

    private string _currentActiveTask = "";

    public void Awake()
    {
        messageType = OrbMessageType.multiple;
    }

    private bool _enalbed = false;
    public override void SetEnabled(bool enabled)
    {
        _enalbed = enabled;
        transform.GetChild(0).gameObject.SetActive(enabled);
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
        if (currentSelectedTasks.Count == 0 || currentSelectedTasks.Count > 5) return;

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

            if (_currentActiveTask.Equals(""))
                _currentActiveTask = taskName;

            i++;
        }

        for (int j = 0; j < tasknameToIndex.Length; j++)
        {
            if (tasknameToIndex[j].Equals(""))
                allPies[j].gameObject.SetActive(false);

            else
                allPies[j].gameObject.SetActive(true);
        }
    }

    private new void Update()
    {
        base.Update();
        if (!IsMessageVisible || IsMessageLerping) return;

        // Update messagebox anchor
        if (ChangeMessageBoxToRight(100))
            UpdateAnchorInstant(MessageAnchor.right);

        else if (ChangeMessageBoxToLeft(100))
            UpdateAnchorInstant(MessageAnchor.left);
    }

    #region Update Data

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

        _currentActiveTask = activeTaskID;
        SetTaskMessage(manual[activeTaskID]);
    }

    #endregion

    #region Update UI

    public void UpdateAnchorInstant(MessageAnchor anchor)
    {
        Debug.Log("Update anchor: " + anchor);
        float deg = _startDegRight;
        if (anchor.Equals(MessageAnchor.left))
        {
            deg = _startDegLeft;
        }

        CurrentAnchor = anchor;

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

    /// <summary>
    /// Turn on or off message fading
    /// </summary>
    /// <param name="active"></param>
    public override void SetFadeOutMessage(bool active)
    {
        if (active)
        {
            StartCoroutine(FadeOutMessage());
        }
        else
        {
            StopCoroutine(FadeOutMessage());
            IsMessageFading = false;
        }
    }

    /// <summary>
    /// Fade out message from the moment the user does not look at the message anymore
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOutMessage()
    {
        float fadeOutStep = 0.001f;
        IsMessageFading = true;

        yield return new WaitForSeconds(1.0f);

        float shade = ARUISettings.OrbMessageBGColor.r;
        float alpha = 1f;

        yield return new WaitForEndOfFrame();

        IsMessageFading = false;

        if (shade <= 0)
        {
            SetIsActive(false, false);
            IsMessageVisible = false;
        }
    }

    #endregion


    public override List<BoxCollider> GetAllColliders()
    {
        //throw new System.NotImplementedException();
        var pieColliders = new List<BoxCollider>();
        foreach (Shapes.Disc pie in allPies)
        {
            pieColliders.Add(pie.GetComponentInChildren<BoxCollider>());
        }

        return pieColliders;
    }

    public override string SetTaskMessage(TaskList currentTask)
    {
        int index = Utils.ArrayContainsKey(tasknameToIndex, currentTask.Name);
        TextMeshProUGUI tm = allPies[index].transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        tm.text = currentTask.Steps[currentTask.CurrStepIndex].StepDesc;
        
        float ratio = (float)currentTask.CurrStepIndex / (float)(currentTask.Steps.Count - 1);

        if (ratio == 0)
        {
            allProgress[index].Radius = 0;
            allProgress[index].Thickness = 0;
        }
        else
        {
            if (allPies[index].gameObject.name.Equals(_currentActiveTask))
            {
                allProgress[index].Radius = _minRadius + (ratio * (_maxRadiusActive - _minRadius));
                allProgress[index].Thickness = _minThick + (ratio * (_maxThickActive - _minThick));
            }
            else
            {
                allProgress[index].Radius = _minRadius + (ratio * (_maxRadius - _minRadius));
                allProgress[index].Thickness = _minThick + (ratio * (_maxThick - _minThick));
            }
        }

        foreach (var pie in allPies)
        {
            if (pie.gameObject.gameObject.name.Equals(_currentActiveTask))
                allCurrentStepText[index].gameObject.SetActive(true);
            else
                allCurrentStepText[index].gameObject.SetActive(false);
        }

        return "";
    }

    public override void AddNotification(NotificationType type, string message, OrbFace face) { }

    public override void RemoveNotification(NotificationType type, OrbFace face) { }

    public override void RemoveAllNotifications() { }

    public override void SetIsActive(bool active, bool newTask) { }

    public override bool IsInteractingWithBtn() => false;
}
