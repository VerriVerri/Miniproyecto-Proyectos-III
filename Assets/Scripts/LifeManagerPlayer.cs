using UnityEngine;
using UnityEngine.UI;

public class LifeManagerPlayer : Life
{
    public float healingRate = 15f; 
    private float lastDamageTime;
    public Slider lifeSlider;

    void Start()
    {
        life = maxLife;
        lifeSlider.maxValue = maxLife;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {

        RegenerateLife();
        lifeSlider.value = life;
    }

    public override void LoseLife(float damage)
    {
        base.LoseLife(damage);
        lastDamageTime = Time.time; // Update the last time the player took damage
    }
    private void RegenerateLife()
    {
        if (Time.time - lastDamageTime > 3f)
            {
                GainLife(healingRate * Time.fixedDeltaTime); 
            }
    }
}
