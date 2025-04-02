using UnityEngine;

[ExecuteInEditMode] // Makes the script run in the editor as well
public class TextureScaler : MonoBehaviour
{
    public Renderer objectRenderer;

    private Vector3 previousScale;

    // Run in the editor and during play mode
    void Start()
    {
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();  // Get the renderer if not set

        previousScale = transform.localScale;  // Store initial scale
    }

    void Update()
    {
        // In the editor or during play mode, check if the scale has changed
        if (transform.localScale != previousScale)
        {
            AdjustTextureSize();
            previousScale = transform.localScale;
        }
    }

    // Adjust the texture tiling based on the scale of the object
    void AdjustTextureSize()
    {
        Vector3 scale = transform.localScale;
        // Assuming you're adjusting tiling based on the x and y axes (adjust as needed)
        objectRenderer.material.mainTextureScale = new Vector2(1 / scale.x, 1 / scale.y);
    }
}

