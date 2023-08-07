using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
            GameObject currItem = Instantiate(ItemTextPrefab, VerticalLayoutGroupObj.transform);
            currItem.GetComponent<TMP_Text>().SetText(item);
        }
    }

    //function to clear all items
}
