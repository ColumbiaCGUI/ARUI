using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleListsContainer : MonoBehaviour
{
    public List<GameObject> itemsMenus;

    int currIndex = 0;

    [SerializeField]
    bool isMenu = false;

    void Update()
    {
        if (isMenu)
        {
            //if eye gaze not on task objects then do fade out currentindex
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
                itemsMenus[i].SetActive(false);
            }
        }

    }
}
