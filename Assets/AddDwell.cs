using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AddDwell : Singleton<AddDwell>
{
    public List<GameObject> DwellBtns = new List<GameObject>();

    public List<GameObject> DwellMenus = new List<GameObject>();
    // Start is called before the first frame update
    void Awake()
    {
        for(int i = 0; i < DwellBtns.Count; i++) 
        {
            DwellButton currdwell = DwellBtns[i].AddComponent<DwellButton>();
            currdwell.InitializeButton(EyeTarget.orbtasklistButton, () => AddDwell.Instance.SetDwellMenuActive(i - 1),
            null, true, DwellButtonType.Toggle);
        }
    }

    // Update is called once per frame
    void Update()
    {
   
    }

    public void SetDwellMenuActive(int index)
    {
        UnityEngine.Debug.Log(index);
        DwellMenus[index].SetActive(true);
    }
}
