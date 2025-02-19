using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    /// <summary>
    /// The hypothetical position at which the player will aim. This is not the direction of the bullet
    /// </summary>
    public Vector3 aimPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void TakeDamage(float damage)
    {
        currentHealth -= Mathf.FloorToInt(damage);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {


    }
}
