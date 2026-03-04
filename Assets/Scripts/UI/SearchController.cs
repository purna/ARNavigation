using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

/// <summary>
/// SearchController handles the predictive search UI for finding locations.
/// It provides instant search results as the user types.
///
/// Corresponds to build plan Phase 6 - UI Search
/// </summary>
public class SearchController : MonoBehaviour
{
    public static SearchController Instance { get; private set; }

    [Header("UI Components")]
    [Tooltip("Search input field")]
    public TMP_InputField searchInput;

    [Tooltip("Search results scroll view")]
    public ScrollRect resultsScrollView;

    [Tooltip("Results container")]
    public Transform resultsContainer;

    [Tooltip("Result item prefab")]
    public GameObject resultItemPrefab;

    [Header("Settings")]
    [Tooltip("Minimum characters to trigger search")]
    public int minCharacters = 1;

    [Tooltip("Maximum results to show")]
    public int maxResults = 10;

    [Tooltip("Search delay in seconds")]
    public float searchDelay = 0.1f;

    [Header("Events")]
    public Action<LocationEntry> onLocationSelected;

    // State
    private List<LocationEntry> currentResults = new List<LocationEntry>();
    private List<GameObject> resultItemInstances = new List<GameObject>();
    private float lastSearchTime = 0f;
    private bool isResultsVisible = false;

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
        // Set up search input listener
        if (searchInput != null)
        {
            searchInput.onValueChanged.AddListener(OnSearchTextChanged);
            searchInput.onSubmit.AddListener(OnSearchSubmit);
        }

        // Hide results initially
        HideResults();
    }

    /// <summary>
    /// Handle search text changes
    /// </summary>
    private void OnSearchTextChanged(string text)
    {
        lastSearchTime = Time.time;
        
        if (text.Length >= minCharacters)
        {
            // Schedule search
            Invoke(nameof(PerformSearch), searchDelay);
        }
        else
        {
            HideResults();
        }
    }

    /// <summary>
    /// Handle search submit
    /// </summary>
    private void OnSearchSubmit(string text)
    {
        if (currentResults.Count > 0)
        {
            SelectLocation(currentResults[0]);
        }
    }

    /// <summary>
    /// Perform the search
    /// </summary>
    private void PerformSearch()
    {
        string searchText = searchInput != null ? searchInput.text : "";
        
        if (searchText.Length < minCharacters)
            return;

        // Get search results from LocationDataManager
        if (LocationDataManager.Instance != null)
        {
            currentResults = LocationDataManager.Instance.SearchLocations(searchText);
            
            // Limit results
            if (currentResults.Count > maxResults)
            {
                currentResults = currentResults.GetRange(0, maxResults);
            }

            // Update UI
            DisplayResults(currentResults);
        }
    }

    /// <summary>
    /// Display search results in the UI
    /// </summary>
    private void DisplayResults(List<LocationEntry> results)
    {
        // Clear existing results
        ClearResultItems();

        // Check if we have results
        if (results.Count == 0)
        {
            ShowNoResultsMessage();
            return;
        }

        // Create result items
        foreach (var location in results)
        {
            CreateResultItem(location);
        }

        // Show results
        ShowResults();
    }

    /// <summary>
    /// Create a single result item
    /// </summary>
    private void CreateResultItem(LocationEntry location)
    {
        if (resultItemPrefab == null || resultsContainer == null)
            return;

        GameObject item = Instantiate(resultItemPrefab, resultsContainer);
        
        // Set up item data
        SearchResultItem itemScript = item.GetComponent<SearchResultItem>();
        if (itemScript != null)
        {
            itemScript.Setup(location, OnResultItemClicked);
        }
        
        resultItemInstances.Add(item);
    }

    /// <summary>
    /// Handle result item click
    /// </summary>
    private void OnResultItemClicked(LocationEntry location)
    {
        SelectLocation(location);
    }

    /// <summary>
    /// Select a location from search results
    /// </summary>
    public void SelectLocation(LocationEntry location)
    {
        if (location == null)
            return;

        if (searchInput != null)
        {
            searchInput.text = location.title;
        }

        HideResults();
        
        onLocationSelected?.Invoke(location);
        
        Debug.Log($"SearchController: Selected location - {location.title}");
    }

    /// <summary>
    /// Clear all result items
    /// </summary>
    private void ClearResultItems()
    {
        foreach (var item in resultItemInstances)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        resultItemInstances.Clear();
    }

    /// <summary>
    /// Show no results message
    /// </summary>
    private void ShowNoResultsMessage()
    {
        ClearResultItems();
        
        if (resultItemPrefab != null && resultsContainer != null)
        {
            GameObject noResults = Instantiate(resultItemPrefab, resultsContainer);
            SearchResultItem noResultsScript = noResults.GetComponent<SearchResultItem>();
            if (noResultsScript != null)
            {
                noResultsScript.ShowNoResults();
            }
            resultItemInstances.Add(noResults);
        }
        
        ShowResults();
    }

    /// <summary>
    /// Show results panel
    /// </summary>
    private void ShowResults()
    {
        if (resultsScrollView != null)
        {
            resultsScrollView.gameObject.SetActive(true);
        }
        isResultsVisible = true;
    }

    /// <summary>
    /// Hide results panel
    /// </summary>
    public void HideResults()
    {
        if (resultsScrollView != null)
        {
            resultsScrollView.gameObject.SetActive(false);
        }
        isResultsVisible = false;
    }

    /// <summary>
    /// Clear search input
    /// </summary>
    public void ClearSearch()
    {
        if (searchInput != null)
        {
            searchInput.text = "";
        }
        
        ClearResultItems();
        currentResults.Clear();
        HideResults();
    }

    /// <summary>
    /// Focus on search input
    /// </summary>
    public void FocusSearch()
    {
        if (searchInput != null)
        {
            searchInput.ActivateInputField();
        }
    }

    /// <summary>
    /// Check if results are visible
    /// </summary>
    public bool IsResultsVisible()
    {
        return isResultsVisible;
    }
}

/// <summary>
/// SearchResultItem represents a single item in the search results list
/// </summary>
public class SearchResultItem : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI floorText;
    public TextMeshProUGUI descriptionText;
    public Image categoryIcon;
    public Button selectButton;

    private LocationEntry location;
    private Action<LocationEntry> onClickCallback;

    private void Start()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnClicked);
        }
    }

    /// <summary>
    /// Set up the result item
    /// </summary>
    public void Setup(LocationEntry location, Action<LocationEntry> onClick)
    {
        this.location = location;
        this.onClickCallback = onClick;

        if (titleText != null)
        {
            titleText.text = location.title;
        }

        if (floorText != null)
        {
            floorText.text = location.GetFloorName();
        }

        if (descriptionText != null)
        {
            // Truncate description if too long
            string desc = location.description;
            if (desc.Length > 50)
            {
                desc = desc.Substring(0, 47) + "...";
            }
            descriptionText.text = desc;
        }
    }

    /// <summary>
    /// Show no results message
    /// </summary>
    public void ShowNoResults()
    {
        if (titleText != null)
        {
            titleText.text = "No results found";
        }
        
        if (selectButton != null)
        {
            selectButton.interactable = false;
        }
    }

    /// <summary>
    /// Handle click
    /// </summary>
    private void OnClicked()
    {
        if (location != null && onClickCallback != null)
        {
            onClickCallback.Invoke(location);
        }
    }
}
