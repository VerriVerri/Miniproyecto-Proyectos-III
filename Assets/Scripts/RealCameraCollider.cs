using UnityEngine;
using Unity.Cinemachine;

public class RealCameraCollider : MonoBehaviour
{
    public CinemachineOrbitalFollow orbitalFollow;
    public CinemachineCamera cinemachineCamera;
    public CinemachineHardLookAt hardLookAt;
    public float radius;
    public float baseRadius;

    public Camera targetCamera;  // Assign the camera in the inspector
    public Transform raycastSource; // Assign the object that will cast rays
    public float sphereRadius = 0.2f;

    private Vector3[] frustumCorners = new Vector3[4]; // Store the 4 near-plane corners
    private Vector3[] hitPoints = new Vector3[4]; // Store hit points
    [SerializeField] private Vector3 closestHitPoint = Vector3.zero;
    private float closestDistance;
    private float adjustedDistance;
    public LayerMask collisionLayers;

    private Vector3 hypotheticalCameraPos;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseRadius = orbitalFollow.Radius;
        targetCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        GetFrustumCorners();
        CastRays();
    }

    void GetFrustumCorners()
    {
        targetCamera.CalculateFrustumCorners(
            new Rect(0, 0, 1, 1),
            targetCamera.nearClipPlane,
            Camera.MonoOrStereoscopicEye.Mono,
            frustumCorners
        );

        // Convert to world space
        for (int i = 0; i < 4; i++)
        {
            frustumCorners[i] = targetCamera.transform.TransformPoint(frustumCorners[i]);
        }
    }
    void CastRays()
    {
        float closestDistance = Mathf.Infinity;
        closestHitPoint = Vector3.zero;

        for (int i = 0; i < 4; i++)
        {
            Vector3 direction = (frustumCorners[i] - raycastSource.position).normalized;
            Ray ray = new Ray(raycastSource.position, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, collisionLayers))
            {
                float distance = Vector3.Distance(raycastSource.position, hit.point);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestHitPoint = hit.point;
                }
            }
        }

        if (closestHitPoint != Vector3.zero)
        {
            // Find distance from any frustum corner to camera center
            float frustumCornerDistance = Vector3.Distance(frustumCorners[0], targetCamera.transform.position);

            // Compute new camera position by moving backwards
            adjustedDistance = closestDistance - frustumCornerDistance;
            hypotheticalCameraPos = closestHitPoint - targetCamera.transform.forward * adjustedDistance;
        }
    }
    void DrawFrustum(Vector3 camPos, Color color)
    {
        Gizmos.color = color;

        // Get frustum corners relative to this "new camera position"
        Vector3[] newFrustumCorners = new Vector3[4];
        targetCamera.CalculateFrustumCorners(
            new Rect(0, 0, 1, 1),
            targetCamera.nearClipPlane,
            Camera.MonoOrStereoscopicEye.Mono,
            newFrustumCorners
        );

        for (int i = 0; i < 4; i++)
        {
            newFrustumCorners[i] = camPos + targetCamera.transform.rotation * newFrustumCorners[i];
        }

        // Draw the frustum edges
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(camPos, newFrustumCorners[i]);
        }

        // Connect corners to complete frustum shape
        Gizmos.DrawLine(newFrustumCorners[0], newFrustumCorners[1]);
        Gizmos.DrawLine(newFrustumCorners[1], newFrustumCorners[3]);
        Gizmos.DrawLine(newFrustumCorners[3], newFrustumCorners[2]);
        Gizmos.DrawLine(newFrustumCorners[2], newFrustumCorners[0]);
    }

    private void OnDrawGizmos()
    {
        if (targetCamera == null || raycastSource == null)
            return;

        // Draw original camera frustum
        DrawFrustum(targetCamera.transform.position, Color.green);

        // Draw hypothetical camera frustum
        if (closestHitPoint != Vector3.zero)
        {
            DrawFrustum(hypotheticalCameraPos, Color.blue);

            // Draw the hypothetical camera center
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(hypotheticalCameraPos, 0.1f);
        }

        // Draw collision point
        if (closestHitPoint != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(closestHitPoint, sphereRadius * 1.5f);
        }
    }
}
