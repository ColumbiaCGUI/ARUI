using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupCurrTaskOverview : MonoBehaviour
{
    public ManageStepFlashcardMulti currFlashcard;
    public ManageStepFlashcardSolo prevFlashcard;
    public ManageStepFlashcardSolo nextFlashcard;
    public void SetupCurrTask(Step currStep)
    {
        currFlashcard.InitializeFlashcad(currStep);
    }

    public void SetupPrevTask(Step prevStep)
    {
        prevFlashcard.InitializeFlashcard(prevStep);
    }
    public void SetupNextTask(Step nextStep)
    {
        nextFlashcard.InitializeFlashcard(nextStep);
    }
}
