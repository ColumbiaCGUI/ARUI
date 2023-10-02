using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationCanvasControl : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private GameObject nameObj;
    [SerializeField]
    private GameObject imgObj;
    [SerializeField]
    private GameObject videoObj;
    [SerializeField]
    private GameObject dscpObj;

    [SerializeField]
    public bool bHasName = false;
    [SerializeField]
    public bool bHasDescription = false;
    [SerializeField]
    public bool bHasImage = false;
    [SerializeField]
    public bool bHasVideo = false;

    void Awake()
    {
        /* Get canvas objects */
        canvas = GetComponent<Canvas>();

        nameObj = transform.Find("Name").gameObject;
        imgObj = transform.Find("Image").gameObject;
        videoObj = transform.Find("Video").gameObject;
        dscpObj = transform.Find("Description").gameObject;

        /* ---- Dynamically locate name ---- */
        if (!bHasName)
        {
            // Should not happen
        }
        else if (!bHasImage && !bHasDescription && !bHasVideo)
        {
            /* Only has name - Place name in the middle */
            // Set the RectTransform
            RectTransform rectTransform = nameObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        else
        {
            /* Name go to the top */
            RectTransform rectTransform = nameObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
        }

        /* ---- Dynamically locate image ---- */
        if (!bHasName)
        {
            RectTransform rectTransform = imgObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        else
        {
            RectTransform rectTransform = imgObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        /* ---- Dynamically locate video ---- */
        if (!bHasName)
        {
            RectTransform rectTransform = videoObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        else
        {
            RectTransform rectTransform = videoObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    public void canvasInitDone()
    {
        nameObj.SetActive(bHasName);
        dscpObj.SetActive(bHasDescription);
        imgObj.SetActive(bHasImage);
        videoObj.SetActive(bHasVideo);
    }
}
