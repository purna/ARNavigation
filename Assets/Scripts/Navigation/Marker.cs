using UnityEngine;
using System;

/// <summary>
/// Marker represents a custom marker placed by the user on detected AR planes.
/// These are user-placed points of interest.
///
/// Corresponds to Unreal Engine's BP_Marker
/// </summary>
public class Marker : MonoBehaviour
{
    [Header("Marker Properties")]
    [Tooltip("Name/label of this marker")]
    public string markerName = "Marker";

    [Tooltip("Distance to player")]
    public float distanceToPlayer = 0f;

    [Header("Visual Settings")]
    [Tooltip("Marker material")]
    public Material markerMaterial;

    [Tooltip("Marker color")]
    public Color markerColor = Color.red;

    [Tooltip("Marker scale")]
    public Vector3 markerScale = new Vector3(0.2f, 0.2f, 0.2f);

    [Header("Text Display")]
    [Tooltip("Show name label")]
    public bool showName = true;

    [Tooltip("Show distance")]
    public bool showDistance = true;

    // Components
    private Renderer markerRenderer;
    private Transform playerTransform;

    // Events
    public Action<Marker> onMarkerSelected;

    private void Awake()
    {
        // Add or get renderer
        markerRenderer = GetComponent<Renderer>();
        if (markerRenderer == null)
        {
            // Create a cube marker
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = markerScale;
            
            markerRenderer = cube.GetComponent<Renderer>();
        }

        // Apply material
        if (markerMaterial != null && markerRenderer != null)
        {
            markerRenderer.material = markerMaterial;
        }
        else if (markerRenderer != null)
        {
            // Create default material
            markerRenderer.material = new Material(Shader.Find("Standard"));
            markerRenderer.material.color = markerColor;
        }
    }

    private void Start()
    {
        // Get player transform
        ARPlayerController player = FindObjectOfType<ARPlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            // Try to find main camera
            if (Camera.main != null)
            {
                playerTransform = Camera.main.transform;
            }
        }

        // Apply scale
        transform.localScale = markerScale;
    }

    private void Update()
    {
        UpdateDistance();
    }

    /// <summary>
    /// Update distance to player
    /// </summary>
    private void UpdateDistance()
    {
        if (playerTransform != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        }
    }

    /// <summary>
    /// Set marker name
    /// </summary>
    public void SetName(string name)
    {
        markerName = name;
    }

    /// <summary>
    /// Set marker color
    /// </summary>
    public void SetColor(Color color)
    {
        markerColor = color;
        if (markerRenderer != null)
        {
            markerRenderer.material.color = color;
        }
    }

    /// <summary>
    /// Set marker material
    /// </summary>
    public void SetMaterial(Material material)
    {
        markerMaterial = material;
        if (markerRenderer != null)
        {
            markerRenderer.material = material;
        }
    }

    /// <summary>
    /// Get marker info string
    /// </summary>
    public string GetMarkerInfo()
    {
        string info = markerName;
        if (showDistance)
        {
            info += $"\n{distanceToPlayer:F1}m";
        }
        return info;
    }

    /// <summary>
    /// Handle marker selection (when user taps on it)
    /// </summary>
    public void Select()
    {
        onMarkerSelected?.Invoke(this);
        Debug.Log($"Marker selected: {markerName}");
    }

    /// <summary>
    /// Get distance to player
    /// </summary>
    public float GetDistance()
    {
        return distanceToPlayer;
    }

    /// <summary>
    /// Get marker name
    /// </summary>
    public string GetName()
    {
        return markerName;
    }

    private void OnMouseDown()
    {
        // Allow clicking on marker in editor
        Select();
    }

    private void OnValidate()
    {
        // Ensure scale is positive
        if (markerScale.x <= 0) markerScale.x = 0.2f;
        if (markerScale.y <= 0) markerScale.y = 0.2f;
        if (markerScale.z <= 0) markerScale.z = 0.2f;
    }
}
