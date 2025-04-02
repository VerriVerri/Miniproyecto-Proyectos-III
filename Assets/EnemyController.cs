using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    private Renderer characterRenderer;
    public Material originalMaterial;
    public Material clonedMaterial;
    public float fresnelPower;
    public Guns guns; // Reference to the Guns script
    public LifeManagerEnemy lifeManager;
    public bool isTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        guns = FindFirstObjectByType<Guns>();
        // Get the Renderer component of the character (e.g., a mesh renderer)
        characterRenderer = GetComponent<Renderer>();
        originalMaterial = characterRenderer.material;

        if (characterRenderer != null && originalMaterial != null)
        {
            // Make a copy of the material (this creates a unique instance)
            clonedMaterial = new Material(originalMaterial);

            fresnelPower = 0;
            // Assign the cloned material to the renderer
            characterRenderer.material = clonedMaterial;

        }
        else
        {
            Debug.LogError("Renderer or Original Material not assigned.");
        }
        if (lifeManager == null)
        {
            lifeManager = GetComponentInChildren<LifeManagerEnemy>();
            if (lifeManager == null)
            {
                Debug.LogError("LifeManagerEnemy is not assigned or found on this GameObject.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        clonedMaterial.SetFloat("_FresnelPower", fresnelPower);
        // Check if the target is marked and if missileTarget matches this one

    }

    public void OnMarked()
    {
        StopAllCoroutines();
        StartCoroutine(GraduallyChangeFresnelPower(50, 1.5f)); // Gradually increase fresnel power
    }

    public void OnUnmarked()
    {
        StopAllCoroutines();
        StartCoroutine(GraduallyChangeFresnelPower(0, 0.5f)); // Gradually reset fresnel power to 0
    }

    private IEnumerator GraduallyChangeFresnelPower(float targetValue, float duration)
    {
        float startValue = fresnelPower;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            fresnelPower = Mathf.Lerp(startValue, targetValue, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fresnelPower = targetValue;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Boolet boolet = other.gameObject.GetComponent<Boolet>();
            Debug.Log("Enemy collided with bullet");
            lifeManager.LoseLife(boolet.damage);
        }
    }
}
