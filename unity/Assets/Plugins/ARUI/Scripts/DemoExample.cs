using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoExample : MonoBehaviour
{

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            AngelARUI.Instance.PrintVMDebug = !AngelARUI.Instance.PrintVMDebug;
        }

        if (Input.GetKeyUp(KeyCode.L))
        {
            GuidanceMaterialManager.Instance.TaskImage.Untether();
        }
    }

}
