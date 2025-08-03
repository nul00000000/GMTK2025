using UnityEngine;

[ExecuteInEditMode] // Allows the script to run in the editor, showing changes immediately
public class Scaler : MonoBehaviour
{
    public float pixelsPerUnit = 100f; // Defines how many texture pixels correspond to one world unit

    private Vector3 previousScale;

    void Start()
    {
        previousScale = transform.localScale;
        UpdateMaterialTiling();
    }

    void Update()
    {
        // Only update if the object's scale has changed
        if (transform.localScale != previousScale)
        {
            UpdateMaterialTiling();
            previousScale = transform.localScale;
        }
    }

    void UpdateMaterialTiling()
    {
        Renderer objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null && objectRenderer.sharedMaterial != null)
        {
            // Calculate desired tiling based on current scale and pixels per unit
            Vector2 newTiling = new Vector2(
                transform.localScale.x * (objectRenderer.bounds.size.x / objectRenderer.bounds.extents.x) / pixelsPerUnit,
                transform.localScale.y * (objectRenderer.bounds.size.y / objectRenderer.bounds.extents.y) / pixelsPerUnit
            );

            // Apply the new tiling to the material's main texture
            objectRenderer.sharedMaterial.mainTextureScale = newTiling;
        }
    }
}