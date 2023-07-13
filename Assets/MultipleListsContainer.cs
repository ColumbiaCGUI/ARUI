using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class MultipleListsContainer : MonoBehaviour
{
    public List<GameObject> itemsMenus;

    int currIndex = 0;

    [SerializeField]
    bool isMenu = false;


    private float delta;

    [SerializeField]
    private float disableDelay = 1.0f;

    void Update()
    {
        if (isMenu)
        {
            //if eye gaze not on task objects then do fade out currentindex
            if (EyeGazeManager.Instance.CurrentHit != EyeTarget.listmenuButton_tasks)
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
            //else fade back in currentindex 
        }
    }
    public void SetMenuActive(int index)
    {
        currIndex = index;
        for(int i = 0; i < itemsMenus.Count; i++)
        {
            if(i == index)
            {
                itemsMenus[i].SetActive(true);
            } else
            {
                CanvasGroup canvasGroup = itemsMenus[i].GetComponent<CanvasGroup>();
                canvasGroup.alpha = 1.0f;
                itemsMenus[i].SetActive(false);
            }
        }

    }
    private IEnumerator FadeOut()
    {
        GameObject canvas = itemsMenus[currIndex];
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        float counter = 0f;
        float duration = 1.0f;
        float startAlpha = 1.0f;
        float targetAlpha = 0.0f;
        bool broken = false;
        while (counter < duration)
        {
            if (EyeGazeManager.Instance.CurrentHit == EyeTarget.listmenuButton_tasks)
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
            canvasGroup.alpha = 1.0f;
        }
        else
        {
            delta = 0.0f;
            canvasGroup.alpha = 1.0f;
            canvas.SetActive(true);
        }

    }
}
