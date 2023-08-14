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

    public float movementSpeed = 1.0f;

    public float zOffset = 0.5f;
    // Debug end

    Dictionary<string, GameObject> objsDict = new Dictionary<string, GameObject>();
    Dictionary<string, GameObject> linesDict = new Dictionary<string, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 centroid = new Vector3(0, 0, 0);
        foreach(KeyValuePair<string, GameObject> pair in objsDict){
            UpdateLines(pair.Key, pair.Value);
        }
        // Debug end
        foreach (KeyValuePair<string, GameObject> obj in objsDict){
            centroid += obj.Value.transform.position;
        }
        centroid = centroid / objs.Count;

        UnityEngine.Debug.Log(centroid);

        Vector3 MainCameraTransform = Camera.main.transform.position;

        centroid = new Vector3(centroid.x, centroid.y, MainCameraTransform.z + zOffset);

        this.transform.position = Vector3.Lerp(transform.position, centroid, Time.deltaTime * movementSpeed);

    }

    public void RemoveObj(string key){
        objs.Remove(objsDict[key]);
        objsDict.Remove(key);
        Destroy(linesDict[key]);
        linesDict.Remove(key);
        //Delete corresponding lines
    }

    public void ClearObjs(){
        objsDict.Clear();
        objs.Clear();
        foreach (KeyValuePair<string, GameObject> line in linesDict)
        {
            Destroy(line.Value);
        }
        linesDict.Clear();
        //Delete all lines here
    }

    public void AddObj(string key, GameObject obj){
        objsDict.Add(key, obj);
        objs.Add(obj);
        GameObject pointerObj = Instantiate(LinePrefab);
        linesDict.Add(key, pointerObj);
        Line pointer = pointerObj.GetComponent<Line>();
        pointer.Start = transform.position;
        pointer.End = obj.transform.position;
        //Add new line here
    }

    public void UpdateLines(string key, GameObject obj)
    {
        if (key != "MainCam")
        {
            Line pointer = linesDict[key].GetComponent<Line>();
            pointer.Start = transform.position;
            pointer.End = obj.transform.position;
        }
    }


}
