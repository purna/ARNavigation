using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// ArrivalScreen displays the arrival notification when the user reaches their destination.
/// It shows:
/// - A 3D marker at the destination location in AR
/// - A message showing the location name
/// - An OK/Dismiss button
///
/// This appears when NavigationManager reports NavigationState.Arrived.
/// </summary>
public class ArrivalScreen : MonoBehaviour
{
    public static ArrivalScreen Instance { get; private set; }

    [Header("UI Components")]
    [Tooltip("Arrival notification panel")]
    public GameObject notificationPanel;

    [Tooltip("Location name text")]
    public TextMeshProUGUI locationNameText;

    [Tooltip("Arrival message text")]
    public TextMeshProUGUI arrivalMessageText;

    [Tooltip("OK/Dismiss button")]
    public Button dismissButton;

    [Header("3D Marker")]
    [Tooltip("Marker prefab to spawn at destination")]
    public GameObject markerPrefab;

    [Tooltip("Marker spawn point (destination location)")]
    public Transform markerSpawnPoint;

    [Tooltip("Marker animation duration")]
    public float markerAnimationDuration = 2f;

    [Header("Settings")]
    [Tooltip("Arrival message")]
    public string arrivalMessage = "You have arrived at";

    [Tooltip("Dismiss button text")]
    public string dismissButtonText = "OK";

    // Current marker instance
    private GameObject currentMarker;

    // Events
    public Action onDismissed;

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
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }

        // Subscribe to navigation events
        NavigationManager navManager = FindObjectOfType<NavigationManager>();
        if (navManager != null)
        {
            navManager.onDestinationReached += OnDestinationReached;
            navManager.onStateChanged += OnNavigationStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        NavigationManager navManager = FindObjectOfType<NavigationManager>();
        if (navManager != null)
        {
            navManager.onDestinationReached -= OnDestinationReached;
            navManager.onStateChanged -= OnNavigationStateChanged;
        }

        // Clean up marker
        if (currentMarker != null)
        {
            Destroy(currentMarker);
        }
    }

    /// <summary>
    /// Set up UI components
    /// </summary>
    private void SetupUI()
    {
        // Notification panel
        if (notificationPanel == null)
        {
            Debug.LogWarning("ArrivalScreen: Notification panel not assigned");
        }

        // Arrival message
        if (arrivalMessageText != null)
        {
            arrivalMessageText.text = arrivalMessage;
        }

        // Dismiss button
        if (dismissButton != null)
        {
            dismissButton.onClick.AddListener(OnDismissClicked);
            
            Text buttonText = dismissButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = dismissButtonText;
            }
        }
    }

    /// <summary>
    /// Handle destination reached event
    /// </summary>
    private void OnDestinationReached()
    {
        NavigationManager navManager = FindObjectOfType<NavigationManager>();
        if (navManager?.CurrentDestination != null)
        {
            ShowArrival(navManager.CurrentDestination);
        }
    }

    /// <summary>
    /// Handle navigation state changed
    /// </summary>
    private void OnNavigationStateChanged(NavigationManager.NavigationState state)
    {
        if (state == NavigationManager.NavigationState.Arrived)
        {
            // Already handled by OnDestinationReached
        }
        else if (state == NavigationManager.NavigationState.Idle || 
                 state == NavigationManager.NavigationState.Navigating)
        {
            // Hide when navigating starts again
            Hide();
        }
    }

    /// <summary>
    /// Show arrival notification
    /// </summary>
    public void ShowArrival(LocationEntry location)
    {
        // Update UI text
        if (locationNameText != null && location != null)
        {
            locationNameText.text = location.title;
        }

        // Show notification panel
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
        }

        // Spawn 3D marker
        if (location != null)
        {
            SpawnMarker(location.GetWorldPosition());
        }
    }

    /// <summary>
    /// Spawn 3D marker at destination
    /// </summary>
    private void SpawnMarker(Vector3 position)
    {
        // Remove existing marker
        if (currentMarker != null)
        {
            Destroy(currentMarker);
        }

        // Spawn new marker
        if (markerPrefab != null)
        {
            currentMarker = Instantiate(markerPrefab, position, Quaternion.identity);
            
            // Animate marker (scale up)
            StartCoroutine(AnimateMarker(currentMarker));
        }
        else
        {
            // Create default marker if no prefab
            currentMarker = CreateDefaultMarker(position);
        }
    }

    /// <summary>
    /// Create default marker (cylinder)
    /// </summary>
    private GameObject CreateDefaultMarker(Vector3 position)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.transform.position = position + Vector3.up * 0.5f;
        marker.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        
        // Add material
        Renderer renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = Color.green;
        }

        // Add animation
        StartCoroutine(AnimateMarker(marker));
        
        return marker;
    }

    /// <summary>
    /// Animate marker (scale up animation)
    /// </summary>
    private System.Collections.IEnumerator AnimateMarker(GameObject marker)
    {
        if (marker == null)
            yield break;

        float elapsed = 0f;
        Vector3 initialScale = Vector3.zero;
        Vector3 targetScale = marker.transform.localScale;

        while (elapsed < markerAnimationDuration && marker != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / markerAnimationDuration;
            
            // Ease out bounce
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            
            marker.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            
            yield return null;
        }

        if (marker != null)
        {
            marker.transform.localScale = targetScale;
        }
    }

    /// <summary>
    /// Hide arrival notification
    /// </summary>
    public void Hide()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }

        // Remove marker after delay
        if (currentMarker != null)
        {
            Destroy(currentMarker, 1f);
            currentMarker = null;
        }
    }

    /// <summary>
    /// Handle dismiss button click
    /// </summary>
    private void OnDismissClicked()
    {
        Hide();
        onDismissed?.Invoke();
    }

    /// <summary>
    /// Check if arrival screen is visible
    /// </summary>
    public bool IsVisible()
    {
        return notificationPanel != null && notificationPanel.activeSelf;
    }

    /// <summary>
    /// Get current marker
    /// </summary>
    public GameObject GetCurrentMarker()
    {
        return currentMarker;
    }
}
