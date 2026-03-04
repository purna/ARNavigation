using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

/// <summary>
/// LocationSyncManager synchronizes locations between the Unity scene and the locations.json file.
/// It can export scene positions to JSON and import JSON positions to scene objects.
///
/// This is an Editor-only utility for positioning locations in the scene
/// and saving them to the data file.
///
/// Corresponds to build plan Phase 5 - Data Layer sync capability
/// </summary>
public class LocationSyncManager : MonoBehaviour
{
    public static LocationSyncManager Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("Path to locations.json in StreamingAssets")]
    public string dataFilePath = "locations.json";

    [Header("Map Segment Reference")]
    [Tooltip("Parent object containing all MapSegments")]
    public Transform mapSegmentsParent;

    [Header("Settings")]
    [Tooltip("Default floor height in meters")]
    public float defaultFloorHeight = 4f;

    [Header("Debug")]
    [Tooltip("Show debug info")]
    public bool showDebugInfo = false;

    private List<MapSegment> mapSegments = new List<MapSegment>();
    private LocationData locationData;

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
        DiscoverMapSegments();
    }

    /// <summary>
    /// Discover all MapSegments in the scene
    /// </summary>
    public void DiscoverMapSegments()
    {
        mapSegments.Clear();

        if (mapSegmentsParent != null)
        {
            mapSegments.AddRange(mapSegmentsParent.GetComponentsInChildren<MapSegment>());
        }
        else
        {
            // Find all MapSegments in scene
            mapSegments.AddRange(FindObjectsOfType<MapSegment>());
        }

        if (showDebugInfo)
            Debug.Log($"LocationSyncManager: Found {mapSegments.Count} MapSegments");
    }

    /// <summary>
    /// Export all scene positions to JSON (call from editor menu)
    /// </summary>
    public void ExportToJson()
    {
        StartCoroutine(ExportToJsonCoroutine());
    }

    private IEnumerator ExportToJsonCoroutine()
    {
        // Discover segments if needed
        if (mapSegments.Count == 0)
        {
            DiscoverMapSegments();
        }

        // Create location data
        locationData = new LocationData
        {
            version = "1.0",
            building = "Main Building",
            locations = new List<LocationEntry>(),
            qrAnchors = new List<QrAnchor>()
        };

        // Process each map segment
        foreach (var segment in mapSegments)
        {
            if (segment == null)
                continue;

            int floor = segment.floor;
            string floorPrefix = floor == 0 ? "F0" : $"F{floor}";

            // Export navigation vertices as locations
            if (segment.vertices != null)
            {
                foreach (var vertex in segment.vertices)
                {
                    if (vertex == null)
                        continue;

                    LocationEntry entry = new LocationEntry
                    {
                        id = !string.IsNullOrEmpty(vertex.vertexId) 
                            ? vertex.vertexId 
                            : $"VERTEX_{floorPrefix}_{locationData.locations.Count}",
                        title = !string.IsNullOrEmpty(vertex.displayName) 
                            ? vertex.displayName 
                            : $"Vertex {locationData.locations.Count}",
                        description = $"Navigation vertex on floor {floor}",
                        floor = floor,
                        worldPosition = new WorldPosition
                        {
                            x = vertex.transform.position.x,
                            y = vertex.transform.position.y,
                            z = vertex.transform.position.z
                        },
                        category = "navigation",
                        tags = new List<string> { "vertex", "navigation" }
                    };

                    locationData.locations.Add(entry);
                }
            }

            // Export floating icons as locations
            if (segment.floatingIcons != null)
            {
                foreach (var icon in segment.floatingIcons)
                {
                    if (icon == null)
                        continue;

                    LocationEntry entry = new LocationEntry
                    {
                        id = !string.IsNullOrEmpty(icon.poiName) 
                            ? icon.poiName 
                            : $"ICON_{floorPrefix}_{locationData.locations.Count}",
                        title = !string.IsNullOrEmpty(icon.iconText) 
                            ? icon.iconText 
                            : $"POI {locationData.locations.Count}",
                        description = "Point of Interest",
                        floor = floor,
                        worldPosition = new WorldPosition
                        {
                            x = icon.transform.position.x,
                            y = icon.transform.position.y,
                            z = icon.transform.position.z
                        },
                        category = "poi",
                        tags = new List<string> { "poi", "icon" }
                    };

                    locationData.locations.Add(entry);
                }
            }

            // Export rooms as locations
            if (segment.rooms != null)
            {
                foreach (var room in segment.rooms)
                {
                    if (room == null)
                        continue;

                    LocationEntry entry = new LocationEntry
                    {
                        id = !string.IsNullOrEmpty(room.roomTitle) 
                            ? room.roomTitle 
                            : $"ROOM_{floorPrefix}_{locationData.locations.Count}",
                        title = !string.IsNullOrEmpty(room.roomTitle) 
                            ? room.roomTitle 
                            : $"Room {locationData.locations.Count}",
                        description = room.floorText ?? "Room",
                        floor = floor,
                        worldPosition = new WorldPosition
                        {
                            x = room.transform.position.x,
                            y = room.transform.position.y,
                            z = room.transform.position.z
                        },
                        category = "rooms",
                        tags = new List<string> { "room" }
                    };

                    locationData.locations.Add(entry);
                }
            }
        }

        // Generate QR anchors from location IDs (simple approach)
        // In a real scenario, you'd place QR code objects in the scene
        foreach (var location in locationData.locations)
        {
            if (location.category == "entrance" || location.category == "circulation")
            {
                QrAnchor anchor = new QrAnchor
                {
                    qrCode = location.id,
                    worldPosition = location.worldPosition,
                    worldRotationY = 0f,
                    floor = location.floor
                };
                locationData.qrAnchors.Add(anchor);
            }
        }

        // Save to file
        string json = JsonUtility.ToJson(locationData, true);
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, dataFilePath);

        #if UNITY_EDITOR
        System.IO.File.WriteAllText(filePath, json);
        Debug.Log($"LocationSyncManager: Exported {locationData.locations.Count} locations to {filePath}");
        #else
        Debug.LogWarning("LocationSyncManager: Export only works in Unity Editor");
        #endif

        yield return null;
    }

    /// <summary>
    /// Import positions from JSON to scene objects
    /// </summary>
    public void ImportFromJson()
    {
        StartCoroutine(ImportFromJsonCoroutine());
    }

    private IEnumerator ImportFromJsonCoroutine()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, dataFilePath);

        string jsonContent = "";

        #if UNITY_ANDROID && !UNITY_EDITOR
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                jsonContent = request.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"LocationSyncManager: Failed to load file - {request.error}");
                yield break;
            }
        }
        #else
        if (System.IO.File.Exists(filePath))
        {
            jsonContent = System.IO.File.ReadAllText(filePath);
        }
        else
        {
            Debug.LogError($"LocationSyncManager: File not found at {filePath}");
            yield break;
        }
        #endif

        // Parse JSON
        locationData = JsonUtility.FromJson<LocationData>(jsonContent);

        if (showDebugInfo)
            Debug.Log($"LocationSyncManager: Loaded {locationData.locations.Count} locations from JSON");

        // Discover segments if needed
        if (mapSegments.Count == 0)
        {
            DiscoverMapSegments();
        }

        // Apply positions to scene objects
        ApplyPositionsToScene();

        Debug.Log($"LocationSyncManager: Imported positions from JSON");
    }

    /// <summary>
    /// Apply JSON positions to scene objects
    /// </summary>
    private void ApplyPositionsToScene()
    {
        if (locationData == null || mapSegments.Count == 0)
            return;

        // Create lookup for locations by ID
        var locationLookup = locationData.locations.ToDictionary(l => l.id.ToUpperInvariant());

        // Apply to each segment
        foreach (var segment in mapSegments)
        {
            if (segment == null)
                continue;

            int floor = segment.floor;

            // Apply to vertices
            if (segment.vertices != null)
            {
                foreach (var vertex in segment.vertices)
                {
                    if (vertex == null || string.IsNullOrEmpty(vertex.vertexId))
                        continue;

                    string lookupKey = vertex.vertexId.ToUpperInvariant();
                    if (locationLookup.TryGetValue(lookupKey, out LocationEntry entry))
                    {
                        vertex.transform.position = entry.GetWorldPosition();
                        vertex.floor = entry.floor;

                        if (showDebugInfo)
                            Debug.Log($"LocationSyncManager: Positioned vertex {vertex.vertexId} at {entry.GetWorldPosition()}");
                    }
                }
            }

            // Apply to floating icons
            if (segment.floatingIcons != null)
            {
                foreach (var icon in segment.floatingIcons)
                {
                    if (icon == null || string.IsNullOrEmpty(icon.poiName))
                        continue;

                    string lookupKey = icon.poiName.ToUpperInvariant();
                    if (locationLookup.TryGetValue(lookupKey, out LocationEntry entry))
                    {
                        icon.transform.position = entry.GetWorldPosition();

                        if (showDebugInfo)
                            Debug.Log($"LocationSyncManager: Positioned icon {icon.poiName} at {entry.GetWorldPosition()}");
                    }
                }
            }

            // Apply to rooms
            if (segment.rooms != null)
            {
                foreach (var room in segment.rooms)
                {
                    if (room == null || string.IsNullOrEmpty(room.roomTitle))
                        continue;

                    string lookupKey = room.roomTitle.ToUpperInvariant();
                    if (locationLookup.TryGetValue(lookupKey, out LocationEntry entry))
                    {
                        room.transform.position = entry.GetWorldPosition();

                        if (showDebugInfo)
                            Debug.Log($"LocationSyncManager: Positioned room {room.roomTitle} at {entry.GetWorldPosition()}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get all map segments
    /// </summary>
    public List<MapSegment> GetMapSegments()
    {
        return mapSegments;
    }

    /// <summary>
    /// Get location data
    /// </summary>
    public LocationData GetLocationData()
    {
        return locationData;
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor menu for sync operations
/// </summary>
public class LocationSyncMenu
{
    [UnityEditor.MenuItem("AR Navigation/Sync/Export Scene to JSON")]
    public static void ExportToJson()
    {
        LocationSyncManager manager = UnityEngine.Object.FindObjectOfType<LocationSyncManager>();
        if (manager != null)
        {
            manager.ExportToJson();
        }
        else
        {
            UnityEditor.EditorUtility.DisplayDialog("Error", "LocationSyncManager not found in scene", "OK");
        }
    }

    [UnityEditor.MenuItem("AR Navigation/Sync/Import JSON to Scene")]
    public static void ImportFromJson()
    {
        LocationSyncManager manager = UnityEngine.Object.FindObjectOfType<LocationSyncManager>();
        if (manager != null)
        {
            manager.ImportFromJson();
        }
        else
        {
            UnityEditor.EditorUtility.DisplayDialog("Error", "LocationSyncManager not found in scene", "OK");
        }
    }

    [UnityEditor.MenuItem("AR Navigation/Sync/Discover Map Segments")]
    public static void DiscoverMapSegments()
    {
        LocationSyncManager manager = UnityEngine.Object.FindObjectOfType<LocationSyncManager>();
        if (manager != null)
        {
            manager.DiscoverMapSegments();
            UnityEditor.EditorUtility.DisplayDialog("Success", $"Found {manager.GetMapSegments().Count} MapSegments", "OK");
        }
        else
        {
            UnityEditor.EditorUtility.DisplayDialog("Error", "LocationSyncManager not found in scene", "OK");
        }
    }
}
#endif
