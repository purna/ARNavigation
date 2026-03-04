using UnityEngine;
using System.Collections.Generic;
using System;

namespace ARNavigation.Configuration
{
    /// <summary>
    /// NavigationConfig stores the configuration for the AR Navigation system.
    /// This is a ScriptableObject that can be created and configured in the Unity Editor.
    ///
    /// Corresponds to Unreal Engine's BP_Configuration
    /// </summary>
    [CreateAssetMenu(fileName = "NavigationConfig", menuName = "AR Navigation/Configuration")]
    public class NavigationConfig : ScriptableObject
    {
        [Header("Pathfinding")]
        [Tooltip("Pathfinding algorithm type")]
        public SearchType pathBuildingType = SearchType.Regular;

        [Header("Initial Positions")]
        [Tooltip("List of possible initial positions for the user")]
        public List<InitialPosition> initialPositions = new List<InitialPosition>();

        [Header("Quick Destinations")]
        [Tooltip("List of quick destination buttons for the main UI")]
        public List<QuickDestinationButton> quickDestinations = new List<QuickDestinationButton>();

        [Header("UI Settings")]
        [Tooltip("Show debug mode by default")]
        public bool showDebugByDefault = false;

        [Tooltip("Distance threshold to consider destination reached (meters)")]
        public float destinationThreshold = 0.5f;

        [Tooltip("Minimap zoom level")]
        public float minimapZoom = 1.0f;

        [Header("AR Settings")]
        [Tooltip("Height offset for navigation elements above detected planes")]
        public float navigationHeightOffset = 0.1f;

        [Tooltip("Auto-start AR session on app launch")]
        public bool autoStartAR = true;

        [Tooltip("Enable QR code scanning for initial positioning")]
        public bool enableQRCodeScanning = true;

        [Header("Marker Settings")]
        [Tooltip("Default marker color")]
        public Color defaultMarkerColor = Color.red;

        [Tooltip("Maximum number of custom markers")]
        public int maxCustomMarkers = 10;

        /// <summary>
        /// Get initial position by index
        /// </summary>
        public InitialPosition GetInitialPosition(int index)
        {
            if (index >= 0 && index < initialPositions.Count)
            {
                return initialPositions[index];
            }
            return null;
        }

        /// <summary>
        /// Get initial position by vertex ID
        /// </summary>
        public InitialPosition GetInitialPositionByVertexId(string vertexId)
        {
            foreach (var pos in initialPositions)
            {
                if (pos != null && pos.teleportToVertexTag.Equals(vertexId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return pos;
                }
            }
            return null;
        }

        /// <summary>
        /// Get quick destination by index
        /// </summary>
        public QuickDestinationButton GetQuickDestination(int index)
        {
            if (index >= 0 && index < quickDestinations.Count)
            {
                return quickDestinations[index];
            }
            return null;
        }

        /// <summary>
        /// Get quick destination by vertex ID
        /// </summary>
        public QuickDestinationButton GetQuickDestinationByVertexId(string vertexId)
        {
            foreach (var dest in quickDestinations)
            {
                if (dest != null && dest.destinationTag.Equals(vertexId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return dest;
                }
            }
            return null;
        }

        /// <summary>
        /// Add a new initial position
        /// </summary>
        public void AddInitialPosition(string displayText, string vertexId)
        {
            InitialPosition newPos = new InitialPosition
            {
                text = displayText,
                teleportToVertexTag = vertexId
            };
            initialPositions.Add(newPos);
        }

        /// <summary>
        /// Add a new quick destination
        /// </summary>
        public void AddQuickDestination(string displayText, string floor, string vertexId)
        {
            QuickDestinationButton newDest = new QuickDestinationButton
            {
                text = displayText,
                floor = floor,
                destinationTag = vertexId
            };
            quickDestinations.Add(newDest);
        }

        /// <summary>
        /// Remove an initial position
        /// </summary>
        public void RemoveInitialPosition(int index)
        {
            if (index >= 0 && index < initialPositions.Count)
            {
                initialPositions.RemoveAt(index);
            }
        }

        /// <summary>
        /// Remove a quick destination
        /// </summary>
        public void RemoveQuickDestination(int index)
        {
            if (index >= 0 && index < quickDestinations.Count)
            {
                quickDestinations.RemoveAt(index);
            }
        }

        /// <summary>
        /// Clear all initial positions
        /// </summary>
        public void ClearInitialPositions()
        {
            initialPositions.Clear();
        }

        /// <summary>
        /// Clear all quick destinations
        /// </summary>
        public void ClearQuickDestinations()
        {
            quickDestinations.Clear();
        }

        /// <summary>
        /// Get total number of initial positions
        /// </summary>
        public int GetInitialPositionCount()
        {
            return initialPositions != null ? initialPositions.Count : 0;
        }

        /// <summary>
        /// Get total number of quick destinations
        /// </summary>
        public int GetQuickDestinationCount()
        {
            return quickDestinations != null ? quickDestinations.Count : 0;
        }
    }

    /// <summary>
    /// Represents a single initial position option
    /// </summary>
    [Serializable]
    public class InitialPosition
    {
        [Tooltip("Display text shown in the UI")]
        public string text = "";

        [Tooltip("Vertex ID to teleport to")]
        public string teleportToVertexTag = "";

        public InitialPosition()
        {
        }

        public InitialPosition(string displayText, string vertexId)
        {
            text = displayText;
            teleportToVertexTag = vertexId;
        }
    }

    /// <summary>
    /// Represents a quick destination button
    /// </summary>
    [Serializable]
    public class QuickDestinationButton
    {
        [Tooltip("Display text shown on the button")]
        public string text = "";

        [Tooltip("Floor name displayed under the destination")]
        public string floor = "";

        [Tooltip("Vertex ID of the destination")]
        public string destinationTag = "";

        public QuickDestinationButton()
        {
        }

        public QuickDestinationButton(string displayText, string floorName, string vertexId)
        {
            text = displayText;
            floor = floorName;
            destinationTag = vertexId;
        }
    }
}
