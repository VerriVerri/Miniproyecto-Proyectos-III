using UnityEngine;
using System.Collections;


public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    public float gravityScale;
    public float fallingGravityScale;
    public float gravityAcceleration;
    [SerializeField] private Vector3 rotationDiff;
    public float rotationSpeed;

    [Header("Movement variables")]
    public float baseSpeed;
    [Tooltip("The time it takes to accelerate from standing still")] public float acceleration;
    public Vector3 velocity;
    [Tooltip("The velocity of the jump")] public float jump;

    public bool isGrounded;
    public bool canJump;
    public bool canWallJump;
    public bool isWalled;
    public bool isRunning;
    public float runningMultiplier = 1.5f;
    public int wallJumps;
    private int wallJumpsInternal;
    public Ray groundRay;
    public LayerMask groundLayers;
    public float rayDistance;
    public Collider closestWall;
    private Vector3 closestWallNormal;

    [Header("Wall Variables")]
    public LayerMask wallLayers;
    public Vector3 rayDirection = Vector3.right;
    public float wallRayDistance = 2f;
    public bool touchingWall = false;
    [Header("The ratio of horizontal movement to vertical movement, the higher the number is, the ")]
    public float hToVRatio;
    public float wallJump;

    [Header("Internal variables")]
    [Tooltip("x = horizontal direction \ny = frontal direction")] public Vector2 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //rb = gameObject.GetComponent<Rigidbody>();
        wallJumpsInternal = wallJumps;
    }
    private void FixedUpdate()
    {
        ApplyGravity();
    }
    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Update");
        CheckCameraRotation();

        Movement();

    }

    void CheckCameraRotation()
    {
        Quaternion camRotation = Camera.main.transform.rotation;

        camRotation.eulerAngles = new Vector3(0f, camRotation.eulerAngles.y, 0f);


        //rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, camRotation, Time.deltaTime * rotationSpeed);
        rb.rotation = Quaternion.Slerp(rb.transform.rotation, camRotation, Time.deltaTime * rotationSpeed);
        //rb.rotation = camRotation;
        rotationDiff = rb.transform.rotation.eulerAngles;
    }
    void Movement()
    {
        //Debug.Log("Movement");
        GetDirection();
        CheckJump();
        CheckWallJump();
        ApplySpeed();
    }
    void GetDirection()
    {
        direction.x = Input.GetAxis("Horizontal");
        direction.y = Input.GetAxis("Vertical");
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isRunning = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRunning = false;
        }
        //Debug.Log(direction);
    }
    void CheckJump()
    {
        //RaycastHit raycast 
        if (Physics.Raycast(transform.position, Vector3.down, rayDistance, groundLayers))
        {
            isGrounded = true;
            wallJumps = 1;
        }
        else
        {
            isGrounded = false;
        }
        if (Input.GetButtonDown("Jump"))
        {
            if (isWalled)
            {
                WallJump();
            }
            else if (isGrounded && canJump)
            {
                Jump();
            }
            else
            {
                StartCoroutine(Buffer(0.2f));
            }

        }

    }
    void CheckWallJump()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position - new Vector3(0f, -0.5f), wallRayDistance, wallLayers);
        isWalled = hitColliders.Length > 0;

        if (isWalled)
        {
            Collider nearest = null;
            float minDistance = float.MaxValue;
            Vector3 currentPosition = transform.position - new Vector3(0f, -0.5f);
            Vector3 normal = Vector3.zero;

            foreach (Collider col in hitColliders)
            {
                Vector3 closestPoint = col.ClosestPoint(currentPosition);
                float distance = Vector3.Distance(currentPosition, closestPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = col;

                    // Get normal of the wall
                    if (Physics.Raycast(closestPoint + (currentPosition - closestPoint).normalized * 0.1f,
                                        (closestPoint - currentPosition).normalized,
                                        out RaycastHit hit, 0.2f, wallLayers))
                    {
                        normal = hit.normal;
                    }
                }
            }

            closestWall = nearest;
            closestWallNormal = normal;
        }
    }
    void ApplyGravity()
    {
        if (rb.linearVelocity.y >= 0)
        {
            rb.AddForce(new Vector3(0f, -gravityScale, 0f));
        }
        else if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(new Vector3(0f, -fallingGravityScale, 0f));
        }


    }
    void Jump()
    {
        Debug.Log("Succesfully Jumped");
        Timer(0.2f);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jump, rb.linearVelocity.z);

    }
    void WallJump()
    {
        if (wallJumps == 0) return;
        Vector2 jumpRatio = new Vector2(hToVRatio, 1f);

        if (canJump && isWalled)
        {
            // Check if the wall is basically a floor
            if (Vector3.Dot(closestWallNormal, Vector3.up) > 0.9f)
            {
                Jump();
                return;
            }
            else
            {
                // Otherwise, perform a wall jump
                Vector3 jumpDirection = new Vector3(closestWallNormal.x, jumpRatio.y, closestWallNormal.z).normalized * wallJump;

                rb.linearVelocity = Vector3.zero;
                rb.AddForce(jumpDirection, ForceMode.Impulse);
                StartCoroutine(WallTimer(0.2f));
            }
            
        }
    }


    void ApplySpeed()
    {

        if (direction.x == 0 && direction.y == 0 && !isGrounded && !isWalled)
        {
            return;
        }

        float currentYVelocity = rb.linearVelocity.y;


        Vector3 forward = Quaternion.Euler(0, rotationDiff.y, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, rotationDiff.y, 0) * Vector3.right;


        Vector3 targetVelocity = (right * direction.x * baseSpeed * (isRunning? runningMultiplier: 1f)) + (forward * direction.y * baseSpeed * (isRunning ? runningMultiplier : 1f));
        targetVelocity = new Vector3(targetVelocity.x, currentYVelocity, targetVelocity.z);
        float threshold = 0.1f;

        if ((rb.linearVelocity - targetVelocity).sqrMagnitude < threshold * threshold)
        {
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            velocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.deltaTime);
            rb.linearVelocity = velocity;
        }
    }



    public IEnumerator Timer(float duration)
    {
        canJump = false;
        yield return new WaitForSeconds(duration);
        canJump = true;
    }
    IEnumerator WallTimer(float duration)
    {
        canWallJump = false;
        yield return new WaitForSeconds(duration);
    }
    public IEnumerator Buffer(float duration)
    {
        float interval = duration / 20f;
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(interval);

            if (isGrounded)
            {
                Jump();
                yield break;
            }
        }
    }

    private void OnDrawGizmos()
    {

    }
}
