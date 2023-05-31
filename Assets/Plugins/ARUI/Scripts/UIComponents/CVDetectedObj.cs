using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CVDetectedObj : VMNonControllable
{
    //private bool VMControllable annotation;
    private GameObject zBufferCopy;
    private MeshRenderer meshRenderer;

    private Color color;
    public Color GetColor() { return color; }

    public bool IsLookingAt = false;

    private bool _isDestroyed = false;
    public bool IsDestroyed  
    {
        get => _isDestroyed; 
        set {
            ViewManagement.Instance.DeRegisterNonControllable(this); 
            _isDestroyed = value; 
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();

        zBufferCopy = new GameObject(gameObject.name + "_zBuffer");
        zBufferCopy.layer = StringResources.LayerToLayerInt[StringResources.zBuffer_layer];
        zBufferCopy.transform.parent = this.transform;
        zBufferCopy.transform.localPosition = Vector3.zero;
        zBufferCopy.transform.localRotation = Quaternion.identity;
        zBufferCopy.transform.localScale = Vector3.one;
        MeshFilter meshFilter = zBufferCopy.AddComponent<MeshFilter>();
        meshFilter.mesh = gameObject.GetComponent<MeshFilter>().mesh;
        meshRenderer = zBufferCopy.AddComponent<MeshRenderer>();

        StartCoroutine(UpdateAABB());

        //Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        //rb.isKinematic = true;
        //rb.useGravity = false;

        //collider.isTrigger = false;
    }

    private IEnumerator UpdateAABB()
    {
        while (ProcessObjectVisibility.Instance == null)
            yield return new WaitForEndOfFrame();

        List<Material> mr = new List<Material>();
        Material mat = new Material(Shader.Find("Unlit/Color"));

        color = ProcessObjectVisibility.Instance.RegisterNonControllable(this);
        mat.color = color;
        foreach (var item in gameObject.GetComponent<MeshRenderer>().sharedMaterials)
            mr.Add(mat);

        meshRenderer.sharedMaterials = mr.ToArray();

        while (true)
        {
            if (transform.InFrontOfCamera(AngelARUI.Instance.ARCamera))
            {
                AABB = transform.RectFromObjs(AngelARUI.Instance.ARCamera, new List<BoxCollider> { collider });
            }
            else
            {
                AABB = Rect.zero;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnDestroy()
    {
        Destroy(zBufferCopy);
        Destroy(gameObject.GetComponent<Rigidbody>());
        Destroy(gameObject.GetComponent<BoxCollider>());
    }

}
