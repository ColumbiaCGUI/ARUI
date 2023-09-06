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
    // ========================================================================
    // public
    // ========================================================================
    public EyeTarget CurrentHit = EyeTarget.nothing;
    public EyeTarget PrevHit = EyeTarget.nothing;
    public EyeTarget LastFrameHit = EyeTarget.nothing;

    public GameObject CurrentHitObj;

    public Collider[] hitColliders;

    public Vector3 gazePosition;
    public float sizeRatio;

    // ========================================================================
    // private
    // ========================================================================
    // ** Debug eye gaze target cube
    private IMixedRealityEyeGazeProvider eyeGazeProvider;

    private MeshRenderer _eyeGazeTargetCube;
    private bool _showRayDebugCube = false;

    [SerializeField] 
    private float sphereRadius = 0.1f;

    private int rayLayerMask;

    [SerializeField]
    private float velocityThreshold = 100f;
    [SerializeField]
    private float angularVelocityThreshold = 100f;
    [SerializeField]
    private float minFixationDuration = 0.1f;

    private Vector3 previousGazePoint;
    private Vector3 previousGazeDirection;
    private float currentFixationDuration;

    // ========================================================================
    // Awake()
    // ========================================================================
    private void Awake()
    {
        eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
        _eyeGazeTargetCube = gameObject.GetComponent<MeshRenderer>();

        rayLayerMask = LayerMask.GetMask(
            StringResources.UI_layer,
            StringResources.VM_layer,
            StringResources.Annotation_layer,
            StringResources.ObjectCollider_Layer);

        previousGazePoint = eyeGazeProvider.GazeOrigin;
        previousGazeDirection = eyeGazeProvider.GazeDirection;
        currentFixationDuration = 0;
    }

    // ========================================================================
    // Update()
    // ========================================================================
    private void Update()
    {
        if (eyeGazeProvider != null)
        {
            // Get gaze point
            gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * 2.0f;
            _eyeGazeTargetCube.enabled = false;

            Ray rayToCenter = new Ray(eyeGazeProvider.GazeOrigin, eyeGazeProvider.GazeDirection);
            Physics.Raycast(rayToCenter, out RaycastHit hitInfo, 100f, rayLayerMask);

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
                {
                    CurrentHitObj = null;
                }
            }
            else
            {
                // If no target is hit, show the object at a default distance along the gaze ray.
                gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * 2.0f;
                CurrentHit = EyeTarget.nothing;
                CurrentHitObj = null;
            }

            /* ----- I-VT algorithm ----- */
            Vector3 currentGazePoint = gameObject.transform.position;
            Vector3 currentGazeDirection = eyeGazeProvider.GazeDirection;
            gazePosition = currentGazePoint;

            float dotProduct = Vector3.Dot(previousGazeDirection.normalized, currentGazeDirection.normalized);
            float clampedDotProduct = Mathf.Clamp(dotProduct, -1f + Mathf.Epsilon, 1f - Mathf.Epsilon);
            float angularDistanceRadians = Mathf.Acos(clampedDotProduct);
            float angularDistanceDegrees = angularDistanceRadians * Mathf.Rad2Deg;
            float angularVelocity = angularDistanceDegrees / Time.deltaTime;

            if (angularVelocity < angularVelocityThreshold)
            {
                currentFixationDuration += Time.deltaTime;
            }
            else
            {
                currentFixationDuration = 0;
            }

            if (currentFixationDuration >= minFixationDuration)
            {
                if (sphereRadius >= 0.01f)
                {
                    sphereRadius -= (0.01f * Time.deltaTime);
                }

                if (sizeRatio >= 0.1)
                {
                    sizeRatio -= (0.33f * Time.deltaTime);
                }
            }
            else
            {
                sphereRadius = 0.1f;
                sizeRatio = 1;
            }

            previousGazePoint = currentGazePoint;
            previousGazeDirection = currentGazeDirection;
        }
        else
        {
            CurrentHit = EyeTarget.nothing;
            CurrentHitObj = null;
        }
    }

    // ========================================================================
    // ShowDebugTarget()
    // ========================================================================
    public void ShowDebugTarget(bool showEyeGazeTarget) => _showRayDebugCube = showEyeGazeTarget;
}
