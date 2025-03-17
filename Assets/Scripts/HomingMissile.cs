using UnityEngine;
using System.Collections;

public class HomingMissile : MonoBehaviour
{
    public Transform target;
    public Vector3 targetPosition;
    public Rigidbody rb;

    public float initialForce;
    public float timeToActivate = 1f;
    public bool activatedRocket;

    public float maxAngularVelocity = 9.42f;
    public float thrustForce = 10f;  
    public float acceleration = 5f;  
    public float maxThrust = 100f;   
    public float gravityReductionFactor = 2f;
    public float maxVelocity = 50f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ActivateRocket());
        rb = GetComponent < Rigidbody>();
        rb.maxAngularVelocity = maxAngularVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            targetPosition = target.position;
        }
    }

    private void FixedUpdate()
    {
        if (activatedRocket)
        {
            CalculateRotation();
        }
    }
    void ApplyDrag()
    {
        Vector3 velocity = rb.linearVelocity;

        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

        float airResistance = horizontalVelocity.sqrMagnitude * 0.05f;

        Vector3 resistanceForce = -horizontalVelocity.normalized * airResistance;

        rb.AddForce(resistanceForce, ForceMode.Force);
    }
    void ApplyInitialTrust()
    {

    }
    void AdjustGravity()
    {
        // Gravity effect decreases as thrustForce approaches maxThrust
        float gravityMultiplier = Mathf.Lerp(1f, 0f, thrustForce / maxThrust);

        // Apply an upward force to counteract gravity
        Vector3 gravityCompensation = Vector3.up * (Physics.gravity.y * rb.mass * (1f - gravityMultiplier) * gravityReductionFactor);
        rb.AddForce(gravityCompensation, ForceMode.Acceleration);
    }
    void CalculateRotation()
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

        // Calculate the difference between the current rotation and the target rotation
        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(transform.rotation);

        Vector3 axis;
        float angle;
        deltaRotation.ToAngleAxis(out angle, out axis);

        if (angle > 0.1f)
        {
           rb.AddTorque(axis * angle * Time.deltaTime, ForceMode.VelocityChange);
        }

    }
    void ApplyThrust()
    {
        // Increase thrust gradually, but don't exceed max thrust force
        thrustForce = Mathf.Min(thrustForce + acceleration * Time.fixedDeltaTime, maxThrust);

        // Apply force in the forward direction
        rb.AddForce(transform.forward * thrustForce, ForceMode.Force);

        // Limit max velocity (but keep the y-component free)
        Vector3 velocity = rb.linearVelocity;
        float horizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude; // Speed in X-Z plane

        if (horizontalSpeed > maxVelocity)
        {
            // Scale velocity to max speed while keeping the Y velocity unaffected
            Vector3 clampedVelocity = velocity.normalized * maxVelocity;
            clampedVelocity.y = velocity.y; // Preserve vertical speed
            rb.linearVelocity = clampedVelocity;
        }
    }

    void AlignWithVelocity()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
           
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity.normalized);

            // Smoothly rotate towards the velocity direction
            transform.rotation = targetRotation;
        }
    }
    IEnumerator ActivateRocket()
    {
        yield return new WaitForSeconds(timeToActivate);
        activatedRocket = true;
    }
    void OnDrawGizmosSelected()
    {
        // Set the color of the Gizmo
        Gizmos.color = Color.red;

        // Determine the position to draw the sphere
        Vector3 drawPosition = (target != null) ? target.position : targetPosition;

        // Draw a wireframe sphere at the target position
        Gizmos.DrawWireSphere(drawPosition, 1f); // The second parameter is the radius of the sphere
    }

}
