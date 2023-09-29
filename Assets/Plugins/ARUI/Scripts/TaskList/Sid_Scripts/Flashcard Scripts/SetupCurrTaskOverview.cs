using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles setting up the text + required items for the flashcards 

public class SetupCurrTaskOverview : MonoBehaviour
{
    public ManageStepFlashcardMulti currFlashcardMulti;
    public ManageStepFlashcardSolo currFlashcardSolo;
    public ManageStepFlashcardSolo prevFlashcard;
    public ManageStepFlashcardSolo nextFlashcard;
    public void SetupCurrTask(Step currStep, Center_of_Objs centerScript = null)
    {
        List<string> reqList;
        if (currStep.SubSteps.Count > 0)
        {
            reqList = currStep.SubSteps[currStep.CurrSubStepIndex].RequiredItems;
            currFlashcardMulti.gameObject.SetActive(true);
            currFlashcardSolo.gameObject.SetActive(false);
            currFlashcardMulti.InitializeFlashcad(currStep);
        } else
        {
            currFlashcardMulti.gameObject.SetActive(false);
            currFlashcardSolo.gameObject.SetActive(true);
            reqList = currStep.RequiredItems;
            currFlashcardSolo.InitializeFlashcard(currStep);
        }
        if (centerScript != null)
        {
            centerScript.ClearObjs();
            foreach (string str in reqList)
            {
                centerScript.AddObj(str);
            }
        }
    }

    public void SetupPrevTask(Step prevStep)
    {
        prevFlashcard.gameObject.SetActive(true);
        prevFlashcard.InitializeFlashcard(prevStep);
    }

    public void DeactivatePrevTask()
    {
        prevFlashcard.gameObject.SetActive(false);
    }

    public void SetupNextTask(Step nextStep)
    {
        nextFlashcard.gameObject.SetActive(true);
        nextFlashcard.InitializeFlashcard(nextStep);
    }

    public void DeactivateNextTask()
    {
        nextFlashcard.gameObject.SetActive(false);
    }
}
