using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InFrontOfObj : MonoBehaviour
{
    public GameObject frontObject;
    public float distance;
    float currY;
    float currX;

    private void Start()
    {
        currY = frontObject.transform.position.y;
    }

    void Update()
    {
        Vector3 FinalPosition = Camera.main.transform.position + Camera.main.transform.forward * distance;
        FinalPosition.y = currY;
        frontObject.transform.position = FinalPosition;

    }
}
