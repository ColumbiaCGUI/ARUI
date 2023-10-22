using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualManager : Singleton<ManualManager>
{
    private List<DwellButton> allButtons;

    private bool _menuActive = false;
    private GameObject _okayButton;

    // Start is called before the first frame update
    private void Start()
    {
        allButtons = new List<DwellButton>();
        for (int i = 0; i<5; i++)
        {
            DwellButton bn = transform.GetChild(i).gameObject.AddComponent<DwellButton>();
            allButtons.Add(bn);

            bn.InitializeButton(EyeTarget.menuBtn, () => Debug.Log("MenuBtn pressed"), null, false, DwellButtonType.Toggle, true);
        }

        DwellButton okayBtn = transform.GetChild(5).gameObject.AddComponent<DwellButton>();
        _okayButton = okayBtn.gameObject;
        okayBtn.InitializeButton(EyeTarget.menuBtn, () => SubmitSelection(), null, true, DwellButtonType.Select, true);

        DataProvider.Instance.InitManual(new List<string> { "Pinwheels", "Coffee", "Oatmeal", "Quesadilla", "Tea" });
        _menuActive = true;
    }

    private void Update()
    {
        if (!_menuActive) return;

        int toggledCount = 0;
        foreach (DwellButton btn in allButtons)
        {
            if (btn.Toggled)
                toggledCount++;
        }

        if (toggledCount == 0 && _okayButton.gameObject.activeSelf)
        {
            _okayButton.gameObject.SetActive(false);
        } else if (toggledCount > 0 && !_okayButton.gameObject.activeSelf)
        {
            _okayButton.gameObject.SetActive(true);
        } 
    }

    /// <summary>
    /// 
    /// </summary>
    private void SubmitSelection()
    {
        List<string> allToggled = new List<string>();
        foreach (DwellButton btn in allButtons)
        {
            if (btn.Toggled) 
                allToggled.Add(btn.gameObject.name.Substring(0, btn.gameObject.name.LastIndexOf('_')));
        }

        if (allToggled.Count > 0) {
            DataProvider.Instance.SetSelectedTasks(allToggled);
            _menuActive = false;

            for (int i = 0; i < 6; i++)
                transform.GetChild(i).gameObject.SetActive(false);
        } else
            Debug.Log("Nothing selected");
    }
}
