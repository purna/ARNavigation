using UnityEngine;
using System;

/// <summary>
/// UIManager coordinates all UI state transitions in the AR Navigation app.
/// It manages which UI elements are visible based on the current app state.
///
/// Corresponds to build plan Phase 6 - UI System
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [Tooltip("Top bar panel")]
    public GameObject topBarPanel;

    [Tooltip("AR overlay panel")]
    public GameObject arOverlayPanel;

    [Tooltip("Bottom panel")]
    public GameObject bottomPanel;

    [Tooltip("Settings panel")]
    public GameObject settingsPanel;

    [Tooltip("Onboarding panel")]
    public GameObject onboardingPanel;

    [Tooltip("QR Scan prompt panel")]
    public GameObject qrScanPromptPanel;

    [Header("AR Overlay Elements")]
    [Tooltip("Directional arrow")]
    public GameObject directionalArrow;

    [Tooltip("Distance text")]
    public UnityEngine.UI.Text distanceText;

    [Tooltip("Floor indicator")]
    public GameObject floorIndicator;

    [Header("AR Session UI")]
    [Tooltip("AR not supported message")]
    public GameObject arNotSupportedPanel;

    [Header("Debug")]
    [Tooltip("Show debug info")]
    public bool showDebugInfo = false;

    // UI State
    public enum UIState
    {
        Initializing,
        Onboarding,
        WaitingForScan,
        Ready,
        Navigating,
        Settings
    }

    public UIState CurrentState { get; private set; }

    // Events
    public Action<UIState> onStateChanged;

    // References
    private ARSessionManager arSessionManager;
    private UserPositionManager userPositionManager;
    private NavigationManager navigationManager;
    private SettingsManager settingsManager;

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
        arSessionManager = FindObjectOfType<ARSessionManager>();
        userPositionManager = FindObjectOfType<UserPositionManager>();
        navigationManager = FindObjectOfType<NavigationManager>();
        settingsManager = FindObjectOfType<SettingsManager>();

        // Subscribe to events
        if (arSessionManager != null)
        {
            arSessionManager.onARSessionStarted += OnARSessionStarted;
            arSessionManager.onARSessionStopped += OnARSessionStopped;
            arSessionManager.onARSessionError += OnARSessionError;
        }

        if (navigationManager != null)
        {
            navigationManager.onStateChanged += OnNavigationStateChanged;
        }

        // Initial state
        SetState(UIState.Initializing);

        // Check first launch
        CheckFirstLaunch();
    }

    private void Update()
    {
        // Update based on AR session state
        if (CurrentState == UIState.Initializing)
        {
            if (arSessionManager != null && arSessionManager.IsARRunning)
            {
                SetState(UIState.WaitingForScan);
            }
        }
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
            arSessionManager.onARSessionError -= OnARSessionError;
        }

        if (navigationManager != null)
        {
            navigationManager.onStateChanged -= OnNavigationStateChanged;
        }
    }

    /// <summary>
    /// Check if this is the first launch
    /// </summary>
    private void CheckFirstLaunch()
    {
        if (settingsManager != null && settingsManager.IsFirstLaunch)
        {
            ShowOnboarding();
        }
        else
        {
            // Check AR support
            if (arSessionManager != null && !arSessionManager.IsARSupported)
            {
                ShowARNotSupported();
            }
        }
    }

    /// <summary>
    /// Set the UI state
    /// </summary>
    public void SetState(UIState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        
        if (showDebugInfo)
            Debug.Log($"UIManager: State changed to {newState}");

        // Update UI based on state
        UpdateUIForState(newState);
        
        onStateChanged?.Invoke(newState);
    }

    /// <summary>
    /// Update UI elements for the current state
    /// </summary>
    private void UpdateUIForState(UIState state)
    {
        // Hide all panels first
        HideAllPanels();

        // Show relevant panels based on state
        switch (state)
        {
            case UIState.Initializing:
                // Show loading indicator
                break;

            case UIState.Onboarding:
                if (onboardingPanel != null)
                    onboardingPanel.SetActive(true);
                break;

            case UIState.WaitingForScan:
                if (topBarPanel != null)
                    topBarPanel.SetActive(true);
                if (qrScanPromptPanel != null)
                    qrScanPromptPanel.SetActive(true);
                if (bottomPanel != null)
                    bottomPanel.SetActive(true);
                break;

            case UIState.Ready:
                if (topBarPanel != null)
                    topBarPanel.SetActive(true);
                if (bottomPanel != null)
                    bottomPanel.SetActive(true);
                break;

            case UIState.Navigating:
                if (topBarPanel != null)
                    topBarPanel.SetActive(true);
                if (arOverlayPanel != null)
                    arOverlayPanel.SetActive(true);
                if (bottomPanel != null)
                    bottomPanel.SetActive(true);
                break;

            case UIState.Settings:
                if (topBarPanel != null)
                    topBarPanel.SetActive(true);
                if (settingsPanel != null)
                    settingsPanel.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// Hide all panels
    /// </summary>
    private void HideAllPanels()
    {
        if (topBarPanel != null)
            topBarPanel.SetActive(false);
        if (arOverlayPanel != null)
            arOverlayPanel.SetActive(false);
        if (bottomPanel != null)
            bottomPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (onboardingPanel != null)
            onboardingPanel.SetActive(false);
        if (qrScanPromptPanel != null)
            qrScanPromptPanel.SetActive(false);
        if (arNotSupportedPanel != null)
            arNotSupportedPanel.SetActive(false);
    }

    /// <summary>
    /// Handle AR session started
    /// </summary>
    private void OnARSessionStarted()
    {
        if (settingsManager != null && settingsManager.IsFirstLaunch)
        {
            SetState(UIState.Onboarding);
        }
        else
        {
            SetState(UIState.WaitingForScan);
        }
    }

    /// <summary>
    /// Handle AR session stopped
    /// </summary>
    private void OnARSessionStopped()
    {
        HideAllPanels();
    }

    /// <summary>
    /// Handle AR session error
    /// </summary>
    private void OnARSessionError(string error)
    {
        Debug.LogError($"UIManager: AR Session Error - {error}");
    }

    /// <summary>
    /// Handle navigation state changed
    /// </summary>
    private void OnNavigationStateChanged(NavigationManager.NavigationState state)
    {
        switch (state)
        {
            case NavigationManager.NavigationState.Navigating:
            case NavigationManager.NavigationState.NeedsFloorTransition:
                SetState(UIState.Navigating);
                break;

            case NavigationManager.NavigationState.Idle:
                SetState(UIState.Ready);
                break;

            case NavigationManager.NavigationState.Arrived:
                SetState(UIState.Ready);
                break;
        }
    }

    /// <summary>
    /// Show onboarding panel
    /// </summary>
    public void ShowOnboarding()
    {
        SetState(UIState.Onboarding);
    }

    /// <summary>
    /// Complete onboarding
    /// </summary>
    public void CompleteOnboarding()
    {
        if (settingsManager != null)
        {
            settingsManager.CompleteFirstLaunch();
        }
        
        if (arSessionManager != null && arSessionManager.IsARRunning)
        {
            SetState(UIState.WaitingForScan);
        }
    }

    /// <summary>
    /// Show settings panel
    /// </summary>
    public void ShowSettings()
    {
        SetState(UIState.Settings);
    }

    /// <summary>
    /// Close settings panel
    /// </summary>
    public void CloseSettings()
    {
        if (navigationManager != null && navigationManager.IsNavigating())
        {
            SetState(UIState.Navigating);
        }
        else
        {
            SetState(UIState.Ready);
        }
    }

    /// <summary>
    /// Show AR not supported panel
    /// </summary>
    public void ShowARNotSupported()
    {
        if (arNotSupportedPanel != null)
        {
            arNotSupportedPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Update distance text in AR overlay
    /// </summary>
    public void UpdateDistanceText(float distance)
    {
        if (distanceText != null)
        {
            distanceText.text = DistanceCalculator.FormatDistance(distance);
        }
    }

    /// <summary>
    /// Update floor indicator
    /// </summary>
    public void UpdateFloorIndicator(int currentFloor, int destinationFloor)
    {
        if (floorIndicator != null)
        {
            UnityEngine.UI.Text floorText = floorIndicator.GetComponent<UnityEngine.UI.Text>();
            if (floorText != null)
            {
                if (currentFloor != destinationFloor)
                {
                    floorText.text = $"Floor {currentFloor} → Floor {destinationFloor}";
                }
                else
                {
                    floorText.text = $"Floor {currentFloor}";
                }
            }
        }
    }

    /// <summary>
    /// Show QR scan prompt
    /// </summary>
    public void ShowQRScanPrompt()
    {
        if (qrScanPromptPanel != null)
        {
            qrScanPromptPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hide QR scan prompt
    /// </summary>
    public void HideQRScanPrompt()
    {
        if (qrScanPromptPanel != null)
        {
            qrScanPromptPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Get current UI state
    /// </summary>
    public UIState GetState()
    {
        return CurrentState;
    }

    /// <summary>
    /// Check if in navigating state
    /// </summary>
    public bool IsNavigating()
    {
        return CurrentState == UIState.Navigating;
    }
}
