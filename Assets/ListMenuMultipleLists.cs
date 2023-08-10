using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class ListMenuMultipleLists : MonoBehaviour
{
    public int index;
    public GameObject ListContainer;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (EyeGazeManager.Instance.CurrentHitObj.GetInstanceID() == this.gameObject.GetInstanceID())
        {
            FadeIn();
            //Put orb into area
        }
    }
    private void FadeIn()
    {
        ListContainer.GetComponent<MultipleListsContainer>().SetMenuActive(index);
    }
}
