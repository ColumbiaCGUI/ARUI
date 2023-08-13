using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class MultipleListsContainer : MonoBehaviour
{
    public List<GameObject> itemsMenus;

    //List of containers for each of the current lists
    public List<TaskOverviewContainerRepo> containers;

    //Prefab for additional tasklists
    public GameObject SecondaryListPrefab;

    public Transform currOverviewParent;

    int currIndex = 0;

    [SerializeField]
    bool isMenu = false;


    private float delta;

    [SerializeField]
    private float disableDelay = 1.0f;

    void Update()
    {
        if (isMenu)
        {
            //if eye gaze not on task objects then do fade out currentindex
            if (EyeGazeManager.Instance.CurrentHit != EyeTarget.listmenuButton_tasks)
            {
                if (delta > disableDelay)
                {
                    StartCoroutine(FadeOut());
                }
                else
                {
                    delta += Time.deltaTime;
                }

            }
            //else fade back in currentindex 
        }
    }

    #region Setting menu active and inactive
    public void SetMenuActive(int index)
    {
        currIndex = index;
        for(int i = 0; i < itemsMenus.Count; i++)
        {
            if(i == index)
            {
                itemsMenus[i].SetActive(true);
            } else
            {
                CanvasGroup canvasGroup = itemsMenus[i].GetComponent<CanvasGroup>();
                canvasGroup.alpha = 1.0f;
                itemsMenus[i].SetActive(false);
            }
        }

    }
    private IEnumerator FadeOut()
    {
        GameObject canvas = itemsMenus[currIndex];
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        float counter = 0f;
        float duration = 1.0f;
        float startAlpha = 1.0f;
        float targetAlpha = 0.0f;
        bool broken = false;
        while (counter < duration)
        {
            if (EyeGazeManager.Instance.CurrentHit == EyeTarget.listmenuButton_tasks)
            {
                broken = true;
                break;
            }
            counter += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, counter / duration);

            yield return null;
        }
        if (!broken)
        {
            canvas.SetActive(false);
            canvasGroup.alpha = 1.0f;
            ResetOrb();
        }
        else
        {
            delta = 0.0f;
            canvasGroup.alpha = 1.0f;
            canvas.SetActive(true);
        }

    }
    #endregion

    #region Managing Orb
    public void AttachOrb()
    {
        Orb currOrb = GameObject.FindObjectOfType<Orb>();
        //currOrb.enabled = false;
        GameObject orb = currOrb.gameObject;
        //TODO: Change to current task object's thingy
        orb.transform.parent = currOverviewParent;
        //Change child position + rotation
        Transform child = orb.transform.GetChild(0);
        child.gameObject.GetComponent<OrbFollowerSolver>().enabled = false;
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.Euler(0f, 0f, 0f);
        //Change position + rotation
        orb.transform.localPosition = Vector3.zero;
        orb.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        //Disable box colldider 
        child.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    public void ResetOrb()
    {
        Orb currOrb = GameObject.FindObjectOfType<Orb>();
        GameObject orb = currOrb.gameObject;
        //TODO: Change to current task object's thingy
        orb.transform.parent = AngelARUI.Instance.transform;
        //Change child position + rotation
        Transform child = orb.transform.GetChild(0);
        child.gameObject.GetComponent<OrbFollowerSolver>().enabled = true;
        //Disable box colldider 
        child.gameObject.GetComponent<BoxCollider>().enabled = true;
    }
    #endregion

    //INCOMPLETE!!
    #region Managing task overview steps and recipes
    //TODO: COMPLETE!!!
    public void UpdateAllSteps(List<TaskList> tasks, int currTaskIndex)
    {
        for(int i = 0; i < tasks.Count; i++) { 
            if(i== currTaskIndex)
            {

            } else
            {

            }
        }
        //Clear all lists from currContainer (except for current one of course) 
        //For loop to go through all tasklists
        //Set up tasklist based on currTaskindex WE NEED A WAY TO ACCESS SetupCurrTaskOverview COMPONENTS!
        //IF CURRENTTASK INDEX ALSO UPDATE CENTER OBJECTS 
        //if not currtasklistindex then create a new object and set up based on task
    }

    public void AddNewTaskOverview()
    {
        //Take Main_TaskOverview_Container position and subtract 0.07 to the y value
        //Add 0.015 to line start y value
        //Increase multiple Menu 1 y position by 0.02

    }
    #endregion
}
