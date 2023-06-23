using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Up_Down_Button_Coordinator : MonoBehaviour
{
    public List<GameObject> taskmenus = new List<GameObject>();
    public GameObject upButton;
    public GameObject downButton;

    [SerializeField]
    int currIndex = 0;
    // Start is called before the first frame update
    void Awake()
    {
        DwellButton upDwell = upButton.AddComponent<DwellButton>();
        upDwell.InitializeButton(EyeTarget.upButton, () => this.ActivatePrevMenu(), null, true, DwellButtonType.Toggle);
        DwellButton downDwell = downButton.AddComponent<DwellButton>();
        downDwell.InitializeButton(EyeTarget.downButton, () => this.ActivateNextMenu(),
        null, true, DwellButtonType.Toggle);
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
        if(currIndex >= taskmenus.Count)
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

}
