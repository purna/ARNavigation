using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// PathfindingManager handles all pathfinding operations using Dijkstra or FastSearch algorithms.
/// It provides methods to find optimal paths between navigation vertices.
///
/// Corresponds to Unreal Engine's BP_NavigationPathSpline
/// </summary>
public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("Pathfinding algorithm type")]
    public SearchType pathBuildingType = SearchType.Regular;

    [Tooltip("Maximum path length to prevent infinite loops")]
    public int maxPathLength = 1000;

    [Header("Navigation Vertices")]
    [Tooltip("All navigation vertices in the scene")]
    public List<NavigationVertex> allVertices = new List<NavigationVertex>();

    [Header("Events")]
    public Action<List<NavigationVertex>> onPathFound;
    public Action onPathNotFound;

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
        // Collect all vertices if not set
        if (allVertices.Count == 0)
        {
            CollectAllVertices();
        }
    }

    /// <summary>
    /// Collect all navigation vertices from the scene
    /// </summary>
    public void CollectAllVertices()
    {
        allVertices.Clear();
        
        // Get from ARWorldManager if available
        ARWorldManager worldManager = FindObjectOfType<ARWorldManager>();
        if (worldManager != null)
        {
            allVertices = worldManager.GetAllVertices();
        }
        else
        {
            // Fallback: find all vertices in scene
            NavigationVertex[] found = FindObjectsOfType<NavigationVertex>();
            allVertices = new List<NavigationVertex>(found);
        }

        Debug.Log($"PathfindingManager: Found {allVertices.Count} navigation vertices");
    }

    /// <summary>
    /// Find path between two vertices
    /// </summary>
    public List<NavigationVertex> FindPath(NavigationVertex start, NavigationVertex target)
    {
        if (start == null || target == null)
        {
            Debug.LogWarning("PathfindingManager: Start or target vertex is null");
            return null;
        }

        if (start == target)
        {
            return new List<NavigationVertex> { start };
        }

        // Reset all vertices before pathfinding
        ResetAllVertices();

        switch (pathBuildingType)
        {
            case SearchType.Regular:
                return Dijkstra(start, target);
            case SearchType.Fast:
                return FastSearch(start, target);
            default:
                return Dijkstra(start, target);
        }
    }

    /// <summary>
    /// Find path between two vertices by ID
    /// </summary>
    public List<NavigationVertex> FindPathById(string startId, string targetId)
    {
        NavigationVertex start = FindVertexById(startId);
        NavigationVertex target = FindVertexById(targetId);

        if (start == null)
        {
            Debug.LogWarning($"PathfindingManager: Start vertex not found: {startId}");
            return null;
        }

        if (target == null)
        {
            Debug.LogWarning($"PathfindingManager: Target vertex not found: {targetId}");
            return null;
        }

        return FindPath(start, target);
    }

    /// <summary>
    /// Find the nearest vertex to a position
    /// </summary>
    public NavigationVertex FindNearestVertex(Vector3 position)
    {
        if (allVertices.Count == 0)
            return null;

        NavigationVertex nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var vertex in allVertices)
        {
            if (vertex == null)
                continue;

            float distance = Vector3.Distance(position, vertex.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = vertex;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Find the nearest vertex with a specific ID
    /// </summary>
    public NavigationVertex FindNearestVertexWithId(Vector3 position, string vertexId)
    {
        if (string.IsNullOrEmpty(vertexId))
            return null;

        List<NavigationVertex> matchingVertices = allVertices
            .Where(v => v != null && v.vertexId.Equals(vertexId, System.StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matchingVertices.Count == 0)
            return null;

        // Find nearest among matching vertices
        NavigationVertex nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var vertex in matchingVertices)
        {
            float distance = Vector3.Distance(position, vertex.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = vertex;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Find a vertex by ID
    /// </summary>
    public NavigationVertex FindVertexById(string vertexId)
    {
        if (string.IsNullOrEmpty(vertexId))
            return null;

        return allVertices.FirstOrDefault(v => 
            v != null && v.vertexId.Equals(vertexId, System.StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Find all vertices with a specific ID (for non-unique IDs)
    /// </summary>
    public List<NavigationVertex> FindVerticesById(string vertexId)
    {
        if (string.IsNullOrEmpty(vertexId))
            return new List<NavigationVertex>();

        return allVertices
            .Where(v => v != null && v.vertexId.Equals(vertexId, System.StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Dijkstra's algorithm - visits all vertices to find shortest path
    /// Use for: graphs with multiple paths, non-unique vertices
    /// </summary>
    private List<NavigationVertex> Dijkstra(NavigationVertex start, NavigationVertex target)
    {
        // Priority queue would be ideal, but using sorted list for simplicity
        List<NavigationVertex> openSet = new List<NavigationVertex>();
        HashSet<NavigationVertex> closedSet = new HashSet<NavigationVertex>();

        start.gScore = 0;
        start.hScore = Vector3.Distance(start.transform.position, target.transform.position);
        openSet.Add(start);

        int iterations = 0;

        while (openSet.Count > 0)
        {
            iterations++;
            if (iterations > maxPathLength)
            {
                Debug.LogWarning("PathfindingManager: Dijkstra exceeded maximum iterations");
                break;
            }

            // Get vertex with lowest fScore
            openSet.Sort((a, b) => (a.gScore + a.hScore).CompareTo(b.gScore + b.hScore));
            NavigationVertex current = openSet[0];

            // Check if we reached target
            if (current == target)
            {
                List<NavigationVertex> path = ReconstructPath(current);
                onPathFound?.Invoke(path);
                return path;
            }

            openSet.RemoveAt(0);
            closedSet.Add(current);

            // Process neighbors
            foreach (NavigationVertex neighbor in current.GetAllNeighbors())
            {
                if (neighbor == null || closedSet.Contains(neighbor))
                    continue;

                // Calculate distance
                float distance = Vector3.Distance(current.transform.position, neighbor.transform.position);
                float tentativeG = current.gScore + distance;

                if (tentativeG < neighbor.gScore)
                {
                    neighbor.parentVertex = current;
                    neighbor.gScore = tentativeG;
                    neighbor.hScore = Vector3.Distance(neighbor.transform.position, target.transform.position);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        // No path found
        Debug.LogWarning("PathfindingManager: No path found using Dijkstra");
        onPathNotFound?.Invoke();
        return null;
    }

    /// <summary>
    /// Fast search - stops immediately when target found
    /// Use for: tree structures with unique vertices only
    /// </summary>
    private List<NavigationVertex> FastSearch(NavigationVertex start, NavigationVertex target)
    {
        Queue<NavigationVertex> queue = new Queue<NavigationVertex>();
        HashSet<NavigationVertex> visited = new HashSet<NavigationVertex>();

        start.gScore = 0;
        queue.Enqueue(start);

        int iterations = 0;

        while (queue.Count > 0)
        {
            iterations++;
            if (iterations > maxPathLength)
            {
                Debug.LogWarning("PathfindingManager: FastSearch exceeded maximum iterations");
                break;
            }

            NavigationVertex current = queue.Dequeue();

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            // Stop immediately if we found the target
            if (current == target)
            {
                List<NavigationVertex> path = ReconstructPath(current);
                onPathFound?.Invoke(path);
                return path;
            }

            // Process neighbors
            foreach (NavigationVertex neighbor in current.GetAllNeighbors())
            {
                if (neighbor == null || visited.Contains(neighbor))
                    continue;

                float distance = Vector3.Distance(current.transform.position, neighbor.transform.position);
                neighbor.gScore = current.gScore + distance;
                neighbor.parentVertex = current;
                queue.Enqueue(neighbor);
            }
        }

        // No path found
        Debug.LogWarning("PathfindingManager: No path found using FastSearch");
        onPathNotFound?.Invoke();
        return null;
    }

    /// <summary>
    /// Reconstruct path from target to start
    /// </summary>
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

    /// <summary>
    /// Reset all vertex pathfinding state
    /// </summary>
    private void ResetAllVertices()
    {
        foreach (var vertex in allVertices)
        {
            if (vertex != null)
            {
                vertex.ResetPathfindingState();
            }
        }
    }

    /// <summary>
    /// Set the search algorithm type
    /// </summary>
    public void SetSearchType(SearchType type)
    {
        pathBuildingType = type;
    }

    /// <summary>
    /// Get total path distance
    /// </summary>
    public float GetPathDistance(List<NavigationVertex> path)
    {
        if (path == null || path.Count < 2)
            return 0;

        float distance = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            distance += Vector3.Distance(path[i].transform.position, path[i + 1].transform.position);
        }

        return distance;
    }

    /// <summary>
    /// Get path as waypoint positions
    /// </summary>
    public List<Vector3> GetPathWaypoints(List<NavigationVertex> path)
    {
        if (path == null)
            return new List<Vector3>();

        return path.ConvertAll(v => v.transform.position);
    }

    /// <summary>
    /// Check if a path is valid
    /// </summary>
    public bool IsPathValid(List<NavigationVertex> path)
    {
        if (path == null || path.Count == 0)
            return false;

        // Check for null vertices
        if (path.Any(v => v == null))
            return false;

        // Check for connected path
        for (int i = 0; i < path.Count - 1; i++)
        {
            if (!path[i].IsConnectedTo(path[i + 1]))
                return false;
        }

        return true;
    }
}

/// <summary>
/// Pathfinding algorithm type
/// </summary>
public enum SearchType
{
    Regular,  // Dijkstra - for graphs with multiple paths
    Fast      // Fast search - for trees with single path
}
