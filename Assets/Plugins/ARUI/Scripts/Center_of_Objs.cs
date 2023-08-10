using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class Center_of_Objs : MonoBehaviour
{
    // Debug only
    public List<GameObject> objs;

    public GameObject LinePrefab;

    public float movementSpeed = 1.0f;
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
        // Debug code
        //ClearObjs();
        foreach(KeyValuePair<string, GameObject> pair in objsDict){
            UpdateLines(pair.Key, pair.Value);
        }
        // Debug end
        foreach (KeyValuePair<string, GameObject> obj in objsDict){
            centroid += obj.Value.transform.position;
        }
        centroid = centroid / objs.Count;

        this.transform.position = Vector3.Lerp(transform.position, centroid, Time.deltaTime * movementSpeed);

    }

    public void RemoveObj(string key){
        objsDict.Remove(key);
        Destroy(linesDict[key]);
        linesDict.Remove(key);
        //Delete corresponding lines
    }

    public void ClearObjs(){
        objsDict.Clear();
        foreach(KeyValuePair<string, GameObject> line in linesDict)
        {
            Destroy(line.Value);
        }
        linesDict.Clear();
        //Delete all lines here
    }

    public void AddObj(string key, GameObject obj){
        objsDict.Add(key, obj);
        GameObject pointerObj = Instantiate(LinePrefab, Vector3.zero, this.transform.rotation);
        linesDict.Add(key, pointerObj);
        Line pointer = pointerObj.GetComponent<Line>();
        pointer.Start = transform.position;
        pointer.End = obj.transform.position;
        //Add new line here
    }

    public void UpdateLines(string key, GameObject obj)
    {
        Line pointer = linesDict[key].GetComponent<Line>();
        pointer.Start = transform.position;
        pointer.End = obj.transform.position;
    }


}
