using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateTaskList : MonoBehaviour
{
    public GameObject canvas;
    public GameObject anchor;
    private bool isLookingAtDot;
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
        EyeTarget currHit = EyeGazeManager.Instance.CurrentHit;
        if (currHit == EyeTarget.listmenuButton_tasks || currHit == EyeTarget.listmenuButton_items || currHit == EyeTarget.tasklist || currHit == EyeTarget.upButton || currHit == EyeTarget.downButton || currHit == EyeTarget.resetButton)
        {
            delta = 0.0f;
            FadeIn();
        }
        else
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
            EyeTarget currHit = EyeGazeManager.Instance.CurrentHit;
            if (currHit == EyeTarget.listmenuButton_tasks || currHit == EyeTarget.listmenuButton_items || currHit == EyeTarget.tasklist || currHit == EyeTarget.upButton || currHit == EyeTarget.downButton || currHit == EyeTarget.resetButton)
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
            this.GetComponent<Up_Down_Button_Coordinator>().Reset();
            canvas.SetActive(false);
            anchor.SetActive(true);
            canvasGroup.alpha = 1.0f;
        }

    }

    private void FadeIn()
    {
        anchor.SetActive(false);
        Debug.Log("Fade In");
        canvas.SetActive(true);
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1.0f;
    }
}
