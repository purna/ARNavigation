using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// SettingsManager handles all user preferences and settings persistence.
/// It manages units, colors, themes, and other configuration options.
///
/// Corresponds to build plan Phase 6 - Settings Panel
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Default Settings")]
    [Tooltip("Default unit system")]
    public DistanceCalculator.UnitSystem defaultUnit = DistanceCalculator.UnitSystem.Metric;

    [Tooltip("Default theme")]
    public ThemeType defaultTheme = ThemeType.Default;

    [Header("PlayerPrefs Keys")]
    private const string KEY_UNIT_SYSTEM = "Settings_UnitSystem";
    private const string KEY_THEME = "Settings_Theme";
    private const string KEY_PATH_COLOR = "Settings_PathColor";
    private const string KEY_ARROW_COLOR = "Settings_ArrowColor";
    private const string KEY_FIRST_LAUNCH = "Settings_FirstLaunch";

    // Current settings
    public DistanceCalculator.UnitSystem CurrentUnit { get; private set; }
    public ThemeType CurrentTheme { get; private set; }
    public Color PathColor { get; private set; }
    public Color ArrowColor { get; private set; }
    public bool IsFirstLaunch { get; private set; }

    // Events
    public Action<DistanceCalculator.UnitSystem> onUnitChanged;
    public Action<ThemeType> onThemeChanged;
    public Action<Color> onPathColorChanged;
    public Action<Color> onArrowColorChanged;

    // Theme colors
    public enum ThemeType
    {
        Default,
        HighContrast,
        Dark
    }

    private Dictionary<ThemeType, ThemeProfile> themeProfiles;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize theme profiles
        InitializeThemeProfiles();
    }

    private void Start()
    {
        LoadSettings();
    }

    /// <summary>
    /// Initialize theme color profiles
    /// </summary>
    private void InitializeThemeProfiles()
    {
        themeProfiles = new Dictionary<ThemeType, ThemeProfile>();

        // Default theme
        themeProfiles[ThemeType.Default] = new ThemeProfile
        {
            name = "Default",
            primaryColor = new Color(0f, 0.48f, 1f),
            secondaryColor = new Color(0.5f, 0.5f, 0.5f),
            backgroundColor = new Color(1f, 1f, 1f, 0.9f),
            textColor = Color.black,
            pathColor = Color.blue,
            arrowColor = new Color(1f, 0.48f, 0f)
        };

        // High contrast theme
        themeProfiles[ThemeType.HighContrast] = new ThemeProfile
        {
            name = "High Contrast",
            primaryColor = Color.yellow,
            secondaryColor = Color.white,
            backgroundColor = Color.black,
            textColor = Color.white,
            pathColor = Color.yellow,
            arrowColor = Color.cyan
        };

        // Dark theme
        themeProfiles[ThemeType.Dark] = new ThemeProfile
        {
            name = "Dark",
            primaryColor = new Color(0.2f, 0.6f, 1f),
            secondaryColor = new Color(0.3f, 0.3f, 0.3f),
            backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f),
            textColor = Color.white,
            pathColor = new Color(0f, 0.7f, 1f),
            arrowColor = new Color(1f, 0.6f, 0f)
        };
    }

    /// <summary>
    /// Load settings from PlayerPrefs
    /// </summary>
    public void LoadSettings()
    {
        // Load unit system
        int unitValue = PlayerPrefs.GetInt(KEY_UNIT_SYSTEM, (int)defaultUnit);
        CurrentUnit = (DistanceCalculator.UnitSystem)unitValue;
        DistanceCalculator.SetUnitSystem(CurrentUnit);

        // Load theme
        int themeValue = PlayerPrefs.GetInt(KEY_THEME, (int)defaultTheme);
        CurrentTheme = (ThemeType)themeValue;

        // Load colors
        string pathColorHex = PlayerPrefs.GetString(KEY_PATH_COLOR, "");
        if (!string.IsNullOrEmpty(pathColorHex))
        {
            if (ColorUtility.TryParseHtmlString(pathColorHex, out Color color))
            {
                PathColor = color;
            }
            else
            {
                PathColor = themeProfiles[CurrentTheme].pathColor;
            }
        }
        else
        {
            PathColor = themeProfiles[CurrentTheme].pathColor;
        }

        string arrowColorHex = PlayerPrefs.GetString(KEY_ARROW_COLOR, "");
        if (!string.IsNullOrEmpty(arrowColorHex))
        {
            if (ColorUtility.TryParseHtmlString(arrowColorHex, out Color color))
            {
                ArrowColor = color;
            }
            else
            {
                ArrowColor = themeProfiles[CurrentTheme].arrowColor;
            }
        }
        else
        {
            ArrowColor = themeProfiles[CurrentTheme].arrowColor;
        }

        // Check first launch
        IsFirstLaunch = PlayerPrefs.GetInt(KEY_FIRST_LAUNCH, 1) == 1;

        Debug.Log($"SettingsManager: Loaded settings - Unit: {CurrentUnit}, Theme: {CurrentTheme}");
    }

    /// <summary>
    /// Save settings to PlayerPrefs
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetInt(KEY_UNIT_SYSTEM, (int)CurrentUnit);
        PlayerPrefs.SetInt(KEY_THEME, (int)CurrentTheme);
        PlayerPrefs.SetString(KEY_PATH_COLOR, "#" + ColorUtility.ToHtmlStringRGB(PathColor));
        PlayerPrefs.SetString(KEY_ARROW_COLOR, "#" + ColorUtility.ToHtmlStringRGB(ArrowColor));
        PlayerPrefs.SetInt(KEY_FIRST_LAUNCH, IsFirstLaunch ? 0 : 1);
        PlayerPrefs.Save();

        Debug.Log("SettingsManager: Settings saved");
    }

    /// <summary>
    /// Set unit system
    /// </summary>
    public void SetUnitSystem(DistanceCalculator.UnitSystem unit)
    {
        if (CurrentUnit != unit)
        {
            CurrentUnit = unit;
            DistanceCalculator.SetUnitSystem(unit);
            SaveSettings();
            onUnitChanged?.Invoke(unit);
        }
    }

    /// <summary>
    /// Toggle unit system
    /// </summary>
    public void ToggleUnitSystem()
    {
        DistanceCalculator.UnitSystem newUnit = CurrentUnit == DistanceCalculator.UnitSystem.Metric
            ? DistanceCalculator.UnitSystem.Imperial
            : DistanceCalculator.UnitSystem.Metric;
        
        SetUnitSystem(newUnit);
    }

    /// <summary>
    /// Set theme
    /// </summary>
    public void SetTheme(ThemeType theme)
    {
        if (CurrentTheme != theme)
        {
            CurrentTheme = theme;
            
            // Apply theme colors
            ThemeProfile profile = themeProfiles[theme];
            PathColor = profile.pathColor;
            ArrowColor = profile.arrowColor;
            
            SaveSettings();
            onThemeChanged?.Invoke(theme);
        }
    }

    /// <summary>
    /// Set path color
    /// </summary>
    public void SetPathColor(Color color)
    {
        PathColor = color;
        SaveSettings();
        onPathColorChanged?.Invoke(color);
    }

    /// <summary>
    /// Set arrow color
    /// </summary>
    public void SetArrowColor(Color color)
    {
        ArrowColor = color;
        SaveSettings();
        onArrowColorChanged?.Invoke(color);
    }

    /// <summary>
    /// Mark first launch complete
    /// </summary>
    public void CompleteFirstLaunch()
    {
        IsFirstLaunch = false;
        SaveSettings();
    }

    /// <summary>
    /// Get theme profile
    /// </summary>
    public ThemeProfile GetThemeProfile(ThemeType theme)
    {
        if (themeProfiles.TryGetValue(theme, out ThemeProfile profile))
        {
            return profile;
        }
        return themeProfiles[ThemeType.Default];
    }

    /// <summary>
    /// Get current theme profile
    /// </summary>
    public ThemeProfile GetCurrentThemeProfile()
    {
        return GetThemeProfile(CurrentTheme);
    }

    /// <summary>
    /// Reset all settings to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        SetUnitSystem(defaultUnit);
        SetTheme(defaultTheme);
        
        ThemeProfile profile = themeProfiles[defaultTheme];
        PathColor = profile.pathColor;
        ArrowColor = profile.arrowColor;
        
        SaveSettings();
    }

    /// <summary>
    /// Check if a theme is available
    /// </summary>
    public bool HasTheme(ThemeType theme)
    {
        return themeProfiles.ContainsKey(theme);
    }

    /// <summary>
    /// Get all available themes
    /// </summary>
    public List<ThemeType> GetAvailableThemes()
    {
        return new List<ThemeType>(themeProfiles.Keys);
    }
}

/// <summary>
/// Theme color profile
/// </summary>
[System.Serializable]
public class ThemeProfile
{
    public string name = "";
    public Color primaryColor;
    public Color secondaryColor;
    public Color backgroundColor;
    public Color textColor;
    public Color pathColor;
    public Color arrowColor;
}
