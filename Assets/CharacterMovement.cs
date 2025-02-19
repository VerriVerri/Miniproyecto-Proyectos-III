using UnityEngine;
using System.Collections;


public class CharacterMovement : MonoBehaviour
{
    [SerializeField]private Rigidbody rb;
    [SerializeField] private Vector3 rotationDiff;
    public float rotationSpeed;

    [Header("Movement variables")]
    public float baseSpeed;
    [Tooltip("The time it takes to accelerate from standing still")]public float acceleration;
    public Vector3 velocity;
    [Tooltip("The velocity of the jump")] public float jump;

    public bool isGrounded;
    public bool canJump;
    public Ray groundRay;
    public LayerMask groundLayers;
    public float rayDistance;

    [Header("Wall Variables")]
    public LayerMask wallLayers;
    public Vector3 rayDirection = Vector3.right; 
    public float wallRayDistance = 2f; 
    public bool touchingWall = false; 

    [Header("Internal variables")]
    [Tooltip("x = horizontal direction \ny = frontal direction")]public Vector2 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //rb = gameObject.GetComponent<Rigidbody>();
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
        rb.rotation = camRotation;
        rotationDiff = rb.transform.rotation.eulerAngles;
    }
    void Movement()
    {
        //Debug.Log("Movement");
        GetDirection();
        CheckJump();
        ApplySpeed();
    }
    void GetDirection()
    {
        direction.x = Input.GetAxis("Horizontal");
        direction.y = Input.GetAxis("Vertical");
        //Debug.Log(direction);
    }
    void CheckJump()
    {
        //RaycastHit raycast 
        if (Physics.Raycast(transform.position, Vector3.down, rayDistance, groundLayers))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump();
            }
            else
            {
                StartCoroutine(Buffer(0.2f));
            }

        }

    }
    void Jump()
    {
        if (canJump && isGrounded)
        {
            Debug.Log("Succesfully Jumped");
            Timer(0.2f);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jump, rb.linearVelocity.z);
        }
    }
    void ApplySpeed()
    {
        float currentYVelocity = rb.linearVelocity.y;

        
        Vector3 forward = Quaternion.Euler(0, rotationDiff.y, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, rotationDiff.y, 0) * Vector3.right;

        
        Vector3 targetVelocity = (right * direction.x * baseSpeed) + (forward * direction.y * baseSpeed);
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
