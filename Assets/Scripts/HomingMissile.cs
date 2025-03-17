using UnityEngine;
using System.Collections;

public class HomingMissile : MonoBehaviour
{
    public Transform target;
    public Vector3 targetPosition;
    public Rigidbody rb;
    public float explosionForce = 1000f;
    public float explosionRadius = 8f;
    public float upwardsModifier = 1f;
    public GameObject explosionPrefab;

    public float initialSpeed;
    public float timeToActivate = 1f;
    public bool activatedRocket;

    public float maxSpeed = 9f;
    public float currentSpeed;
    public float linearAcceleration;

    public float maxRotation;
    public bool maxRotationAccelerating;
    public float maxRotationAccelerationRate = 1.3f;
    private float currentRotation;
    public float angularAcceleration;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ActivateRocket());
        rb = GetComponent < Rigidbody>();
        rb.useGravity = true;
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
            if (!maxRotationAccelerating)
            {
                StartCoroutine(CheckIfStuck());
            }
            else
            {
                AccelerateMaxRotation();
            }
            // Apply linear and angular acceleration
            LinearAcceleration();
            AngularAcceleration();

            // Calculate and apply rotation
            CalculateRotation();

            // Calculate and apply velocity
            CalculateVelocity();
        }
    }
    void LinearAcceleration()
    {
        if (currentSpeed >= maxSpeed)
        {
            currentSpeed = maxSpeed;
        }
        else
        {
            currentSpeed = currentSpeed + linearAcceleration >= maxSpeed? maxSpeed: currentSpeed + linearAcceleration;
        }
    }

    void AngularAcceleration() //Calculates the current rotation
    {
        if (currentRotation >= maxRotation)
        {
            currentRotation = maxRotation;
        }
        else
        {
            currentRotation = currentRotation + angularAcceleration >= maxRotation ? maxRotation : currentRotation + angularAcceleration;
        }
    }
    void CalculateRotation() //Calculates the rotation from this rigidbody to the target location, using the current rotation variable for a smooth rotation
    {
        Vector3 directionToTarget = targetPosition - transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, currentRotation * Time.deltaTime);

    }
    void CalculateVelocity() //Moves the object in the direction of the previously calculated rotation, using the currentSpeed variable to calculate the magnitude of the velocity vector
    {
        Vector3 directionToTarget = transform.forward;

        Vector3 velocity = directionToTarget * currentSpeed;

        rb.linearVelocity = velocity;
    }
    void AccelerateMaxRotation()
    {
        maxRotation = maxRotation * maxRotationAccelerationRate;
    }
    IEnumerator DrawSphere(Vector3 explosionPoint)
    {
        float drawTime = 3f;
        float timer = 0f;
        while (timer < drawTime)
        {
            // You can use Debug.DrawLine to simulate a sphere
            float sphereRadius = 1f;  // Set the radius of the sphere here
            int segments = 12;
            for (int i = 0; i < segments; i++)
            {
                // Draw lines around the explosion to form a sphere
                float angle = (i * Mathf.PI * 2) / segments;
                Vector3 lineStart = explosionPoint + new Vector3(Mathf.Sin(angle) * sphereRadius, 0, Mathf.Cos(angle) * sphereRadius);
                Vector3 lineEnd = explosionPoint + new Vector3(Mathf.Sin(angle + Mathf.PI / 2) * sphereRadius, 0, Mathf.Cos(angle + Mathf.PI / 2) * sphereRadius);
                Debug.DrawLine(lineStart, lineEnd, Color.red);
            }

            // Increment the timer
            timer += Time.deltaTime;

            // Wait until the next frame
            yield return null;
        }

    }
    IEnumerator CheckIfStuck()
    {
        yield return new WaitForSeconds(3f);
        maxRotationAccelerating = true;

    }
    IEnumerator ActivateRocket()
    {
        yield return new WaitForSeconds(timeToActivate);
        activatedRocket = true;
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("WallLimiter"))
        {
            return; // Ignore this collision
        }
        // Trigger explosion and destroy missile
        TriggerExplosion(transform.position - rb.linearVelocity.normalized * 3f);
        Destroy(gameObject);
    }
    private void TriggerExplosion(Vector3 explosionCenter)
    {
        GameObject explosionInstance = Instantiate(explosionPrefab, explosionCenter, Quaternion.identity);
        // Find all objects with the "Player" or "Enemy" tag
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Combine both arrays into one
        GameObject[] enemiesAndPlayers = new GameObject[players.Length + enemies.Length];
        players.CopyTo(enemiesAndPlayers, 0);
        enemies.CopyTo(enemiesAndPlayers, players.Length);

        // Loop through all objects
        foreach (GameObject obj in enemiesAndPlayers)
        {
            // Get the Rigidbody component of the object
            Rigidbody targetRb = obj.GetComponent<Rigidbody>();

            // Check if the object has a Rigidbody
            if (targetRb != null)
            {
                // Calculate the direction from the explosion point to the target object
                Vector3 directionToTarget = obj.transform.position - explosionCenter;

                // Calculate the distance between the explosion and the target
                float distance = directionToTarget.magnitude;

                // If the target is within the explosion radius, apply the full force
                if (distance <= explosionRadius)
                {
                    // Normalize the direction to get the direction only
                    directionToTarget.Normalize();

                    // Apply the full explosion force in the direction of the target
                    targetRb.AddForce(directionToTarget * explosionForce, ForceMode.Impulse);
                }
            }
        }
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
