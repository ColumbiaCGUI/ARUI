using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupTaskOverview : MonoBehaviour
{
    public SetupCurrTaskOverview currOverview;
    public GameObject OverviewObj;
    public void SetupCurrentStep(TaskList list)
    {
        OverviewObj.SetActive(true);
        if (list != null)
        {
            if (list.CurrStepIndex != -1)
            {
                currOverview.SetupCurrTask(list.Steps[list.CurrStepIndex]);
            } 
            if(list.NextStepIndex != -1)
            {
                currOverview.SetupNextTask(list.Steps[list.NextStepIndex]);
            }
            if (list.PrevStepIndex != -1)
            {
                currOverview.SetupPrevTask(list.Steps[list.PrevStepIndex]);
            }
        }
        OverviewObj.SetActive(false);
    }
}
