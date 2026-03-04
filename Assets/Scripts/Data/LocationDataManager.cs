using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// LocationDataManager loads and manages location data from locations.json.
/// It provides methods to search, filter, and retrieve location information.
///
/// Corresponds to the data layer in the build plan (Phase 5)
/// </summary>
public class LocationDataManager : MonoBehaviour
{
    public static LocationDataManager Instance { get; private set; }

    [Header("Data Source")]
    [Tooltip("Path to locations.json in StreamingAssets")]
    public string dataFilePath = "locations.json";

    [Header("Data")]
    [Tooltip("Loaded location data")]
    public LocationData locationData;

    [Header("Events")]
    public Action onDataLoaded;
    public Action<string> onDataLoadError;

    // Cached search results
    private List<LocationEntry> searchResults = new List<LocationEntry>();
    private Dictionary<string, LocationEntry> locationLookup = new Dictionary<string, LocationEntry>();
    private Dictionary<string, QrAnchor> qrAnchorLookup = new Dictionary<string, QrAnchor>();
    private Dictionary<int, List<LocationEntry>> floorIndex = new Dictionary<int, List<LocationEntry>>();

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
        LoadLocationData();
    }

    /// <summary>
    /// Load location data from JSON file
    /// </summary>
    public void LoadLocationData()
    {
        try
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, dataFilePath);
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Android requires UnityWebRequest
            StartCoroutine(LoadFromAndroid(filePath));
            #else
            // Other platforms can read directly
            if (System.IO.File.Exists(filePath))
            {
                string jsonContent = System.IO.File.ReadAllText(filePath);
                ParseLocationData(jsonContent);
            }
            else
            {
                Debug.LogError($"LocationDataManager: File not found at {filePath}");
                onDataLoadError?.Invoke($"File not found: {dataFilePath}");
            }
            #endif
        }
        catch (Exception e)
        {
            Debug.LogError($"LocationDataManager: Error loading data - {e.Message}");
            onDataLoadError?.Invoke(e.Message);
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private IEnumerator LoadFromAndroid(string filePath)
    {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                ParseLocationData(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"LocationDataManager: Error downloading - {request.error}");
                onDataLoadError?.Invoke(request.error);
            }
        }
    }
#endif

    /// <summary>
    /// Parse JSON data into LocationData object
    /// </summary>
    private void ParseLocationData(string jsonContent)
    {
        try
        {
            // Parse JSON
            locationData = JsonUtility.FromJson<LocationData>(jsonContent);
            
            // Build lookup indexes
            BuildIndexes();
            
            Debug.Log($"LocationDataManager: Loaded {locationData.locations.Count} locations and {locationData.qrAnchors.Count} QR anchors");
            onDataLoaded?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"LocationDataManager: Error parsing JSON - {e.Message}");
            onDataLoadError?.Invoke($"Error parsing data: {e.Message}");
        }
    }

    /// <summary>
    /// Build lookup indexes for fast querying
    /// </summary>
    private void BuildIndexes()
    {
        if (locationData == null)
            return;

        // Clear existing indexes
        locationLookup.Clear();
        qrAnchorLookup.Clear();
        floorIndex.Clear();

        // Index locations by ID
        foreach (var location in locationData.locations)
        {
            if (!string.IsNullOrEmpty(location.id))
            {
                locationLookup[location.id.ToUpperInvariant()] = location;
            }

            // Index by floor
            if (!floorIndex.ContainsKey(location.floor))
            {
                floorIndex[location.floor] = new List<LocationEntry>();
            }
            floorIndex[location.floor].Add(location);
        }

        // Index QR anchors by code
        foreach (var anchor in locationData.qrAnchors)
        {
            if (!string.IsNullOrEmpty(anchor.qrCode))
            {
                qrAnchorLookup[anchor.qrCode.ToUpperInvariant()] = anchor;
            }
        }
    }

    /// <summary>
    /// Search locations by query string (fuzzy search on title and tags)
    /// </summary>
    public List<LocationEntry> SearchLocations(string query)
    {
        if (locationData == null || string.IsNullOrWhiteSpace(query))
        {
            return new List<LocationEntry>();
        }

        string lowerQuery = query.ToLowerInvariant();
        
        // Search in title, description, and tags
        searchResults = locationData.locations
            .Where(l => 
                (!string.IsNullOrEmpty(l.title) && l.title.ToLowerInvariant().Contains(lowerQuery)) ||
                (!string.IsNullOrEmpty(l.description) && l.description.ToLowerInvariant().Contains(lowerQuery)) ||
                (l.tags != null && l.tags.Any(t => t.ToLowerInvariant().Contains(lowerQuery))))
            .OrderByDescending(l => GetRelevanceScore(l, lowerQuery))
            .ToList();

        return searchResults;
    }

    /// <summary>
    /// Calculate relevance score for search results
    /// </summary>
    private int GetRelevanceScore(LocationEntry location, string query)
    {
        int score = 0;
        
        // Exact title match gets highest score
        if (!string.IsNullOrEmpty(location.title) && location.title.ToLowerInvariant().StartsWith(query))
            score += 100;
        
        // Title contains query
        if (!string.IsNullOrEmpty(location.title) && location.title.ToLowerInvariant().Contains(query))
            score += 50;
        
        // Tag match
        if (location.tags != null)
        {
            foreach (var tag in location.tags)
            {
                if (tag.ToLowerInvariant() == query)
                    score += 30;
                else if (tag.ToLowerInvariant().Contains(query))
                    score += 10;
            }
        }
        
        return score;
    }

    /// <summary>
    /// Get location by ID
    /// </summary>
    public LocationEntry GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        if (locationLookup.TryGetValue(id.ToUpperInvariant(), out LocationEntry entry))
        {
            return entry;
        }

        return null;
    }

    /// <summary>
    /// Get all locations on a specific floor
    /// </summary>
    public List<LocationEntry> GetByFloor(int floor)
    {
        if (floorIndex.TryGetValue(floor, out List<LocationEntry> floorLocations))
        {
            return new List<LocationEntry>(floorLocations);
        }

        return new List<LocationEntry>();
    }

    /// <summary>
    /// Get QR anchor by QR code string
    /// </summary>
    public QrAnchor GetQrAnchor(string qrCode)
    {
        if (string.IsNullOrEmpty(qrCode))
            return null;

        if (qrAnchorLookup.TryGetValue(qrCode.ToUpperInvariant(), out QrAnchor anchor))
        {
            return anchor;
        }

        return null;
    }

    /// <summary>
    /// Get all locations by category
    /// </summary>
    public List<LocationEntry> GetByCategory(string category)
    {
        if (locationData == null || string.IsNullOrEmpty(category))
            return new List<LocationEntry>();

        return locationData.locations
            .Where(l => l.category.Equals(category, System.StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Get all available categories
    /// </summary>
    public List<string> GetAllCategories()
    {
        if (locationData == null)
            return new List<string>();

        return locationData.locations
            .Select(l => l.category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    /// <summary>
    /// Get all floors in the building
    /// </summary>
    public List<int> GetAllFloors()
    {
        if (locationData == null)
            return new List<int>();

        return locationData.locations
            .Select(l => l.floor)
            .Distinct()
            .OrderBy(f => f)
            .ToList();
    }

    /// <summary>
    /// Get total number of locations
    /// </summary>
    public int GetLocationCount()
    {
        return locationData?.locations?.Count ?? 0;
    }

    /// <summary>
    /// Get data version
    /// </summary>
    public string GetVersion()
    {
        return locationData?.version ?? "0.0";
    }

    /// <summary>
    /// Get building name
    /// </summary>
    public string GetBuildingName()
    {
        return locationData?.building ?? "Unknown Building";
    }

    /// <summary>
    /// Find nearest location to a position
    /// </summary>
    public LocationEntry FindNearestLocation(Vector3 position)
    {
        if (locationData == null || locationData.locations.Count == 0)
            return null;

        LocationEntry nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var location in locationData.locations)
        {
            float distance = Vector3.Distance(position, location.GetWorldPosition());
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = location;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Find nearest QR anchor to a position
    /// </summary>
    public QrAnchor FindNearestQrAnchor(Vector3 position)
    {
        if (locationData == null || locationData.qrAnchors.Count == 0)
            return null;

        QrAnchor nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var anchor in locationData.qrAnchors)
        {
            float distance = Vector3.Distance(position, anchor.GetWorldPosition());
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = anchor;
            }
        }

        return nearest;
    }
}

/// <summary>
/// Root object for location data JSON
/// </summary>
[Serializable]
public class LocationData
{
    public string version = "1.0";
    public string building = "";
    public List<LocationEntry> locations = new List<LocationEntry>();
    public List<QrAnchor> qrAnchors = new List<QrAnchor>();
}

/// <summary>
/// Represents a single location/POI
/// </summary>
[Serializable]
public class LocationEntry
{
    public string id = "";
    public string title = "";
    public string description = "";
    public int floor = 0;
    public WorldPosition worldPosition = new WorldPosition();
    public string category = "";
    public List<string> tags = new List<string>();

    /// <summary>
    /// Get world position as Vector3
    /// </summary>
    public Vector3 GetWorldPosition()
    {
        return new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
    }

    /// <summary>
    /// Get formatted floor name (e.g., "Floor 1" or "Ground")
    /// </summary>
    public string GetFloorName()
    {
        if (floor == 0)
            return "Ground";
        return $"Floor {floor}";
    }
}

/// <summary>
/// Represents a world position
/// </summary>
[Serializable]
public class WorldPosition
{
    public float x = 0f;
    public float y = 0f;
    public float z = 0f;
}

/// <summary>
/// Represents a QR code anchor point
/// </summary>
[Serializable]
public class QrAnchor
{
    public string qrCode = "";
    public WorldPosition worldPosition = new WorldPosition();
    public float worldRotationY = 0f;
    public int floor = 0;

    /// <summary>
    /// Get world position as Vector3
    /// </summary>
    public Vector3 GetWorldPosition()
    {
        return new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
    }

    /// <summary>
    /// Get rotation as Quaternion
    /// </summary>
    public Quaternion GetRotation()
    {
        return Quaternion.Euler(0, worldRotationY, 0);
    }

    /// <summary>
    /// Get floor name
    /// </summary>
    public string GetFloorName()
    {
        if (floor == 0)
            return "Ground";
        return $"Floor {floor}";
    }
}
