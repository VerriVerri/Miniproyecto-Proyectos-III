using UnityEngine;
using Unity.Cinemachine;

public class CameraCollider : MonoBehaviour
{
    private bool previousCollidedState = false;
    public float minRadiusDifference = 0.2f;
    public CinemachineCamera cam;
    

    public Transform cameraTarget; //The object whose collider will spawn 
    public Vector3 cameraOffset;
    public Vector3 targetOffset = new Vector3(1.05f, 0.97f, 0);
    public float collisionOffset;
    public float forwardOffset;
    Vector3 spawnPoint;
    public LayerMask collidesWithCamera;
    public bool collidesWithFloor;

    public LayerMask floor, wall;
    public Camera cam1;

    public float lineLength = 40f;// Length of the gizmo line
    public bool collided,targetCollided;
    [Range(0f, 1f)]
    public float lerpAmount;
    public float hOffset, baseHOffset,minHOffset,changeRate,accelerationRate,sphereRadius,safetyFrustrumMultiplier; //Change rate = The speed of the movement of the target offset Acceleration rate = The acceleration of change rate
    [SerializeField]private float hOffsetOffset; //An offset for the overlap spheres to check 
    public bool isShrinking,isGrowing;
    public float positionSmoothTime = 0.1f;
    [SerializeField] private float smoothTime = 0.2f;





    [Header("OrbitalFollow Settings")]
    public CinemachineOrbitalFollow camFollow;
    public CinemachineHardLookAt camController;
    public float baseRadius, targetRadius, minimumRadius;


    private float collisionStayTime = 0f; 
    private const float minCollisionTime = 0.1f;

    Vector3 topRightFrustrumCorner;
    Vector3 bottomLeftFrustrumCorner;
    Vector3 firstCollisionPoint;
    Vector3 secondCollisionPoint;

    void OnDrawGizmos()
    {
        if (cameraTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cameraTarget.position, cameraTarget.localScale.x / 2f);
        }

        if (Camera.main == null)
            return;

        if (collided)
        {
            Gizmos.DrawSphere(firstCollisionPoint, 0.5f);
            Gizmos.DrawSphere(secondCollisionPoint, 0.5f);
        }
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
        topRightFrustrumCorner = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        bottomLeftFrustrumCorner = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));

        // Apply the safety frustum multiplier to make the camera appear larger in frustum corner detection
        topRightFrustrumCorner = cameraTarget.position + (topRightFrustrumCorner - cameraTarget.position) * safetyFrustrumMultiplier;
        bottomLeftFrustrumCorner = cameraTarget.position + (bottomLeftFrustrumCorner - cameraTarget.position) * safetyFrustrumMultiplier;

        CheckCollisions();
        CheckTargetCollisions();
        ChangeRadius(collided);
        CheckCollidedStateChange();
    }
    void CheckTargetCollisions()
    {
        Vector3 position = transform.position;

        // Checks an overlap sphere the size of the target to avoid the raycasts starting from within the object
        Collider[] colliders = Physics.OverlapSphere(cameraTarget.position + new Vector3(hOffsetOffset, 0f, 0f),
            cameraTarget.localScale.x / 2f, wall);

        // Checks a bigger overlap sphere in order to avoid shrinking and growing every frame
        Collider[] safetyColliders = Physics.OverlapSphere(cameraTarget.position + new Vector3(hOffsetOffset, 0f, 0f),
            cameraTarget.localScale.x / 2f * 2f, wall);

        // If there are colliders in the normal range, start shrinking
        if (colliders.Length > 0)
        {
            isShrinking = true;
        }

        // SHRINKING PHASE: Reduce hOffset
        if (isShrinking)
        {
            hOffset -= changeRate * Time.deltaTime;
            if (hOffset <= minHOffset)  // Ensure hOffset does not go below minHOffset
            {
                hOffset = minHOffset;
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
        else if (safetyColliders.Length > 0)
        {
            // If colliders are within the safety range, no action (can be left empty if not needed)
        }
        else
        {
            // Growing phase: Ensure hOffset doesn't exceed baseHOffset
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
    /*void CheckCollisions()
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
    */
    void CheckCollisions()
    {
        if (safetyFrustrumMultiplier == 0) { safetyFrustrumMultiplier = 1; }
        Vector3 collisionPoint;
        Vector3 endPosition = cam.Follow.position;
        Vector3 originalCameraPosition = cam1.transform.position;

        // Get the top-right and bottom-left corners of the camera's frustrum at the near clipping plane
        topRightFrustrumCorner = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        bottomLeftFrustrumCorner = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));

        // Apply the safety frustum multiplier to make the camera appear larger in frustum corner detection
        topRightFrustrumCorner = cameraTarget.position + (topRightFrustrumCorner - cameraTarget.position) * safetyFrustrumMultiplier;
        bottomLeftFrustrumCorner = cameraTarget.position + (bottomLeftFrustrumCorner - cameraTarget.position) * safetyFrustrumMultiplier;

        // Calculate the direction from the top-right to the bottom-left frustum corner
        Vector3 directionTRtoBL = (bottomLeftFrustrumCorner - topRightFrustrumCorner).normalized;

        // Calculate the distance between the top-right and bottom-left corners
        float distanceTRtoBL = Vector3.Distance(topRightFrustrumCorner, bottomLeftFrustrumCorner);

        // Calculate the target camera center using the direction and distance of the frustum corners
        Vector3 targetCameraCenter = topRightFrustrumCorner + directionTRtoBL * (distanceTRtoBL / 2f);

        // Declare separate RaycastHit variables for each ray
        RaycastHit hitTopRight, hitBottomLeft;

        // Start rays from the cameraTarget to the top-right and bottom-left corners of the camera's frustrum
        bool topRightHit = Physics.Raycast(cameraTarget.position, (topRightFrustrumCorner - cameraTarget.position).normalized, out hitTopRight, lineLength, floor | wall, QueryTriggerInteraction.Ignore);

        bool bottomLeftHit = Physics.Raycast(cameraTarget.position, (bottomLeftFrustrumCorner - cameraTarget.position).normalized, out hitBottomLeft, lineLength, floor | wall, QueryTriggerInteraction.Ignore);
        collidesWithFloor = Physics.Raycast(cameraTarget.position, (topRightFrustrumCorner - cameraTarget.position).normalized, lineLength, floor, QueryTriggerInteraction.Ignore)
                    || Physics.Raycast(cameraTarget.position, (bottomLeftFrustrumCorner - cameraTarget.position).normalized, lineLength, floor, QueryTriggerInteraction.Ignore);
        // Draw debug lines for both rays
        Color rayColor = Color.green; // Default color (no collision)

        if (topRightHit || bottomLeftHit)
        {
            // If either corner hit an object, change the color to red (collision detected)
            rayColor = Color.red;
        }

        // Draw the debug line for the top-right corner
        Debug.DrawLine(cameraTarget.position, topRightFrustrumCorner, rayColor);

        // Draw the debug line for the bottom-left corner
        Debug.DrawLine(cameraTarget.position, bottomLeftFrustrumCorner, rayColor);

        // Only draw safety raycast if collided is true
        if (collided)
        {
            // Draw the safety raycast (newly added for visualization)
            Vector3 safetyDirection = (topRightFrustrumCorner - bottomLeftFrustrumCorner).normalized;
            Vector3 safetyRayStart = cameraTarget.position + new Vector3(hOffsetOffset, 0, 0); // Slightly offset the ray start to match your logic
            Debug.DrawRay(safetyRayStart, safetyDirection * lineLength, Color.blue); // Draw the safety ray in blue
        }

        // Handle collision and radius adjustment
        if (topRightHit || bottomLeftHit)
        {
            float radiusRight = 0f;
            float radiusLeft = 0f;
            // Handle top-right ray hit
            if (topRightHit)
            {
                collisionPoint = hitTopRight.point;
                firstCollisionPoint = collisionPoint;
                targetCameraCenter = collisionPoint + directionTRtoBL * (distanceTRtoBL / 2f);
                radiusRight = Vector3.Distance(cameraTarget.position, collisionPoint) - forwardOffset;
            }

            // Handle bottom-left ray hit
            if (bottomLeftHit)
            {
                collisionPoint = hitBottomLeft.point;
                secondCollisionPoint = collisionPoint;
                targetCameraCenter = collisionPoint + (-directionTRtoBL) * (distanceTRtoBL / 2f);
                radiusLeft = Vector3.Distance(cameraTarget.position, collisionPoint) - forwardOffset;
            }
            radiusRight = radiusRight == 0 ? radiusLeft : radiusRight;
            radiusLeft = radiusLeft == 0 ? radiusRight : radiusLeft;
            targetRadius = Mathf.Min(radiusRight, radiusLeft);
            collided = true;
        }
        else if (collided)
        {
            RaycastHit hit;
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
        // Ensure target radius does not exceed the base radius
        if (targetRadius > baseRadius)
        {
            targetRadius = baseRadius;
        }

        // Set the target radius based on whether collided is true
        float target = collided ? targetRadius : baseRadius;

        // Only apply the minimum radius logic if collidesWithFloor is true
        if (collidesWithFloor)
        {
            // Check the difference between current radius and target radius
            float radiusDifference = Mathf.Abs(camFollow.Radius - target);

            // If the difference is smaller than the minRadiusDifference, use the smallest value
            if (radiusDifference < minRadiusDifference)
            {
                // Select the smallest value between the current radius and the target
                camFollow.Radius = Mathf.Min(camFollow.Radius, target);
            }
            else
            {
                // If not within the threshold, interpolate between current and target
                if (!isShrinking)
                {
                    camFollow.Radius = Mathf.Lerp(camFollow.Radius, target - forwardOffset <= minimumRadius ? minimumRadius : target - forwardOffset, lerpAmount);
                }
            }
        }
        else
        {
            // If not colliding with the floor, simply interpolate the radius without the minimum radius logic
            if (!isShrinking)
            {
                camFollow.Radius = Mathf.Lerp(camFollow.Radius, target - forwardOffset <= minimumRadius ? minimumRadius : target - forwardOffset, lerpAmount);
            }
        }

        // Final adjustment if the radius is close to the target
        if (Mathf.Abs(camFollow.Radius - target) < 0.01f)
        {
            camFollow.Radius = target;
        }
    }
    void ChangeOffset(bool targetCollided)
    {

    }
    void CheckCollidedStateChange()
    {
        if (collided != previousCollidedState)
        {
            // Log the state change
            Debug.Log("Collided state changed. New value: " + collided);

            // Update previous state
            previousCollidedState = collided;
        }
    }
}
