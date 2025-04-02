using UnityEngine;
using System.Collections;

public class LifeManager : Life
{
    private void Start()
    {

        life = maxLife;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            LoseLife(20f);
        }
    }
    public override void Die()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with something");
        if (other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            Debug.Log("Collided with bullet");
            LoseLife(20f);
        }
    }


}
