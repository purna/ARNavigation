using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// MapSegment represents a single floor or area within the target building.
/// It serves as a container for navigation vertices, floating icons, and rooms.
///
/// Corresponds to Unreal Engine's BP_MapSegment
/// </summary>
public class MapSegment : MonoBehaviour
{
    [Header("Segment Properties")]
    [Tooltip("Floor number (e.g., 1 for first floor, 2 for second floor)")]
    public int floor = 0;

    [Tooltip("Localized text describing the floor (e.g., 'First Floor')")]
    public string floorDisplayedText = "First Floor";

    [Tooltip("3D model/mesh that represents this floor area")]
    public GameObject segmentMesh;

    [Header("Mesh Configuration")]
    [Tooltip("X,Y offset of the segment mesh relative to segment origin")]
    public Vector2 segmentOffset = Vector2.zero;

    [Tooltip("Height of the segment mesh in meters")]
    public float segmentHeight = 1.0f;

    [Tooltip("Scale multiplier for the segment mesh")]
    public float segmentScale = 1.0f;

    [Tooltip("Material for displaying floor plan (editor only)")]
    public Material segmentMapMaterial;

    [Tooltip("X,Y scale for the floor plan material")]
    public float segmentMapScale = 200.0f;

    [Header("Child Elements")]
    [Tooltip("Navigation vertices in this segment")]
    public List<NavigationVertex> vertices = new List<NavigationVertex>();

    [Tooltip("Floating icons (points of interest) in this segment")]
    public List<FloatingIcon> floatingIcons = new List<FloatingIcon>();

    [Tooltip("Rooms in this segment")]
    public List<Room> rooms = new List<Room>();

    [Header("ARWorldBase Reference")]
    [Tooltip("Parent ARWorldBase transform")]
    public Transform arWorldBase;

    [Header("Visibility")]
    [Tooltip("Show segment in minimap")]
    public bool showInMinimap = true;

    [Tooltip("Show segment mesh in AR view")]
    public bool showMeshInAR = true;

    // Events
    public Action<MapSegment> onSegmentInitialized;

    // State
    private bool isInitialized = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// Initialize the map segment
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        // Register with ARWorldManager if available
        if (arWorldBase != null)
        {
            ARWorldManager worldManager = arWorldBase.GetComponent<ARWorldManager>();
            if (worldManager != null)
            {
                worldManager.AddMapSegment(this);
            }
        }

        // Apply mesh configuration
        if (segmentMesh != null)
        {
            ConfigureMesh();
        }

        // Collect child elements
        CollectChildElements();

        isInitialized = true;
        onSegmentInitialized?.Invoke(this);
        
        Debug.Log($"MapSegment initialized: Floor {floor} - {floorDisplayedText}");
    }

    private void ConfigureMesh()
    {
        // Apply offset
        Vector3 meshPosition = segmentMesh.transform.position;
        meshPosition.x += segmentOffset.x;
        meshPosition.z += segmentOffset.y;
        segmentMesh.transform.position = meshPosition;

        // Apply height
        Vector3 meshScale = segmentMesh.transform.localScale;
        meshScale.y = segmentHeight * segmentScale;
        segmentMesh.transform.localScale = meshScale;
    }

    private void CollectChildElements()
    {
        // Get all navigation vertices in children
        if (vertices.Count == 0)
        {
            NavigationVertex[] foundVertices = GetComponentsInChildren<NavigationVertex>();
            vertices.AddRange(foundVertices);
        }

        // Get all floating icons in children
        if (floatingIcons.Count == 0)
        {
            FloatingIcon[] foundIcons = GetComponentsInChildren<FloatingIcon>();
            floatingIcons.AddRange(foundIcons);
        }

        // Get all rooms in children
        if (rooms.Count == 0)
        {
            Room[] foundRooms = GetComponentsInChildren<Room>();
            rooms.AddRange(foundRooms);
        }

        // Set floor number for all child vertices
        foreach (var vertex in vertices)
        {
            if (vertex != null)
            {
                vertex.floor = this.floor;
            }
        }

        // Set floor for all child rooms
        foreach (var room in rooms)
        {
            if (room != null)
            {
                room.floorText = this.floorDisplayedText;
            }
        }
    }

    /// <summary>
    /// Align this segment to true north
    /// </summary>
    public void AlignToTrueNorth(float angle)
    {
        // Calculate rotation based on true north angle
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
        
        // Apply rotation around the segment center
        transform.rotation = targetRotation;
        
        Debug.Log($"MapSegment: Floor {floor} aligned to {angle} degrees");
    }

    /// <summary>
    /// Apply world offset to all elements in this segment
    /// </summary>
    public void ApplyWorldOffset(Vector3 offset)
    {
        // Offset all vertices
        foreach (var vertex in vertices)
        {
            if (vertex != null)
            {
                vertex.transform.position += offset;
            }
        }

        // Offset all floating icons
        foreach (var icon in floatingIcons)
        {
            if (icon != null)
            {
                icon.transform.position += offset;
            }
        }

        // Offset all rooms
        foreach (var room in rooms)
        {
            if (room != null)
            {
                room.transform.position += offset;
            }
        }

        // Offset mesh
        if (segmentMesh != null)
        {
            segmentMesh.transform.position += offset;
        }

        Debug.Log($"MapSegment: Floor {floor} offset by {offset}");
    }

    /// <summary>
    /// Add a navigation vertex to this segment
    /// </summary>
    public void AddVertex(NavigationVertex vertex)
    {
        if (vertex != null && !vertices.Contains(vertex))
        {
            vertex.transform.SetParent(this.transform);
            vertex.floor = this.floor;
            vertices.Add(vertex);
        }
    }

    /// <summary>
    /// Remove a navigation vertex from this segment
    /// </summary>
    public void RemoveVertex(NavigationVertex vertex)
    {
        if (vertex != null)
        {
            vertices.Remove(vertex);
        }
    }

    /// <summary>
    /// Add a floating icon to this segment
    /// </summary>
    public void AddFloatingIcon(FloatingIcon icon)
    {
        if (icon != null && !floatingIcons.Contains(icon))
        {
            icon.transform.SetParent(this.transform);
            floatingIcons.Add(icon);
        }
    }

    /// <summary>
    /// Remove a floating icon from this segment
    /// </summary>
    public void RemoveFloatingIcon(FloatingIcon icon)
    {
        if (icon != null)
        {
            floatingIcons.Remove(icon);
        }
    }

    /// <summary>
    /// Add a room to this segment
    /// </summary>
    public void AddRoom(Room room)
    {
        if (room != null && !rooms.Contains(room))
        {
            room.transform.SetParent(this.transform);
            room.floorText = this.floorDisplayedText;
            rooms.Add(room);
        }
    }

    /// <summary>
    /// Remove a room from this segment
    /// </summary>
    public void RemoveRoom(Room room)
    {
        if (room != null)
        {
            rooms.Remove(room);
        }
    }

    /// <summary>
    /// Find a vertex by ID within this segment
    /// </summary>
    public NavigationVertex FindVertexById(string vertexId)
    {
        if (string.IsNullOrEmpty(vertexId))
            return null;

        foreach (var vertex in vertices)
        {
            if (vertex != null && vertex.vertexId.Equals(vertexId, System.StringComparison.OrdinalIgnoreCase))
            {
                return vertex;
            }
        }
        return null;
    }

    /// <summary>
    /// Get the total number of vertices in this segment
    /// </summary>
    public int GetVertexCount()
    {
        return vertices != null ? vertices.Count : 0;
    }

    /// <summary>
    /// Show or hide the segment in AR view
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (segmentMesh != null)
        {
            segmentMesh.SetActive(visible && showMeshInAR);
        }
    }

    /// <summary>
    /// Show or hide segment in minimap
    /// </summary>
    public void SetMinimapVisibility(bool visible)
    {
        showInMinimap = visible;
    }

    /// <summary>
    /// Reset segment to original transform
    /// </summary>
    public void Reset()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
    }

    private void OnValidate()
    {
        // Ensure floor is non-negative
        floor = Mathf.Max(0, floor);
        
        // Ensure scale is positive
        segmentScale = Mathf.Max(0.01f, segmentScale);
        segmentHeight = Mathf.Max(0.01f, segmentHeight);
    }
}
