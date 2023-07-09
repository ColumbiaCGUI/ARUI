using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleListsContainer : MonoBehaviour
{
    public List<GameObject> itemsMenus;
    // Start is called before the first frame update
    public void SetMenuActive(int index)
    {
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
