using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//using Shapes??

public class ManageStepFlashcardMulti : MonoBehaviour
{
    public GameObject BorderRect;
    public TMP_Text ParentTaskText;
    public GameObject SubTaskPrefab;
    public GameObject VerticalLayoutGroupObj;
    public ManageRequiredItems ReqItemsScript;

    //Function that takes in task and then uses it to initiailize the list 
    //Also need to add all required items
    public void InitializeFlashcad(Step currStep)
    {
        ParentTaskText.SetText(currStep.StepDesc);
        int stepNum = 1;
        foreach(SubStep sub in currStep.SubSteps)
        {
            GameObject currSubtask = Instantiate(SubTaskPrefab, VerticalLayoutGroupObj.transform);
            currSubtask.GetComponent<SubTaskStep>().SetSubStepText(sub.StepDesc); 
            if(stepNum > 1)
            {
                //Increase rectangle border height and center
                //Increase box collider border height and center
            }
        }
        int currIndex = currStep.CurrSubStepIndex;
        ReqItemsScript.AddItems(currStep.SubSteps[currIndex].RequiredItems);
    }

    //Function to reset the list to original values (delete all subtasks, reset text) 
}
