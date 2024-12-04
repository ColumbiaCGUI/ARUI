using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Label : MonoBehaviour
{
    public TextMeshProUGUI TextContainer;

    // Update is called once per frame
    void Update()
    {
        if (TextContainer == null)
        {
            TextContainer = GetComponentInChildren<TextMeshProUGUI>();
        }
    }
}
