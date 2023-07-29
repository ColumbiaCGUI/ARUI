using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Center_of_Objs : MonoBehaviour
{
    // Debug only
    public List<GameObject> objs;

    public float movementSpeed = 1.0f;
    // Debug end

    Dictionary<string, GameObject> objsDict = new Dictionary<string, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Debug code
        ClearObjs();
        Vector3 centroid = new Vector3(0, 0, 0);
        foreach(GameObject o in objs){
            AddObj(o.name, o);
        }
        // Debug end
        foreach(KeyValuePair<string, GameObject> obj in objsDict){
            centroid += obj.Value.transform.position;
        }
        centroid = centroid / objs.Count;

        this.transform.position = Vector3.Lerp(transform.position, centroid, Time.deltaTime * movementSpeed);

    }

    public void RemoveObj(string key){
        objsDict.Remove(key);
        //Delete corresponding lines
    }

    public void ClearObjs(){
        objsDict.Clear();
        //Delete all lines here
    }

    public void AddObj(string key, GameObject obj){
        objsDict.Add(key, obj);
        //Add new line here
    }


}
