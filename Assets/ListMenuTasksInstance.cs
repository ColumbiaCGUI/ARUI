using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class ListMenuTasksInstance : MonoBehaviour
{
    public GameObject canvas;
    public GameObject anchor;
    private bool isLookingAtDot;
    [SerializeField]
    private float enableDelay = 1.0f;
    [SerializeField]
    private float disableDelay = 1.0f;

    private float delta;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (EyeGazeManager.Instance.CurrentHit != EyeTarget.listmenuButton_tasks || EyeGazeManager.Instance.CurrentHitObj == null || (EyeGazeManager.Instance.CurrentHitObj.GetInstanceID() != anchor.GetInstanceID() && EyeGazeManager.Instance.CurrentHitObj.GetInstanceID() != canvas.GetInstanceID()))
        {
            if (delta > disableDelay)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                delta += Time.deltaTime;
            }

        }
        else if (EyeGazeManager.Instance.CurrentHit == EyeTarget.listmenuButton_tasks && (EyeGazeManager.Instance.CurrentHitObj.GetInstanceID() == anchor.GetInstanceID() || EyeGazeManager.Instance.CurrentHitObj.GetInstanceID() == canvas.GetInstanceID()))
        {
            delta = 0.0f;
            FadeIn();
        }
    }
    private IEnumerator FadeOut()
    {
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        float counter = 0f;
        float duration = 1.0f;
        float startAlpha = 1.0f;
        float targetAlpha = 0.0f;
        bool broken = false;
        while (counter < duration)
        {
            if (EyeGazeManager.Instance.CurrentHit == EyeTarget.listmenuButton_tasks && (EyeGazeManager.Instance.CurrentHitObj.GetInstanceID() == anchor.GetInstanceID() || EyeGazeManager.Instance.CurrentHitObj.GetInstanceID() == canvas.GetInstanceID()))
            {
                broken = true;
                break;
            }
            counter += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, counter / duration);

            yield return null;
        }
        if (!broken)
        {
            canvas.SetActive(false);
            anchor.SetActive(true);
            canvasGroup.alpha = 1.0f;
        }

    }

    private void FadeIn()
    {
        Debug.Log("Called");
        canvas.SetActive(true);
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1.0f;
        anchor.SetActive(false);
    }
}
