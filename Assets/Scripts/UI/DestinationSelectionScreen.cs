using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// DestinationSelectionScreen provides the destination selection UI when navigating.
/// It slides up from the bottom (25% to 75% of screen) and includes:
/// - Room number search field
/// - Quick access buttons by name (Main Entrance, Assembly Hall, Canteen, Library)
///
/// When collapsed, it shows just "Select Destination" with a search icon (5% of screen).
/// The minimap shows in the next 20% of screen space.
/// </summary>
public class DestinationSelectionScreen : MonoBehaviour
{
    public static DestinationSelectionScreen Instance { get; private set; }

    [Header("UI Components")]
    [Tooltip("Main panel (sliding)")]
    public RectTransform mainPanel;

    [Tooltip("Collapsed header")]
    public GameObject collapsedHeader;

    [Tooltip("Expanded content")]
    public GameObject expandedContent;

    [Tooltip("Collapsed title text")]
    public TextMeshProUGUI collapsedTitleText;

    [Tooltip("Search icon")]
    public Image searchIcon;

    [Tooltip("Expanded title")]
    public TextMeshProUGUI expandedTitleText;

    [Tooltip("Room number input field")]
    public TMP_InputField roomNumberInput;

    [Tooltip("Room number label")]
    public Text roomNumberLabel;

    [Tooltip("Name label")]
    public Text nameLabel;

    [Tooltip("Quick access button container")]
    public Transform quickAccessContainer;

    [Tooltip("Quick access button prefab")]
    public GameObject quickAccessButtonPrefab;

    [Tooltip("Confirm button")]
    public Button confirmButton;

    [Tooltip("Close/collapse button")]
    public Button closeButton;

    [Header("Settings")]
    [Tooltip("Collapsed height (% of screen height)")]
    public float collapsedHeightPercent = 0.05f;

    [Tooltip("Expanded height (% of screen height)")]
    public float expandedHeightPercent = 0.75f;

    [Tooltip("Animation speed")]
    public float animationSpeed = 8f;

    // Quick access locations
    private List<string> quickLocations = new List<string> { "MAIN_ENTRANCE", "ASSEMBLY_HALL", "CANTEEN", "LIBRARY" };

    // State
    private bool isExpanded = false;
    private float targetHeight;
    private float screenHeight;

    // Events
    public Action<string> onDestinationSelected;
    public Action onPanelCollapsed;

    private List<Button> quickAccessButtons = new List<Button>();
    private string selectedRoomNumber = "";

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
        // Get screen height
        screenHeight = Screen.height;

        // Set up UI
        SetupUI();

        // Initialize collapsed state
        SetCollapsed(false);
    }

    private void Update()
    {
        // Animate panel height
        if (mainPanel != null)
        {
            Vector2 size = mainPanel.sizeDelta;
            size.y = Mathf.Lerp(size.y, targetHeight, Time.deltaTime * animationSpeed);
            mainPanel.sizeDelta = size;
        }
    }

    /// <summary>
    /// Set up UI components
    /// </summary>
    private void SetupUI()
    {
        // Collapsed header
        if (collapsedTitleText != null)
        {
            collapsedTitleText.text = "Select Destination";
        }

        // Expanded title
        if (expandedTitleText != null)
        {
            expandedTitleText.text = "Select Destination";
        }

        // Room number label
        if (roomNumberLabel != null)
        {
            roomNumberLabel.text = "Find by room number:";
        }

        // Name label
        if (nameLabel != null)
        {
            nameLabel.text = "Find by Name:";
        }

        // Room number input
        if (roomNumberInput != null)
        {
            roomNumberInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter room number";
            roomNumberInput.onValueChanged.AddListener(OnRoomNumberChanged);
        }

        // Confirm button
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            confirmButton.interactable = false;
        }

        // Close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        // Populate quick access buttons
        PopulateQuickAccessButtons();
    }

    /// <summary>
    /// Populate quick access buttons
    /// </summary>
    private void PopulateQuickAccessButtons()
    {
        ClearQuickAccessButtons();

        List<LocationEntry> locations = GetQuickLocations();

        foreach (var location in locations)
        {
            CreateQuickAccessButton(location);
        }
    }

    /// <summary>
    /// Get quick access locations
    /// </summary>
    private List<LocationEntry> GetQuickLocations()
    {
        List<LocationEntry> result = new List<LocationEntry>();

        if (LocationDataManager.Instance != null)
        {
            foreach (string id in quickLocations)
            {
                LocationEntry entry = LocationDataManager.Instance.GetById(id);
                if (entry != null)
                {
                    result.Add(entry);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Create a quick access button
    /// </summary>
    private void CreateQuickAccessButton(LocationEntry location)
    {
        if (quickAccessButtonPrefab == null || quickAccessContainer == null)
            return;

        GameObject buttonObj = Instantiate(quickAccessButtonPrefab, quickAccessContainer);

        // Set button text
        Text buttonText = buttonObj.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = location.title;
        }

        // Add click handler
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnQuickAccessClicked(location.id));
            quickAccessButtons.Add(button);
        }

        buttonObj.SetActive(true);
    }

    /// <summary>
    /// Clear quick access buttons
    /// </summary>
    private void ClearQuickAccessButtons()
    {
        foreach (var button in quickAccessButtons)
        {
            if (button != null && button.gameObject != null)
            {
                Destroy(button.gameObject);
            }
        }
        quickAccessButtons.Clear();
    }

    /// <summary>
    /// Show the destination selection screen (collapsed)
    /// </summary>
    public void Show()
    {
        if (mainPanel != null)
        {
            mainPanel.gameObject.SetActive(true);
            SetCollapsed(true);
        }
    }

    /// <summary>
    /// Hide the destination selection screen
    /// </summary>
    public void Hide()
    {
        if (mainPanel != null)
        {
            mainPanel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Expand the panel
    /// </summary>
    public void Expand()
    {
        SetExpanded(true);
    }

    /// <summary>
    /// Collapse the panel
    /// </summary>
    public void Collapse()
    {
        SetExpanded(false);
        onPanelCollapsed?.Invoke();
    }

    /// <summary>
    /// Set collapsed state
    /// </summary>
    private void SetCollapsed(bool animate)
    {
        isExpanded = false;
        targetHeight = screenHeight * collapsedHeightPercent;

        if (collapsedHeader != null)
            collapsedHeader.SetActive(true);
        if (expandedContent != null)
            expandedContent.SetActive(false);
    }

    /// <summary>
    /// Set expanded state
    /// </summary>
    private void SetExpanded(bool expand)
    {
        isExpanded = expand;
        targetHeight = screenHeight * (expand ? expandedHeightPercent : collapsedHeightPercent);

        if (collapsedHeader != null)
            collapsedHeader.SetActive(!expand);
        if (expandedContent != null)
            expandedContent.SetActive(expand);
    }

    /// <summary>
    /// Handle room number input changed
    /// </summary>
    private void OnRoomNumberChanged(string value)
    {
        selectedRoomNumber = value;

        // Enable confirm button if we have a valid room number
        if (confirmButton != null)
        {
            confirmButton.interactable = !string.IsNullOrWhiteSpace(value);
        }
    }

    /// <summary>
    /// Handle quick access button click
    /// </summary>
    private void OnQuickAccessClicked(string locationId)
    {
        // Find matching location and select it
        if (roomNumberInput != null)
        {
            // Try to find by ID prefix
            LocationEntry location = LocationDataManager.Instance?.GetById(locationId);
            if (location != null)
            {
                roomNumberInput.text = location.title;
            }
        }

        Collapse();
        onDestinationSelected?.Invoke(locationId);
    }

    /// <summary>
    /// Handle confirm button click
    /// </summary>
    private void OnConfirmClicked()
    {
        if (string.IsNullOrWhiteSpace(selectedRoomNumber))
            return;

        // Try to find by room number
        LocationEntry location = FindByRoomNumber(selectedRoomNumber);

        if (location != null)
        {
            Collapse();
            onDestinationSelected?.Invoke(location.id);
        }
    }

    /// <summary>
    /// Handle close button click
    /// </summary>
    private void OnCloseClicked()
    {
        Collapse();
    }

    /// <summary>
    /// Find location by room number
    /// </summary>
    private LocationEntry FindByRoomNumber(string roomNumber)
    {
        if (LocationDataManager.Instance == null || string.IsNullOrWhiteSpace(roomNumber))
            return null;

        // Search by ID or title containing the room number
        string search = roomNumber.ToUpperInvariant().Trim();
        
        var allLocations = LocationDataManager.Instance.SearchLocations(search);
        
        // Return first match
        return allLocations.FirstOrDefault();
    }

    /// <summary>
    /// Check if panel is expanded
    /// </summary>
    public bool IsExpanded()
    {
        return isExpanded;
    }

    /// <summary>
    /// Check if screen is visible
    /// </summary>
    public bool IsVisible()
    {
        return mainPanel != null && mainPanel.gameObject.activeSelf;
    }
}
