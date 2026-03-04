using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// NavigationVertex represents a single node in the navigation graph.
/// Vertices are connected to form paths for indoor navigation.
///
/// Corresponds to Unreal Engine's BP_Vertex
/// </summary>
[ExecuteInEditMode]
public class NavigationVertex : MonoBehaviour
{
    [Header("Vertex Identification")]
    [Tooltip("Unique identifier for this vertex (used for search)")]
    public string vertexId = "";

    [Tooltip("Whether this vertex ID is unique across the entire map")]
    public bool isUnique = true;

    [Tooltip("Display name for this vertex")]
    public string displayName = "";

    [Header("Connections")]
    [Tooltip("Bidirectional connections (corridor, stairs) - path works both ways")]
    public List<NavigationVertex> bilateralConnections = new List<NavigationVertex>();

    [Tooltip("Unidirectional connections (turnstiles, escalators) - path works only one way")]
    public List<NavigationVertex> lateralConnections = new List<NavigationVertex>();

    [Header("Floor")]
    [Tooltip("Floor number this vertex belongs to")]
    public int floor = 0;

    [Header("Debug Visualization")]
    [Tooltip("Show debug axis in editor")]
    public bool showDebugAxis = false;

    [Tooltip("Axis length for debug visualization")]
    public float axisLength = 0.5f;

    [Tooltip("Color for vertex sphere")]
    public Color vertexColor = Color.yellow;

    // Pathfinding properties (runtime only, not serialized)
    [HideInInspector]
    public float gScore = float.MaxValue;

    [HideInInspector]
    public float hScore = 0;

    [HideInInspector]
    public NavigationVertex parentVertex;

    [HideInInspector]
    public bool isVisited;

    // Components
    private Renderer meshRenderer;
    private SphereCollider sphereCollider;

    // Constants
    private const float DEFAULT_RADIUS = 0.15f;

    private void Awake()
    {
        // Add sphere collider if not present
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = DEFAULT_RADIUS;
            sphereCollider.isTrigger = true;
        }

        // Add mesh renderer for visualization
        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer == null)
        {
            // Create a primitive sphere for visualization
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * DEFAULT_RADIUS * 2;
            
            meshRenderer = sphere.GetComponent<Renderer>();
            
            // Don't destroy on load - this is just for visual debugging
            Destroy(sphere); // We'll add it back in editor mode
        }
    }

    private void Start()
    {
        // Runtime initialization
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Editor mode - show visualization
            return;
        }
        #endif

        // Reset pathfinding state
        ResetPathfindingState();
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying && showDebugAxis)
        {
            DebugDrawAxis();
        }
        #endif
    }

    /// <summary>
    /// Reset pathfinding state for a new search
    /// </summary>
    public void ResetPathfindingState()
    {
        gScore = float.MaxValue;
        hScore = 0;
        parentVertex = null;
        isVisited = false;
    }

    /// <summary>
    /// Check if this vertex matches the target ID
    /// </summary>
    public bool IsDestination(string targetId)
    {
        if (string.IsNullOrEmpty(vertexId) || string.IsNullOrEmpty(targetId))
            return false;

        return vertexId.Equals(targetId, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get all neighbors (both bilateral and lateral)
    /// </summary>
    public List<NavigationVertex> GetAllNeighbors()
    {
        List<NavigationVertex> neighbors = new List<NavigationVertex>();

        // Add bilateral connections (bidirectional)
        if (bilateralConnections != null)
        {
            foreach (var vertex in bilateralConnections)
            {
                if (vertex != null && !neighbors.Contains(vertex))
                {
                    neighbors.Add(vertex);
                }
            }
        }

        // Add lateral connections (unidirectional)
        if (lateralConnections != null)
        {
            foreach (var vertex in lateralConnections)
            {
                if (vertex != null && !neighbors.Contains(vertex))
                {
                    neighbors.Add(vertex);
                }
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Get only bilateral connections
    /// </summary>
    public List<NavigationVertex> GetBilateralConnections()
    {
        if (bilateralConnections == null)
            return new List<NavigationVertex>();
        
        return bilateralConnections.FindAll(v => v != null);
    }

    /// <summary>
    /// Get only lateral connections
    /// </summary>
    public List<NavigationVertex> GetLateralConnections()
    {
        if (lateralConnections == null)
            return new List<NavigationVertex>();
        
        return lateralConnections.FindAll(v => v != null);
    }

    /// <summary>
    /// Calculate distance to another vertex
    /// </summary>
    public float DistanceTo(NavigationVertex other)
    {
        if (other == null)
            return float.MaxValue;

        return Vector3.Distance(transform.position, other.transform.position);
    }

    /// <summary>
    /// Calculate heuristic (estimated distance) to target
    /// </summary>
    public float CalculateHeuristic(NavigationVertex target)
    {
        if (target == null)
            return 0;

        return Vector3.Distance(transform.position, target.transform.position);
    }

    /// <summary>
    /// Check if this vertex is connected to another vertex
    /// </summary>
    public bool IsConnectedTo(NavigationVertex other)
    {
        if (other == null)
            return false;

        // Check bilateral
        if (bilateralConnections != null && bilateralConnections.Contains(other))
            return true;

        // Check lateral
        if (lateralConnections != null && lateralConnections.Contains(other))
            return true;

        return false;
    }

    /// <summary>
    /// Add a bilateral connection (both ways)
    /// </summary>
    public void AddBilateralConnection(NavigationVertex other)
    {
        if (other == null || other == this)
            return;

        // Add to this vertex
        if (!bilateralConnections.Contains(other))
        {
            bilateralConnections.Add(other);
        }

        // Add reciprocal connection
        if (!other.bilateralConnections.Contains(this))
        {
            other.bilateralConnections.Add(this);
        }
    }

    /// <summary>
    /// Add a lateral connection (one way only)
    /// </summary>
    public void AddLateralConnection(NavigationVertex other)
    {
        if (other == null || other == this)
            return;

        if (!lateralConnections.Contains(other))
        {
            lateralConnections.Add(other);
        }
    }

    /// <summary>
    /// Remove a connection
    /// </summary>
    public void RemoveConnection(NavigationVertex other)
    {
        if (other == null)
            return;

        bilateralConnections?.Remove(other);
        lateralConnections?.Remove(other);
    }

    /// <summary>
    /// Clear all connections
    /// </summary>
    public void ClearConnections()
    {
        bilateralConnections?.Clear();
        lateralConnections?.Clear();
    }

    /// <summary>
    /// Get total number of connections
    /// </summary>
    public int GetConnectionCount()
    {
        int count = 0;
        if (bilateralConnections != null)
            count += bilateralConnections.Count;
        if (lateralConnections != null)
            count += lateralConnections.Count;
        return count;
    }

    /// <summary>
    /// Get vertex display name (ID or display name)
    /// </summary>
    public string GetDisplayName()
    {
        if (!string.IsNullOrEmpty(displayName))
            return displayName;
        
        if (!string.IsNullOrEmpty(vertexId))
            return vertexId;
        
        return "Vertex";
    }

    /// <summary>
    /// Check if this is a terminal vertex (no connections)
    /// </summary>
    public bool IsTerminal()
    {
        return GetConnectionCount() == 0;
    }

    /// <summary>
    /// Check if this is an intersection (multiple connections)
    /// </summary>
    public bool IsIntersection()
    {
        return GetConnectionCount() > 2;
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Draw debug axis in editor
    /// </summary>
    private void DebugDrawAxis()
    {
        Vector3 pos = transform.position;
        
        // X axis - Red
        Debug.DrawLine(pos, pos + transform.right * axisLength, Color.red);
        
        // Y axis - Green
        Debug.DrawLine(pos, pos + transform.up * axisLength, Color.green);
        
        // Z axis - Blue
        Debug.DrawLine(pos, pos + transform.forward * axisLength, Color.blue);
    }

    private void OnDrawGizmos()
    {
        // Draw sphere at vertex position
        Gizmos.color = vertexColor;
        Gizmos.DrawSphere(transform.position, DEFAULT_RADIUS);

        // Draw lines to connected vertices
        if (bilateralConnections != null)
        {
            Gizmos.color = Color.green;
            foreach (var vertex in bilateralConnections)
            {
                if (vertex != null)
                {
                    Gizmos.DrawLine(transform.position, vertex.transform.position);
                }
            }
        }

        if (lateralConnections != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var vertex in lateralConnections)
            {
                if (vertex != null)
                {
                    Gizmos.DrawLine(transform.position, vertex.transform.position);
                }
            }
        }
    }
    #endif

    private void OnValidate()
    {
        // Ensure radius is positive
        if (sphereCollider != null)
        {
            sphereCollider.radius = Mathf.Max(0.01f, DEFAULT_RADIUS);
        }

        // Ensure floor is non-negative
        floor = Mathf.Max(0, floor);
    }
}
