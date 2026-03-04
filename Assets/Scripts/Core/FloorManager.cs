using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// FloorManager manages floor visibility and transitions in multi-floor AR navigation.
/// It controls which floor models are visible and updates the minimap camera.
///
/// Corresponds to build plan Phase 1 - Floor Management
/// </summary>
public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }

    [Header("Floor Configuration")]
    [Tooltip("List of floor GameObjects in the building")]
    public List<FloorInfo> floors = new List<FloorInfo>();

    [Header("Settings")]
    [Tooltip("Current active floor")]
    public int currentFloor = 0;

    [Tooltip("Smooth transition speed")]
    public float transitionSpeed = 2f;

    [Tooltip("Height per floor (meters)")]
    public float floorHeight = 4f;

    [Header("Debug")]
    [Tooltip("Show debug info")]
    public bool showDebugInfo = false;

    // Events
    public Action<int> onFloorChanged;
    public Action<int, int> onFloorTransitionStart; // from, to
    public Action onFloorTransitionComplete;

    // State
    private bool isTransitioning = false;
    private int targetFloor = 0;
    private float transitionProgress = 0f;

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
        // Initialize floors
        InitializeFloors();
        
        // Set initial floor
        SetFloor(currentFloor, false);
    }

    private void Update()
    {
        // Handle floor transition animation
        if (isTransitioning)
        {
            UpdateFloorTransition();
        }
    }

    /// <summary>
    /// Initialize floor information
    /// </summary>
    private void InitializeFloors()
    {
        // If no floors configured, try to find them in the scene
        if (floors.Count == 0)
        {
            AutoDiscoverFloors();
        }

        // Sort floors by level
        floors = floors.OrderBy(f => f.floorLevel).ToList();

        if (showDebugInfo)
        {
            Debug.Log($"FloorManager: Initialized {floors.Count} floors");
            foreach (var floor in floors)
            {
                Debug.Log($"  Floor {floor.floorLevel}: {floor.floorName}");
            }
        }
    }

    /// <summary>
    /// Auto-discover floor GameObjects in the scene
    /// </summary>
    private void AutoDiscoverFloors()
    {
        // Look for GameObjects with "Floor" in the name
        // Since FloorInfo is not a MonoBehaviour, we find GameObjects and create FloorInfo from them
        GameObject[] foundGameObjects = FindObjectsOfType<GameObject>();
        
        List<FloorInfo> discoveredFloors = new List<FloorInfo>();
        
        foreach (var go in foundGameObjects)
        {
            if (go.name.Contains("Floor_") || go.name.Contains("Floor"))
            {
                FloorInfo floorInfo = new FloorInfo
                {
                    floorLevel = ExtractFloorLevel(go.name),
                    floorName = ExtractFloorLevel(go.name) == 0 ? "Ground" : $"Floor {ExtractFloorLevel(go.name)}",
                    floorObject = go,
                    isVisible = true
                };
                discoveredFloors.Add(floorInfo);
            }
        }

        // Add discovered floors that aren't already in the list
        foreach (var floor in discoveredFloors)
        {
            bool alreadyExists = floors.Any(f => f.floorLevel == floor.floorLevel);
            if (!alreadyExists)
            {
                floors.Add(floor);
            }
        }

        // Also look for child objects with Floor_XX naming under Building Root
        Transform buildingRoot = transform.Find("Building Root");
        if (buildingRoot != null)
        {
            foreach (Transform child in buildingRoot)
            {
                if (child.name.Contains("Floor_") || child.name.Contains("Floor"))
                {
                    // Create a new FloorInfo instance (not as a component)
                    FloorInfo floorInfo = new FloorInfo
                    {
                        floorLevel = ExtractFloorLevel(child.name),
                        floorName = ExtractFloorLevel(child.name) == 0 ? "Ground" : $"Floor {ExtractFloorLevel(child.name)}",
                        floorObject = child.gameObject,
                        isVisible = true
                    };
                    
                    bool alreadyExists = floors.Any(f => f.floorLevel == floorInfo.floorLevel);
                    if (!alreadyExists)
                    {
                        floors.Add(floorInfo);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Extract floor level from GameObject name
    /// </summary>
    private int ExtractFloorLevel(string name)
    {
        // Look for patterns like "Floor_00", "Floor_0", "Floor_1", etc.
        string[] parts = name.Split('_');
        if (parts.Length >= 2)
        {
            if (int.TryParse(parts[parts.Length - 1], out int level))
            {
                return level;
            }
        }
        return 0;
    }

    /// <summary>
    /// Set the active floor
    /// </summary>
    public void SetFloor(int floor, bool animate = true)
    {
        if (floor < 0 || floor >= floors.Count)
        {
            Debug.LogWarning($"FloorManager: Invalid floor {floor}");
            return;
        }

        if (floor == currentFloor && !animate)
            return;

        if (showDebugInfo)
            Debug.Log($"FloorManager: Setting floor to {floor} (animate: {animate})");

        if (animate && currentFloor != floor)
        {
            // Start transition
            onFloorTransitionStart?.Invoke(currentFloor, floor);
            targetFloor = floor;
            isTransitioning = true;
            transitionProgress = 0f;
        }
        else
        {
            // Immediate change
            ApplyFloorChange(floor);
        }
    }

    /// <summary>
    /// Update floor transition animation
    /// </summary>
    private void UpdateFloorTransition()
    {
        transitionProgress += Time.deltaTime * transitionSpeed;

        if (transitionProgress >= 1f)
        {
            // Transition complete
            ApplyFloorChange(targetFloor);
            isTransitioning = false;
            transitionProgress = 0f;
            onFloorTransitionComplete?.Invoke();
        }
    }

    /// <summary>
    /// Apply the floor change immediately
    /// </summary>
    private void ApplyFloorChange(int floor)
    {
        // Hide all floors
        foreach (var floorInfo in floors)
        {
            if (floorInfo.floorObject != null)
            {
                floorInfo.floorObject.SetActive(floorInfo.floorLevel == floor);
            }
        }

        currentFloor = floor;
        
        if (showDebugInfo)
            Debug.Log($"FloorManager: Switched to floor {floor}");

        onFloorChanged?.Invoke(floor);
    }

    /// <summary>
    /// Go to the next floor (up)
    /// </summary>
    public void GoUp()
    {
        if (currentFloor < floors.Count - 1)
        {
            SetFloor(currentFloor + 1);
        }
    }

    /// <summary>
    /// Go to the previous floor (down)
    /// </summary>
    public void GoDown()
    {
        if (currentFloor > 0)
        {
            SetFloor(currentFloor - 1);
        }
    }

    /// <summary>
    /// Get the current floor
    /// </summary>
    public int GetCurrentFloor()
    {
        return currentFloor;
    }

    /// <summary>
    /// Get total number of floors
    /// </summary>
    public int GetFloorCount()
    {
        return floors.Count;
    }

    /// <summary>
    /// Get floor info by level
    /// </summary>
    public FloorInfo GetFloorInfo(int floor)
    {
        return floors.FirstOrDefault(f => f.floorLevel == floor);
    }

    /// <summary>
    /// Get the Y position for a floor
    /// </summary>
    public float GetFloorYPosition(int floor)
    {
        return floor * floorHeight;
    }

    /// <summary>
    /// Calculate floor from Y position
    /// </summary>
    public int GetFloorFromY(float y)
    {
        return Mathf.RoundToInt(y / floorHeight);
    }

    /// <summary>
    /// Check if a floor exists
    /// </summary>
    public bool HasFloor(int floor)
    {
        return floors.Any(f => f.floorLevel == floor);
    }

    /// <summary>
    /// Add a floor
    /// </summary>
    public void AddFloor(int level, string name, GameObject floorObject)
    {
        // Check if floor already exists
        FloorInfo existing = floors.FirstOrDefault(f => f.floorLevel == level);
        if (existing != null)
        {
            existing.floorObject = floorObject;
            existing.floorName = name;
            return;
        }

        FloorInfo newFloor = new FloorInfo
        {
            floorLevel = level,
            floorName = name,
            floorObject = floorObject
        };
        
        floors.Add(newFloor);
        floors = floors.OrderBy(f => f.floorLevel).ToList();
    }

    /// <summary>
    /// Remove a floor
    /// </summary>
    public void RemoveFloor(int level)
    {
        floors.RemoveAll(f => f.floorLevel == level);
    }

    /// <summary>
    /// Get all floor names
    /// </summary>
    public List<string> GetFloorNames()
    {
        return floors.Select(f => f.floorName).ToList();
    }

    /// <summary>
    /// Check if currently transitioning
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
}

/// <summary>
/// Floor information container
/// </summary>
[System.Serializable]
public class FloorInfo
{
    public int floorLevel = 0;
    public string floorName = "";
    public GameObject floorObject;
    public bool isVisible = true;

    public FloorInfo()
    {
    }

    public FloorInfo(int level, string name, GameObject obj)
    {
        floorLevel = level;
        floorName = name;
        floorObject = obj;
    }
}
