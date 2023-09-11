using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//TOBE: DEPREICATED!!

public class SetupTaskOverview : MonoBehaviour
{
    public SetupCurrTaskOverview currOverview;
    public GameObject OverviewObj;
    public TMP_Text tasknametxt;
    public MultipleListsContainer currContainer;
    public void SetupCurrentStep(TaskList list, string taskname)
    {
        tasknametxt.SetText(taskname);
        //OverviewObj.SetActive(true);
        if (list != null)
        {
            if (list.CurrStepIndex != -1)
            {
                currOverview.SetupCurrTask(list.Steps[list.CurrStepIndex], this.GetComponent<Center_of_Objs>());
            }
            if (list.NextStepIndex != -1)
            {
                currOverview.SetupNextTask(list.Steps[list.NextStepIndex]);
            }
            if (list.PrevStepIndex != -1)
            {
                currOverview.SetupPrevTask(list.Steps[list.PrevStepIndex]);
            }
        }
        //OverviewObj.SetActive(false);
    }
}
