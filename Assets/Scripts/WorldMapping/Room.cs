using UnityEngine;
using System;

/// <summary>
/// Room represents a single room or area within a map segment.
/// Rooms can be used for player location tracking and displaying room information.
///
/// Corresponds to Unreal Engine's BP_Room
/// </summary>
public class Room : MonoBehaviour
{
    [Header("Room Properties")]
    [Tooltip("Name/title of the room")]
    public string roomTitle = "No Title";

    [Tooltip("Floor text (e.g., 'First Floor')")]
    public string floorText = "First Floor";

    [Tooltip("Type of room")]
    public RoomType roomType = RoomType.Room;

    [Header("Size Configuration")]
    [Tooltip("Room size (set to zero for automatic detection)")]
    public Vector2 size = Vector2.zero;

    [Tooltip("Room height in meters")]
    public float height = 3.0f;

    [Header("Debug")]
    [Tooltip("Material for debug visualization")]
    public Material debugMaterial;

    [Tooltip("Show debug mesh in editor")]
    public bool showDebug = false;

    [Header("Map Segment Reference")]
    [Tooltip("Parent map segment transform")]
    public Transform mapSegment;

    // Events
    public Action<Room> OnPlayerEntered;
    public Action<Room> OnPlayerExited;

    // Components
    private BoxCollider roomCollider;
    private Renderer roomRenderer;

    // State
    private bool isPlayerInside = false;
    private Vector3 calculatedSize;

    private void Awake()
    {
        // Add or get box collider
        roomCollider = GetComponent<BoxCollider>();
        if (roomCollider == null)
        {
            roomCollider = gameObject.AddComponent<BoxCollider>();
        }
        roomCollider.isTrigger = true;

        // Add or get renderer
        roomRenderer = GetComponent<Renderer>();
        if (roomRenderer == null && showDebug)
        {
            // Create a cube for debug visualization
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localRotation = Quaternion.identity;
            
            roomRenderer = cube.GetComponent<Renderer>();
            Destroy(cube); // Just checking
        }
    }

    private void Start()
    {
        // Apply size configuration
        ConfigureRoom();
    }

    /// <summary>
    /// Configure the room based on settings
    /// </summary>
    private void ConfigureRoom()
    {
        if (size != Vector2.zero)
        {
            // Use manually set size
            ApplySize(size);
        }
        else
        {
            // Calculate size automatically
            CalculateSize();
        }
    }

    /// <summary>
    /// Apply specific size to the room
    /// </summary>
    public void ApplySize(Vector2 newSize)
    {
        size = newSize;
        calculatedSize = new Vector3(size.x, height, size.y);

        if (roomCollider != null)
        {
            roomCollider.size = calculatedSize;
        }

        // Update debug mesh if present
        if (roomRenderer != null && roomRenderer.transform.childCount > 0)
        {
            Transform debugMesh = roomRenderer.transform.GetChild(0);
            debugMesh.localScale = calculatedSize;
        }
    }

    /// <summary>
    /// Calculate room size automatically based on surroundings
    /// This would typically use raycasting or mesh analysis
    /// </summary>
    private void CalculateSize()
    {
        // Simple automatic detection - use a default size
        // In a real implementation, this would analyze walls or use AR plane data
        float defaultSize = 5.0f; // 5 meters
        ApplySize(new Vector2(defaultSize, defaultSize));
    }

    /// <summary>
    /// Set the room title
    /// </summary>
    public void SetTitle(string title)
    {
        roomTitle = title;
    }

    /// <summary>
    /// Set the floor text
    /// </summary>
    public void SetFloorText(string floor)
    {
        floorText = floor;
    }

    /// <summary>
    /// Set the room type
    /// </summary>
    public void SetRoomType(RoomType type)
    {
        roomType = type;
    }

    /// <summary>
    /// Check if a position is inside this room
    /// </summary>
    public bool ContainsPosition(Vector3 position)
    {
        if (roomCollider == null)
            return false;

        return roomCollider.bounds.Contains(position);
    }

    /// <summary>
    /// Get distance from a position to this room
    /// </summary>
    public float GetDistance(Vector3 position)
    {
        return Vector3.Distance(transform.position, position);
    }

    /// <summary>
    /// Get the room center position
    /// </summary>
    public Vector3 GetCenter()
    {
        return transform.position;
    }

    /// <summary>
    /// Get room dimensions
    /// </summary>
    public Vector3 GetDimensions()
    {
        if (roomCollider != null)
        {
            return roomCollider.size;
        }
        return calculatedSize;
    }

    /// <summary>
    /// Show debug visualization
    /// </summary>
    public void ShowDebugVisuals()
    {
        showDebug = true;
        
        if (debugMaterial != null && roomRenderer != null)
        {
            // Enable the debug mesh
            // This would typically add or enable a debug visual
        }
    }

    /// <summary>
    /// Hide debug visualization
    /// </summary>
    public void HideDebugVisuals()
    {
        showDebug = false;
    }

    /// <summary>
    /// Toggle debug visualization
    /// </summary>
    public void ToggleDebug()
    {
        showDebug = !showDebug;
    }

    /// <summary>
    /// Get room type as string
    /// </summary>
    public string GetRoomTypeString()
    {
        return roomType.ToString();
    }

    /// <summary>
    /// Get full room info
    /// </summary>
    public string GetRoomInfo()
    {
        return $"{roomTitle} ({floorText})";
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered (player should have tag "Player")
        if (other.CompareTag("Player"))
        {
            if (!isPlayerInside)
            {
                isPlayerInside = true;
                OnPlayerEntered?.Invoke(this);
                Debug.Log($"Room: Player entered {roomTitle}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if player exited
        if (other.CompareTag("Player"))
        {
            if (isPlayerInside)
            {
                isPlayerInside = false;
                OnPlayerExited?.Invoke(this);
                Debug.Log($"Room: Player exited {roomTitle}");
            }
        }
    }

    private void OnValidate()
    {
        // Ensure height is positive
        height = Mathf.Max(0.1f, height);

        // Ensure size is non-negative
        if (size.x < 0) size.x = 0;
        if (size.y < 0) size.y = 0;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showDebug)
        {
            // Draw room bounds
            Gizmos.color = GetRoomTypeColor();
            
            Vector3 roomSize = (size != Vector2.zero) 
                ? new Vector3(size.x, height, size.y) 
                : new Vector3(5f, height, 5f);
            
            Gizmos.DrawWireCube(transform.position + Vector3.up * height / 2, roomSize);
            
            // Draw room label
            UnityEditor.Handles.Label(transform.position + Vector3.up * height + Vector3.up * 0.3f, roomTitle);
        }
    }

    private Color GetRoomTypeColor()
    {
        switch (roomType)
        {
            case RoomType.Room:
                return Color.blue;
            case RoomType.Corridor:
                return Color.gray;
            case RoomType.Staircase:
                return Color.green;
            case RoomType.Other:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
#endif
}

/// <summary>
/// Room type enumeration
/// </summary>
public enum RoomType
{
    Room,        // Regular room
    Corridor,    // Corridor/hallway
    Staircase,   // Stairs or elevator
    Other        // Other area type
}
