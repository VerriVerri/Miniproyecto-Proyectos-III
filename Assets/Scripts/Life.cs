using UnityEngine;

public abstract class Life : MonoBehaviour
{
    public float maxLife;
    public float life;
    
    public virtual void LoseLife(float damage)
    {
        life = life - damage < 0f ? 0f : life - damage;
        if (life == 0f)
        {
            Die();
        }
    }

    public virtual void GainLife(float healing)
    {
        life = life + healing > maxLife ? maxLife : life + healing;
    }

    public virtual void Die()
    {
        Debug.LogError("No die mechanic used");
    }

}
