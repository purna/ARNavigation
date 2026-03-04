using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// BottomPanelController manages the bottom panel UI containing the minimap and POI info card.
/// It handles panel expand/collapse and displays navigation information.
///
/// Corresponds to build plan Phase 6 - Bottom Panel
/// </summary>
public class BottomPanelController : MonoBehaviour
{
    public static BottomPanelController Instance { get; private set; }

    [Header("UI Components")]
    [Tooltip("Main bottom panel")]
    public RectTransform bottomPanel;

    [Tooltip("Minimap display")]
    public RawImage minimapDisplay;

    [Tooltip("POI info card")]
    public GameObject poiInfoCard;

    [Tooltip("Destination title text")]
    public TextMeshProUGUI destinationTitleText;

    [Tooltip("Destination description text")]
    public TextMeshProUGUI destinationDescriptionText;

    [Tooltip("Destination floor text")]
    public TextMeshProUGUI destinationFloorText;

    [Tooltip("Distance text")]
    public TextMeshProUGUI distanceText;

    [Tooltip("ETA text")]
    public TextMeshProUGUI etaText;

    [Tooltip("Cancel navigation button")]
    public Button cancelButton;

    [Tooltip("Expand/collapse button")]
    public Button expandButton;

    [Header("Settings")]
    [Tooltip("Panel collapsed height")]
    public float collapsedHeight = 100f;

    [Tooltip("Panel expanded height")]
    public float expandedHeight = 300f;

    [Tooltip("Animation speed")]
    public float animationSpeed = 5f;

    [Tooltip("Walking speed in m/s for ETA calculation")]
    public float walkingSpeed = 1.4f;

    // State
    private bool isExpanded = false;
    private float targetHeight;
    private NavigationManager navigationManager;
    private UserPositionManager userPositionManager;
    private FloorManager floorManager;

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
        floorManager = FindObjectOfType<FloorManager>();

        // Set up button listeners
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        if (expandButton != null)
        {
            expandButton.onClick.AddListener(OnExpandClicked);
        }

        // Set initial state
        targetHeight = collapsedHeight;
        
        // Hide POI card initially
        if (poiInfoCard != null)
        {
            poiInfoCard.SetActive(false);
        }

        // Subscribe to navigation events
        if (navigationManager != null)
        {
            navigationManager.onDestinationSet += OnDestinationSet;
            navigationManager.onDestinationReached += OnDestinationReached;
            navigationManager.onStateChanged += OnNavigationStateChanged;
        }
    }

    private void Update()
    {
        // Animate panel height
        if (bottomPanel != null)
        {
            Vector2 size = bottomPanel.sizeDelta;
            size.y = Mathf.Lerp(size.y, targetHeight, Time.deltaTime * animationSpeed);
            bottomPanel.sizeDelta = size;
        }

        // Update distance and ETA
        if (navigationManager != null && navigationManager.IsNavigating())
        {
            UpdateDistanceAndETA();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Unsubscribe from events
        if (navigationManager != null)
        {
            navigationManager.onDestinationSet -= OnDestinationSet;
            navigationManager.onDestinationReached -= OnDestinationReached;
            navigationManager.onStateChanged -= OnNavigationStateChanged;
        }
    }

    /// <summary>
    /// Handle destination set event
    /// </summary>
    private void OnDestinationSet(LocationEntry destination)
    {
        if (poiInfoCard != null)
        {
            poiInfoCard.SetActive(true);
        }

        if (destinationTitleText != null)
        {
            destinationTitleText.text = destination.title;
        }

        if (destinationDescriptionText != null)
        {
            destinationDescriptionText.text = destination.description;
        }

        if (destinationFloorText != null)
        {
            destinationFloorText.text = destination.GetFloorName();
        }
    }

    /// <summary>
    /// Handle destination reached event
    /// </summary>
    private void OnDestinationReached()
    {
        if (poiInfoCard != null)
        {
            poiInfoCard.SetActive(false);
        }
    }

    /// <summary>
    /// Handle navigation state changed
    /// </summary>
    private void OnNavigationStateChanged(NavigationManager.NavigationState state)
    {
        bool showInfo = state == NavigationManager.NavigationState.Navigating || 
                       state == NavigationManager.NavigationState.NeedsFloorTransition;

        if (poiInfoCard != null)
        {
            poiInfoCard.SetActive(showInfo);
        }

        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(showInfo);
        }
    }

    /// <summary>
    /// Update distance and ETA display
    /// </summary>
    private void UpdateDistanceAndETA()
    {
        if (navigationManager == null || distanceText == null)
            return;

        float distance = navigationManager.GetRemainingPathDistance();
        
        // Format distance
        distanceText.text = DistanceCalculator.FormatDistance(distance);

        // Calculate ETA
        if (etaText != null)
        {
            float etaSeconds = distance / walkingSpeed;
            int minutes = Mathf.FloorToInt(etaSeconds / 60f);
            int seconds = Mathf.FloorToInt(etaSeconds % 60f);
            
            if (minutes > 0)
            {
                etaText.text = $"{minutes}m {seconds}s";
            }
            else
            {
                etaText.text = $"{seconds}s";
            }
        }
    }

    /// <summary>
    /// Handle cancel button click
    /// </summary>
    private void OnCancelClicked()
    {
        if (navigationManager != null)
        {
            navigationManager.CancelNavigation();
        }
    }

    /// <summary>
    /// Handle expand button click
    /// </summary>
    private void OnExpandClicked()
    {
        ToggleExpanded();
    }

    /// <summary>
    /// Toggle panel expanded state
    /// </summary>
    public void ToggleExpanded()
    {
        isExpanded = !isExpanded;
        targetHeight = isExpanded ? expandedHeight : collapsedHeight;
    }

    /// <summary>
    /// Set panel expanded state
    /// </summary>
    public void SetExpanded(bool expanded)
    {
        isExpanded = expanded;
        targetHeight = expanded ? expandedHeight : collapsedHeight;
    }

    /// <summary>
    /// Check if panel is expanded
    /// </summary>
    public bool IsExpanded()
    {
        return isExpanded;
    }

    /// <summary>
    /// Show the bottom panel
    /// </summary>
    public void Show()
    {
        if (bottomPanel != null)
        {
            bottomPanel.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hide the bottom panel
    /// </summary>
    public void Hide()
    {
        if (bottomPanel != null)
        {
            bottomPanel.gameObject.SetActive(false);
        }
    }
}
