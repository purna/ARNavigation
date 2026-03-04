using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System;

/// <summary>
/// ARWorldManager is the root container for all map segments in the AR Navigation system.
/// It manages the world coordinate system, reference images for QR code detection,
/// and serves as the central hub for world mapping functionality.
///
/// Corresponds to Unreal Engine's BP_ARWorldBase
/// </summary>
public class ARWorldManager : MonoBehaviour
{
    public static ARWorldManager Instance { get; private set; }

    [Header("World Configuration")]
    [Tooltip("Rotation of the target area relative to true north (in degrees)")]
    [Range(0, 360)]
    public float worldTrueNorthAngle = 0f;

    [Tooltip("Height offset for placing navigation elements above detected planes")]
    public float navigationHeightOffset = 0.1f;

    [Header("Map Segments")]
    [Tooltip("List of all map segments in this world")]
    public List<MapSegment> mapSegments = new List<MapSegment>();

    [Header("AR Components")]
    public ARSessionManager arSessionManager;
    public ARTrackedImageManager trackedImageManager;

    [Header("Reference Image Tracking")]
    [Tooltip("Enable QR code/image tracking for initial positioning")]
    public bool enableImageTracking = true;

    [Tooltip("Callback when a reference image is detected")]
    public Action<string, Pose> onReferenceImageDetected;

    // World state
    private bool isWorldAligned = false;
    private Vector3 worldOrigin = Vector3.zero;
    private Quaternion worldRotation = Quaternion.identity;

    // Tracking state
    private string lastDetectedImageName = "";
    private Pose initialPositionPose;

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
        // Initialize world alignment
        InitializeWorld();

        // Set up image tracking if enabled
        if (enableImageTracking)
        {
            SetupImageTracking();
        }
    }

    private void InitializeWorld()
    {
        // Set world rotation based on true north angle
        // AR uses Z+ as forward, so we rotate around Y axis
        worldRotation = Quaternion.Euler(0, worldTrueNorthAngle, 0);
        
        // Apply initial rotation with delay to allow AR session to initialize
        StartCoroutine(AlignWorldAfterDelay());
    }

    private System.Collections.IEnumerator AlignWorldAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);
        
        // Rotate all map segments to align with true north
        foreach (var segment in mapSegments)
        {
            if (segment != null)
            {
                segment.AlignToTrueNorth(worldTrueNorthAngle);
            }
        }
        
        isWorldAligned = true;
        Debug.Log($"ARWorldManager: World aligned to {worldTrueNorthAngle} degrees from true north");
    }

    private void SetupImageTracking()
    {
        if (trackedImageManager == null)
        {
            // Try to get from ARSessionManager
            if (arSessionManager != null && arSessionManager.xrOrigin != null)
            {
                trackedImageManager = arSessionManager.xrOrigin.GetComponent<ARTrackedImageManager>();
            }
        }

        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            HandleImageDetection(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                HandleImageDetection(trackedImage);
            }
        }
    }

    private void HandleImageDetection(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        
        // Only trigger on new detection
        if (imageName != lastDetectedImageName)
        {
            lastDetectedImageName = imageName;
            
            // Get the pose where the image was detected
            initialPositionPose = trackedImage.transform.ToPose();
            
            // Invoke callback
            onReferenceImageDetected?.Invoke(imageName, initialPositionPose);
            
            Debug.Log($"ARWorldManager: Reference image detected - {imageName} at {initialPositionPose.position}");
        }
    }

    /// <summary>
    /// Set the initial position based on a detected reference image (QR code)
    /// </summary>
    /// <param name="vertexId">The vertex ID to set as initial position</param>
    /// <param name="pose">The pose where the reference image was detected</param>
    public void SetInitialPositionFromQRCode(string vertexId, Pose pose)
    {
        // Find the vertex with the matching ID
        NavigationVertex initialVertex = FindVertexById(vertexId);
        
        if (initialVertex != null)
        {
            // Calculate offset between QR code position and vertex position
            Vector3 offset = initialVertex.transform.position - pose.position;
            
            // Apply offset to all map segments
            foreach (var segment in mapSegments)
            {
                segment.ApplyWorldOffset(offset);
            }
            
            Debug.Log($"ARWorldManager: Initial position set from QR code. Vertex: {vertexId}");
        }
        else
        {
            Debug.LogWarning($"ARWorldManager: Could not find vertex with ID: {vertexId}");
        }
    }

    /// <summary>
    /// Find a vertex by its ID across all map segments
    /// </summary>
    /// <param name="vertexId">The vertex ID to search for</param>
    /// <returns>The NavigationVertex if found, null otherwise</returns>
    public NavigationVertex FindVertexById(string vertexId)
    {
        if (string.IsNullOrEmpty(vertexId))
            return null;

        foreach (var segment in mapSegments)
        {
            if (segment != null && segment.vertices != null)
            {
                foreach (var vertex in segment.vertices)
                {
                    if (vertex != null && vertex.vertexId.Equals(vertexId, StringComparison.OrdinalIgnoreCase))
                    {
                        return vertex;
                    }
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// Find all vertices with a specific ID (for non-unique vertices)
    /// </summary>
    public List<NavigationVertex> FindVerticesById(string vertexId)
    {
        List<NavigationVertex> results = new List<NavigationVertex>();
        
        if (string.IsNullOrEmpty(vertexId))
            return results;

        foreach (var segment in mapSegments)
        {
            if (segment != null && segment.vertices != null)
            {
                foreach (var vertex in segment.vertices)
                {
                    if (vertex != null && vertex.vertexId.Equals(vertexId, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(vertex);
                    }
                }
            }
        }
        
        return results;
    }

    /// <summary>
    /// Get all vertices in the world
    /// </summary>
    public List<NavigationVertex> GetAllVertices()
    {
        List<NavigationVertex> allVertices = new List<NavigationVertex>();
        
        foreach (var segment in mapSegments)
        {
            if (segment != null && segment.vertices != null)
            {
                allVertices.AddRange(segment.vertices);
            }
        }
        
        return allVertices;
    }

    /// <summary>
    /// Get all map segments in the world
    /// </summary>
    public List<MapSegment> GetMapSegments()
    {
        return mapSegments;
    }

    /// <summary>
    /// Get map segment by floor number
    /// </summary>
    public MapSegment GetMapSegmentByFloor(int floor)
    {
        foreach (var segment in mapSegments)
        {
            if (segment != null && segment.floor == floor)
            {
                return segment;
            }
        }
        return null;
    }

    /// <summary>
    /// Get the world origin position
    /// </summary>
    public Vector3 GetWorldOrigin()
    {
        return worldOrigin;
    }

    /// <summary>
    /// Get the world rotation aligned to true north
    /// </summary>
    public Quaternion GetWorldRotation()
    {
        return worldRotation;
    }

    /// <summary>
    /// Check if world is aligned
    /// </summary>
    public bool IsWorldAligned()
    {
        return isWorldAligned;
    }

    /// <summary>
    /// Add a map segment to the world
    /// </summary>
    public void AddMapSegment(MapSegment segment)
    {
        if (segment != null && !mapSegments.Contains(segment))
        {
            mapSegments.Add(segment);
        }
    }

    /// <summary>
    /// Remove a map segment from the world
    /// </summary>
    public void RemoveMapSegment(MapSegment segment)
    {
        if (segment != null)
        {
            mapSegments.Remove(segment);
        }
    }

    /// <summary>
    /// Convert a position from AR world space to local navigation space
    /// </summary>
    public Vector3 ConvertToNavigationSpace(Vector3 arPosition)
    {
        return Quaternion.Inverse(worldRotation) * (arPosition - worldOrigin);
    }

    /// <summary>
    /// Convert a position from local navigation space to AR world space
    /// </summary>
    public Vector3 ConvertToARWorldSpace(Vector3 navigationPosition)
    {
        return worldRotation * navigationPosition + worldOrigin;
    }

    /// <summary>
    /// Get the last detected reference image name
    /// </summary>
    public string GetLastDetectedImageName()
    {
        return lastDetectedImageName;
    }

    /// <summary>
    /// Reset detection state for new QR code scan
    /// </summary>
    public void ResetQRCodeDetection()
    {
        lastDetectedImageName = "";
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Unsubscribe from events
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    private void OnValidate()
    {
        // Ensure angle is within 0-360 range
        worldTrueNorthAngle = Mathf.Clamp(worldTrueNorthAngle, 0f, 360f);
    }
}

/// <summary>
/// Extension method to convert Transform to Pose
/// </summary>
public static class TransformExtensions
{
    public static Pose ToPose(this Transform transform)
    {
        return new Pose(transform.position, transform.rotation);
    }
}

