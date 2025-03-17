using UnityEngine;
using UnityEditor; // Needed for EditorUtility functions

[ExecuteInEditMode] // Ensures this runs in the editor
public class TransparencyController : MonoBehaviour
{
    [Range(0, 255)] public int alpha = 255;
    [Range(0, 255)] public int maxAlpha = 255;
    [Range(0, 255)] public int minAlpha = 0;

    public float minDistance = 5f; // Minimum distance for transparency
    public float maxDistance = 20f; // Maximum distance for transparency

    // Expose these fields so they can be assigned in the editor
    public Camera mainCamera; // Reference to the camera
    public GameObject targetObject; // Reference to the object whose transparency will change

    private Renderer objRenderer;
    public Material objMaterial;

    void Update()
    {
        if (mainCamera != null && targetObject != null)
        {
            ApplyAlphaBasedOnDistance();
        }
    }

    void ApplyAlphaBasedOnDistance()
    {
        if (objRenderer == null)
            objRenderer = targetObject.GetComponent<Renderer>();

        if (objRenderer != null)
        {
            if (objMaterial == null)
                objMaterial = objRenderer.sharedMaterial; // Use sharedMaterial to modify actual material

            if (objMaterial != null)
            {
                // Calculate the distance from the assigned camera to the target object
                float distance = Vector3.Distance(mainCamera.transform.position, targetObject.transform.position);

                // If the distance is below the minimum, set alpha to minAlpha (more transparent)
                if (distance <= minDistance)
                {
                    alpha = minAlpha;
                }
                // If the distance is above the maximum, set alpha to maxAlpha (fully opaque)
                else if (distance >= maxDistance)
                {
                    alpha = maxAlpha;
                }
                // Otherwise, calculate alpha based on linear interpolation
                else
                {
                    float t = (distance - minDistance) / (maxDistance - minDistance); // Linear factor
                    alpha = Mathf.RoundToInt(Mathf.Lerp(minAlpha, maxAlpha, t)); // Reverse the alpha interpolation
                }

                // Ensure alpha is within bounds
                alpha = Mathf.Clamp(alpha, minAlpha, maxAlpha);
                ApplyAlpha();
            }
        }
    }

    void ApplyAlpha()
    {
        if (objMaterial != null)
        {
            // Ensure the material supports transparency
            objMaterial.SetOverrideTag("RenderType", "Transparent");
            objMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            objMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            objMaterial.SetInt("_ZWrite", 0);
            objMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            // Adjust the alpha value in the material
            if (objMaterial.HasProperty("_Base_Color")) // Standard Shader
            {
                Color color = objMaterial.GetColor("_Base_Color");
                color.a = alpha / 255f;
                objMaterial.SetColor("_Base_Color", color);
            }

            // Mark material as dirty so Unity updates it
#if UNITY_EDITOR
            EditorUtility.SetDirty(objMaterial);
            EditorUtility.SetDirty(gameObject);
            SceneView.RepaintAll(); // Force scene update
#endif
        }
    }
}








