using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Shapes;

public class ManageRequiredItems : MonoBehaviour
{
    public GameObject BorderRectObj;
    public GameObject ItemTextPrefab;
    public GameObject VerticalLayoutGroupObj;
    public float RectIncrease = 0.05f;
    public float ColIncrease = 0.05f;

    //fucntion to add items
    public void AddItems(List<string> items)
    {
        foreach(string item in items)
        {
            GameObject currItem = Instantiate(ItemTextPrefab, VerticalLayoutGroupObj.transform);
            currItem.GetComponent<TMP_Text>().SetText(item);
            //Increase border rect size
            BorderRectObj.GetComponent<Rectangle>().Height += RectIncrease;
            //Increase size of collider
            BoxCollider col = this.GetComponent<BoxCollider>();
            col.size = new Vector3(col.size.x, col.size.y + ColIncrease, col.size.z);


        }
    }

    //function to clear all items
    public void ClearAll()
    {
        foreach (Transform child in VerticalLayoutGroupObj.transform)
        {
            Destroy(child);
            //Decrease border rect size
            BorderRectObj.GetComponent<Rectangle>().Height -= RectIncrease;
            //Decrease size of collider
            BoxCollider col = this.GetComponent<BoxCollider>();
            col.size = new Vector3(col.size.x, col.size.y - ColIncrease, col.size.z);
        }
    }
}
