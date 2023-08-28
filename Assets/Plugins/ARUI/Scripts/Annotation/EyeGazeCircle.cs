using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class EyeGazeCircle : MonoBehaviour
{
    private IMixedRealityEyeGazeProvider m_EyeGazeProvider;

    private bool init;

    [SerializeField]
    private Vector3 initDiscSize = new Vector3(0.1f, 0.1f, 0.05f);

    // Start is called before the first frame update
    void Start()
    {
        m_EyeGazeProvider = CoreServices.InputSystem.EyeGazeProvider;

        init = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!EyeGazeManager.Instance) return;

        if (!init)
        {
            Transform fakeItems = GameObject.Find("FakeItem").transform;
            float total = 0;
            int childCount = 0;

            foreach (Transform child in fakeItems)
            {
                if (!child.gameObject.activeSelf) continue;
                Vector3 size = child.Find("Annotation(Clone)").Find("AnnotationCollider").GetComponent<BoxCollider>().bounds.size;
                float mean = (size.x + size.y + size.z) / 3;
                total += mean;
                childCount += 1;
            }

            float radius = total / childCount;

            initDiscSize = new Vector3(radius, radius, 0.05f);

            init = true;
        }

        transform.position = EyeGazeManager.Instance.gazePosition;
        transform.LookAt(m_EyeGazeProvider.GazeOrigin);

        transform.localScale = initDiscSize * EyeGazeManager.Instance.sizeRatio;
    }

    private void OnTriggerEnter(Collider other)
    {
        other.transform.parent.GetComponent<Annotation>().isInDisc = true;
    }

    private void OnTriggerExit(Collider other)
    {
        other.transform.parent.GetComponent<Annotation>().isInDisc = false;
    }
}
