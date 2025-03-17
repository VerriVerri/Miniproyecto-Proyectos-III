using UnityEngine;
using System.Collections.Generic;

public class Guns : MonoBehaviour
{
    public Vector3 aimVector;
    public CameraCollider camCollider;
    public float aimDistance = 30f;


    public GameObject boolet;
    public Vector3 booletRotation;
    [Range(1, 10)] [Tooltip("1 = Pistol, 2 = Shotgun")]
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
    }
    // Update is called once per frame
    void Update()
    {
        GetAimDirection();
        if (Input.GetButtonDown("Shoot"))
        {
            Shoot();
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
    public void Shoot()
    {
        ShootBoolet(2000f);
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
}
