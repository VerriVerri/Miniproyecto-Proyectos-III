using UnityEngine;

using System.Collections;

public class LifeManagerEnemy : Life
{
    public GameObject parent;
    Material material;
    public Renderer objectRenderer;
    public float fadeDuration = 1.5f;
    private void Start()
    {
        if (objectRenderer != null)
        {
            // Clone the material to ensure unique changes per object
            material = new Material(objectRenderer.material);
            objectRenderer.material = material;
        }
        else
        {
            Debug.LogWarning("Renderer not assigned!");
        }
        
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
        StartCoroutine(FadeOut());
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


    private IEnumerator FadeOut()
    {
        Color color = material.GetColor("_Base_Color"); // Get current color
        float startAlpha = color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            color.a = newAlpha;
            material.SetColor("_Base_Color", color);
            yield return null; // Wait for the next frame
        }

        // Ensure alpha is exactly 0 at the end
        color.a = 0f;
        material.SetColor("_Base_Color", color);
        Destroy(parent);
        Destroy(gameObject);
    }

}
