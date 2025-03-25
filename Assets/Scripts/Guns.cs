using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Text;

public class Guns : MonoBehaviour
{
    public GameObject gunPoint;
    public Vector3 aimOffset;
    public Vector3 aimVector;
    public Vector3 aimDirection;
    public CameraCollider camCollider;
    public float aimDistance = 30f;
    Rigidbody rb;



    public GameObject boolet;
    public Vector3 booletRotation;
    public GameObject missile;
    public float rocketForce;
    public Transform missileTarget;
    public Vector3 targetPosition;
    [Range(1, 10)] [Tooltip("1 = Pistol, 2 = Shotgun, 3 = Missile")]
    public int shootMode;
    [Range(1, 20)]
    public int pellets;
    public float spread;
    public float knockBackMultiplier;
    public Vector3[] shotgunVector;
    private List<(Vector3 position, float time)> debugPoints = new List<(Vector3, float)>();

    int layerMask;

    [Header("Cooldown")]
    public float pistolCooldown, shotgunCooldown, missileCooldown;
    public bool pistolAvailable, shotgunAvailable, missileAvailable;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        
        GetAimDirection();
        if (Input.GetButtonDown("Shoot"))
        {
            switch (shootMode)
            {
                case 1:
                    if (pistolAvailable) 
                    {
                        ShootBoolet(4000f); 
                        StartCoroutine(PistolCooldown()); 
                    }
                    break;
                case 2:
                    if (shotgunAvailable) 
                    {
                        shotgunVector = new Vector3[pellets];
                        Shotgun(pellets);
                        ShootBoolet(4000f, pellets);
                        KnockBack(knockBackMultiplier);
                        StartCoroutine(ShotgunCooldown());
                    }
                    

                    break;
                case 3:
                    if (missileAvailable)
                    {

                        ShootMissile();
                        StartCoroutine(MissileCooldown());
                    }
                    break;

            }

        }
        if (Input.GetButtonDown("Shoot2"))
        {
            FindTarget( out missileTarget, out targetPosition);
            
        }
    }
    void GetAimDirection()
    {
        Vector3 direction = (camCollider.cameraTarget.position - camCollider.cam1.transform.position).normalized;
        aimDirection = direction;
        aimVector = camCollider.cam1.transform.position + direction * aimDistance;
    }
    Vector3 ApplySpread(Vector3 direction, float spread)
    {
        float spreadX = Random.Range(-spread, spread);
        float spreadY = Random.Range(-spread, spread);

        Quaternion spreadRotation = Quaternion.Euler(spreadX,spreadY,0f);

        return spreadRotation * direction;


    }
    private void OnDrawGizmos()
    {
        if (camCollider != null && camCollider.cam1 != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(camCollider.cam1.transform.position, (aimVector - camCollider.cam1.transform.position).normalized * aimDistance);
            Gizmos.DrawWireSphere(aimVector, 0.2f);
        }
        Gizmos.color = Color.red;

        for (int i = debugPoints.Count - 1; i >= 0; i--)
        {
            if (Time.time > debugPoints[i].time)
            {
                debugPoints.RemoveAt(i);
            }
            else
            {
                Gizmos.DrawSphere(debugPoints[i].position, 0.3f);
            }
        }
    }
    public void FindTarget(out Transform targetObject, out Vector3 target)
    {
        layerMask = LayerMask.GetMask("Floor", "Wall", "Enemy");
        targetObject = null;
        target = Vector3.zero;

        Vector3 direction = (camCollider.cameraTarget.position - camCollider.cam1.transform.position).normalized;
        Vector3 aimVector = camCollider.cam1.transform.position + direction * aimDistance;
        GameObject firstcollision;

        StartCoroutine(DrawDebugRay(camCollider.cam1.transform.position, direction, aimDistance, Color.red, 3f)); // Draws ray for 3 seconds

        if (Physics.Raycast(camCollider.cam1.transform.position, direction, out RaycastHit hit, aimDistance, layerMask))
        {
            target = hit.point;
            StartCoroutine(DrawDebugSphere(hit.point, 3f, Color.green));
            string overlapObjects;
            Collider[] colliders = Physics.OverlapSphere(hit.point, 3f);
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    targetObject = col.transform;
                    target = col.transform.position;

                    StartCoroutine(DrawDebugSphere(target, 3f, Color.yellow));
                    Debug.Log($"Target Found: {targetObject.name} at {target}");
                    return;
                }
                
            }
            if (colliders.Length != 0)
            {
                firstcollision = colliders[0].gameObject;
                Debug.Log($"No enemy found, collided with {firstcollision}.");
            }
        }
        else
        {
            target = Vector3.zero;
            Debug.Log("No solid target found");
        }

        StartCoroutine(DrawDebugSphere(target, 3f, Color.blue));
        //Debug.Log($"No enemy found. Target set to: {target}");
    }
    public void ShootMissile()
    {
        
        GameObject homingMissile = Instantiate(missile, gunPoint.transform.position, Camera.main.transform.rotation);
        HomingMissile missileScript = homingMissile.GetComponent<HomingMissile>();
        Rigidbody missileRb = homingMissile.GetComponent<Rigidbody>();
        missileScript.target = missileTarget;
        missileScript.targetPosition = this.targetPosition;
        
           
            Rigidbody characterRb = GetComponent<Rigidbody>(); 

            Vector3 characterVelocity = characterRb != null ? characterRb.linearVelocity : Vector3.zero;

            
            missileRb.linearVelocity = characterVelocity / 2 + homingMissile.transform.forward * (rocketForce <1? 1: rocketForce);
        
        Collider missileCollider = homingMissile.GetComponent<Collider>();
        Collider characterCollider = GetComponent<Collider>();

        if (missileCollider != null && characterCollider != null)
        {
            Physics.IgnoreCollision(missileCollider, characterCollider);
        }


    }
    public void ShootBoolet(float force)
    {
        if (boolet == null) return;

        GameObject bullet = Instantiate(boolet, gunPoint.transform.position, Quaternion.identity);
        Boolet bulletScript = bullet.GetComponent<Boolet>();
        Destroy(bullet, 5f);
        if (bulletScript != null)
        {
            bulletScript.aimDirection = (aimVector - transform.position).normalized;
            bulletScript.force = force;
        }
    }
    public void ShootBoolet(float force, int amount)
    {
        if (boolet == null) return;
        for (int i = 0; i < amount; i++)
        {
            GameObject bullet = Instantiate(boolet, gunPoint.transform.position, Quaternion.identity);
            Boolet bulletScript = bullet.GetComponent<Boolet>();
            Destroy(bullet, 5f);
            if (bulletScript != null)
            {
                bulletScript.aimDirection = (shotgunVector[i] - transform.position).normalized;
                bulletScript.force = force;
            }
        }
        
    }
    public void Shotgun(int pellets)
    {
        Vector3[] shotgunVector = new Vector3[pellets];
        GetAimDirection();
        for (int i = 0; i < shotgunVector.Length; i++)
        {
            float x = Random.Range(-spread,spread);
            float y = Random.Range(-spread,spread);

            Quaternion rotation = Quaternion.Euler(x,y,0f);
            Vector3 shotgunDirection = rotation * aimDirection;
            shotgunVector[i] = camCollider.cam1.transform.position + shotgunDirection * aimDistance;
            this.shotgunVector[i] = shotgunVector[i];
            Debug.Log($"Set pellet {i}");
        }


        
    }
    public void KnockBack(float amount)
    {
        rb.AddForce(-aimDirection.normalized * amount * 100f);
    }
    private IEnumerator DrawDebugSphere(Vector3 position, float duration, Color color)
    {
        float timeElapsed = 0f;
        float radius = 0.5f; // Sphere size
        int segments = 16;   // Number of lines for wireframe

        while (timeElapsed < duration)
        {
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * Mathf.PI * 2;
                float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2;

                // XZ Plane (Top-down view)
                Vector3 p1 = position + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius;
                Vector3 p2 = position + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * radius;
                Debug.DrawLine(p1, p2, color);

                // XY Plane (Front view)
                p1 = position + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
                p2 = position + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;
                Debug.DrawLine(p1, p2, color);

                // YZ Plane (Side view)
                p1 = position + new Vector3(0, Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
                p2 = position + new Vector3(0, Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;
                Debug.DrawLine(p1, p2, color);
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator DrawDebugRay(Vector3 origin, Vector3 direction, float distance, Color color, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            Debug.DrawRay(origin, direction * distance, color);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator ShotgunCooldown()
    {
        shotgunAvailable = false;
        yield return new WaitForSeconds(shotgunCooldown);
        shotgunAvailable = true;
    }
    private IEnumerator PistolCooldown()
    {
        pistolAvailable = false;
        yield return new WaitForSeconds(pistolCooldown);
        pistolAvailable = true;
    }
    private IEnumerator MissileCooldown()
    {
        missileAvailable = false;
        yield return new WaitForSeconds(missileCooldown);
        missileAvailable = true;
    }
}
