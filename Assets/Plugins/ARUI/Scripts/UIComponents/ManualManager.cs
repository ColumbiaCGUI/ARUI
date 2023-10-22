using System.Collections.Generic;
using UnityEngine;

public class ManualManager : Singleton<ManualManager>
{
    private bool _manualInitialized = false;

    private bool _menuActive = false;
    public bool MenuActive
    {
        get => _menuActive;
    }

    private Dictionary<string, DwellButton> allTaskBtns;
    private GameObject _okayButton;

    // Start is called before the first frame update
    private void Start()
    {
        allTaskBtns = new Dictionary<string, DwellButton>();
        for (int i = 0; i<5; i++)
        {
            DwellButton btn = transform.GetChild(i).gameObject.AddComponent<DwellButton>();
            btn.InitializeButton(EyeTarget.menuBtn, () => Debug.Log("MenuBtn pressed"), null, false, DwellButtonType.Toggle, true);
            allTaskBtns.Add(btn.gameObject.name.Substring(0, btn.gameObject.name.LastIndexOf('_')), btn);
        }

        DwellButton okayBtn = transform.GetChild(5).gameObject.AddComponent<DwellButton>();
        _okayButton = okayBtn.gameObject;
        okayBtn.InitializeButton(EyeTarget.menuBtn, () => SubmitSelection(), null, true, DwellButtonType.Select, true);

        _okayButton.gameObject.SetActive(false);
        foreach (DwellButton btn in allTaskBtns.Values)
        {
            btn.Toggled = false;
            btn.gameObject.SetActive(false);
        }
    }

    public void SetMenuActive(bool isActive)
    {
        if (!_manualInitialized) return;

        _menuActive = isActive;
        _okayButton.gameObject.SetActive(isActive);
        foreach (DwellButton btn in allTaskBtns.Values)
        {
            btn.Toggled = false;
            btn.gameObject.SetActive(isActive);
        }
    }

    public void SetManual(List<string> manual)
    {
        if (_manualInitialized) return;

        _manualInitialized = true;
        DataProvider.Instance.InitManual(manual);

        foreach (string btnNames in allTaskBtns.Keys)
            allTaskBtns[btnNames].IsDisabled = !manual.Contains(btnNames);
    }



    private void Update()
    {
        if (!_menuActive || !_manualInitialized) return;

        int toggledCount = 0;
        foreach (DwellButton btn in allTaskBtns.Values)
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
        foreach (DwellButton btn in allTaskBtns.Values)
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
