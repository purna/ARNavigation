using UnityEngine;
using System;
using UnityEngine.UI;

/// <summary>
/// AppController coordinates the overall flow of the AR Navigation app:
/// 1. Compass Calibration Screen (initial)
/// 2. Location Selection Screen (choose starting location)
/// 3. Destination Selection Screen (choose where to go)
/// 4. Navigation Overlay (AR view with directions)
/// 5. Arrival Screen (destination reached)
///
/// This is the main entry point that connects all the UI screens together.
/// </summary>
public class AppController : MonoBehaviour
{
    public static AppController Instance { get; private set; }

    [Header("Screen References")]
    [Tooltip("Compass calibration screen")]
    public CompassCalibrationScreen compassCalibrationScreen;

    [Tooltip("Location selection screen (initial)")]
    public LocationSelectionScreen locationSelectionScreen;

    [Tooltip("Destination selection screen")]
    public DestinationSelectionScreen destinationSelectionScreen;

    [Tooltip("Navigation overlay screen")]
    public NavigationOverlayScreen navigationOverlayScreen;

    [Tooltip("Arrival screen")]
    public ArrivalScreen arrivalScreen;

    [Header("Core Managers")]
    [Tooltip("AR Session Manager")]
    public ARSessionManager arSessionManager;

    [Tooltip("User Position Manager")]
    public UserPositionManager userPositionManager;

    [Tooltip("Navigation Manager")]
    public NavigationManager navigationManager;

    [Header("Settings")]
    [Tooltip("Auto-start AR on launch")]
    public bool autoStartAR = true;

    [Tooltip("Show debug info")]
    public bool showDebugInfo = false;

    // App state
    public enum AppState
    {
        Initializing,
        CompassCalibration,
        LocationSelection,
        Ready,
        Navigating,
        Arrived
    }

    public AppState CurrentState { get; private set; }

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
        // Get references if not assigned
        GetReferences();

        // Set up screen event handlers
        SetupScreenEvents();

        // Start the app flow
        StartApp();
    }

    private void GetReferences()
    {
        // Find managers if not assigned
        if (arSessionManager == null)
            arSessionManager = FindObjectOfType<ARSessionManager>();
        
        if (userPositionManager == null)
            userPositionManager = FindObjectOfType<UserPositionManager>();
        
        if (navigationManager == null)
            navigationManager = FindObjectOfType<NavigationManager>();

        // Find screens if not assigned
        if (compassCalibrationScreen == null)
            compassCalibrationScreen = FindObjectOfType<CompassCalibrationScreen>();
        
        if (locationSelectionScreen == null)
            locationSelectionScreen = FindObjectOfType<LocationSelectionScreen>();
        
        if (destinationSelectionScreen == null)
            destinationSelectionScreen = FindObjectOfType<DestinationSelectionScreen>();
        
        if (navigationOverlayScreen == null)
            navigationOverlayScreen = FindObjectOfType<NavigationOverlayScreen>();
        
        if (arrivalScreen == null)
            arrivalScreen = FindObjectOfType<ArrivalScreen>();
    }

    private void SetupScreenEvents()
    {
        // Compass calibration
        if (compassCalibrationScreen != null)
        {
            compassCalibrationScreen.onCalibrationComplete += OnCalibrationComplete;
        }

        // Location selection
        if (locationSelectionScreen != null)
        {
            locationSelectionScreen.onLocationSelected += OnStartLocationSelected;
            locationSelectionScreen.onOtherSelected += OnOtherLocationSelected;
        }

        // Destination selection
        if (destinationSelectionScreen != null)
        {
            destinationSelectionScreen.onDestinationSelected += OnDestinationSelected;
            destinationSelectionScreen.onPanelCollapsed += OnDestinationPanelCollapsed;
        }

        // Navigation
        if (navigationManager != null)
        {
            navigationManager.onDestinationReached += OnDestinationReached;
            navigationManager.onStateChanged += OnNavigationStateChanged;
        }

        // Arrival
        if (arrivalScreen != null)
        {
            arrivalScreen.onDismissed += OnArrivalDismissed;
        }
    }

    private void StartApp()
    {
        SetState(AppState.Initializing);

        // Start AR session
        if (autoStartAR && arSessionManager != null)
        {
            arSessionManager.StartAR();
        }
        else
        {
            // Skip to location selection if AR not auto-started
            ShowLocationSelection();
        }
    }

    /// <summary>
    /// Set app state
    /// </summary>
    private void SetState(AppState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;

        if (showDebugInfo)
            Debug.Log($"AppController: State changed to {newState}");

        // Handle state changes
        switch (newState)
        {
            case AppState.CompassCalibration:
                ShowCompassCalibration();
                break;
            case AppState.LocationSelection:
                ShowLocationSelection();
                break;
            case AppState.Ready:
                ShowReady();
                break;
            case AppState.Navigating:
                ShowNavigating();
                break;
            case AppState.Arrived:
                ShowArrived();
                break;
        }
    }

    /// <summary>
    /// Show compass calibration screen
    /// </summary>
    private void ShowCompassCalibration()
    {
        if (compassCalibrationScreen != null)
        {
            compassCalibrationScreen.Show();
        }
    }

    /// <summary>
    /// Show location selection screen
    /// </summary>
    private void ShowLocationSelection()
    {
        SetState(AppState.LocationSelection);

        if (locationSelectionScreen != null)
        {
            locationSelectionScreen.Show();
        }
    }

    /// <summary>
    /// Show ready state (destination selection collapsed)
    /// </summary>
    private void ShowReady()
    {
        if (destinationSelectionScreen != null)
        {
            destinationSelectionScreen.Show();
        }

        if (navigationOverlayScreen != null)
        {
            navigationOverlayScreen.Hide();
        }
    }

    /// <summary>
    /// Show navigating state
    /// </summary>
    private void ShowNavigating()
    {
        // Destination selection should be collapsed
        if (destinationSelectionScreen != null && destinationSelectionScreen.IsExpanded())
        {
            destinationSelectionScreen.Collapse();
        }

        // Navigation overlay should be visible
        if (navigationOverlayScreen != null)
        {
            navigationOverlayScreen.Show();
        }
    }

    /// <summary>
    /// Show arrived state
    /// </summary>
    private void ShowArrived()
    {
        // Arrival screen handles itself
    }

    // Event handlers

    private void OnCalibrationComplete()
    {
        if (showDebugInfo)
            Debug.Log("AppController: Calibration complete");

        // Proceed to location selection
        ShowLocationSelection();
    }

    private void OnStartLocationSelected(string locationId)
    {
        if (showDebugInfo)
            Debug.Log($"AppController: Start location selected: {locationId}");

        // Set user position to selected location
        if (userPositionManager != null)
        {
            userPositionManager.SetInitialPosition(locationId);
        }

        // Proceed to ready state
        SetState(AppState.Ready);
    }

    private void OnOtherLocationSelected()
    {
        if (showDebugInfo)
            Debug.Log("AppController: Other location selected - showing input");

        // Could show a custom input dialog here
        // For now, just show the destination selection
        if (destinationSelectionScreen != null)
        {
            destinationSelectionScreen.Expand();
        }
    }

    private void OnDestinationSelected(string destinationId)
    {
        if (showDebugInfo)
            Debug.Log($"AppController: Destination selected: {destinationId}");

        // Start navigation
        if (navigationManager != null)
        {
            navigationManager.SetDestination(destinationId);
        }

        // Update state
        SetState(AppState.Navigating);
    }

    private void OnDestinationPanelCollapsed()
    {
        if (showDebugInfo)
            Debug.Log("AppController: Destination panel collapsed");

        // If not navigating, go back to ready state
        if (CurrentState != AppState.Navigating)
        {
            SetState(AppState.Ready);
        }
    }

    private void OnDestinationReached()
    {
        if (showDebugInfo)
            Debug.Log("AppController: Destination reached");

        SetState(AppState.Arrived);
    }

    private void OnNavigationStateChanged(NavigationManager.NavigationState state)
    {
        switch (state)
        {
            case NavigationManager.NavigationState.Navigating:
            case NavigationManager.NavigationState.NeedsFloorTransition:
                SetState(AppState.Navigating);
                break;

            case NavigationManager.NavigationState.Idle:
                SetState(AppState.Ready);
                break;

            case NavigationManager.NavigationState.Arrived:
                SetState(AppState.Arrived);
                break;
        }
    }

    private void OnArrivalDismissed()
    {
        if (showDebugInfo)
            Debug.Log("AppController: Arrival dismissed");

        // Cancel navigation and go back to ready
        if (navigationManager != null)
        {
            navigationManager.CancelNavigation();
        }

        SetState(AppState.Ready);
    }

    /// <summary>
    /// Get current app state
    /// </summary>
    public AppState GetState()
    {
        return CurrentState;
    }

    /// <summary>
    /// Check if app is in navigation state
    /// </summary>
    public bool IsNavigating()
    {
        return CurrentState == AppState.Navigating;
    }
}
