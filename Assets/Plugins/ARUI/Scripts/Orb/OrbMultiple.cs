using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class OrbMultiple : OrbMessage
{
    private List<OrbPie> _allPies = new List<OrbPie>();

    private OrbPie _nextAvailablePie;
    private int _nextAvailablePieIndex = 0;

    private Dictionary<string, OrbPie> taskNameToOrbPie;

    private OrbPie _currentActivePie = null;

    private bool _enalbed = false;
    public override void SetEnabled(bool enabled)
    {
        _enalbed = enabled;
        transform.GetChild(0).gameObject.SetActive(enabled);
    }

    /// <summary>
    /// Init component, get reference to gameobjects from children
    /// </summary>
    public override void InitializeComponents()
    {
        messageType = OrbMessageType.multiple;

        for (int i = 0; i < 5; i++)
        {
            GameObject ob = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(i).gameObject;
            OrbPie current = ob.AddComponent<OrbPie>();
            _allPies.Add(current);
        }

        _nextAvailablePie = _allPies[_nextAvailablePieIndex];
        taskNameToOrbPie = new Dictionary<string, OrbPie>();
    }

    public override void UpdateTaskList(Dictionary<string, TaskList> currentSelectedTasks)
    {
        if (currentSelectedTasks.Count == 0 || currentSelectedTasks.Count > 5) return;

        int i = 0;
        foreach (string taskName in currentSelectedTasks.Keys)
        {
            if (!taskNameToOrbPie.ContainsKey(taskName))
            {
                taskNameToOrbPie.Add(taskName, _nextAvailablePie);
                _nextAvailablePie.TaskName = taskName;
                _nextAvailablePie.gameObject.name = currentSelectedTasks[taskName].Name;

                _nextAvailablePie.SetTaskMessage(currentSelectedTasks[taskName].Name + " : " +
                    currentSelectedTasks[taskName].Steps[currentSelectedTasks[taskName].CurrStepIndex].StepDesc);

                if (_currentActivePie == null)
                    _currentActivePie = _nextAvailablePie;

                _nextAvailablePieIndex++;
                _nextAvailablePie = _allPies[_nextAvailablePieIndex];
            }

            i++;
        }

        foreach (OrbPie pie in _allPies)
            pie.SetActive(taskNameToOrbPie.ContainsValue(pie));

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
        foreach (OrbPie pie in _allPies)
            pie.UpdateSlice(activeTaskID);

        _currentActivePie = taskNameToOrbPie[activeTaskID];
        SetTaskMessage(manual[activeTaskID]);
    }

    #endregion

    #region Update UI

    public void UpdateAnchorInstant(MessageAnchor anchor)
    {
        //Debug.Log("Update anchor: " + anchor);
        //float deg = _startDegRight;
        //if (anchor.Equals(MessageAnchor.left))
        //{
        //    deg = _startDegLeft;
        //}

        //CurrentAnchor = anchor;

        //foreach (Shapes.Disc ob in _allPies)
        //{
        //    ob.AngRadiansEnd = (deg) * Mathf.Deg2Rad;
        //    ob.AngRadiansStart = (deg - 21) * Mathf.Deg2Rad;
        //    deg += -23;
        //}

        //foreach (Shapes.Disc ob in allProgress)
        //{
        //    ob.AngRadiansEnd = (deg) * Mathf.Deg2Rad;
        //    ob.AngRadiansStart = (deg - 5) * Mathf.Deg2Rad;
        //    deg += -23;
        //}
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
        foreach (OrbPie pie in _allPies)
        {
            pieColliders.AddRange(pie.GetComponentsInChildren<BoxCollider>());
        }

        return pieColliders;
    }

    public override string SetTaskMessage(TaskList currentTask)
    {
        taskNameToOrbPie[currentTask.Name].SetTaskMessage(currentTask.Steps[currentTask.CurrStepIndex].StepDesc);

        float ratio = (float)currentTask.CurrStepIndex / (float)(currentTask.Steps.Count - 1);
        taskNameToOrbPie[currentTask.Name].UpdateProgressbar(ratio, currentTask.Name);

        return "";
    }

    public override void AddNotification(NotificationType type, string message, OrbFace face) { }

    public override void RemoveNotification(NotificationType type, OrbFace face) { }

    public override void RemoveAllNotifications() { }

    public override void SetIsActive(bool active, bool newTask) { }

    public override bool IsInteractingWithBtn() => false;
}
