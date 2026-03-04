using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// CompassCalibrationScreen displays the initial compass calibration message
/// when the app first starts. It shows "Please hold the device still to calibrate
/// the compass" with an OK button.
///
/// This is the first screen shown when the app launches.
/// </summary>
public class CompassCalibrationScreen : MonoBehaviour
{
    public static CompassCalibrationScreen Instance { get; private set; }

    [Header("UI Components")]
    [Tooltip("Main panel")]
    public GameObject panel;

    [Tooltip("Message text")]
    public Text messageText;

    [Tooltip("OK button")]
    public Button okButton;

    [Tooltip("Animation panel")]
    public RectTransform calibrationAnimation;

    [Header("Settings")]
    [Tooltip("Message to display")]
    public string calibrationMessage = "Please hold the device still to calibrate the compass";

    [Tooltip("Button text")]
    public string buttonText = "OK";

    // Events
    public Action onCalibrationComplete;

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

        // Subscribe to AR session events
        ARSessionManager arSession = FindObjectOfType<ARSessionManager>();
        if (arSession != null)
        {
            arSession.onARSessionStarted += OnARSessionStarted;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        ARSessionManager arSession = FindObjectOfType<ARSessionManager>();
        if (arSession != null)
        {
            arSession.onARSessionStarted -= OnARSessionStarted;
        }
    }

    /// <summary>
    /// Set up UI text and button
    /// </summary>
    private void SetupUI()
    {
        if (messageText != null)
        {
            messageText.text = calibrationMessage;
        }

        if (okButton != null)
        {
            okButton.onClick.AddListener(OnOkClicked);
            
            Text buttonTextComponent = okButton.GetComponentInChildren<Text>();
            if (buttonTextComponent != null)
            {
                buttonTextComponent.text = buttonText;
            }
        }
    }

    /// <summary>
    /// Handle AR session started
    /// </summary>
    private void OnARSessionStarted()
    {
        Show();
    }

    /// <summary>
    /// Handle OK button click
    /// </summary>
    private void OnOkClicked()
    {
        Hide();
        onCalibrationComplete?.Invoke();
    }

    /// <summary>
    /// Show the calibration screen
    /// </summary>
    public void Show()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        // Start calibration animation
        if (calibrationAnimation != null)
        {
            StartCoroutine(CalibrationAnimation());
        }
    }

    /// <summary>
    /// Hide the calibration screen
    /// </summary>
    public void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// Calibration animation coroutine
    /// </summary>
    private System.Collections.IEnumerator CalibrationAnimation()
    {
        float duration = 3f;
        float elapsed = 0f;
        
        while (elapsed < duration && panel != null && panel.activeSelf)
        {
            elapsed += Time.deltaTime;
            
            // Rotate animation
            if (calibrationAnimation != null)
            {
                calibrationAnimation.localRotation = Quaternion.Euler(0, 0, -elapsed * 30f);
            }
            
            yield return null;
        }
    }

    /// <summary>
    /// Check if calibration screen is visible
    /// </summary>
    public bool IsVisible()
    {
        return panel != null && panel.activeSelf;
    }
}
