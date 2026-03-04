using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR;
using Unity.XR.CoreUtils;  // Required for XROrigin
using System;
using System.Collections;
using ARNavigation.Configuration;

/// <summary>
/// Manages the AR session lifecycle and handles AR subsystem initialization.
/// This is the core AR foundation component that handles starting/stopping AR sessions.
/// </summary>
public class ARSessionManager : MonoBehaviour
{
    public static ARSessionManager Instance { get; private set; }

    [Header("AR Components")]
    [Tooltip("AR Session component for managing AR state")]
    public ARSession arSession;

    [Tooltip("XR Origin transform (XROrigin in AR Foundation 6.x)")]
    public XROrigin xrOrigin;

    [Header("AR Configuration")]
    [Tooltip("Enable horizontal plane detection")]
    public bool detectHorizontalPlanes = true;

    [Tooltip("Enable vertical plane detection")]
    public bool detectVerticalPlanes = true;

    [Tooltip("Enable plane detection on AR start")]
    public bool autoStartPlaneDetection = true;

    [Header("Reference Images")]
    [Tooltip("Reference image library for QR code/image tracking")]
    public XRReferenceImageLibrary referenceImageLibrary;

    [Header("Configuration")]
    [Tooltip("Navigation configuration asset")]
    public NavigationConfig navigationConfig;

    [Header("Events")]
    public Action onARSessionStarted;
    public Action onARSessionStopped;
    public Action<string> onARSessionError;

    // AR Subsystems
    private ARPlaneManager planeManager;
    private ARRaycastManager raycastManager;
    private ARTrackedImageManager trackedImageManager;
    private ARAnchorManager anchorManager;

    // State tracking
    public bool IsARRunning { get; private set; }
    public bool IsARSupported { get; private set; }

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
        // Get or create AR components
        InitializeARComponents();
        
        // Check AR support
        CheckARSupport();
    }

    private void InitializeARComponents()
    {
        // Ensure we have an AR Session
        if (arSession == null)
        {
            arSession = FindObjectOfType<ARSession>();
            if (arSession == null)
            {
                GameObject sessionObj = new GameObject("AR Session");
                arSession = sessionObj.AddComponent<ARSession>();
            }
        }

        // Ensure we have an XR Origin
        if (xrOrigin == null)
        {
            xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("ARSessionManager: XR Origin not found. Please add an XROrigin to the scene.");
            }
        }

        // Get or add AR subsystems from the XROrigin GameObject
        if (xrOrigin != null)
        {
            planeManager = xrOrigin.GetComponent<ARPlaneManager>();
            raycastManager = xrOrigin.GetComponent<ARRaycastManager>();
            trackedImageManager = xrOrigin.GetComponent<ARTrackedImageManager>();
            anchorManager = xrOrigin.GetComponent<ARAnchorManager>();
        }
    }

    private void CheckARSupport()
    {
#if UNITY_ANDROID
        IsARSupported = ARCoreSessionSubsystem.CheckAvailability() == ArAvailability.Supported;
#elif UNITY_IOS
        IsARSupported = ARKitSessionSubsystem.IsSupported();
#else
        IsARSupported = false;
#endif

        if (!IsARSupported)
        {
            Debug.LogWarning("ARSessionManager: AR is not supported on this device/platform.");
        }
    }

    /// <summary>
    /// Start the AR session with plane detection enabled
    /// </summary>
    public void StartAR()
    {
        if (arSession == null)
        {
            onARSessionError?.Invoke("AR Session not initialized");
            return;
        }

        if (IsARRunning)
        {
            Debug.LogWarning("ARSessionManager: AR session is already running");
            return;
        }

        StartCoroutine(StartARSession());
    }

    private IEnumerator StartARSession()
    {
        // Configure plane detection
        ConfigurePlaneDetection();

        // Start the AR session
        arSession.enabled = true;

        // Wait for session to initialize
        yield return new WaitForSeconds(0.5f);

        IsARRunning = true;
        onARSessionStarted?.Invoke();
        
        Debug.Log("ARSessionManager: AR session started successfully");
    }

    private void ConfigurePlaneDetection()
    {
        if (planeManager != null && autoStartPlaneDetection)
        {
            PlaneDetectionMode detectionMode = PlaneDetectionMode.None;

            if (detectHorizontalPlanes)
                detectionMode |= PlaneDetectionMode.Horizontal;

            if (detectVerticalPlanes)
                detectionMode |= PlaneDetectionMode.Vertical;

            planeManager.requestedDetectionMode = detectionMode;
            planeManager.enabled = true;
        }

        // Configure image tracking if reference library is set
        if (trackedImageManager != null && referenceImageLibrary != null)
        {
            trackedImageManager.referenceLibrary = referenceImageLibrary;
            trackedImageManager.enabled = true;
        }
    }

    /// <summary>
    /// Stop the AR session
    /// </summary>
    public void StopAR()
    {
        if (!IsARRunning || arSession == null)
            return;

        // Disable the session to stop it
        arSession.enabled = false;
        IsARRunning = false;
        
        onARSessionStopped?.Invoke();
        
        Debug.Log("ARSessionManager: AR session stopped");
    }

    /// <summary>
    /// Pause the AR session temporarily
    /// </summary>
    public void PauseAR()
    {
        if (arSession != null && IsARRunning)
        {
            // Note: ARFoundation doesn't have a direct pause, but we can stop and restart
            // For now, we'll just disable plane detection to reduce processing
            if (planeManager != null)
            {
                planeManager.enabled = false;
            }
            
            Debug.Log("ARSessionManager: AR session paused (plane detection disabled)");
        }
    }

    /// <summary>
    /// Resume the AR session
    /// </summary>
    public void ResumeAR()
    {
        if (arSession != null && IsARRunning)
        {
            if (planeManager != null)
            {
                planeManager.enabled = true;
            }
            
            Debug.Log("ARSessionManager: AR session resumed");
        }
    }

    /// <summary>
    /// Get the current AR session state
    /// </summary>
    public ARSessionState GetSessionState()
    {
        if (arSession != null)
            return ARSession.state;
        
        return ARSessionState.None;
    }

    /// <summary>
    /// Check if a particular plane detection mode is supported
    /// </summary>
    public bool IsPlaneDetectionSupported(PlaneDetectionMode mode)
    {
        // Check available detection modes
        if (planeManager != null)
        {
            try {
                var supported = planeManager.requestedDetectionMode;
                return (supported & mode) == mode;
            } catch {
                return true; // Default to true if we can't check
            }
        }
        return false;
    }

    /// <summary>
    /// Get all currently detected planes
    /// </summary>
    public System.Collections.Generic.List<ARPlane> GetDetectedPlanes()
    {
        if (planeManager != null)
        {
            var planes = new System.Collections.Generic.List<ARPlane>();
            foreach (var plane in planeManager.trackables)
            {
                planes.Add(plane);
            }
            return planes;
        }
        return null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            PauseAR();
        }
        else
        {
            ResumeAR();
        }
    }

    private void OnApplicationQuit()
    {
        StopAR();
    }
}