using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Shapes;

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
        ResetSubTasks();
        ParentTaskText.SetText(currStep.StepDesc);
        ReqItemsScript.AddItems(currStep.SubSteps[currStep.CurrSubStepIndex].RequiredItems);
        for(int i = currStep.CurrSubStepIndex; i < currStep.SubSteps.Count; i++)
        {
            SubStep sub = currStep.SubSteps[i];
            GameObject currSubtask = Instantiate(SubTaskPrefab, VerticalLayoutGroupObj.transform);
            currSubtask.GetComponent<SubTaskStep>().SetSubStepText(sub.StepDesc); 
            //Increase rectangle border height and center
            BorderRect.GetComponent<Rectangle>().Height += 0.03f;
            RectTransform rect = BorderRect.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - 0.015f);
            //Increase box collider border height and center
            BoxCollider collider = VerticalLayoutGroupObj.GetComponent<BoxCollider>();
            collider.center = new Vector3(collider.center.x, collider.center.y - 0.015f, collider.center.z);
            collider.size = new Vector3(collider.size.x, collider.size.y + 0.03f, collider.size.z);
        }
        int currIndex = currStep.CurrSubStepIndex;
        ReqItemsScript.AddItems(currStep.SubSteps[currIndex].RequiredItems);
    }


    //Function to reset the list to original values (delete all subtasks, rescale rects)
    public void ResetSubTasks()
    {
        foreach (Transform child in VerticalLayoutGroupObj.transform)
        {
            Destroy(child.gameObject);
            //Decrease rectangle border height and center
            BorderRect.GetComponent<Rectangle>().Height -= 0.03f;
            RectTransform rect = BorderRect.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + 0.015f);
            //Fecrease box collider border height and center
            BoxCollider collider = VerticalLayoutGroupObj.GetComponent<BoxCollider>();
            collider.center = new Vector3(collider.center.x, collider.center.y + 0.015f, collider.center.z);
            collider.size = new Vector3(collider.size.x, collider.size.y - 0.03f, collider.size.z);
        }
    }
}
