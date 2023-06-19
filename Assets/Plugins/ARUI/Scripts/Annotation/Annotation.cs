using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class Annotation : MonoBehaviour
{
    public GameObject canvas;
    public Line line;
    private bool _isLookingAtAnnotation = false;

    void Start()
    {
        line.Start = transform.position;
    }

    void Update()
    {
        if (!EyeGazeManager.Instance) 
        {
            Debug.Log("we are here");
            return;
        }

        // if (!_isLookingAtAnnotation && EyeGazeManager.Instance.CurrentHit == EyeTarget.objectIndicator)
        // {
        //     Debug.Log("Looking at the anchor");
        //     _isLookingAtAnnotation = true;
        //     StartCoroutine(EnableAnnotationAfterDelay(1));
        // }
        // else if (_isLookingAtAnnotation && EyeGazeManager.Instance.CurrentHit != EyeTarget.objectIndicator)
        // {
        //     Debug.Log("Not looking at the anchor");
        //     _isLookingAtAnnotation = false;
        //     StartCoroutine(DisableAnnotationAfterDelay(1));
        // }
    }

    IEnumerator EnableAnnotationAfterDelay(float duration)
    {
        float elapsed = 0f;
        bool success = false;

        line.Thickness = 0.125f;
        Vector3 endPoint = new Vector3(0, 8, 0);

        Vector3 direction = (endPoint - transform.position).normalized;
        float length = (endPoint - transform.position).magnitude;

        while (_isLookingAtAnnotation && elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float percent = elapsed / duration;
            line.End = direction * (length * percent);

            if (elapsed > duration && _isLookingAtAnnotation)
                success = true;

            yield return null;
        }

        if (success)
        {
            canvas.SetActive(true);
        }
    }

    IEnumerator DisableAnnotationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        canvas.SetActive(false);
        line.Thickness = 0;
    }
}
