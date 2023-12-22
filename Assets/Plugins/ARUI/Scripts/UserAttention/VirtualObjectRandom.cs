using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class VirtualObjectRandom : MonoBehaviour
{
    // Name of this object
    private string objectNum;

    private void Start()
    {
        int firstIndex = gameObject.name.IndexOf('(');
        int secondIndex = gameObject.name.LastIndexOf(")");
        objectNum = gameObject.name.Substring(firstIndex, secondIndex - firstIndex + 1);

        string newName = "Object " + objectNum;

        int min = 10000000; // Minimum for 8-digit number
        int max = 99999999; // Maximum for 8-digit number

        // Random.Range is inclusive for the minimum value and exclusive for the maximum value.
        string newDesc = "" + UnityEngine.Random.Range(min, max + 1);
        

        Transform annotaion = transform.Find("Annotation");
        annotaion.Find("Brief").Find("Canvas").Find("Name").GetComponent<TextMeshProUGUI>().text = newName;
        //annotaion.Find("Brief").Find("Canvas").Find("Name").GetComponent<TextMeshProUGUI>().color = Color.green;
        Transform annotationDetailCanvas = annotaion.Find("Detail").Find("Canvas");
        annotationDetailCanvas.Find("Name").GetComponent<TextMeshProUGUI>().text = newName;
        //annotationDetailCanvas.Find("Name").GetComponent<TextMeshProUGUI>().color = Color.green;
        annotationDetailCanvas.Find("Description").GetComponent<TextMeshProUGUI>().text = newDesc;
        //annotationDetailCanvas.Find("Description").GetComponent<TextMeshProUGUI>().color = Color.green;
    }
}
