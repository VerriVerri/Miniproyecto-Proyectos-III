using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

public class Guns : MonoBehaviour
{
    public Vector3 aimVector;
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
    public List<Vector3> ShotgunVector = new List<Vector3>();
    private List<(Vector3 position, float time)> debugPoints = new List<(Vector3, float)>();

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
                    ShootBoolet(4000f);
                    break;
                case 2:
                    break;
                case 3:
                    ShootMissile();
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
        aimVector = camCollider.cam1.transform.position + direction * aimDistance;
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
        targetObject = null;
        target = Vector3.zero;

        Vector3 direction = (camCollider.cameraTarget.position - camCollider.cam1.transform.position).normalized;
        Vector3 aimVector = camCollider.cam1.transform.position + direction * Mathf.Infinity;

        if (Physics.Raycast(camCollider.cam1.transform.position, direction, out RaycastHit hit, aimDistance))
        {
            Collider[] colliders = Physics.OverlapSphere(hit.point, 3f);
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    targetObject = col.transform;
                    target = col.transform.position;

                    // Single Debug.Log() after determining target
                    Debug.Log($"Target Found: {targetObject.name} at {target}");
                    return;
                }
            }

            target = hit.point;
        }
        else
        {
            target = aimVector;
        }

        // Single Debug.Log() after determining target (for non-enemy cases)
        Debug.Log($"No enemy found. Target set to: {target}");
    }

    public void Shoot()
    {
        ShootBoolet(2000f);
    }
    public void ShootMissile()
    {
        
        GameObject homingMissile = Instantiate(missile, transform.position + new Vector3(-0.58f, 0.366f, 0.843f), transform.rotation);
        HomingMissile missileScript = homingMissile.GetComponent<HomingMissile>();

        if (rb != null)
        {
           
            Rigidbody characterRb = GetComponent<Rigidbody>(); 

            Vector3 characterVelocity = characterRb != null ? characterRb.linearVelocity : Vector3.zero;

            
            rb.linearVelocity = characterVelocity + homingMissile.transform.forward * (rocketForce <1? 1: rocketForce);
        }
        

    }
    public void ShootBoolet(float force)
    {
        if (boolet == null) return;

        GameObject bullet = Instantiate(boolet, transform.position, Quaternion.identity);
        Boolet bulletScript = bullet.GetComponent<Boolet>();
        Destroy(bullet, 5f);
        if (bulletScript != null)
        {
            bulletScript.aimDirection = (aimVector - transform.position).normalized;
            bulletScript.force = force;
        }
    }
    public void ShotGun(int pellets)
    {

    }
    IEnumerator drawWireSphereFor3Seconds()
    {
        Gizmos.DrawSphere(Vector3.zero,0f);
        yield return new WaitForEndOfFrame();
    }
}
