using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OrbTaskList : MonoBehaviour
{
    private List<Shapes.Disc> allPies = new List<Shapes.Disc>();
    private List<Shapes.Disc> allProgress = new List<Shapes.Disc>();

    private List<TextMeshProUGUI> allCurrentStepText = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> allCurrentStepTextProgress = new List<TextMeshProUGUI>();

    private float _minRadius = 0.0175f;
    private float _minThick = 0.005f;
    private float _maxRadius = 0.027f;
    private float _maxThick = 0.02f;
    private float _maxRadiusActive = 0.032f;
    private float _maxThickActive = 0.03f;

    private float _startDegRight = 23;
    private float _startDegLeft = 23;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 5; i++)
            allPies.Add(transform.GetChild(0).GetChild(0).GetChild(0).GetChild(i).GetComponent<Shapes.Disc>());

        float deg = _startDegRight;
        foreach (Shapes.Disc ob in allPies)
        {
            ob.gameObject.SetActive(false);
            Shapes.Disc progressDisc = ob.transform.GetChild(0).GetComponent<Shapes.Disc>();
            allProgress.Add(progressDisc);
            progressDisc.Radius = 0;
            progressDisc.Thickness = 0;

            TextMeshProUGUI tm = ob.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            allCurrentStepText.Add(tm);
            allCurrentStepTextProgress.Add(progressDisc.transform.GetChild(0).GetComponent<TextMeshProUGUI>());

            ob.AngRadiansEnd = (deg) * Mathf.Deg2Rad;
            ob.AngRadiansStart = (deg - 21) * Mathf.Deg2Rad;
            progressDisc.AngRadiansEnd = (deg) * Mathf.Deg2Rad;
            progressDisc.AngRadiansStart = (deg-5) * Mathf.Deg2Rad;
            deg += -23;

        }
    }

    public void AddTask(Dictionary<string, TaskList> manual)
    {
        if (allPies.Count == 0) return;

        int i = 0;
        foreach (string ss in manual.Keys)
        { 
            allPies[i].gameObject.SetActive(true);
            allPies[i].gameObject.name = manual[ss].Name;

            TextMeshProUGUI tm = allPies[i].transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            tm.text = manual[ss].Steps[manual[ss].CurrStepIndex].StepDesc;

            i++;
        }

        for (int j = i; j<5; j++)
        {
            allPies[i].gameObject.SetActive(false);
        }
    }

    public void UpdateActiveStep(Dictionary<string, TaskList> manual, string activeTaskID)
    {
        int i = 0;
        foreach (string ss in manual.Keys)
        {
            TextMeshProUGUI tm = allPies[i].transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            tm.text = manual[ss].Steps[manual[ss].CurrStepIndex].StepDesc;

            float ratio = (float)manual[ss].CurrStepIndex / (float)(manual[ss].Steps.Count - 1);

            if (ratio == 0)
            {
                allProgress[i].Radius = 0;
                allProgress[i].Thickness = 0;
            }
            else { 
            if (allPies[i].gameObject.name.Equals(activeTaskID))
            {

                allProgress[i].Radius = _minRadius + (ratio * (_maxRadiusActive - _minRadius));
                allProgress[i].Thickness = _minThick + (ratio * (_maxThickActive - _minThick));
            }
            else
            {
                allProgress[i].Radius = _minRadius + (ratio * (_maxRadius - _minRadius));
                allProgress[i].Thickness = _minThick + (ratio * (_maxThick - _minThick));
            }
            }

            i++;
        }
    }

    public void UpdateActiveTask(Dictionary<string, TaskList> manual, string activeTaskID)
    {
        for (int i = 0; i < 5; i++)
        {
            if (allPies[i].gameObject.name.Equals(activeTaskID))
            {
                allPies[i].Radius = 0.032f;
                allPies[i].Thickness = 0.03f;
            }
            else
            {
                allPies[i].Radius = 0.027f;
                allPies[i].Thickness = 0.02f;
            }
        }

        UpdateActiveStep(manual, activeTaskID);
    }
}
