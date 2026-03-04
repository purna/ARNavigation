using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// NavigationOverlayScreen manages the AR overlay displayed during navigation.
/// It shows:
/// - Directional arrow pointing to next waypoint
/// - Distance text
/// - Floor indicator
///
/// This overlay appears in the top 75% of the screen (above the minimap).
/// </summary>
public class NavigationOverlayScreen : MonoBehaviour
{
    public static NavigationOverlayScreen Instance { get; private set; }

    [Header("UI Components")]
    [Tooltip("Main overlay panel")]
    public GameObject overlayPanel;

    [Tooltip("Directional arrow image")]
    public Image directionalArrow;

    [Tooltip("Distance text")]
    public TextMeshProUGUI distanceText;

    [Tooltip("Floor indicator text")]
    public TextMeshProUGUI floorIndicatorText;

    [Tooltip("Instruction text (e.g., 'Turn left')")]
    public TextMeshProUGUI instructionText;

    [Tooltip("Minimap area")]
    public RectTransform minimapArea;

    [Header("Settings")]
    [Tooltip("Minimap height percentage of screen")]
    public float minimapHeightPercent = 0.20f;

    [Tooltip("Arrow animation speed")]
    public float arrowAnimationSpeed = 2f;

    [Tooltip("Arrow pulse amplitude")]
    public float pulseAmplitude = 0.1f;

    // References
    private NavigationManager navigationManager;
    private UserPositionManager userPositionManager;
    private ArrowDirectionController arrowController;
    private float screenHeight;

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
        arrowController = FindObjectOfType<ArrowDirectionController>();

        // Get screen height
        screenHeight = Screen.height;

        // Set up UI
        SetupUI();

        // Hide initially
        if (overlayPanel != null)
        {
            overlayPanel.SetActive(false);
        }

        // Subscribe to navigation events
        if (navigationManager != null)
        {
            navigationManager.onStateChanged += OnNavigationStateChanged;
            navigationManager.onDestinationSet += OnDestinationSet;
        }
    }

    private void Update()
    {
        if (!IsNavigating())
            return;

        UpdateOverlay();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (navigationManager != null)
        {
            navigationManager.onStateChanged -= OnNavigationStateChanged;
            navigationManager.onDestinationSet -= OnDestinationSet;
        }
    }

    /// <summary>
    /// Set up UI components
    /// </summary>
    private void SetupUI()
    {
        // Set up minimap area size
        if (minimapArea != null)
        {
            float height = screenHeight * minimapHeightPercent;
            minimapArea.sizeDelta = new Vector2(minimapArea.sizeDelta.x, height);
        }
    }

    /// <summary>
    /// Handle navigation state changed
    /// </summary>
    private void OnNavigationStateChanged(NavigationManager.NavigationState state)
    {
        if (state == NavigationManager.NavigationState.Navigating ||
            state == NavigationManager.NavigationState.NeedsFloorTransition)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    /// <summary>
    /// Handle destination set
    /// </summary>
    private void OnDestinationSet(LocationEntry destination)
    {
        // Update floor indicator
        UpdateFloorIndicator();
    }

    /// <summary>
    /// Update overlay elements
    /// </summary>
    private void UpdateOverlay()
    {
        // Update distance
        if (navigationManager != null && distanceText != null)
        {
            float distance = navigationManager.GetRemainingPathDistance();
            distanceText.text = DistanceCalculator.FormatDistance(distance);
        }

        // Update arrow rotation
        if (arrowController != null)
        {
            // Arrow is controlled by ArrowDirectionController
        }

        // Pulse animation for arrow
        if (directionalArrow != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * arrowAnimationSpeed) * pulseAmplitude;
            directionalArrow.rectTransform.localScale = Vector3.one * pulse;
        }
    }

    /// <summary>
    /// Update floor indicator
    /// </summary>
    private void UpdateFloorIndicator()
    {
        if (floorIndicatorText == null || navigationManager == null)
            return;

        int currentFloor = userPositionManager != null ? userPositionManager.GetCurrentFloor() : 0;
        int destFloor = navigationManager.CurrentDestination?.floor ?? 0;

        if (currentFloor != destFloor)
        {
            floorIndicatorText.text = $"Floor {currentFloor} → Floor {destFloor}";
        }
        else
        {
            floorIndicatorText.text = $"Floor {currentFloor}";
        }
    }

    /// <summary>
    /// Show the navigation overlay
    /// </summary>
    public void Show()
    {
        if (overlayPanel != null)
        {
            overlayPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hide the navigation overlay
    /// </summary>
    public void Hide()
    {
        if (overlayPanel != null)
        {
            overlayPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Check if currently navigating (overlay visible)
    /// </summary>
    public bool IsNavigating()
    {
        return navigationManager != null && navigationManager.IsNavigating();
    }

    /// <summary>
    /// Update instruction text
    /// </summary>
    public void SetInstruction(string instruction)
    {
        if (instructionText != null)
        {
            instructionText.text = instruction;
        }
    }

    /// <summary>
    /// Get the minimap area rect transform
    /// </summary>
    public RectTransform GetMinimapArea()
    {
        return minimapArea;
    }
}
