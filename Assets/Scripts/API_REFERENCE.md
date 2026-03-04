# AR Navigation - Unity API Reference

This document outlines all the Unity APIs and classes required to build the AR Navigation application.

---

## 1. ARFoundation APIs

### Core AR Components

#### ARSession
```csharp
using UnityEngine.XR.ARFoundation;

public ARSession arSession;

// Start AR session
arSession.Start();

// Stop AR session
arSession.Stop();

// Check session state
if (arSession.state == ARSessionState.Running)
{
    // AR is active
}
```

**Namespace:** `UnityEngine.XR.ARFoundation`  
**Required Components:**
- `ARSession.subsystem` - XR subsystem for AR

---

#### ARSessionOrigin
```csharp
using UnityEngine.XR.ARFoundation;

public ARSessionOrigin arSessionOrigin;

// Get camera position in AR space
Transform cameraTransform = arSessionOrigin.camera.transform;

// Trackables
var trackables = arSessionOrigin.GetTrackables<ARPlane>();
```

**Namespace:** `UnityEngine.XR.ARFoundation`  
**Purpose:** Transforms AR coordinates to Unity world space

---

#### ARCameraBackground
```csharp
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARCamera))]
public class ARCameraBackground : MonoBehaviour
{
    public bool enabled;
    public Material cameraMaterial;
}
```

**Purpose:** Renders AR camera feed as background

---

### Plane Detection

#### ARPlaneManager
```csharp
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public ARPlaneManager planeManager;

// Get all detected planes
foreach (var plane in planeManager.trackables)
{
    Vector3 position = plane.transform.position;
    Vector2 size = plane.size;
    Pose pose = plane.transform;
}

// Plane detection mode
planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;
```

**TrackableType:** `TrackableType.PlaneWithinBounds`, `TrackableType.PlaneWithinPolygon`

---

#### ARPlane
```csharp
using UnityEngine.XR.ARFoundation;

public ARPlane arPlane;

// Plane boundary points (in local space)
Vector3[] boundaryPoints = arPlane.boundary;

// Plane alignment
PlaneAlignment alignment = arPlane.alignment;

// Plane classification
PlaneClassifications classification = arPlane.classifications;
```

---

### Raycasting

#### ARRaycastManager
```csharp
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine;

public ARRaycastManager raycastManager;

public bool Raycast(Vector2 screenPosition, out ARRaycastHit hit, TrackableType trackableTypes)
{
    return raycastManager.Raycast(screenPosition, out hit, trackableTypes);
}

// Usage
if (raycastManager.Raycast(touch.position, out ARRaycastHit hit, TrackableType.PlaneWithinPolygon))
{
    Pose hitPose = hit.pose;
    ARPlane plane = hit.trackable as ARPlane;
}
```

---

### Image Tracking

#### ARTrackedImageManager
```csharp
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public ARTrackedImageManager trackedImageManager;

// Enable image tracking
trackedImageManager.enabled = true;

// Get tracked images
foreach (var image in trackedImageManager.trackables)
{
    string name = image.referenceImage.name;
    Pose pose = image.transform;
    TrackingState state = image.trackingState;
}
```

---

#### ARReferenceImageLibrary
```csharp
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[CreateAssetMenu(fileName = "ARReferenceImageLibrary", menuName = "XR/AR Reference Image Library")]
public class ARReferenceImageLibrary : ScriptableObject
{
    // Add reference images in Unity Editor
    // TextureD,2 width (meters), name, specifyUsage
}
```

---

### Anchors

#### ARAnchorManager
```csharp
using UnityEngine.XR.ARFoundation;

public ARAnchorManager anchorManager;

// Add anchor at pose
ARAnchor anchor = anchorManager.AddAnchor(new Pose(position, rotation));

// Remove anchor
anchorManager.RemoveAnchor(anchor);

// Get all anchors
foreach (var anchor in anchorManager.trackables)
{
    // Process anchor
}
```

---

## 2. ARCore APIs

### ARCoreSession
```csharp
using UnityEngine.XR.ARCore;

public ARCoreSessionSubsystem arCoreSubsystem;

// Check ARCore availability
if (ARCoreSessionSubsystem.CheckAvailability() == ArAvailability.Supported)
{
    // ARCore is available
}

// Configure ARCore specific
arCoreSubsystem.session settingsConfig.planeFindingMode = NativeSessionConfig.PlaneFindingMode.HorizontalAndVertical;
```

**Namespace:** `UnityEngine.XR.ARCore`  
**Platform:** Android only

---

## 3. ARKit APIs

### ARKitSession
```csharp
using UnityEngine.XR.ARKit;

public ARKitSessionSubsystem arKitSubsystem;

// Check ARKit availability
if (ARKitSessionSubsystem.IsSupported())
{
    // ARKit is supported
}

// Configure ARKit specific settings
arKitSubsystem.worldAlignment = ARWorldAlignment.GravityAndHeading;
```

**Namespace:** `UnityEngine.XR.ARKit`  
**Platform:** iOS only

---

## 4. XR Management APIs

### XRManagerSettings
```csharp
using UnityEngine.XR.Management;

public XRManagerSettings xrManager;

// Initialize XR
yield return xrManager.InitializeManagers();

// Start XR
yield return xrManager.StartSubsystems();

// Stop XR
xrManager.StopSubsystems();

// Deinitialize
xrManager.Deinitialize();
```

---

## 5. UnityEngine.XR Namespace

### Input Devices
```csharp
using UnityEngine.XR;

InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);
```

### Common Usages
| Usage | Type | Description |
|-------|------|-------------|
| `devicePosition` | Vector3 | Device position |
| `deviceRotation` | Quaternion | Device rotation |
| `centerEyePosition` | Vector3 | Center eye position |
| `centerEyeRotation` | Quaternion | Center eye rotation |
| `cameraPosition` | Vector3 | Camera position |
| `cameraRotation` | Quaternion | Camera rotation |

---

## 6. Required Scripts Structure

### Core Scripts

#### ARWorldManager.cs
```csharp
using UnityEngine;
using UnityEngine.XR.ARFoundation.Generic;

public class ARWorld;
using System.CollectionsManager : MonoBehaviour
{
    [Header("World Configuration")]
    [Tooltip("Rotation of the target area relative to true north")]
    public float worldTrueNorthAngle = 0f;
    
    [Tooltip("List of map segments in this world")]
    public List<MapSegment> mapSegments;
    
    [Header("Reference Images")]
    [Tooltip("Reference image library for QR code detection")]
    public ARReferenceImageLibrary referenceImageLibrary;
    
    [Header("AR Components")]
    public ARSession arSession;
    public ARSessionOrigin arSessionOrigin;
    
    // Singleton instance
    public static ARWorldManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public void Initialize()
    {
        // Initialize AR session
        // Load map segments
        // Set up reference images
    }
}
```

---

#### MapSegment.cs
```csharp
using UnityEngine;
using System.Collections.Generic;

public class MapSegment : MonoBehaviour
{
    [Header("Segment Properties")]
    public int floor = 0;
    public string floorDisplayedText = "First Floor";
    
    [Header("Mesh Configuration")]
    public GameObject segmentMesh;
    public Vector2 segmentOffset = Vector2.zero;
    public float segmentHeight = 1f;
    public float segmentScale = 1f;
    public Material segmentMapMaterial;
    public float segmentMapScale = 200f;
    
    [Header("Child Elements")]
    public List<NavigationVertex> vertices;
    public List<FloatingIcon> floatingIcons;
    public List<Room> rooms;
    
    [Header("ARWorldBase Reference")]
    public Transform arWorldBase;
    
    // Called when segment is initialized
    public void Initialize()
    {
        // Set up mesh, calculate bounds
        // Initialize child vertices
    }
}
```

---

#### NavigationVertex.cs
```csharp
using UnityEngine;
using System.Collections.Generic;

public class NavigationVertex : MonoBehaviour
{
    [Header("Vertex Identification")]
    [Tooltip("Unique identifier for search")]
    public string vertexId = "";
    
    [Tooltip("Whether this vertex ID is unique across the entire map")]
    public bool isUnique = true;
    
    [Header("Connections")]
    [Tooltip("Bidirectional connections (corridor, stairs)")]
    public List<NavigationVertex> bilateralConnections;
    
    [Tooltip("Unidirectional connections (turnstiles, escalators)")]
    public List<NavigationVertex> lateralConnections;
    
    [Header("Floor")]
    public int floor = 0;
    
    // Pathfinding properties (set during algorithm execution)
    [HideInInspector]
    public float gScore = float.MaxValue;
    
    [HideInInspector]
    public float hScore = 0;
    
    [HideInInspector]
    public NavigationVertex parentVertex;
    
    // Check if vertex is the target
    public bool IsDestination(string targetId)
    {
        return !string.IsNullOrEmpty(vertexId) && 
               vertexId.Equals(targetId, System.StringComparison.OrdinalIgnoreCase);
    }
    
    // Get all neighbors (both bilateral and lateral)
    public List<NavigationVertex> GetAllNeighbors()
    {
        List<NavigationVertex> neighbors = new List<NavigationVertex>();
        
        if (bilateralConnections != null)
            neighbors.AddRange(bilateralConnections);
        
        if (lateralConnections != null)
            neighbors.AddRange(lateralConnections);
        
        return neighbors;
    }
}
```

---

#### PathfindingManager.cs
```csharp
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum SearchType
{
    Regular,  // Dijkstra - for graphs
    Fast      // Fast search - for trees
}

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance { get; private set; }
    
    [Header("Configuration")]
    public SearchType pathBuildingType = SearchType.Regular;
    
    [Header("Navigation Vertices")]
    public List<NavigationVertex> allVertices;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    /// <summary>
    /// Find path from start vertex to target vertex
    /// </summary>
    public List<NavigationVertex> FindPath(NavigationVertex start, NavigationVertex target)
    {
        if (start == null || target == null)
            return null;
            
        switch (pathBuildingType)
        {
            case SearchType.Regular:
                return Dijkstra(start, target);
            case SearchType.FastSearch(start, target);
            default:
                return Dijkstra:
                return Fast(start, target);
        }
    }
    
    /// <summary>
    /// Find path using vertex ID
    /// </summary>
    public List<NavigationVertex> FindPathById(string startId, string targetId)
    {
        NavigationVertex start = FindVertexById(startId);
        NavigationVertex target = FindVertexById(targetId);
        
        return FindPath(start, target);
    }
    
    /// <summary>
    /// Dijkstra's algorithm for graph traversal
    /// </summary>
    private List<NavigationVertex> Dijkstra(NavigationVertex start, NavigationVertex target)
    {
        // Reset all vertices
        ResetVertices();
        
        // Priority queue would be ideal, but for simplicity using sorted list
        List<NavigationVertex> openSet = new List<NavigationVertex>();
        HashSet<NavigationVertex> closedSet = new HashSet<NavigationVertex>();
        
        start.gScore = 0;
        openSet.Add(start);
        
        while (openSet.Count > 0)
        {
            // Get vertex with lowest fScore
            openSet.Sort((a, b) => (a.gScore + a.hScore).CompareTo(b.gScore + b.hScore));
            NavigationVertex current = openSet[0];
            
            // Check if we reached target
            if (current == target)
            {
                return ReconstructPath(current);
            }
            
            openSet.RemoveAt(0);
            closedSet.Add(current);
            
            // Process neighbors
            foreach (NavigationVertex neighbor in current.GetAllNeighbors())
            {
                if (closedSet.Contains(neighbor))
                    continue;
                    
                // Calculate distance (Euclidean distance between vertices)
                float distance = Vector3.Distance(current.transform.position, neighbor.transform.position);
                float tentativeG = current.gScore + distance;
                
                if (tentativeG < neighbor.gScore)
                {
                    neighbor.parentVertex = current;
                    neighbor.gScore = tentativeG;
                    
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
        
        // No path found
        return null;
    }
    
    /// <summary>
    /// Fast search for tree structures - stops immediately when target found
    /// </summary>
    private List<NavigationVertex> FastSearch(NavigationVertex start, NavigationVertex target)
    {
        ResetVertices();
        
        Queue<NavigationVertex> queue = new Queue<NavigationVertex>();
        HashSet<NavigationVertex> visited = new HashSet<NavigationVertex>();
        
        start.gScore = 0;
        queue.Enqueue(start);
        
        while (queue.Count > 0)
        {
            NavigationVertex current = queue.Dequeue();
            
            if (visited.Contains(current))
                continue;
                
            visited.Add(current);
            
            // Stop immediately if found the we target
            if (current == target)
            {
                return ReconstructPath(current);
            }
            
            // Process neighbors
            foreach (NavigationVertex neighbor in current.GetAllNeighbors())
            {
                if (!visited.Contains(neighbor))
                {
                    float distance = Vector3.Distance(current.transform.position, neighbor.transform.position);
                    neighbor.gScore = current.gScore + distance;
                    neighbor.parentVertex = current;
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        return null;
    }
    
    private void ResetVertices()
    {
        foreach (var vertex in allVertices)
        {
            vertex.gScore = float.MaxValue;
            vertex.hScore = 0;
            vertex.parentVertex = null;
        }
    }
    
    private List<NavigationVertex> ReconstructPath(NavigationVertex target)
    {
        List<NavigationVertex> path = new List<NavigationVertex>();
        NavigationVertex current = target;
        
        while (current != null)
        {
            path.Add(current);
            current = current.parentVertex;
        }
        
        path.Reverse();
        return path;
    }
    
    public NavigationVertex FindVertexById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;
            
        return allVertices.FirstOrDefault(v => 
            v.vertexId.Equals(id, System.StringComparison.OrdinalIgnoreCase));
    }
}
```

---

#### FloatingIcon.cs
```csharp
using UnityEngine;

public class FloatingIcon : MonoBehaviour
{
    [Header("Icon Properties")]
    public Material iconMaterial;
    public string iconText = "";
    
    [Header("Billboard Settings")]
    public bool faceCamera = true;
    public bool lockYRotation = false;
    
    [Header("AR World Reference")]
    public Transform arCamera;
    
    private void Start()
    {
        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }
        
        // Apply material if specified
        if (iconMaterial != null)
        {
            GetComponent<Renderer>().material = iconMaterial;
        }
    }
    
    private void LateUpdate()
    {
        if (faceCamera && arCamera != null)
        {
            // Billboard effect - face the camera
            Vector3 direction = arCamera.position - transform.position;
            
            if (lockYRotation)
            {
                // Only rotate around Y axis
                direction.y = 0;
            }
            
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}
```

---

#### Room.cs
```csharp
using UnityEngine;
using System;

public enum RoomType
{
    Room,
    Corridor,
    Staircase,
    Other
}

public class Room : MonoBehaviour
{
    [Header("Room Properties")]
    public string roomTitle = "No Title";
    public string floorText = "First Floor";
    public RoomType roomType = RoomType.Room;
    
    [Header("Size Configuration")]
    [Tooltip("Set to zero for automatic size detection")]
    public Vector2 size = Vector2.zero;
    public float height = 3f;
    
    [Header("Debug")]
    public Material debugMaterial;
    public bool showDebug = false;
    
    [Header("Map Segment Reference")]
    public Transform mapSegment;
    
    // Events
    public event Action<Room> OnPlayerEntered;
    public event Action<Room> OnPlayerExited;
    
    private BoxCollider boxCollider;
    
    private void Start()
    {
        // Set up collider
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        
        boxCollider.isTrigger = true;
        
        // Set size
        if (size != Vector2.zero)
        {
            boxCollider.size = new Vector3(size.x, height, size.y);
        }
        
        // Apply debug material if in debug mode
        if (debugMaterial != null && showDebug)
        {
            GetComponent<Renderer>().material = debugMaterial;
        }
    }
    
    public void SetSize(Vector2 newSize)
    {
        size = newSize;
        if (boxCollider != null && size != Vector2.zero)
        {
            boxCollider.size = new Vector3(size.x, height, size.y);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered
        if (other.CompareTag("Player"))
        {
            OnPlayerEntered?.Invoke(this);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if player exited
        if (other.CompareTag("Player"))
        {
            OnPlayerExited?.Invoke(this);
        }
    }
}
```

---

## 7. Package Dependencies Summary

### Required Packages
| Package | Version | Registry |
|---------|---------|----------|
| com.unity.xr.arcore | 5.1.2 | Unity Registry |
| com.unity.xr.arkit | 5.1.2 | Unity Registry |
| com.unity.xr.management | 4.4.0 | Unity Registry |
| com.unity.render-pipelines.universal | 14.0.9 | Unity Registry |
| com.unity.textmeshpro | 3.0.6 | Unity Registry |

### Namespace Summary
| Namespace | Purpose |
|-----------|---------|
| `UnityEngine.XR.ARFoundation` | Core AR functionality |
| `UnityEngine.XR.ARCore` | Android-specific AR |
| `UnityEngine.XR.ARKit` | iOS-specific AR |
| `UnityEngine.XR.Management` | XR plugin management |
| `UnityEngine.XR.ARSubsystems` | AR subsystems |
| `UnityEngine.Rendering` | URP rendering |

---

*API Reference v1.0 - AR Navigation v3.0.0*
