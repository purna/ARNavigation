using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using System;
using System.Collections.Generic;

/// <summary>
/// UserPositionManager tracks the user's position in AR space using SLAM and QR code re-localization.
/// This is critical for indoor navigation where GPS is unavailable.
///
/// Corresponds to build plan Phase 3 - User Localisation
/// </summary>
public class UserPositionManager : MonoBehaviour
{
    public static UserPositionManager Instance { get; private set; }

    [Header("AR Components")]
    [Tooltip("XR Origin for position tracking")]
    public XROrigin xrOrigin;

    [Tooltip("AR Camera transform")]
    public Transform arCamera;

    [Header("AR Session Manager Reference")]
    public ARSessionManager arSessionManager;

    [Header("Localization Settings")]
    [Tooltip("Enable QR code re-localization")]
    public bool enableRelocalization = true;

    [Tooltip("Maximum time between relocalization (seconds)")]
    public float maxTimeBetweenRelocalization = 60f;

    [Header("Debug")]
    [Tooltip("Show debug info")]
    public bool showDebugInfo = false;

    // Position tracking state
    public Vector3 CurrentWorldPosition { get; private set; }
    public Quaternion CurrentWorldRotation { get; private set; }
    public int CurrentFloor { get; private set; }
    public bool IsLocalized { get; private set; }

    // Events
    public Action<Vector3, int> onPositionUpdated;
    public Action<int> onFloorChanged;
    public Action<string> onRelocalized;

    // Tracking state
    private float lastRelocalizationTime = 0f;
    private string lastQRCode = "";
    private bool hasWarnedNoQR = false;

    // Floor thresholds (Y positions)
    private float[] floorHeights = { 0f, 4f, 8f }; // Ground, Floor 1, Floor 2

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
        // Get XR Origin
        if (xrOrigin == null)
        {
            xrOrigin = FindObjectOfType<XROrigin>();
        }

        // Get AR Camera
        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        // Subscribe to AR session events
        if (arSessionManager != null)
        {
            arSessionManager.onARSessionStarted += OnARSessionStarted;
            arSessionManager.onARSessionStopped += OnARSessionStopped;
        }

        // Initialize position
        CurrentWorldPosition = Vector3.zero;
        CurrentWorldRotation = Quaternion.identity;
        CurrentFloor = 0;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Unsubscribe from events
        if (arSessionManager != null)
        {
            arSessionManager.onARSessionStarted -= OnARSessionStarted;
            arSessionManager.onARSessionStopped -= OnARSessionStopped;
        }
    }

    private void Update()
    {
        if (!IsLocalized)
            return;

        // Update position from AR camera
        UpdatePositionFromARCamera();

        // Check floor change
        CheckFloorChange();

        // Check for re-localization needed
        if (enableRelocalization)
        {
            CheckRelocalizationNeeded();
        }
    }

    /// <summary>
    /// Handle AR session started
    /// </summary>
    private void OnARSessionStarted()
    {
        if (showDebugInfo)
            Debug.Log("UserPositionManager: AR session started");
    }

    /// <summary>
    /// Handle AR session stopped
    /// </summary>
    private void OnARSessionStopped()
    {
        IsLocalized = false;
        if (showDebugInfo)
            Debug.Log("UserPositionManager: AR session stopped");
    }

    /// <summary>
    /// Update position from AR camera
    /// </summary>
    private void UpdatePositionFromARCamera()
    {
        if (arCamera == null)
            return;

        Vector3 newPosition = arCamera.position;
        Quaternion newRotation = arCamera.rotation;

        // Check if position changed significantly
        if (Vector3.Distance(newPosition, CurrentWorldPosition) > 0.01f || 
            Quaternion.Angle(newRotation, CurrentWorldRotation) > 0.1f)
        {
            CurrentWorldPosition = newPosition;
            CurrentWorldRotation = newRotation;
            onPositionUpdated?.Invoke(CurrentWorldPosition, CurrentFloor);
        }
    }

    /// <summary>
    /// Check if user changed floors
    /// </summary>
    private void CheckFloorChange()
    {
        int newFloor = CalculateFloorFromY(CurrentWorldPosition.y);

        if (newFloor != CurrentFloor)
        {
            int oldFloor = CurrentFloor;
            CurrentFloor = newFloor;
            
            if (showDebugInfo)
                Debug.Log($"UserPositionManager: Floor changed from {oldFloor} to {CurrentFloor}");
            
            onFloorChanged?.Invoke(CurrentFloor);
        }
    }

    /// <summary>
    /// Calculate floor from Y position
    /// </summary>
    private int CalculateFloorFromY(float y)
    {
        int floor = 0;
        
        for (int i = 1; i < floorHeights.Length; i++)
        {
            if (y >= floorHeights[i])
            {
                floor = i;
            }
        }
        
        return floor;
    }

    /// <summary>
    /// Check if re-localization is needed
    /// </summary>
    private void CheckRelocalizationNeeded()
    {
        float timeSinceLastReloc = Time.time - lastRelocalizationTime;

        if (timeSinceLastReloc > maxTimeBetweenRelocalization)
        {
            if (!hasWarnedNoQR)
            {
                Debug.LogWarning("UserPositionManager: No QR code scan for " + maxTimeBetweenRelocalization + " seconds. Consider re-scanning for better accuracy.");
                hasWarnedNoQR = true;
            }
        }
    }

    /// <summary>
    /// Relocalize user position based on QR code scan
    /// </summary>
    /// <param name="qrCode">The QR code string</param>
    /// <param name="detectedPosition">Where the QR code was detected in AR space</param>
    public void RelocalizeFromQRCode(string qrCode, Vector3 detectedPosition)
    {
        if (string.IsNullOrEmpty(qrCode))
        {
            Debug.LogWarning("UserPositionManager: Empty QR code received");
            return;
        }

        // Get the known world position from QR anchor
        QrAnchor anchor = LocationDataManager.Instance?.GetQrAnchor(qrCode);
        
        if (anchor == null)
        {
            Debug.LogWarning($"UserPositionManager: Unknown QR code: {qrCode}");
            return;
        }

        // Calculate offset between detected position and known world position
        Vector3 offset = anchor.GetWorldPosition() - detectedPosition;

        // Apply offset to XR Origin
        if (xrOrigin != null)
        {
            xrOrigin.transform.position += offset;
        }

        // Update tracking state
        lastQRCode = qrCode;
        lastRelocalizationTime = Time.time;
        hasWarnedNoQR = false;
        IsLocalized = true;

        // Update current floor
        CurrentFloor = anchor.floor;

        if (showDebugInfo)
        {
            Debug.Log($"UserPositionManager: Relocalized from QR code: {qrCode}");
            Debug.Log($"  Detected position: {detectedPosition}");
            Debug.Log($"  Known world position: {anchor.GetWorldPosition()}");
            Debug.Log($"  Offset applied: {offset}");
            Debug.Log($"  New floor: {CurrentFloor}");
        }

        onRelocalized?.Invoke(qrCode);
        onFloorChanged?.Invoke(CurrentFloor);
    }

    /// <summary>
    /// Set initial position from a known location (for testing without QR)
    /// </summary>
    /// <param name="locationId">Location ID to start at</param>
    public void SetInitialPosition(string locationId)
    {
        LocationEntry location = LocationDataManager.Instance?.GetById(locationId);
        
        if (location == null)
        {
            Debug.LogWarning($"UserPositionManager: Unknown location ID: {locationId}");
            return;
        }

        // Move XR Origin to known position
        if (xrOrigin != null)
        {
            Vector3 targetPos = location.GetWorldPosition();
            targetPos.y -= 1.7f; // Account for camera height (typical phone held at ~1.7m)
            xrOrigin.transform.position = targetPos;
        }

        CurrentFloor = location.floor;
        IsLocalized = true;
        lastRelocalizationTime = Time.time;

        if (showDebugInfo)
            Debug.Log($"UserPositionManager: Initial position set to {location.title} (Floor {CurrentFloor})");

        onFloorChanged?.Invoke(CurrentFloor);
    }

    /// <summary>
    /// Get the current floor based on Y position
    /// </summary>
    public int GetCurrentFloor()
    {
        return CurrentFloor;
    }

    /// <summary>
    /// Get distance to a target position
    /// </summary>
    public float GetDistanceTo(Vector3 targetPosition)
    {
        return Vector3.Distance(CurrentWorldPosition, targetPosition);
    }

    /// <summary>
    /// Get distance to a location
    /// </summary>
    public float GetDistanceTo(LocationEntry location)
    {
        return GetDistanceTo(location.GetWorldPosition());
    }

    /// <summary>
    /// Get bearing to a target position (in degrees)
    /// </summary>
    public float GetBearingTo(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - CurrentWorldPosition;
        direction.y = 0; // Ignore vertical component
        return Quaternion.LookRotation(direction).eulerAngles.y;
    }

    /// <summary>
    /// Get bearing to a location
    /// </summary>
    public float GetBearingTo(LocationEntry location)
    {
        return GetBearingTo(location.GetWorldPosition());
    }

    /// <summary>
    /// Check if user is at a specific location
    /// </summary>
    public bool IsAtLocation(LocationEntry location, float threshold = 1.0f)
    {
        float distance = GetDistanceTo(location);
        return distance <= threshold;
    }

    /// <summary>
    /// Check if user is at a specific floor
    /// </summary>
    public bool IsAtFloor(int floor)
    {
        return CurrentFloor == floor;
    }
}
