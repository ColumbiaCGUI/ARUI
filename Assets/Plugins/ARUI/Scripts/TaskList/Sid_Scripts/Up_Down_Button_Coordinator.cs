using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Up_Down_Button_Coordinator : MonoBehaviour
{
    public List<GameObject> taskmenus = new List<GameObject>();
    public GameObject upButton;
    public GameObject downButton;
    public GameObject resetButton;

    [SerializeField]
    int currIndex = 0;
    [SerializeField]
    int taskIndex = 0;
    // Start is called before the first frame update
    void Awake()
    {

        upButton.SetActive(true);
        DwellButton upDwell = upButton.AddComponent<DwellButton>();
        upDwell.InitializeButton(EyeTarget.listmenuButton_tasks, () => this.ActivatePrevMenu(), null, true, DwellButtonType.Toggle, true);
        if (currIndex == 0)
        {
            upButton.SetActive(false);
        }
        DwellButton downDwell = downButton.AddComponent<DwellButton>();
        downDwell.InitializeButton(EyeTarget.listmenuButton_tasks, () => this.ActivateNextMenu(),
        null, true, DwellButtonType.Toggle, true);
        resetButton.SetActive(true);
        DwellButton resetDwell = resetButton.AddComponent<DwellButton>();
        resetDwell.InitializeButton(EyeTarget.listmenuButton_tasks, () => this.Reset(), null, true, DwellButtonType.Toggle, true);
        resetButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActivateNextMenu()
    {
        taskmenus[currIndex].SetActive(false);
        currIndex++;
        taskmenus[currIndex].SetActive(true);
        if(currIndex != taskIndex)
        {
            resetButton.SetActive(true);
        }
        else
        {
            resetButton.SetActive(false);
        }
        if(currIndex == taskmenus.Count - 1)
        {
            downButton.SetActive(false);
        } else
        {
            downButton.SetActive(true);
        }
        if (currIndex == 1)
        {
            upButton.SetActive(true);
        }
    }

    public void ActivatePrevMenu()
    {
        taskmenus[currIndex].SetActive(false);
        currIndex--;
        taskmenus[currIndex].SetActive(true);
        if (currIndex != taskIndex)
        {
            resetButton.SetActive(true);
        }
        else
        {
            resetButton.SetActive(false);
        }
        if (currIndex == 0)
        {
            upButton.SetActive(false);
        }
        else
        {
            upButton.SetActive(true);
        }
        if (currIndex == taskmenus.Count - 2)
        {
            downButton.SetActive(true);
        }

    }

    public void Reset()
    {
        taskmenus[currIndex].SetActive(false);
        taskmenus[taskIndex].SetActive(true);
        currIndex = taskIndex;
        resetButton.SetActive(false);
        if (currIndex == 0)
        {
            upButton.SetActive(false);
        }
        else
        {
            upButton.SetActive(true);
        }
        if (currIndex == taskmenus.Count - 1)
        {
            downButton.SetActive(false);
        }
        else
        {
            downButton.SetActive(true);
        }
       
    }

}
