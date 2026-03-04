using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Collections.Generic;

/// <summary>
/// ARPlayerController handles player movement, positioning, and interaction within the AR world.
/// It manages AR session origin positioning, navigation, and marker placement.
///
/// Corresponds to Unreal Engine's BP_ARPawn
/// </summary>
public class ARPlayerController : MonoBehaviour
{
    public static ARPlayerController Instance { get; private set; }

    [Header("AR Components")]
    [Tooltip("XR Origin transform (XROrigin in AR Foundation 6.x, ARSessionOrigin in 5.x)")]
    public Transform arSessionOrigin;

    [Tooltip("AR Camera transform")]
    public Transform arCamera;

    [Header("Player Settings")]
    [Tooltip("Player movement speed")]
    public float movementSpeed = 1.0f;

    [Tooltip("Height offset for player above detected planes")]
    public float heightOffset = 0.0f;

    [Header("Marker Placement")]
    [Tooltip("Prefab for placing custom markers")]
    public GameObject markerPrefab;

    [Tooltip("Enable marker placement on tap")]
    public bool enableMarkerPlacement = true;

    [Header("Navigation")]
    [Tooltip("Current destination vertex")]
    public NavigationVertex currentDestination;

    [Tooltip("Distance threshold to consider destination reached")]
    public float destinationThreshold = 0.5f;

    // Events
    public Action<Vector3> onPositionUpdated;
    public Action<Quaternion> onRotationUpdated;
    public Action<NavigationVertex> onDestinationReached;
    public Action<Vector3, string> onMarkerPlaced;

    // State
    private Vector3 currentPosition;
    private Quaternion currentRotation;
    private bool isPositioned = false;
    private ARRaycastManager raycastManager;

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
        InitializeComponents();
        SubscribeToAREvents();
    }

    private void InitializeComponents()
    {
        // Get XR Origin transform (XROrigin in AR Foundation 6.x, ARSessionOrigin in 5.x)
        // Note: After installing com.unity.xr.coreutils, use XROrigin instead of ARSessionOrigin
        if (arSessionOrigin == null)
        {
            var origin = FindObjectOfType<UnityEngine.XR.ARFoundation.ARSessionOrigin>();
            if (origin != null)
            {
                arSessionOrigin = origin.transform;
            }
        }

        // Get AR Camera
        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        // Get Raycast Manager
        if (arSessionOrigin != null)
        {
            raycastManager = arSessionOrigin.GetComponent<ARRaycastManager>();
        }

        // Initialize position
        if (arSessionOrigin != null)
        {
            currentPosition = arSessionOrigin.position;
            currentRotation = arSessionOrigin.rotation;
        }
    }

    private void SubscribeToAREvents()
    {
        // Subscribe to AR session events if ARSessionManager exists
        var sessionManager = FindObjectOfType<ARSessionManager>();
        if (sessionManager != null)
        {
            sessionManager.onARSessionStarted += OnARSessionStarted;
            sessionManager.onARSessionStopped += OnARSessionStopped;
        }
    }

    private void OnARSessionStarted()
    {
        Debug.Log("ARPlayerController: AR session started, ready for positioning");
    }

    private void OnARSessionStopped()
    {
        Debug.Log("ARPlayerController: AR session stopped");
    }

    private void Update()
    {
        if (arSessionOrigin == null || arCamera == null)
            return;

        // Update current position and rotation from AR session
        UpdatePlayerState();

        // Handle touch input for marker placement
        if (enableMarkerPlacement && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleTouchInput(touch.position);
            }
        }

        // Check if destination reached
        CheckDestinationReached();
    }

    private void UpdatePlayerState()
    {
        // Get position from AR session origin
        Vector3 newPosition = arSessionOrigin.position;
        Quaternion newRotation = arCamera.rotation;

        // Only update if changed significantly
        if (Vector3.Distance(currentPosition, newPosition) > 0.01f ||
            Quaternion.Angle(currentRotation, newRotation) > 1.0f)
        {
            currentPosition = newPosition;
            currentRotation = newRotation;
            isPositioned = true;

            onPositionUpdated?.Invoke(currentPosition);
            onRotationUpdated?.Invoke(currentRotation);
        }
    }

    private void HandleTouchInput(Vector2 touchPosition)
    {
        if (raycastManager == null)
            return;

        // Perform raycast against AR planes
        // AR Foundation 6.x uses List<ARRaycastHit> instead of single out parameter
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            // Get the first hit result
            ARRaycastHit hit = hits[0];
            
            // Place marker at hit position
            PlaceMarker(hit.pose.position, hit.pose.rotation);
        }
    }

    /// <summary>
    /// Place a custom marker at the specified position
    /// </summary>
    public void PlaceMarker(Vector3 position, Quaternion rotation, string markerName = "Custom Marker")
    {
        if (markerPrefab == null)
        {
            Debug.LogWarning("ARPlayerController: Marker prefab not set");
            return;
        }

        // Instantiate marker
        GameObject marker = Instantiate(markerPrefab, position, rotation);
        
        // Add marker component if not present
        Marker markerComponent = marker.GetComponent<Marker>();
        if (markerComponent == null)
        {
            markerComponent = marker.AddComponent<Marker>();
        }
        
        markerComponent.markerName = markerName;
        
        onMarkerPlaced?.Invoke(position, markerName);
        
        Debug.Log($"ARPlayerController: Marker placed at {position}");
    }

    /// <summary>
    /// Teleport player to a specific vertex position
    /// </summary>
    public void TeleportToVertex(NavigationVertex vertex)
    {
        if (vertex == null || arSessionOrigin == null)
            return;

        // Calculate new position (keeping current height offset)
        Vector3 targetPosition = vertex.transform.position;
        targetPosition.y += heightOffset;

        // Move AR session origin to new position
        arSessionOrigin.position = targetPosition;
        
        Debug.Log($"ARPlayerController: Teleported to vertex: {vertex.vertexId}");
    }

    /// <summary>
    /// Set the current destination for navigation
    /// </summary>
    public void SetDestination(NavigationVertex destination)
    {
        currentDestination = destination;
        
        if (destination != null)
        {
            Debug.Log($"ARPlayerController: Destination set to: {destination.vertexId}");
        }
    }

    /// <summary>
    /// Clear the current destination
    /// </summary>
    public void ClearDestination()
    {
        currentDestination = null;
    }

    private void CheckDestinationReached()
    {
        if (currentDestination == null || arSessionOrigin == null)
            return;

        // Calculate distance to destination
        float distance = Vector3.Distance(arSessionOrigin.position, currentDestination.transform.position);

        if (distance <= destinationThreshold)
        {
            onDestinationReached?.Invoke(currentDestination);
            Debug.Log($"ARPlayerController: Destination reached: {currentDestination.vertexId}");
            
            // Clear destination after reaching
            currentDestination = null;
        }
    }

    /// <summary>
    /// Get the current player position
    /// </summary>
    public Vector3 GetPosition()
    {
        return currentPosition;
    }

    /// <summary>
    /// Get the current player rotation
    /// </summary>
    public Quaternion GetRotation()
    {
        return currentRotation;
    }

    /// <summary>
    /// Get the current player forward direction
    /// </summary>
    public Vector3 GetForward()
    {
        if (arCamera != null)
        {
            return arCamera.forward;
        }
        return Vector3.forward;
    }

    /// <summary>
    /// Get distance to a specific vertex
    /// </summary>
    public float GetDistanceToVertex(NavigationVertex vertex)
    {
        if (vertex == null || arSessionOrigin == null)
            return float.MaxValue;

        return Vector3.Distance(arSessionOrigin.position, vertex.transform.position);
    }

    /// <summary>
    /// Get distance to a specific position
    /// </summary>
    public float GetDistanceToPosition(Vector3 position)
    {
        if (arSessionOrigin == null)
            return float.MaxValue;

        return Vector3.Distance(arSessionOrigin.position, position);
    }

    /// <summary>
    /// Check if player is currently positioned in AR space
    /// </summary>
    public bool IsPositioned()
    {
        return isPositioned;
    }

    /// <summary>
    /// Reset player position
    /// </summary>
    public void ResetPosition()
    {
        if (arSessionOrigin != null)
        {
            arSessionOrigin.position = Vector3.zero;
            currentPosition = Vector3.zero;
            isPositioned = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Unsubscribe from events
        var sessionManager = FindObjectOfType<ARSessionManager>();
        if (sessionManager != null)
        {
            sessionManager.onARSessionStarted -= OnARSessionStarted;
            sessionManager.onARSessionStopped -= OnARSessionStopped;
        }
    }
}
