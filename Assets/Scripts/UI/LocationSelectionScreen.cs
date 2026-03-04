using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// LocationSelectionScreen displays the initial location selection when the app starts.
/// It shows "Please choose your current location" with a list of predefined locations
/// and an "Other" button for manual entry.
///
/// This is shown after the compass calibration screen.
/// </summary>
public class LocationSelectionScreen : MonoBehaviour
{
    public static LocationSelectionScreen Instance { get; private set; }

    [Header("UI Components")]
    [Tooltip("Main panel")]
    public GameObject panel;

    [Tooltip("Title text")]
    public TextMeshProUGUI titleText;

    [Tooltip("Scroll view content")]
    public Transform contentContainer;

    [Tooltip("Location button prefab")]
    public GameObject locationButtonPrefab;

    [Tooltip("Other button")]
    public Button otherButton;

    [Header("Settings")]
    [Tooltip("Title message")]
    public string titleMessage = "Please choose your current location";

    [Tooltip("Other button text")]
    public string otherButtonText = "Other";

    // Quick access locations (shown as buttons)
    private List<string> quickLocations = new List<string> { "MAIN_ENTRANCE", "ASSEMBLY_HALL", "CANTEEN", "LIBRARY" };

    // Events
    public Action<string> onLocationSelected;
    public Action onOtherSelected;

    private List<Button> locationButtons = new List<Button>();

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
        // Set up UI
        SetupUI();

        // Hide initially
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    /// <parameter name="action">Set up UI components</parameter>
    /// <summary>
    /// 
    /// </summary>
    private void SetupUI()
    {
        if (titleText != null)
        {
            titleText.text = titleMessage;
        }

        if (otherButton != null)
        {
            otherButton.onClick.AddListener(OnOtherClicked);
            
            Text buttonText = otherButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = otherButtonText;
            }
        }
    }

    /// <summary>
    /// Show the location selection screen
    /// </summary>
    public void Show()
    {
        // Populate locations from data
        PopulateLocations();

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    /// <summary>
    /// Hide the location selection screen
    /// </summary>
    public void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// Populate location buttons from data
    /// </summary>
    private void PopulateLocations()
    {
        // Clear existing buttons
        ClearButtons();

        // Get quick access locations
        List<LocationEntry> locations = GetQuickLocations();

        // Create buttons for each location
        foreach (var location in locations)
        {
            CreateLocationButton(location);
        }
    }

    /// <summary>
    /// Get quick access locations from data
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
    /// Create a location button
    /// </summary>
    private void CreateLocationButton(LocationEntry location)
    {
        if (locationButtonPrefab == null || contentContainer == null)
            return;

        GameObject buttonObj = Instantiate(locationButtonPrefab, contentContainer);
        
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
            button.onClick.AddListener(() => OnLocationClicked(location.id));
            locationButtons.Add(button);
        }

        buttonObj.SetActive(true);
    }

    /// <summary>
    /// Clear all location buttons
    /// </summary>
    private void ClearButtons()
    {
        foreach (var button in locationButtons)
        {
            if (button != null && button.gameObject != null)
            {
                Destroy(button.gameObject);
            }
        }
        locationButtons.Clear();
    }

    /// <summary>
    /// Handle location button click
    /// </summary>
    private void OnLocationClicked(string locationId)
    {
        Hide();
        onLocationSelected?.Invoke(locationId);
    }

    /// <summary>
    /// Handle "Other" button click
    /// </summary>
    private void OnOtherClicked()
    {
        Hide();
        onOtherSelected?.Invoke();
    }

    /// <summary>
    /// Check if screen is visible
    /// </summary>
    public bool IsVisible()
    {
        return panel != null && panel.activeSelf;
    }
}
