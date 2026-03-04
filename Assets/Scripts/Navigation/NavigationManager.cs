using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// NavigationManager is the core navigation controller that handles pathfinding, 
/// navigation state, and coordinates with the path visualizer.
///
/// Corresponds to build plan Phase 4 - Pathfinding & Navigation
/// </summary>
public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance { get; private set; }

    [Header("Components")]
    [Tooltip("Pathfinding manager")]
    public PathfindingManager pathfindingManager;

    [Tooltip("Path visualizer")]
    public PathVisualizer pathVisualizer;

    [Tooltip("User position manager")]
    public UserPositionManager userPositionManager;

    [Tooltip("Floor manager")]
    public FloorManager floorManager;

    [Header("Navigation Settings")]
    [Tooltip("Distance threshold to consider destination reached (meters)")]
    public float destinationThreshold = 0.5f;

    [Tooltip("Distance to trigger re-routing (meters)")]
    public float rerouteThreshold = 2.0f;

    [Tooltip("Waypoint arrival threshold (meters)")]
    public float waypointThreshold = 0.3f;

    [Header("Debug")]
    [Tooltip("Show debug info")]
    public bool showDebugInfo = false;

    // Navigation state
    public enum NavigationState
    {
        Idle,
        Navigating,
        Arrived,
        NoPath,
        NeedsFloorTransition
    }

    public NavigationState CurrentState { get; private set; }
    public LocationEntry CurrentDestination { get; private set; }
    public List<NavigationVertex> CurrentPath { get; private set; }
    public int CurrentWaypointIndex { get; private set; }

    // Events
    public Action<NavigationState> onStateChanged;
    public Action<LocationEntry> onDestinationSet;
    public Action onDestinationReached;
    public Action onPathRecalculated;
    public Action<int, string> onFloorTransitionNeeded; // floor, instruction

    // Private state
    private Vector3 lastPlayerPosition;
    private bool needsReroute = false;
    private float timeSinceLastRerouteCheck = 0f;

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
        // Get references
        if (pathfindingManager == null)
            pathfindingManager = FindObjectOfType<PathfindingManager>();
        
        if (pathVisualizer == null)
            pathVisualizer = FindObjectOfType<PathVisualizer>();
        
        if (userPositionManager == null)
            userPositionManager = FindObjectOfType<UserPositionManager>();
        
        if (floorManager == null)
            floorManager = FindObjectOfType<FloorManager>();

        // Initialize state
        SetState(NavigationState.Idle);
        CurrentPath = new List<NavigationVertex>();
    }

    private void Update()
    {
        if (CurrentState != NavigationState.Navigating)
            return;

        // Update navigation
        UpdateNavigation();

        // Check for reroute
        timeSinceLastRerouteCheck += Time.deltaTime;
        if (timeSinceLastRerouteCheck >= 1f)
        {
            CheckForReroute();
            timeSinceLastRerouteCheck = 0f;
        }
    }

    /// <summary>
    /// Set the navigation destination
    /// </summary>
    public void SetDestination(LocationEntry destination)
    {
        if (destination == null)
        {
            Debug.LogWarning("NavigationManager: Destination is null");
            return;
        }

        CurrentDestination = destination;

        if (showDebugInfo)
            Debug.Log($"NavigationManager: Setting destination to {destination.title} (Floor {destination.floor})");

        // Get user position
        Vector3 userPos = userPositionManager != null 
            ? userPositionManager.CurrentWorldPosition 
            : Vector3.zero;

        // Check if we need floor transition
        int userFloor = floorManager != null ? floorManager.GetCurrentFloor() : 0;
        
        if (userFloor != destination.floor)
        {
            // Need floor transition - route through stairs/elevator
            HandleFloorTransition(userFloor, destination.floor, userPos);
        }
        else
        {
            // Same floor - direct path
            CalculatePath(userPos, destination);
        }

        onDestinationSet?.Invoke(destination);
    }

    /// <summary>
    /// Set destination by location ID
    /// </summary>
    public void SetDestination(string locationId)
    {
        LocationEntry location = LocationDataManager.Instance?.GetById(locationId);
        
        if (location == null)
        {
            Debug.LogWarning($"NavigationManager: Location not found: {locationId}");
            SetState(NavigationState.NoPath);
            return;
        }

        SetDestination(location);
    }

    /// <summary>
    /// Handle floor transition by routing through stairs/elevator
    /// </summary>
    private void HandleFloorTransition(int fromFloor, int toFloor, Vector3 userPos)
    {
        if (showDebugInfo)
            Debug.Log($"NavigationManager: Floor transition needed from {fromFloor} to {toFloor}");

        // Find nearest stairs/elevator on current floor
        LocationEntry currentFloorTransition = FindNearestTransitionPoint(fromFloor, userPos);
        
        // Find stairs/elevator on destination floor
        LocationEntry destFloorTransition = FindNearestTransitionPoint(toFloor, CurrentDestination.GetWorldPosition());

        if (currentFloorTransition == null || destFloorTransition == null)
        {
            Debug.LogWarning("NavigationManager: Could not find floor transition points");
            SetState(NavigationState.NoPath);
            return;
        }

        // Create a multi-part path:
        // 1. User -> Current floor transition point
        // 2. Transition (stairs/elevator)
        // 3. Destination floor transition point -> Destination

        // For now, just navigate to the transition point first
        // The UI will show "take stairs/elevator" instruction
        CalculatePath(userPos, currentFloorTransition);
        
        // Set floor transition instruction
        string instruction = fromFloor < toFloor 
            ? $"Take the stairs/elevator to Floor {toFloor}" 
            : $"Take the stairs/elevator down to Floor {toFloor}";
        
        onFloorTransitionNeeded?.Invoke(toFloor, instruction);
        SetState(NavigationState.NeedsFloorTransition);
    }

    /// <summary>
    /// Find nearest stairs/elevator point
    /// </summary>
    private LocationEntry FindNearestTransitionPoint(int floor, Vector3 nearPosition)
    {
        var locationsOnFloor = LocationDataManager.Instance?.GetByFloor(floor);
        if (locationsOnFloor == null)
            return null;

        // Find circulation points (stairs/elevators)
        var transitionPoints = locationsOnFloor
            .Where(l => l.category == "circulation")
            .OrderBy(l => Vector3.Distance(nearPosition, l.GetWorldPosition()))
            .ToList();

        return transitionPoints.FirstOrDefault();
    }

    /// <summary>
    /// Calculate path to destination
    /// </summary>
    private void CalculatePath(Vector3 userPos, LocationEntry destination)
    {
        if (pathfindingManager == null)
        {
            Debug.LogWarning("NavigationManager: No pathfinding manager");
            SetState(NavigationState.NoPath);
            return;
        }

        // Find nearest vertex to user
        NavigationVertex startVertex = pathfindingManager.FindNearestVertex(userPos);
        
        // Find nearest vertex to destination
        NavigationVertex destVertex = pathfindingManager.FindNearestVertex(destination.GetWorldPosition());

        if (startVertex == null || destVertex == null)
        {
            Debug.LogWarning("NavigationManager: Could not find path vertices");
            SetState(NavigationState.NoPath);
            return;
        }

        // Find path
        List<NavigationVertex> path = pathfindingManager.FindPath(startVertex, destVertex);

        if (path != null && path.Count > 0)
        {
            CurrentPath = path;
            CurrentWaypointIndex = 0;
            SetState(NavigationState.Navigating);
            
            // Visualize path
            if (pathVisualizer != null)
            {
                pathVisualizer.DisplayPath(path);
            }

            lastPlayerPosition = userPos;
            needsReroute = false;

            if (showDebugInfo)
                Debug.Log($"NavigationManager: Path found with {path.Count} waypoints");

            onPathRecalculated?.Invoke();
        }
        else
        {
            Debug.LogWarning("NavigationManager: No path found");
            SetState(NavigationState.NoPath);
        }
    }

    /// <summary>
    /// Update navigation state each frame
    /// </summary>
    private void UpdateNavigation()
    {
        if (userPositionManager == null || CurrentPath == null || CurrentPath.Count == 0)
            return;

        Vector3 playerPos = userPositionManager.CurrentWorldPosition;

        // Check if we've reached the current waypoint
        if (CurrentWaypointIndex < CurrentPath.Count)
        {
            NavigationVertex currentWaypoint = CurrentPath[CurrentWaypointIndex];
            float distanceToWaypoint = Vector3.Distance(playerPos, currentWaypoint.transform.position);

            if (distanceToWaypoint < waypointThreshold)
            {
                // Move to next waypoint
                CurrentWaypointIndex++;
                
                if (showDebugInfo)
                    Debug.Log($"NavigationManager: Reached waypoint {CurrentWaypointIndex}/{CurrentPath.Count}");
            }
        }

        // Check if we've reached the destination
        if (CurrentDestination != null)
        {
            float distanceToDestination = Vector3.Distance(playerPos, CurrentDestination.GetWorldPosition());
            
            if (distanceToDestination < destinationThreshold)
            {
                // Arrived!
                SetState(NavigationState.Arrived);
                ClearPath();
                onDestinationReached?.Invoke();
                
                if (showDebugInfo)
                    Debug.Log($"NavigationManager: Arrived at {CurrentDestination.title}");
            }
        }
    }

    /// <summary>
    /// Check if user has strayed from path and needs rerouting
    /// </summary>
    private void CheckForReroute()
    {
        if (CurrentState != NavigationState.Navigating || CurrentPath == null || CurrentPath.Count == 0)
            return;

        if (userPositionManager == null)
            return;

        Vector3 playerPos = userPositionManager.CurrentWorldPosition;

        // Calculate distance from player to path
        float distanceToPath = CalculateDistanceToPath(playerPos);

        if (distanceToPath > rerouteThreshold && !needsReroute)
        {
            needsReroute = true;
            
            if (showDebugInfo)
                Debug.Log($"NavigationManager: User has strayed {distanceToPath}m from path, rerouting...");
            
            // Recalculate path
            RecalculatePath();
        }

        lastPlayerPosition = playerPos;
    }

    /// <summary>
    /// Calculate minimum distance from point to path
    /// </summary>
    private float CalculateDistanceToPath(Vector3 point)
    {
        if (CurrentPath == null || CurrentPath.Count < 2)
            return float.MaxValue;

        float minDistance = float.MaxValue;

        for (int i = 0; i < CurrentPath.Count - 1; i++)
        {
            Vector3 lineStart = CurrentPath[i].transform.position;
            Vector3 lineEnd = CurrentPath[i + 1].transform.position;
            
            float distance = DistanceToLineSegment(point, lineStart, lineEnd);
            minDistance = Mathf.Min(minDistance, distance);
        }

        return minDistance;
    }

    /// <summary>
    /// Calculate distance from point to line segment
    /// </summary>
    private float DistanceToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDir = lineEnd - lineStart;
        float lineLength = lineDir.magnitude;
        
        if (lineLength == 0)
            return Vector3.Distance(point, lineStart);
        
        lineDir.Normalize();
        
        float t = Vector3.Dot(point - lineStart, lineDir);
        t = Mathf.Clamp(t, 0, lineLength);
        
        Vector3 projection = lineStart + t * lineDir;
        return Vector3.Distance(point, projection);
    }

    /// <summary>
    /// Recalculate the current path
    /// </summary>
    public void RecalculatePath()
    {
        if (CurrentDestination != null && userPositionManager != null)
        {
            Vector3 userPos = userPositionManager.CurrentWorldPosition;
            
            // Check floor
            int userFloor = floorManager != null ? floorManager.GetCurrentFloor() : 0;
            
            if (userFloor != CurrentDestination.floor)
            {
                HandleFloorTransition(userFloor, CurrentDestination.floor, userPos);
            }
            else
            {
                CalculatePath(userPos, CurrentDestination);
            }
            
            needsReroute = false;
        }
    }

    /// <summary>
    /// Cancel navigation
    /// </summary>
    public void CancelNavigation()
    {
        ClearPath();
        CurrentDestination = null;
        SetState(NavigationState.Idle);
    }

    /// <summary>
    /// Clear the current path
    /// </summary>
    private void ClearPath()
    {
        CurrentPath?.Clear();
        CurrentWaypointIndex = 0;

        if (pathVisualizer != null)
        {
            pathVisualizer.ClearPath();
        }
    }

    /// <summary>
    /// Set navigation state
    /// </summary>
    private void SetState(NavigationState newState)
    {
        if (CurrentState != newState)
        {
            CurrentState = newState;
            onStateChanged?.Invoke(newState);
            
            if (showDebugInfo)
                Debug.Log($"NavigationManager: State changed to {newState}");
        }
    }

    /// <summary>
    /// Get distance to destination
    /// </summary>
    public float GetDistanceToDestination()
    {
        if (CurrentDestination == null || userPositionManager == null)
            return float.MaxValue;

        return userPositionManager.GetDistanceTo(CurrentDestination);
    }

    /// <summary>
    /// Get remaining path distance
    /// </summary>
    public float GetRemainingPathDistance()
    {
        if (pathfindingManager == null || CurrentPath == null)
            return 0;

        return pathfindingManager.GetPathDistance(CurrentPath);
    }

    /// <summary>
    /// Get current waypoint position
    /// </summary>
    public Vector3? GetCurrentWaypointPosition()
    {
        if (CurrentPath == null || CurrentWaypointIndex >= CurrentPath.Count)
            return null;

        return CurrentPath[CurrentWaypointIndex].transform.position;
    }

    /// <summary>
    /// Check if currently navigating
    /// </summary>
    public bool IsNavigating()
    {
        return CurrentState == NavigationState.Navigating || CurrentState == NavigationState.NeedsFloorTransition;
    }

    /// <summary>
    /// Get current navigation state
    /// </summary>
    public NavigationState GetState()
    {
        return CurrentState;
    }
}
