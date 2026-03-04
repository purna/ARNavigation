using UnityEngine;
using System;

/// <summary>
/// FloatingIcon represents a point of interest icon that floats in AR space.
/// These icons billboard (face the camera) and display information about locations.
///
/// Corresponds to Unreal Engine's BP_FloatingIcon
/// </summary>
public class FloatingIcon : MonoBehaviour
{
    [Header("Icon Properties")]
    [Tooltip("Material containing the icon texture")]
    public Material iconMaterial;

    [Tooltip("Text to display with this icon")]
    public string iconText = "";

    [Tooltip("Name/location identifier for this POI")]
    public string poiName = "";

    [Header("Billboard Settings")]
    [Tooltip("Make icon face the camera")]
    public bool faceCamera = true;

    [Tooltip("Only rotate around Y axis (for floor icons)")]
    public bool lockYRotation = false;

    [Tooltip("Offset from camera-facing rotation")]
    public float rotationOffset = 0f;

    [Header("Visual Settings")]
    [Tooltip("Scale of the icon")]
    public Vector3 iconScale = new Vector3(0.3f, 0.3f, 0.3f);

    [Tooltip("Distance from camera at which icon fades in")]
    public float fadeInDistance = 5f;

    [Tooltip("Distance from camera at which icon fades out")]
    public float fadeOutDistance = 15f;

    [Header("Text Display")]
    [Tooltip("Show text label below icon")]
    public bool showTextLabel = true;

    [Tooltip("Text prefab for label")]
    public GameObject textPrefab;

    [Tooltip("Text color")]
    public Color textColor = Color.white;

    [Tooltip("Text size")]
    public float textSize = 0.1f;

    // Components
    private Transform cameraTransform;
    private Renderer iconRenderer;
    private GameObject textLabel;

    // State
    private bool isVisible = true;
    private float currentAlpha = 1f;

    private void Awake()
    {
        // Get or add renderer
        iconRenderer = GetComponent<Renderer>();
        if (iconRenderer == null)
        {
            // Add a quad for the icon
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(transform);
            quad.transform.localPosition = Vector3.zero;
            quad.transform.localRotation = Quaternion.identity;
            quad.transform.localScale = iconScale;
            
            iconRenderer = quad.GetComponent<Renderer>();
            Destroy(quad); // We just needed to check
        }
    }

    private void Start()
    {
        // Get main camera
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Apply material
        if (iconMaterial != null && iconRenderer != null)
        {
            iconRenderer.material = iconMaterial;
        }

        // Apply scale
        transform.localScale = iconScale;

        // Create text label if enabled
        if (showTextLabel)
        {
            CreateTextLabel();
        }
    }

    private void LateUpdate()
    {
        if (faceCamera && cameraTransform != null)
        {
            Billboard();
        }

        UpdateVisibility();
    }

    /// <summary>
    /// Make the icon face the camera
    /// </summary>
    private void Billboard()
    {
        // Get direction to camera
        Vector3 direction = cameraTransform.position - transform.position;

        if (lockYRotation)
        {
            // Only rotate around Y axis
            direction.y = 0;
        }

        // Check if we have a valid direction
        if (direction.sqrMagnitude > 0.001f)
        {
            // Look at camera
            transform.LookAt(transform.position + direction);
            
            // Apply rotation offset
            transform.Rotate(0, rotationOffset, 0);
            
            // Flip to face camera (quads face +Z by default)
            transform.Rotate(0, 180, 0);
        }
    }

    /// <summary>
    /// Update visibility based on distance
    /// </summary>
    private void UpdateVisibility()
    {
        if (cameraTransform == null)
            return;

        float distance = Vector3.Distance(transform.position, cameraTransform.position);

        // Calculate alpha based on distance
        if (distance < fadeInDistance)
        {
            currentAlpha = 1f;
        }
        else if (distance > fadeOutDistance)
        {
            currentAlpha = 0f;
        }
        else
        {
            // Linear fade
            currentAlpha = 1f - ((distance - fadeInDistance) / (fadeOutDistance - fadeInDistance));
        }

        // Apply alpha to material
        if (iconRenderer != null && iconRenderer.material != null)
        {
            Color color = iconRenderer.material.color;
            color.a = currentAlpha;
            iconRenderer.material.color = color;
        }

        // Update game object visibility
        bool shouldBeVisible = currentAlpha > 0.01f;
        if (gameObject.activeSelf != shouldBeVisible)
        {
            gameObject.SetActive(shouldBeVisible);
        }
    }

    /// <summary>
    /// Create text label below icon
    /// </summary>
    private void CreateTextLabel()
    {
        if (string.IsNullOrEmpty(iconText))
            return;

        // Create text object
        textLabel = new GameObject("IconText");
        textLabel.transform.SetParent(transform);
        textLabel.transform.localPosition = new Vector3(0, -iconScale.y - textSize, 0);
        textLabel.transform.localScale = Vector3.one * textSize;

        // Add text component (using TMP if available, otherwise TextMesh)
        TMPro.TextMeshPro textMesh = textLabel.AddComponent<TMPro.TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = iconText;
            textMesh.color = textColor;
            textMesh.alignment = TMPro.TextAlignmentOptions.Center;
            textMesh.fontSize = 3f;
        }
    }

    /// <summary>
    /// Set the icon text
    /// </summary>
    public void SetText(string text)
    {
        iconText = text;
        
        if (textLabel != null)
        {
            TMPro.TextMeshPro textMesh = textLabel.GetComponent<TMPro.TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = text;
            }
        }
    }

    /// <summary>
    /// Set the icon material
    /// </summary>
    public void SetMaterial(Material material)
    {
        iconMaterial = material;
        
        if (iconRenderer != null)
        {
            iconRenderer.material = material;
        }
    }

    /// <summary>
    /// Set the icon scale
    /// </summary>
    public void SetScale(Vector3 scale)
    {
        iconScale = scale;
        transform.localScale = scale;
    }

    /// <summary>
    /// Show the icon
    /// </summary>
    public void Show()
    {
        isVisible = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the icon
    /// </summary>
    public void Hide()
    {
        isVisible = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Toggle visibility
    /// </summary>
    public void Toggle()
    {
        isVisible = !isVisible;
        gameObject.SetActive(isVisible);
    }

    /// <summary>
    /// Get distance to camera
    /// </summary>
    public float GetDistanceToCamera()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform == null)
            return float.MaxValue;

        return Vector3.Distance(transform.position, cameraTransform.position);
    }

    /// <summary>
    /// Get the POI name
    /// </summary>
    public string GetPOIName()
    {
        return poiName;
    }

    private void OnValidate()
    {
        // Ensure scale is positive
        if (iconScale.x <= 0) iconScale.x = 0.3f;
        if (iconScale.y <= 0) iconScale.y = 0.3f;
        if (iconScale.z <= 0) iconScale.z = 0.3f;

        // Ensure text size is positive
        textSize = Mathf.Max(0.01f, textSize);

        // Ensure fade distances are valid
        if (fadeOutDistance <= fadeInDistance)
            fadeOutDistance = fadeInDistance + 5f;
    }
}
