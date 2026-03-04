using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System;


/// <summary>
/// MinimapRenderer handles the 2D minimap display showing player position and rotation.
/// It uses a separate orthographic camera and render texture to create the minimap.
///
/// Corresponds to Unreal Engine's BP_2DMapImageProvider
/// </summary>
public class MinimapRenderer : MonoBehaviour
{
    public static MinimapRenderer Instance { get; private set; }

    [Header("Components")]
    [Tooltip("Camera for minimap rendering")]
    public Camera minimapCamera;

    [Tooltip("Raw image to display minimap")]
    public RawImage minimapDisplay;

    [Header("Render Texture")]
    [Tooltip("Render texture for minimap")]
    public RenderTexture minimapTexture;

    [Tooltip("Minimap texture size")]
    public Vector2Int textureSize = new Vector2Int(512, 512);

    [Header("Player Indicator")]
    [Tooltip("Player indicator transform")]
    public Transform playerIndicator;

    [Tooltip("Rotation indicator transform")]
    public Transform rotationIndicator;

    [Header("Settings")]
    [Tooltip("Minimap zoom level")]
    public float zoom = 1.0f;

    [Tooltip("Follow player")]
    public bool followPlayer = true;

    [Tooltip("Rotate with player")]
    public bool rotateWithPlayer = false;

    [Tooltip("Use compass heading")]
    public bool useCompassHeading = true;

    [Header("Map Bounds")]
    [Tooltip("Center of the map in world coordinates")]
    public Vector3 mapCenter = Vector3.zero;

    [Tooltip("Size of the map area")]
    public Vector2 mapSize = new Vector2(80, 60);

    // Components
    private Transform playerTransform;
    private ARPlayerController playerController;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Get player controller
        playerController = FindObjectOfType<ARPlayerController>();
        if (playerController != null)
        {
            playerTransform = playerController.transform;
        }
        else if (Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }

        // Set up render texture
        SetupRenderTexture();

        // Set up camera
        SetupCamera();
    }

    private void LateUpdate()
    {
        if (minimapCamera == null)
            return;

        // Update camera position to follow player
        if (followPlayer && playerTransform != null)
        {
            Vector3 targetPosition = playerTransform.position;
            targetPosition.y = minimapCamera.transform.position.y;
            
            minimapCamera.transform.position = Vector3.Lerp(
                minimapCamera.transform.position,
                targetPosition,
                Time.deltaTime * 5f
            );
        }

        // Update player indicator
        UpdatePlayerIndicator();

        // Update rotation
        UpdateRotation();
    }

    /// <summary>
    /// Set up the render texture
    /// </summary>
    private void SetupRenderTexture()
    {
        if (minimapTexture == null)
        {
            minimapTexture = new RenderTexture(textureSize.x, textureSize.y, 16);
            minimapTexture.Create();
        }

        if (minimapCamera != null)
        {
            minimapCamera.targetTexture = minimapTexture;
        }

        if (minimapDisplay != null)
        {
            minimapDisplay.texture = minimapTexture;
        }
    }

    /// <summary>
    /// Set up the minimap camera
    /// </summary>
    private void SetupCamera()
    {
        if (minimapCamera == null)
        {
            // Try to find existing camera
            MinimapCamera[] cameras = FindObjectsOfType<MinimapCamera>();
            if (cameras.Length > 0)
            {
                minimapCamera = cameras[0].GetComponent<Camera>();
            }
        }

        if (minimapCamera != null)
        {
            // Configure for orthographic top-down view
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = Mathf.Max(mapSize.x, mapSize.y) / 2f * zoom;
            minimapCamera.transform.position = new Vector3(mapCenter.x, 50f, mapCenter.z);
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            
            // Clear flags
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent
        }
    }

    /// <summary>
    /// Update player indicator position
    /// </summary>
    private void UpdatePlayerIndicator()
    {
        if (playerIndicator != null && playerTransform != null)
        {
            // Convert world position to minimap position
            Vector3 relativePos = playerTransform.position - mapCenter;
            
            // Set indicator position (in local space of minimap container)
            playerIndicator.localPosition = new Vector3(relativePos.x, relativePos.z, 0);
        }
    }

    /// <summary>
    /// Update rotation
    /// </summary>
    private void UpdateRotation()
    {
        if (playerTransform == null)
            return;

        float rotation = 0f;

        if (useCompassHeading && Input.compass.enabled)
        {
            // Use device compass heading
            rotation = Input.compass.trueHeading;
        }
        else
        {
            // Use camera rotation
            rotation = playerTransform.eulerAngles.y;
        }

        // Apply rotation to minimap camera if not following player rotation
        if (!rotateWithPlayer && minimapCamera != null)
        {
            minimapCamera.transform.rotation = Quaternion.Euler(90f, -rotation, 0f);
        }

        // Apply rotation to player indicator
        if (rotationIndicator != null)
        {
            rotationIndicator.localRotation = Quaternion.Euler(0, 0, -rotation);
        }
    }

    /// <summary>
    /// Set the minimap zoom level
    /// </summary>
    public void SetZoom(float newZoom)
    {
        zoom = Mathf.Max(0.1f, newZoom);
        
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = Mathf.Max(mapSize.x, mapSize.y) / 2f * zoom;
        }
    }

    /// <summary>
    /// Zoom in
    /// </summary>
    public void ZoomIn()
    {
        SetZoom(zoom * 0.8f);
    }

    /// <summary>
    /// Zoom out
    /// </summary>
    public void ZoomOut()
    {
        SetZoom(zoom * 1.2f);
    }

    /// <summary>
    /// Set map center
    /// </summary>
    public void SetMapCenter(Vector3 center)
    {
        mapCenter = center;
        
        if (minimapCamera != null)
        {
            minimapCamera.transform.position = new Vector3(center.x, 50f, center.z);
        }
    }

    /// <summary>
    /// Set map size
    /// </summary>
    public void SetMapSize(Vector2 size)
    {
        mapSize = size;
        
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = Mathf.Max(mapSize.x, mapSize.y) / 2f * zoom;
        }
    }

    /// <summary>
    /// Toggle follow player
    /// </summary>
    public void ToggleFollowPlayer()
    {
        followPlayer = !followPlayer;
    }

    /// <summary>
    /// Toggle rotation
    /// </summary>
    public void ToggleRotation()
    {
        rotateWithPlayer = !rotateWithPlayer;
    }

    /// <summary>
    /// Show minimap
    /// </summary>
    public void Show()
    {
        if (minimapCamera != null)
        {
            minimapCamera.enabled = true;
        }
        
        if (minimapDisplay != null)
        {
            minimapDisplay.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hide minimap
    /// </summary>
    public void Hide()
    {
        if (minimapCamera != null)
        {
            minimapCamera.enabled = false;
        }
        
        if (minimapDisplay != null)
        {
            minimapDisplay.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Clean up render texture
        if (minimapTexture != null)
        {
            minimapTexture.Release();
        }
    }
}

/// <summary>
/// Simple minimap camera component
/// </summary>
public class MinimapCamera : MonoBehaviour
{
    // This is just a tag component for finding the minimap camera
    // The actual camera functionality is in MinimapRenderer
}
