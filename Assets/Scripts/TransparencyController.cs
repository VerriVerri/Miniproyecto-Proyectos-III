using UnityEngine;

public class TransparencyController : MonoBehaviour
{
    public float maxDistance = 5f;  // Distance at which alpha is max
    public float minDistance = 1f;  // Distance at which alpha is 10
    public float maxAlpha = 1f;  // Full opacity
    public float minAlpha = 10f / 255f;  // Converted to 0-1 range

    private Renderer objectRenderer;  // The renderer of the object
    private Material material;  // Material with the Shader Graph
    public float distance;
    void Start()
    {
        // Get the renderer component and the material
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError("No Renderer found on this GameObject.");
            return;
        }

        // Get the material of the object (Shader Graph material)
        material = objectRenderer.material;
    }

    void Update()
    {
        if (!material) return;

        // Get the main camera and calculate the distance to it
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        distance = Vector3.Distance(transform.position, mainCamera.transform.position);

        // Clamp distance within min and max range
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Calculate alpha using linear interpolation (lerp)
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (distance - minDistance) / (maxDistance - minDistance));

        // Set the new alpha value to the shader's color property
        Color color = material.GetColor("_Base_Color");  // Get the color from Shader Graph
        color.a = alpha;
        material.SetColor("_Base_Color", color);  // Set the updated color back to Shader Graph
    }
}


