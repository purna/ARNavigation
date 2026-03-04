using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// PathVisualizer displays the navigation path using a LineRenderer and arrow indicators.
/// It shows the path between the player and destination.
///
/// Corresponds to Unreal Engine's BP_NavigationPathSpline visualization
/// </summary>
public class PathVisualizer : MonoBehaviour
{
    public static PathVisualizer Instance { get; private set; }

    [Header("Line Renderer")]
    [Tooltip("Line renderer component for drawing path")]
    public LineRenderer lineRenderer;

    [Tooltip("Line material")]
    public Material lineMaterial;

    [Tooltip("Line color")]
    public Color lineColor = Color.blue;

    [Tooltip("Line width")]
    public float lineWidth = 0.05f;

    [Header("Arrow Prefab")]
    [Tooltip("Arrow prefab for path direction")]
    public GameObject arrowPrefab;

    [Tooltip("Distance between arrows")]
    public float arrowSpacing = 0.5f;

    [Header("Path Settings")]
    [Tooltip("Show path line")]
    public bool showLine = true;

    [Tooltip("Show direction arrows")]
    public bool showArrows = true;

    [Tooltip("Arrow scale")]
    public float arrowScale = 0.2f;

    [Header("Animation")]
    [Tooltip("Animate arrows along path")]
    public bool animateArrows = true;

    [Tooltip("Arrow animation speed")]
    public float animationSpeed = 1.0f;

    // State
    private List<NavigationVertex> currentPath = new List<NavigationVertex>();
    private List<GameObject> arrowInstances = new List<GameObject>();
    private List<Vector3> pathWaypoints = new List<Vector3>();
    private bool isPathActive = false;
    private float animationOffset = 0f;

    // Events
    public Action onPathUpdated;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Create line renderer if not assigned
        if (lineRenderer == null)
        {
            CreateLineRenderer();
        }
    }

    private void Start()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (isPathActive && animateArrows)
        {
            AnimateArrowPositions();
        }
    }

    /// <summary>
    /// Create a default line renderer
    /// </summary>
    private void CreateLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        // Configure line renderer
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0;
        
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
        else
        {
            // Create default material
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
    }

    /// <summary>
    /// Display a navigation path
    /// </summary>
    public void DisplayPath(List<NavigationVertex> path)
    {
        if (path == null || path.Count < 2)
        {
            ClearPath();
            return;
        }

        currentPath = path;
        pathWaypoints = ConvertPathToWaypoints(path);
        
        // Draw line
        if (showLine && lineRenderer != null)
        {
            DrawPathLine(pathWaypoints);
        }

        // Spawn arrows
        if (showArrows && arrowPrefab != null)
        {
            SpawnArrows(pathWaypoints);
        }

        isPathActive = true;
        onPathUpdated?.Invoke();
        
        Debug.Log($"PathVisualizer: Displaying path with {path.Count} vertices");
    }

    /// <summary>
    /// Display path from player to destination
    /// </summary>
    public void DisplayPathToDestination(Vector3 startPosition, NavigationVertex destination)
    {
        if (destination == null)
        {
            Debug.LogWarning("PathVisualizer: Destination is null");
            return;
        }

        // Create path with start position
        List<NavigationVertex> fullPath = new List<NavigationVertex>();
        
        // Add a temporary start vertex at player's position
        // For simplicity, we'll just draw the line from player to first vertex
        if (PathfindingManager.Instance != null)
        {
            NavigationVertex nearest = PathfindingManager.Instance.FindNearestVertex(startPosition);
            if (nearest != null)
            {
                // Find path from nearest to destination
                List<NavigationVertex> computedPath = PathfindingManager.Instance.FindPath(nearest, destination);
                if (computedPath != null)
                {
                    DisplayPath(computedPath);
                    return;
                }
            }
        }

        // If no pathfinding, just show direct line
        pathWaypoints = new List<Vector3> { startPosition, destination.transform.position };
        DrawPathLine(pathWaypoints);
    }

    /// <summary>
    /// Convert vertex path to waypoint list
    /// </summary>
    private List<Vector3> ConvertPathToWaypoints(List<NavigationVertex> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        
        foreach (var vertex in path)
        {
            if (vertex != null)
            {
                waypoints.Add(vertex.transform.position);
            }
        }
        
        return waypoints;
    }

    /// <summary>
    /// Draw the path line
    /// </summary>
    private void DrawPathLine(List<Vector3> waypoints)
    {
        if (lineRenderer == null || waypoints.Count < 2)
            return;

        lineRenderer.positionCount = waypoints.Count;
        lineRenderer.SetPositions(waypoints.ToArray());
        lineRenderer.enabled = true;
    }

    /// <summary>
    /// Spawn arrows along the path
    /// </summary>
    private void SpawnArrows(List<Vector3> waypoints)
    {
        // Clear existing arrows
        ClearArrows();

        if (waypoints.Count < 2 || arrowPrefab == null)
            return;

        // Calculate total path length
        float totalLength = CalculatePathLength(waypoints);
        
        // Calculate number of arrows
        int arrowCount = Mathf.Max(2, Mathf.FloorToInt(totalLength / arrowSpacing));

        for (int i = 0; i < arrowCount; i++)
        {
            float t = (float)i / (arrowCount - 1);
            Vector3 position = GetPointAlongPath(waypoints, t);
            Vector3 direction = GetDirectionAlongPath(waypoints, t);
            
            // Spawn arrow
            GameObject arrow = Instantiate(arrowPrefab, position, Quaternion.LookRotation(direction));
            arrow.transform.localScale = Vector3.one * arrowScale;
            
            arrowInstances.Add(arrow);
        }
    }

    /// <summary>
    /// Get point along the path at parameter t (0-1)
    /// </summary>
    private Vector3 GetPointAlongPath(List<Vector3> waypoints, float t)
    {
        if (waypoints.Count < 2)
            return Vector3.zero;

        // Calculate total path length
        float totalLength = CalculatePathLength(waypoints);
        float targetDistance = t * totalLength;

        float currentDistance = 0;
        
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            float segmentLength = Vector3.Distance(waypoints[i], waypoints[i + 1]);
            
            if (currentDistance + segmentLength >= targetDistance)
            {
                // Interpolate within this segment
                float segmentT = (targetDistance - currentDistance) / segmentLength;
                return Vector3.Lerp(waypoints[i], waypoints[i + 1], segmentT);
            }
            
            currentDistance += segmentLength;
        }

        return waypoints[waypoints.Count - 1];
    }

    /// <summary>
    /// Get direction along the path at parameter t (0-1)
    /// </summary>
    private Vector3 GetDirectionAlongPath(List<Vector3> waypoints, float t)
    {
        if (waypoints.Count < 2)
            return Vector3.forward;

        // Find the current segment
        float totalLength = CalculatePathLength(waypoints);
        float targetDistance = t * totalLength;
        
        float currentDistance = 0;
        
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            float segmentLength = Vector3.Distance(waypoints[i], waypoints[i + 1]);
            
            if (currentDistance + segmentLength >= targetDistance)
            {
                return (waypoints[i + 1] - waypoints[i]).normalized;
            }
            
            currentDistance += segmentLength;
        }

        return (waypoints[waypoints.Count - 1] - waypoints[waypoints.Count - 2]).normalized;
    }

    /// <summary>
    /// Calculate total path length
    /// </summary>
    private float CalculatePathLength(List<Vector3> waypoints)
    {
        if (waypoints.Count < 2)
            return 0;

        float length = 0;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            length += Vector3.Distance(waypoints[i], waypoints[i + 1]);
        }

        return length;
    }

    /// <summary>
    /// Animate arrow positions along the path
    /// </summary>
    private void AnimateArrowPositions()
    {
        if (arrowInstances.Count == 0 || pathWaypoints.Count < 2)
            return;

        animationOffset += Time.deltaTime * animationSpeed;
        if (animationOffset > 1f)
            animationOffset -= 1f;

        // Update arrow positions with animation offset
        for (int i = 0; i < arrowInstances.Count; i++)
        {
            if (arrowInstances[i] == null)
                continue;

            float t = ((float)i / arrowInstances.Count + animationOffset) % 1f;
            
            Vector3 position = GetPointAlongPath(pathWaypoints, t);
            Vector3 direction = GetDirectionAlongPath(pathWaypoints, t);
            
            arrowInstances[i].transform.position = position;
            arrowInstances[i].transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    /// <summary>
    /// Clear the current path
    /// </summary>
    public void ClearPath()
    {
        currentPath.Clear();
        pathWaypoints.Clear();
        isPathActive = false;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }

        ClearArrows();
        
        onPathUpdated?.Invoke();
    }

    /// <summary>
    /// Clear all arrow instances
    /// </summary>
    private void ClearArrows()
    {
        foreach (var arrow in arrowInstances)
        {
            if (arrow != null)
            {
                Destroy(arrow);
            }
        }
        arrowInstances.Clear();
    }

    /// <summary>
    /// Show or hide the path
    /// </summary>
    public void SetPathVisible(bool visible)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = visible && showLine;
        }

        foreach (var arrow in arrowInstances)
        {
            if (arrow != null)
            {
                arrow.SetActive(visible && showArrows);
            }
        }

        isPathActive = visible;
    }

    /// <summary>
    /// Get the current path
    /// </summary>
    public List<NavigationVertex> GetCurrentPath()
    {
        return currentPath;
    }

    /// <summary>
    /// Check if path is currently displayed
    /// </summary>
    public bool IsPathActive()
    {
        return isPathActive;
    }

    /// <summary>
    /// Get total path distance
    /// </summary>
    public float GetPathDistance()
    {
        return CalculatePathLength(pathWaypoints);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        ClearArrows();
    }
}
