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
    public float SnapDelay = 2.0f;
    public float minDistance = 0.5f;

    Dictionary<string, GameObject> objsDict = new Dictionary<string, GameObject>();
    Dictionary<string, GameObject> linesDict = new Dictionary<string, GameObject>();

    float CurrDelay;
    Vector3 LastPosition;
    float LerpTime = 2.0f;

    Vector3 lerpStart;
    Vector3 lerpEnd;
    bool isLerping;

    float timeStarted;

    bool isLooking = false;
    // Start is called before the first frame update
    void Start()
    {
        LastPosition = Camera.main.transform.position;
    }

    void SnapToCentroid()
    {
        Vector3 centroid = new Vector3(0, 0, 0);
        foreach (KeyValuePair<string, GameObject> pair in objsDict)
        {
            centroid += pair.Value.transform.position;
        }
        centroid = centroid / objsDict.Count;
        this.GetComponent<MultipleListsContainer>().SetLineEnd(centroid);
        UnityEngine.Debug.Log(centroid);
        UnityEngine.Debug.Log(Camera.main.transform.position.y);
        Vector3 finalPos = new Vector3(centroid.x, Camera.main.transform.position.y + heightOffset, centroid.z);
        BeginLerp(this.transform.position, finalPos);
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<MultipleListsContainer>().SetLineStart(ListPrefab.transform.position);
        Vector3 centroid = new Vector3(0, 0, 0);
        foreach (KeyValuePair<string, GameObject> pair in objsDict)
        {
            centroid += pair.Value.transform.position;
        }
        centroid = centroid / objsDict.Count;
        this.GetComponent<MultipleListsContainer>().SetLineEnd(centroid);
        // Store last position of camera 
        // Get current position. If distance greater than a certain amount, snap task overview closer to user
        // Set Current position as last position
        if (isLooking)
        {
            int currIndex = this.GetComponent<MultipleListsContainer>().currIndex;
            CurrDelay = 0.0f;
            if (currIndex == 0)
            {
                foreach (KeyValuePair<string, GameObject> pair in objsDict)
                {
                    UpdateLines(pair.Key, pair.Value);
                }
            }
            else
            {
                DeactivateLines();
            }
            //Snap 50 cm away from user??
            #region old code
            /*            this.GetComponent<MultipleListsContainer>().OverviewLine.gameObject.SetActive(false);
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
                        this.transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * movementSpeed);*/
            #endregion
        }
        else
        {
            CurrDelay += Time.deltaTime;
            if (CurrDelay >= SnapDelay)
            {
                DeactivateLines();
                float currDistance = Vector3.Distance(Camera.main.transform.position, this.transform.position);
                if (currDistance > minDistance)
                {
                    Vector3 finalPos = Camera.main.transform.position + Camera.main.transform.forward * zOffset + Camera.main.transform.right * xOffset + Camera.main.transform.up * yOffset;
                    BeginLerp(this.transform.position, finalPos);
                }
                CurrDelay = 0.0f;
            }
            #region old code
            /*            this.GetComponent<MultipleListsContainer>().OverviewLine.gameObject.SetActive(true);
                        DeactivateLines();
                        Vector3 centroid = new Vector3(0, 0, 0);
                        foreach (KeyValuePair<string, GameObject> pair in objsDict)
                        {
                            centroid += pair.Value.transform.position;
                        }
                        centroid = centroid / objsDict.Count;
                        this.GetComponent<MultipleListsContainer>().SetLineEnd(centroid);
                        Vector3 finalPos = new Vector3(centroid.x, Camera.main.transform.position.y + heightOffset, centroid.z);
                        this.transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * movementSpeed);*/
            #endregion
        }

    }
    //Source -> https://www.blueraja.com/blog/404/how-to-use-unity-3ds-linear-interpolation-vector3-lerp-correctly
    void BeginLerp(Vector3 startPos, Vector3 endPos)
    {
        timeStarted = Time.time;
        isLerping = true;
        lerpStart = startPos;
        lerpEnd = endPos;
    }

    void FixedUpdate()
    {
        if (isLerping) {
            float timeSinceStarted = Time.time - timeStarted;
            float percentComplete = timeSinceStarted / LerpTime;
            transform.position = Vector3.Lerp(lerpStart, lerpEnd, percentComplete);
            if (percentComplete >= 1.0f) { 
                isLerping= false;
            }
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
        SnapToCentroid();
        DeactivateLines();
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