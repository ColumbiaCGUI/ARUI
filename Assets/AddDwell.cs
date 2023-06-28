using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AddDwell : Singleton<AddDwell>
{
    public GameObject DwellBtn;

    public GameObject DwellMenu;
    // Start is called before the first frame update
    void Awake()
    {

        DwellButton currdwell = DwellBtn.AddComponent<DwellButton>();
        currdwell.InitializeButton(EyeTarget.orbtasklistButton, () => this.SetDwellMenuActive(),
        null, true, DwellButtonType.Toggle, true);

    }

    // Update is called once per frame
    void Update()
    {
   
    }

    public void SetDwellMenuActive()
    {
        DwellMenu.SetActive(true);
    }
}
