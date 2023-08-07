using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageRequiredItems : MonoBehaviour
{
    public GameObject BorderRectObj;
    public GameObject ItemTextPrefab;
    public GameObject VerticalLayoutGroupObj;

    //fucntion to add items
    public void AddItems(List<string> items)
    {
        foreach(string item in items)
        {
            Instantiate(ItemTextPrefab, VerticalLayoutGroupObj.transform);
        }
    }

    //function to clear all items
}
