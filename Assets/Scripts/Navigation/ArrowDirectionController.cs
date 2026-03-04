using UnityEngine;
using System;

/// <summary>
/// ArrowDirectionController manages the HUD directional arrow that points toward the next waypoint.
/// It calculates bearing and rotates the arrow accordingly.
///
/// Corresponds to build plan Phase 4 - Arrow Direction
/// </summary>
public class ArrowDirectionController : MonoBehaviour
{
    public static ArrowDirectionController Instance { get; private set; }

    [Header("Components")]
    [Tooltip("The arrow transform to rotate")]
    public Transform arrowTransform;

    [Tooltip("Arrow image/sprite for visual reference")]
    public UnityEngine.UI.Image arrowImage;

    [Header("Settings")]
    [Tooltip("Smooth rotation speed")]
    public float rotationSpeed = 5f;

    [Tooltip("Show debug info")]
    public bool showDebugInfo = false;

    [Header("Animation")]
    [Tooltip("Enable pulse animation when heading in right direction")]
    public bool enablePulseAnimation = true;

    [Tooltip("Pulse speed")]
    public float pulseSpeed = 2f;

    [Tooltip("Pulse scale amplitude")]
    public float pulseAmplitude = 0.2f;

    // References
    private NavigationManager navigationManager;
    private UserPositionManager userPositionManager;
    private Transform cameraTransform;

    // State
    private float targetRotation = 0f;
    private float currentRotation = 0f;
    private bool isActive = false;
    private Vector3 baseScale;

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
        // Get references
        navigationManager = FindObjectOfType<NavigationManager>();
        userPositionManager = FindObjectOfType<UserPositionManager>();
        
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Store base scale
        if (arrowTransform != null)
        {
            baseScale = arrowTransform.localScale;
        }

        // Hide initially
        SetActive(false);
    }

    private void Update()
    {
        if (!isActive)
            return;

        UpdateArrowDirection();
        
        if (enablePulseAnimation)
        {
            UpdatePulseAnimation();
        }
    }

    /// <summary>
    /// Update arrow direction based on navigation state
    /// </summary>
    private void UpdateArrowDirection()
    {
        if (navigationManager == null || userPositionManager == null)
            return;

        // Get current waypoint
        Vector3? waypointPos = navigationManager.GetCurrentWaypointPosition();
        
        if (waypointPos == null)
        {
            // No waypoint - try destination
            var dest = navigationManager.CurrentDestination;
            if (dest != null)
            {
                waypointPos = dest.GetWorldPosition();
            }
        }

        if (waypointPos == null)
            return;

        // Calculate bearing to waypoint
        targetRotation = CalculateBearingTo(waypointPos.Value);

        // Smooth rotation
        currentRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime * rotationSpeed);

        // Apply rotation
        if (arrowTransform != null)
        {
            arrowTransform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
    }

    /// <summary>
    /// Calculate bearing from player to target
    /// </summary>
    private float CalculateBearingTo(Vector3 target)
    {
        Vector3 playerPos = userPositionManager.CurrentWorldPosition;
        Vector3 cameraForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        
        // Direction to target (ignoring Y)
        Vector3 direction = target - playerPos;
        direction.y = 0;
        direction.Normalize();

        // Calculate angle from camera forward
        float angle = Vector3.SignedAngle(cameraForward, direction, Vector3.up);
        
        return angle;
    }

    /// <summary>
    /// Update pulse animation
    /// </summary>
    private void UpdatePulseAnimation()
    {
        if (arrowTransform == null || !navigationManager.IsNavigating())
            return;

        // Pulse when moving
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        arrowTransform.localScale = baseScale * pulse;
    }

    /// <summary>
    /// Set the arrow active state
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (arrowTransform != null)
        {
            arrowTransform.gameObject.SetActive(active);
        }
        
        if (arrowImage != null)
        {
            arrowImage.enabled = active;
        }
    }

    /// <summary>
    /// Set the arrow transform directly
    /// </summary>
    public void SetArrowTransform(Transform arrow)
    {
        arrowTransform = arrow;
        
        if (arrowTransform != null)
        {
            baseScale = arrowTransform.localScale;
        }
    }

    /// <summary>
    /// Set the arrow image
    /// </summary>
    public void SetArrowImage(UnityEngine.UI.Image image)
    {
        arrowImage = image;
    }

    /// <summary>
    /// Get current bearing
    /// </summary>
    public float GetCurrentBearing()
    {
        return currentRotation;
    }

    /// <summary>
    /// Get target bearing
    /// </summary>
    public float GetTargetBearing()
    {
        return targetRotation;
    }

    /// <summary>
    /// Check if arrow is active
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }
}

/// <summary>
/// DistanceCalculator provides metric/imperial conversion utilities
/// </summary>
public class DistanceCalculator
{
    public enum UnitSystem
    {
        Metric,
        Imperial
    }

    private static UnitSystem currentUnit = UnitSystem.Metric;

    /// <summary>
    /// Set the current unit system
    /// </summary>
    public static void SetUnitSystem(UnitSystem unit)
    {
        currentUnit = unit;
    }

    /// <summary>
    /// Get current unit system
    /// </summary>
    public static UnitSystem GetUnitSystem()
    {
        return currentUnit;
    }

    /// <summary>
    /// Format distance with unit
    /// </summary>
    public static string FormatDistance(float meters)
    {
        if (currentUnit == UnitSystem.Imperial)
        {
            float feet = meters * 3.28084f;
            if (feet < 10)
            {
                return $"{feet:F1} ft";
            }
            else
            {
                return $"{feet:F0} ft";
            }
        }
        else
        {
            if (meters < 10)
            {
                return $"{meters:F1} m";
            }
            else
            {
                return $"{meters:F0} m";
            }
        }
    }

    /// <summary>
    /// Convert meters to current unit
    /// </summary>
    public static float Convert(float meters)
    {
        if (currentUnit == UnitSystem.Imperial)
        {
            return meters * 3.28084f;
        }
        return meters;
    }

    /// <summary>
    /// Convert meters to feet
    /// </summary>
    public static float MetersToFeet(float meters)
    {
        return meters * 3.28084f;
    }

    /// <summary>
    /// Convert feet to meters
    /// </summary>
    public static float FeetToMeters(float feet)
    {
        return feet / 3.28084f;
    }
}
