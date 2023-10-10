using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskList 
{
    public string Name;
    public List<Step> Steps;
    public int CurrStepIndex;
    public int PrevStepIndex;
    public int NextStepIndex;
}
