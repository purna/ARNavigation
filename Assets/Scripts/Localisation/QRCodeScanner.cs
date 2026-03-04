using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Collections.Generic;

/// <summary>
/// QRCodeScanner handles QR code detection using ARFoundation's ARTrackedImageManager.
/// It can also work with ZXing.Net for camera-based scanning.
///
/// Corresponds to build plan Phase 3 - QR Code Strategy
/// </summary>
public class QRCodeScanner : MonoBehaviour
{
    public static QRCodeScanner Instance { get; private set; }

    [Header("AR Components")]
    [Tooltip("AR Tracked Image Manager for marker detection")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Scanner Settings")]
    [Tooltip("Enable QR code scanning")]
    public bool enableScanning = true;

    [Tooltip("Show debug info")]
    public bool showDebugInfo = false;

    [Header("ZXing Integration (Optional)")]
    [Tooltip("Use ZXing for camera-based scanning instead of ARTrackedImageManager")]
    public bool useZXing = false;

    [Tooltip("Camera to use for ZXing scanning")]
    public Camera scanningCamera;

    // Events
    public Action<string, Vector3, Quaternion> onQRCodeDetected;
    public Action<string> onQRCodeLost;

    // Tracking state
    private Dictionary<string, ARTrackedImage> trackedImages = new Dictionary<string, ARTrackedImage>();
    private HashSet<string> previouslyDetectedCodes = new HashSet<string>();

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
        // Get AR Tracked Image Manager
        if (trackedImageManager == null)
        {
            ARSessionManager arSession = FindObjectOfType<ARSessionManager>();
            if (arSession != null && arSession.xrOrigin != null)
            {
                trackedImageManager = arSession.xrOrigin.GetComponent<ARTrackedImageManager>();
            }
        }

        // Subscribe to tracked images changed events
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        // Get scanning camera for ZXing
        if (useZXing && scanningCamera == null)
        {
            scanningCamera = Camera.main;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Unsubscribe from events
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    /// <summary>
    /// Handle tracked images changed event
    /// </summary>
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        if (!enableScanning)
            return;

        // Handle newly detected images
        foreach (var trackedImage in eventArgs.added)
        {
            ProcessTrackedImage(trackedImage);
        }

        // Handle updated images
        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                ProcessTrackedImage(trackedImage);
            }
            else
            {
                HandleImageLost(trackedImage);
            }
        }

        // Handle removed images
        foreach (var trackedImage in eventArgs.removed)
        {
            HandleImageLost(trackedImage);
        }
    }

    /// <summary>
    /// Process a tracked QR code image
    /// </summary>
    private void ProcessTrackedImage(ARTrackedImage trackedImage)
    {
        string qrCode = trackedImage.referenceImage.name;

        // Update tracked images dictionary
        trackedImages[qrCode] = trackedImage;

        // Check if this is a new detection
        if (!previouslyDetectedCodes.Contains(qrCode))
        {
            previouslyDetectedCodes.Add(qrCode);

            if (showDebugInfo)
            {
                Debug.Log($"QRCodeScanner: New QR code detected - {qrCode}");
                Debug.Log($"  Position: {trackedImage.transform.position}");
                Debug.Log($"  Rotation: {trackedImage.transform.rotation.eulerAngles}");
            }

            onQRCodeDetected?.Invoke(qrCode, trackedImage.transform.position, trackedImage.transform.rotation);
        }
    }

    /// <summary>
    /// Handle when a QR code is no longer tracked
    /// </summary>
    private void HandleImageLost(ARTrackedImage trackedImage)
    {
        string qrCode = trackedImage.referenceImage.name;

        if (trackedImages.ContainsKey(qrCode))
        {
            trackedImages.Remove(qrCode);

            if (showDebugInfo)
            {
                Debug.Log($"QRCodeScanner: QR code lost - {qrCode}");
            }

            onQRCodeLost?.Invoke(qrCode);
        }
    }

    /// <summary>
    /// Enable or disable scanning
    /// </summary>
    public void SetScanningEnabled(bool enabled)
    {
        enableScanning = enabled;

        if (!enabled)
        {
            trackedImages.Clear();
        }
    }

    /// <summary>
    /// Get currently tracked QR codes
    /// </summary>
    public List<string> GetTrackedQRCodes()
    {
        return new List<string>(trackedImages.Keys);
    }

    /// <summary>
    /// Check if a specific QR code is currently tracked
    /// </summary>
    public bool IsQRCodeTracked(string qrCode)
    {
        return trackedImages.ContainsKey(qrCode);
    }

    /// <summary>
    /// Get position of a tracked QR code
    /// </summary>
    public Vector3? GetTrackedQRCodePosition(string qrCode)
    {
        if (trackedImages.TryGetValue(qrCode, out ARTrackedImage image))
        {
            return image.transform.position;
        }
        return null;
    }

    /// <summary>
    /// Get all tracked QR code positions
    /// </summary>
    public Dictionary<string, Vector3> GetAllTrackedPositions()
    {
        Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
        
        foreach (var kvp in trackedImages)
        {
            positions[kvp.Key] = kvp.Value.transform.position;
        }
        
        return positions;
    }

    /// <summary>
    /// Reset tracking state (for new session)
    /// </summary>
    public void ResetTracking()
    {
        trackedImages.Clear();
        previouslyDetectedCodes.Clear();
    }

    /// <summary>
    /// Manually trigger QR code detection (for testing or external input)
    /// </summary>
    public void SimulateQRCodeDetection(string qrCode, Vector3 position, Quaternion rotation)
    {
        if (showDebugInfo)
        {
            Debug.Log($"QRCodeScanner: Simulated detection - {qrCode} at {position}");
        }

        onQRCodeDetected?.Invoke(qrCode, position, rotation);
    }
}

/// <summary>
/// Extension to ARTrackedImage for convenience
/// </summary>
public static class ARTrackedImageExtensions
{
    /// <summary>
    /// Get the QR code string from the reference image name
    /// </summary>
    public static string GetQRCode(this ARTrackedImage trackedImage)
    {
        if (trackedImage?.referenceImage == null)
            return string.Empty;
        
        return trackedImage.referenceImage.name;
    }
}
