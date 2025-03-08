using UnityEngine;
using Unity.Cinemachine;

public class CameraCollider : MonoBehaviour
{
    public CinemachineCamera cam;
    

    public Transform cameraTarget; //The object whose collider will spawn 
    public Vector3 cameraOffset;
    public Vector3 targetOffset = new Vector3(1.05f, 0.97f, 0);
    public float collisionOffset;
    public float forwardOffset;
    Vector3 spawnPoint;
    public LayerMask collidesWithCamera;

    public LayerMask floor, wall;
    public Camera cam1;

    public float lineLength = 40f;// Length of the gizmo line
    public bool collided,targetCollided;
    [Range(0f, 1f)]
    public float lerpAmount;
    public float hOffset, baseHOffset,changeRate,accelerationRate; //Change rate = The speed of the movement of the target offset Acceleration rate = The acceleration of change rate
    [SerializeField]private float hOffsetOffset; //An offset for the overlap spheres to check 
    public bool isShrinking,isGrowing;
    public float positionSmoothTime = 0.1f;
    [SerializeField] private float smoothTime = 0.2f;



    [Header("OrbitalFollow Settings")]
    public CinemachineOrbitalFollow camFollow;
    public CinemachineHardLookAt camController;
    public float baseRadius, targetRadius;


    private float collisionStayTime = 0f; 
    private const float minCollisionTime = 0.1f;

    void OnDrawGizmos()
    {
        if (cameraTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cameraTarget.position, cameraTarget.localScale.x / 2f);
        }

        if (Camera.main == null)
            return;

        Vector3 startPosition = Camera.main.transform.position;
        Vector3 endPosition = cam.Follow.position + targetOffset;
        Vector3 collisionPoint;

        Gizmos.color = Color.red;
        RaycastHit hit;
        if (Physics.Raycast(endPosition, -Camera.main.transform.forward, out hit, lineLength, floor & wall, QueryTriggerInteraction.Ignore))
        {
            Gizmos.color = Color.yellow;
            collisionPoint = hit.point;
            Gizmos.DrawWireSphere(collisionPoint, 0.2f);
        }
        else if (Physics.Raycast(endPosition, -Camera.main.transform.forward, out hit, lineLength, floor, QueryTriggerInteraction.Ignore))
        {
            Gizmos.color = Color.blue;
            collisionPoint = hit.point;
            Gizmos.DrawWireSphere(collisionPoint, 0.2f);
        }
        else if (Physics.Raycast(endPosition, -Camera.main.transform.forward, out hit, lineLength, wall, QueryTriggerInteraction.Ignore))
        {
            Gizmos.color = Color.red;
            collisionPoint = hit.point;
            Gizmos.DrawWireSphere(collisionPoint, 0.2f);
        }
        else
        {
            Gizmos.color = Color.black;
        }
        Gizmos.DrawWireSphere(cam.Follow.position + targetOffset, 0.2f);
        Gizmos.DrawLine(startPosition, endPosition);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraTarget = camFollow.FollowTarget;
        targetOffset = camFollow.FollowTarget.position;
        camFollow = FindFirstObjectByType<CinemachineOrbitalFollow>();
        cam1 = Camera.main;
        baseRadius = camFollow.Radius;
        baseHOffset = cameraTarget.localPosition.x;
        hOffset = baseHOffset;
    }

    private void FixedUpdate()
    {

    }
    // Update is called once per frame
    void Update()
    {

        CheckCollisions();
        CheckTargetCollisions();
        ChangeRadius(collided);
    }
    void CheckTargetCollisions()
    {

        Vector3 position = transform.position;

        //Checks an overlap sphere the size of the target to avoid the raycasts starting from within the object
        Collider[] colliders = Physics.OverlapSphere(cameraTarget.position + new Vector3(hOffsetOffset,0f,0f)
            , cameraTarget.localScale.x / 2f, wall);
        //Checks a bigger overlap sphere in order to avoid shrinking and growing every frame
        Collider[] safetyColliders = Physics.OverlapSphere(cameraTarget.position + new Vector3(hOffsetOffset, 0f, 0f)
            , cameraTarget.localScale.x / 2f * 1.5f, wall);

        // If there are colliders in the normal range, start shrinking
        if (colliders.Length > 0)
        {
            isShrinking = true;
        }

        // SHRINKING PHASE: Reduce hOffset
        if (isShrinking)
        {
            hOffset -= changeRate * Time.deltaTime;
            if (hOffset <= 0)
            {
                hOffset = 0;
            }

            // If colliders are gone, check safetyColliders
            if (colliders.Length == 0)
            {
                if (safetyColliders.Length > 0)
                {
                    isShrinking = false; // Stop shrinking, keep current hOffset
                }
                else
                {
                    // No colliders in either range, start growing back
                    isShrinking = false;
                }
            }
        }
        else if (safetyColliders.Length>0)
        {

        }
        else
        {

            if (hOffset < baseHOffset)
            {
                hOffset += changeRate * Time.deltaTime;
                if (hOffset > baseHOffset)
                {
                    hOffset = baseHOffset;
                }
            }
        }

        // Apply hOffset to the target object's local position
        if (cameraTarget != null)
        {
            Vector3 localPos = cameraTarget.localPosition;
            localPos.x = Mathf.Lerp(localPos.x, hOffset, Time.deltaTime / positionSmoothTime);
            cameraTarget.localPosition = localPos;
        }
       
    }
    

    void ShrinkTargetHDistance(bool isShrinking)
    {

    }
    void CheckCollisions()
    {
        Vector3 collisionPoint;
        Vector3 endPosition = cam.Follow.position;
        Vector3 originalCameraPosition = cam1.transform.position;
        Vector3 bottomRightFrustrumCorner = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, Camera.main.nearClipPlane));
        Collider[] colliders = Physics.OverlapSphere(endPosition, 0.2f, floor | wall, QueryTriggerInteraction.Ignore);
        if (colliders.Length > 0)
        {
            collisionStayTime += Time.deltaTime;

            if (collisionStayTime >= minCollisionTime)
            {
                targetRadius = Mathf.Lerp(targetRadius, 0.01f, Time.deltaTime * 10f);
                collided = true;
            }
            return;
        }
        else
        {
            collisionStayTime = 0f; // Reset timer when not colliding
        }

        RaycastHit hit;
        if (Physics.Raycast(endPosition, -Camera.main.transform.forward, out hit, lineLength, floor | wall, QueryTriggerInteraction.Ignore))
        {
            collisionPoint = hit.point;
            targetRadius = Vector3.Distance(collisionPoint, endPosition) - forwardOffset;
            collided = true;
        }
        else if (collided)
        {
            if (Physics.Raycast(endPosition, (originalCameraPosition - endPosition).normalized, out hit, lineLength, floor | wall, QueryTriggerInteraction.Ignore))
            {
                collided = true;
            }
            else
            {
                collided = false;
            }
        }

    }

    bool IsCameraCollidingWithWall(Vector3 cameraPosition, Vector3 cameraDirection, float checkDistance)
    {
        RaycastHit hit;

        // Perform a raycast from the camera's position forward to see if it's hitting something
        if (Physics.Raycast(cameraPosition, cameraDirection, out hit, checkDistance, floor | wall, QueryTriggerInteraction.Ignore))
        {
            return true; // Camera is colliding with something in front of it
        }

        // Optionally, you can also check the camera's back if needed
        if (Physics.Raycast(cameraPosition, -cameraDirection, out hit, checkDistance, floor | wall, QueryTriggerInteraction.Ignore))
        {
            return true; // Camera is colliding with something behind it
        }

        return false; // No collision detected
    }
    void ChangeRadius(bool collided)
    {
        if (targetRadius > baseRadius)
        {
            targetRadius = baseRadius;
        }
        float target = collided ? targetRadius : baseRadius;

        if (!isShrinking)
        {
            camFollow.Radius = Mathf.Lerp(camFollow.Radius, target - forwardOffset <= 0.3f ? 0.3f : target - forwardOffset, lerpAmount);
        }



        if (Mathf.Abs(camFollow.Radius - target) < 0.01f)
        {
            camFollow.Radius = target;
        }
    }
    void ChangeOffset(bool targetCollided)
    {

    }
}
