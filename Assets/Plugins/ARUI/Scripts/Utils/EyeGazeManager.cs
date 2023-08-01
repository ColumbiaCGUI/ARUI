using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEditor.PackageManager;
using UnityEngine;

public enum EyeTarget
{
    nothing = 0,
    orbFace = 1,
    orbMessage = 2,
    tasklist = 3,
    orbtasklistButton = 4,
    annotation = 5,
    okButton = 7,
    ringindicator = 8,
    textConfirmationWindow = 9
}

public class EyeGazeManager : Singleton<EyeGazeManager>
{
    public EyeTarget CurrentHit = EyeTarget.nothing;
    public GameObject CurrentHitObj;

    public Collider[] hitColliders;

    /// ** Debug eye gaze target cube
    private MeshRenderer _eyeGazeTargetCube;
    private bool _showRayDebugCube = false;

    [SerializeField] 
    private float sphereRadius = 0.1f;
    private Vector3 sphereCenter;

    private int rayLayerMask;
    private int sphereLayerMask;

    private GameObject prevHitObj;

    private void Awake()
    {
        _eyeGazeTargetCube = gameObject.GetComponent<MeshRenderer>();

        rayLayerMask = LayerMask.GetMask(
            StringResources.UI_layer,
            StringResources.VM_layer,
            StringResources.Annotation_layer,
            StringResources.ObjectCollider_Layer);

        sphereLayerMask = LayerMask.GetMask(
            StringResources.ObjectCollider_Layer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(sphereCenter, sphereRadius);
    }

    private void Update()
    {
        var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
        if (eyeGazeProvider != null)
        {
            gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * 2.0f;
            _eyeGazeTargetCube.enabled = false;

            Ray rayToCenter = new Ray(eyeGazeProvider.GazeOrigin, eyeGazeProvider.GazeDirection);
            RaycastHit hitInfo;

            Physics.Raycast(rayToCenter, out hitInfo, 100f, rayLayerMask);
            sphereCenter = hitInfo.point;

            hitColliders = Physics.OverlapSphere(sphereCenter, sphereRadius, sphereLayerMask);

            // Update GameObject to the current eye gaze position at a given distance
            if (hitInfo.collider != null)
            {
                float dist = (hitInfo.point - AngelARUI.Instance.ARCamera.transform.position).magnitude;
                gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * dist;

                string goName = hitInfo.collider.gameObject.name.ToLower();

                if (goName.Contains("flexibletextcontainer_orb"))
                    CurrentHit = EyeTarget.orbMessage;

                else if (goName.Contains("bodyplacement"))
                    CurrentHit = EyeTarget.orbFace;

                else if (goName.Contains(StringResources.tasklist_name.ToLower()))
                    CurrentHit = EyeTarget.tasklist;

                else if (goName.Contains("facetasklistbutton"))
                    CurrentHit = EyeTarget.orbtasklistButton;

                else if (goName.Contains("annotation"))
                    CurrentHit = EyeTarget.annotation;

                else if (goName.Contains("okbutton"))
                    CurrentHit = EyeTarget.okButton;

                else if (goName.Contains("flexibletextcontainer_window"))
                    CurrentHit = EyeTarget.textConfirmationWindow;

                else if (goName.Contains("ringindicator"))
                    CurrentHit = EyeTarget.ringindicator;

                else
                    CurrentHit = EyeTarget.nothing;

                if (CurrentHit != EyeTarget.nothing)
                {
                    CurrentHitObj = hitInfo.collider.gameObject;
                    if (_showRayDebugCube)
                        _eyeGazeTargetCube.enabled = true;

                }
                else if (CurrentHit == EyeTarget.nothing)
                    CurrentHitObj = null;
            }
            else
            {
                // If no target is hit, show the object at a default distance along the gaze ray.
                gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * 2.0f;
                CurrentHit = EyeTarget.nothing;
                CurrentHitObj = null;
            }
        }
        else
        {
            CurrentHit = EyeTarget.nothing;
            CurrentHitObj = null;
        }

        if (prevHitObj == CurrentHitObj)
        {
            if(sphereRadius >= 0.01f)
            {
                sphereRadius -= (0.01f * Time.deltaTime);
            }
        }
        else
        {
            sphereRadius = 0.1f;
            prevHitObj = CurrentHitObj;
        }
    }

    public void ShowDebugTarget(bool showEyeGazeTarget) => _showRayDebugCube = showEyeGazeTarget;

    public bool HitCollidersHave(Collider collider)
    {
        if (hitColliders == null) return false;

        foreach (Collider c in hitColliders)
            if (c == collider)
            {
                return true;
            }

        return false;
    }
}
