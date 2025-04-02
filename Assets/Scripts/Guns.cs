using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Text;

public class Guns : MonoBehaviour
{
    public WeaponManager managerWeapons;


    public GameObject gunPoint;
    public Vector3 aimOffset;
    public Vector3 aimVector;
    public Vector3 aimDirection;
    public CameraCollider camCollider;
    public float aimDistance = 30f;
    Rigidbody rb;
    public bool canShoot = true;
    public GameObject shotGunEffect;


    public GameObject boolet;
    public Vector3 booletRotation;
    public GameObject missile;
    public float rocketForce;
    public Transform missileTarget;
    public Vector3 targetPosition;
    public Transform previousTarget;
    [Range(0, 10)] [Tooltip("1 = Pistol, 2 = Shotgun, 3 = Missile")]
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
        previousTarget = transform;
        shotGunEffect.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        //CheckWeapon();
        GetAimDirection();
        if (Input.GetButtonDown("Shoot") && canShoot)
        {
            switch (shootMode)
            {
                case 1:
                    if (pistolAvailable) 
                    {
                        if (managerWeapons.pistolAmmoLeft == 0) return;
                        managerWeapons.OnShootWeapon(WeaponType.Pistol);
                        ShootBoolet(4000f); 
                        StartCoroutine(PistolCooldown()); 
                    }
                    break;
                case 2:
                    if (shotgunAvailable)
                    {
                        if (managerWeapons.shotgunAmmoLeft == 0) return;
                        GameObject effectInstance = Instantiate(shotGunEffect, shotGunEffect.transform.position, shotGunEffect.transform.rotation);
                        effectInstance.transform.SetParent(shotGunEffect.transform.parent); // Match parent
                        effectInstance.SetActive(true); // Ensure it's active
                        Destroy(effectInstance, 5f); // Destroy after 5 seconds

                        managerWeapons.OnShootWeapon(WeaponType.Shotgun);
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
                        EnemyController controller = new EnemyController();
                        if (managerWeapons.missileAmmoLeft == 0) return;
                        ShootMissile();
                        StartCoroutine(MissileCooldown());
                        managerWeapons.OnShootWeapon(WeaponType.RPG);
                        missileTarget = null;
                        targetPosition = Vector3.zero;
                        
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
        Vector3 aimVector = camCollider.cam1.transform.position + direction * (7 * aimDistance);

        StartCoroutine(DrawDebugRay(camCollider.cam1.transform.position, direction, 7 * aimDistance, Color.red, 3f)); // Draws ray for 3 seconds

        if (Physics.Raycast(camCollider.cam1.transform.position, direction, out RaycastHit hit, 7 * aimDistance, layerMask, QueryTriggerInteraction.Collide))
        {
            target = hit.point;
            StartCoroutine(DrawDebugSphere(hit.point, 3f, Color.green));

            Collider[] colliders = Physics.OverlapSphere(hit.point, 3f);
            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity; // Initialize with a large value

            // Iterate through each collider
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    float distanceToCenter = Vector3.Distance(hit.point, col.transform.position);

                    if (distanceToCenter < closestDistance)
                    {
                        closestDistance = distanceToCenter;
                        closestTarget = col.transform;
                    }
                }
            }

            // If a closest target was found, set it
            if (closestTarget != null)
            {
                targetObject = closestTarget;
                target = closestTarget.position;

                // Get the EnemyController of the closest target and activate OnMarked for it
                if (closestTarget.TryGetComponent<EnemyController>(out EnemyController newTargetController))
                {
                    // Unmark all enemies
                    EnemyController[] allEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                    foreach (EnemyController enemyController in allEnemies)
                    {
                        enemyController.OnUnmarked(); // Unmark other enemies
                    }

                    // Mark the new closest target
                    newTargetController.OnMarked();
                }

                StartCoroutine(DrawDebugSphere(target, 3f, Color.yellow));
                Debug.Log($"Target Found: {targetObject.name} at {target}");
            }
            else
            {
                EnemyController[] allEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                foreach (EnemyController enemyController in allEnemies)
                {
                    enemyController.OnUnmarked(); // Unmark other enemies
                }
                Debug.Log("No valid target found.");
            }
        }
        else
        {
            EnemyController[] allEnemies = FindObjectsOfType<EnemyController>();
            foreach (EnemyController enemyController in allEnemies)
            {
                enemyController.OnUnmarked(); // Unmark other enemies
            }
            target = Vector3.zero;
            Debug.Log("No solid target found");
        }

        StartCoroutine(DrawDebugSphere(target, 3f, Color.blue));
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
        if (boolet == null && managerWeapons.pistolAmmoLeft == 0) return;

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
        rb.linearVelocity /= 2;
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

    private void CheckWeapon()
    {
        switch (managerWeapons.primaryWeapon) 
        {
            case WeaponType.Pistol:
                managerWeapons.StartSwapCoolDown(0.27f, 0);
                shootMode = 1;
                break;
            case WeaponType.Shotgun:
                managerWeapons.StartSwapCoolDown(0.63f, 0);
                shootMode = 2;
                break;
            case WeaponType.RPG:
                managerWeapons.StartSwapCoolDown(1.43f, 0);
                shootMode = 3;
                break;
        }


    }
}
