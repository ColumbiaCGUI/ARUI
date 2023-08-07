using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManageStepFlashcardSolo : MonoBehaviour
{
    public TMP_Text TaskText;
    public ManageRequiredItems ReqItemsScript;

    public void InitializeFlashcard(Step currStep)
    {
        TaskText.SetText(currStep.StepDesc);
        ReqItemsScript.AddItems(currStep.SubSteps[currStep.CurrSubStepIndex].RequiredItems);
    }
}
