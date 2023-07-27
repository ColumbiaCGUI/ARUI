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
    detectedObject = 5,
    annotation = 6,
    okButton = 7,
    ringindicator = 8,
    textConfirmationWindow = 9
}

public class EyeGazeManager : Singleton<EyeGazeManager>
{
    public EyeTarget CurrentHit = EyeTarget.nothing;
    public GameObject CurrentHitObj;

    /// ** Debug eye gaze target cube
    private MeshRenderer _eyeGazeTargetCube;
    private bool _showRayDebugCube = false;

    bool bSphere;
    private Ray rayToCenter_copy;
    private RaycastHit hitInfo_copy;

    [SerializeField] 
    private float sphereRadius = 0.1f;
    private Vector3 sphereCenter;

    int layerMask;

    private void Awake()
    {
        _eyeGazeTargetCube = gameObject.GetComponent<MeshRenderer>();

        // layerMask = LayerMask.GetMask(StringResources.UI_layer, StringResources.VM_layer, "Annotation");
        layerMask = LayerMask.GetMask("Annotation");
    }

    /*
    private void OnDrawGizmos()
    {
        if (bSphere)
        {
            Debug.DrawRay(rayToCenter_copy.origin, rayToCenter_copy.direction * hitInfo_copy.distance, Color.red);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rayToCenter_copy.origin + rayToCenter_copy.direction * hitInfo_copy.distance, sphereRadius);
        }
        else
        {
            Debug.DrawRay(rayToCenter_copy.origin, rayToCenter_copy.direction * 100f, Color.green);
        }
    }
    */

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
            sphereCenter = eyeGazeProvider.HitPosition;
            Collider[] hitColliders = Physics.OverlapSphere(sphereCenter, sphereRadius, layerMask);
            foreach (var hitCollider in hitColliders)
            {
                Debug.Log("Selection Debug ---- " + hitCollider.name);
            }

            gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * 2.0f;
            _eyeGazeTargetCube.enabled = false;

            Ray rayToCenter = new Ray(eyeGazeProvider.GazeOrigin, eyeGazeProvider.GazeDirection);
            rayToCenter_copy = rayToCenter;
            RaycastHit hitInfo;

            //UnityEngine.Physics.Raycast(rayToCenter, out hitInfo, 100f, layerMask);

            bSphere = UnityEngine.Physics.SphereCast(rayToCenter, sphereRadius, out hitInfo, 100, layerMask);
            hitInfo_copy = hitInfo;

            // Update GameObject to the current eye gaze position at a given distance
            if (hitInfo.collider != null)
            {
                float dist = (hitInfo.point - AngelARUI.Instance.ARCamera.transform.position).magnitude;
                gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * dist;
                //Debug.Log(hitInfo.collider.gameObject.name);
                string goName = hitInfo.collider.gameObject.name.ToLower();

                if (goName.Contains("flexibletextcontainer_orb"))
                    CurrentHit = EyeTarget.orbMessage;

                else if (goName.Contains("bodyplacement"))
                    CurrentHit = EyeTarget.orbFace;

                else if (goName.Contains(StringResources.tasklist_name.ToLower()))
                    CurrentHit = EyeTarget.tasklist;

                else if (goName.Contains("facetasklistbutton"))
                    CurrentHit = EyeTarget.orbtasklistButton;

                else if (goName.Contains("detectedobject"))
                    CurrentHit = EyeTarget.detectedObject;

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
    }

    public void ShowDebugTarget(bool showEyeGazeTarget) => _showRayDebugCube = showEyeGazeTarget;
}
