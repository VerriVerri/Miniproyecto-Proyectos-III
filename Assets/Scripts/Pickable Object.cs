using UnityEngine;

public class PickableObject : MonoBehaviour
{
    public WeaponType typeOfWeapon;
    public WeaponManager manager;
    public float amplitude = 0.5f;
    public float frequency = 1f;
    public float angularVelocity = 1f;
    public bool eternal = true;

    private Vector3 startPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        manager = FindFirstObjectByType<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Oscillate();
        Rotate();
    }
    private void FixedUpdate()
    {
        
    }
    void Oscillate()
    {
        float yOffset = amplitude * Mathf.Sin(Time.time * frequency * 2 * Mathf.PI);
        transform.position = startPosition + new Vector3(0, yOffset, 0);
    }
    void Rotate()
    {
        float deltaAngle = angularVelocity * Time.deltaTime;
        Quaternion rotation = Quaternion.Euler(new Vector3 (0f, deltaAngle, 0f));
        transform.rotation *= rotation;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.PickupWeapon(typeOfWeapon);
            if (!eternal)
            {
                Destroy(gameObject);
            }
        }
    }
}
