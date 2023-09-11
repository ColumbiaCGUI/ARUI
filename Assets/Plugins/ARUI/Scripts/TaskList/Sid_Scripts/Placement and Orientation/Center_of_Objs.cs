using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using System.Diagnostics;

public class Center_of_Objs : MonoBehaviour
{
    // Debug only
    public List<GameObject> objs;

    public GameObject LinePrefab;
    public GameObject ListPrefab;

    public float movementSpeed = 1.0f;

    public float xOffset;
    public float yOffset;
    public float zOffset;
    public float heightOffset;

    Dictionary<string, GameObject> objsDict = new Dictionary<string, GameObject>();
    Dictionary<string, GameObject> linesDict = new Dictionary<string, GameObject>();

    bool isLooking = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (isLooking)
        {
            this.GetComponent<MultipleListsContainer>().OverviewLine.gameObject.SetActive(false);
            int currIndex = this.GetComponent<MultipleListsContainer>().currIndex;
            if (currIndex == 0)
            {
                foreach (KeyValuePair<string, GameObject> pair in objsDict)
                {
                    UpdateLines(pair.Key, pair.Value);
                }
            } else
            {
                DeactivateLines();
            }
            Vector3 finalPos = Camera.main.transform.position + Camera.main.transform.forward * zOffset + Camera.main.transform.right * xOffset + Camera.main.transform.up * yOffset;
            this.transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * movementSpeed);
        } else
        {
            this.GetComponent<MultipleListsContainer>().OverviewLine.gameObject.SetActive(true);
            DeactivateLines();
            Vector3 centroid = new Vector3(0, 0, 0);
            foreach (KeyValuePair<string, GameObject> pair in objsDict)
            {
                centroid += pair.Value.transform.position;
            }
            centroid = centroid / objsDict.Count;
            this.GetComponent<MultipleListsContainer>().SetLineEnd(centroid);
            Vector3 finalPos = new Vector3(centroid.x, Camera.main.transform.position.y + heightOffset, centroid.z);
            this.transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * movementSpeed);
        }

    }

    public void RemoveObj(string key)
    {
        objs.Remove(objsDict[key]);
        objsDict.Remove(key);
        Destroy(linesDict[key]);
        linesDict.Remove(key);
    }

    public void ClearObjs()
    {
        objsDict.Clear();
        objs.Clear();
        foreach (KeyValuePair<string, GameObject> line in linesDict)
        {
            Destroy(line.Value);
        }
        linesDict.Clear();
    }

    public void AddObj(string key, GameObject obj)
    {
        objsDict.Add(key, obj);
        objs.Add(obj);
        GameObject pointerObj = Instantiate(LinePrefab);
        pointerObj.name = key;
        linesDict.Add(key, pointerObj);
        Line pointer = pointerObj.GetComponent<Line>();
        pointer.Start = transform.position;
        pointer.End = obj.transform.position;
    }

    public void DeactivateLines()
    {
        foreach(KeyValuePair<string, GameObject> pair in linesDict)
        {
            pair.Value.SetActive(false);
        }
    }

    public void UpdateLines(string key, GameObject obj)
    {
        if (key != "MainCam")
        {
            linesDict[key].SetActive(true);
            Line pointer = linesDict[key].GetComponent<Line>();
            //pointer.Start = transform.position;
            pointer.End = obj.transform.position;
        }
    }

    public void SetLineStart(string key, Vector3 StartPos)
    {
        Line pointer = linesDict[key].GetComponent<Line>();
        pointer.Start = StartPos;
    }

    public void SetIsLooking(bool val)
    {
        isLooking = val;
    }



}