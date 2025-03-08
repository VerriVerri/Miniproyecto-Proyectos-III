using UnityEngine;

public class Boolet : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    public Vector3 gravity;
    public float force;
    public Vector3 aimDirection;
    public Guns guns;

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

}
