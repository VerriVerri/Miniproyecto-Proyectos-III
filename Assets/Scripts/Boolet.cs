using UnityEngine;
using System.Collections;

public class Boolet : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    public Vector3 gravity;
    public float force;
    public Vector3 aimDirection;
    public Guns guns;
    public float damage = 20;
    public LayerMask enemyLayer;
    public bool isFading;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetBullet();
    }
    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ApplyGravity()
    {
        //rb.AddForce()
    }
    protected void AddInitialForce()
    {
        Quaternion additionalRotation = Camera.main.transform.rotation;
        transform.rotation = additionalRotation * Quaternion.Euler(90f,0f,0f);
        rb.AddForce(aimDirection * force);

    }
    /// <summary>
    /// Makes the capsule's top aim at the 
    /// </summary>
    void CapsuleLookAt() 
    {

    }
    private void Collide()
    {
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        TrailRenderer renderer = GetComponent<TrailRenderer>();
        mesh.enabled = false;
        collider.enabled = false;
        rb.linearVelocity = Vector3.zero;
        StartCoroutine(DisappearAfter(1));
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Collide();
        }
    }
        protected void SetBullet()
    {
        guns = FindFirstObjectByType<Guns>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;

        }
        rb.useGravity = false;
        AddInitialForce();
    }
    IEnumerator DisappearAfter(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

}
