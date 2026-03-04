# AR Navigation Unity Setup Guide

This document provides instructions for setting up the AR Navigation project in Unity Editor, including script assignments, GameObject hierarchy, and configuration.

---

## Table of Contents

1. [Complete Component Reference](#complete-component-reference)
2. [Scene Hierarchy Overview](#scene-hierarchy-overview)
3. [Core GameObjects and Scripts](#core-gameobjects-and-scripts)
4. [Prefabs and Components](#prefabs-and-components)
5. [Mobile AR Template Integration](#mobile-ar-template-integration)
6. [Unity Editor Settings](#unity-editor-settings)
7. [AR Foundation Configuration](#ar-foundation-configuration)

---

## Complete Component Reference

### All GameObjects and Their Required Components

| GameObject | Script(s) | Unity Components Required | Notes |
|------------|-----------|---------------------------|-------|
| **XROrigin** | - | Transform, ARRaycastManager, ARPlaneManager, ARAnchorManager | AR Foundation 6.x (or ARSessionOrigin in 5.x) |
| **ARSession** | - | ARSession | Auto-created if missing |
| **ARSessionManager** | ARSessionManager.cs | (none - script creates components) | Singleton manager |
| **ARPlayerController** | ARPlayerController.cs | (none - script auto-finds components) | Player movement |
| **ARWorldManager** | ARWorldManager.cs | (none) | World container |
| **PathfindingManager** | PathfindingManager.cs | (none) | A* pathfinding |
| **PathVisualizer** | PathVisualizer.cs | LineRenderer | Auto-creates LineRenderer if null |
| **MinimapRenderer** | MinimapRenderer.cs | (none) | 2D minimap |
| **MinimapCamera** | MinimapCamera.cs | Camera (Orthographic) | Child of MinimapRenderer |
| **MapSegment** | MapSegment.cs | (none) | Floor container |
| **NavigationVertex** | NavigationVertex.cs | SphereCollider (auto), Transform | Waypoint node |
| **FloatingIcon** | FloatingIcon.cs | Renderer (auto), Transform | POI marker |
| **Room** | Room.cs | BoxCollider (auto), Transform | Area container |
| **Marker** | Marker.cs | Renderer (auto), Collider (auto) | User-placed marker |
| **AR Camera** | - | Camera, ARCameraManager, ARInputManager | Main AR camera |

---

### Detailed Component Breakdown

#### XROrigin (or ARSessionOrigin)
```
XROrigin GameObject
├── Transform
├── ARRaycastManager
├── ARPlaneManager
├── ARAnchorManager
└── Camera Offset (child)
    └── AR Camera
```

**Required Unity Components:**
- Transform (built-in)
- [ARRaycastManager](Unity/ARNavigation/Assets/Scripts/Core/ARPlayerController.cs:96) - for raycasting against planes
- ARPlaneManager - for plane detection
- ARAnchorManager - for anchoring (optional)

**Child AR Camera Required Components:**
- Camera (enabled)
- ARCameraManager
- ARInputManager

---

#### ARSession
```
ARSession GameObject
└── ARSession (component)
```

**Required Unity Components:**
- ARSession (from AR Foundation)

---

#### ARSessionManager
```
ARSessionManager GameObject
└── ARSessionManager.cs (script)
```

**Script Fields that reference other components:**
| Field | Type | Auto-Find |
|-------|------|-----------|
| arSession | ARSession | ✓ FindObjectOfType |
| xrOrigin | XROrigin | ✓ FindObjectOfType |
| referenceImageLibrary | XRReferenceImageLibrary | Manual |
| navigationConfig | NavigationConfig | Manual (ScriptableObject) |

---

#### ARPlayerController
```
ARPlayerController GameObject
└── ARPlayerController.cs (script)
```

**Script Fields:**
| Field | Type | Auto-Find |
|-------|------|-----------|
| arSessionOrigin | Transform | ✓ FindObjectOfType(ARSessionOrigin) |
| arCamera | Transform | ✓ Camera.main.transform |
| markerPrefab | GameObject | Manual |
| currentDestination | NavigationVertex | Manual (set at runtime) |

**Components used from arSessionOrigin:**
- ARRaycastManager

---

#### ARWorldManager
```
ARWorldManager GameObject
└── ARWorldManager.cs (script)
```

**Script Fields:**
| Field | Type | Required |
|-------|------|----------|
| mapSegments | List<MapSegment> | Manual |
| arSessionManager | ARSessionManager | Manual |
| trackedImageManager | ARTrackedImageManager | Auto-find from XR Origin |
| worldTrueNorthAngle | float | Manual |

---

#### PathfindingManager
```
PathfindingManager GameObject
└── PathfindingManager.cs (script)
```

**Script Fields:**
| Field | Type | Required |
|-------|------|----------|
| allVertices | List<NavigationVertex> | Auto-collects from scene |
| pathBuildingType | SearchType (enum) | Manual |
| maxPathLength | int | Manual |

**Auto-finds:**
- ARWorldManager for vertex collection

---

#### PathVisualizer
```
PathVisualizer GameObject
├── PathVisualizer.cs (script)
└── LineRenderer (auto-created if null)
```

**Script Fields:**
| Field | Type | Required |
|-------|------|----------|
| lineRenderer | LineRenderer | Auto-creates if null |
| lineMaterial | Material | Optional (uses default) |
| arrowPrefab | GameObject | Optional |
| showLine | bool | Manual |
| showArrows | bool | Manual |

**Auto-created if missing:**
- LineRenderer with Sprites/Default material

---

#### MinimapRenderer
```
MinimapRenderer GameObject
├── MinimapRenderer.cs (script)
└── MinimapCamera (child)
    └── Camera
        └── MinimapCamera.cs
```

**Script Fields:**
| Field | Type | Required |
|-------|------|----------|
| minimapCamera | Camera | Manual |
| minimapDisplay | RawImage | Manual |
| minimapTexture | RenderTexture | Auto-creates if null |
| playerIndicator | Transform | Optional |
| rotationIndicator | Transform | Optional |
| mapCenter | Vector3 | Manual |
| mapSize | Vector2 | Manual |

**MinimapCamera Child:**
| Component | Setting | Value |
|-----------|---------|-------|
| Camera | Projection | Orthographic |
| Camera | Size | 40 |
| Camera | Position | (mapCenter.x, 50, mapCenter.z) |
| Camera | Rotation | (90, 0, 0) |
| Camera | Clear Flags | Solid Color |
| Camera | Background | (0,0,0,0) transparent |
| MinimapCamera.cs | (tag) | Just a marker component |

---

#### MapSegment
```
MapSegment_Floor0 GameObject
├── MapSegment.cs (script)
├── segmentMesh (child: Plane/Quad)
├── NavigationVertex (children)
├── FloatingIcon (children)
└── Room (children)
```

**Script Fields:**
| Field | Type | Required |
|-------|------|----------|
| floor | int | Manual |
| floorDisplayedText | string | Manual |
| segmentMesh | GameObject | Manual |
| arWorldBase | Transform | **Manual - drag ARWorldManager** |
| showInMinimap | bool | Manual |
| showMeshInAR | bool | Manual |

**Children (auto-detected):**
- NavigationVertex components in children
- FloatingIcon components in children
- Room components in children

---

#### NavigationVertex
```
NavigationVertex GameObject
├── NavigationVertex.cs (script)
├── Transform
└── SphereCollider (auto-created)
```

**Script Fields:**
| Field | Type | Required |
|-------|------|----------|
| vertexId | string | Manual |
| displayName | string | Manual |
| floor | int | Auto-set by parent MapSegment |
| bilateralConnections | List<NavigationVertex> | Manual |
| lateralConnections | List<NavigationVertex> | Manual |

**Auto-created Components:**
| Component | Setting | Value |
|-----------|---------|-------|
| SphereCollider | Radius | 0.15 |
| SphereCollider | isTrigger | true |

---

#### FloatingIcon
```
FloatingIcon GameObject
├── FloatingIcon.cs (script)
└── Transform
```

**Script Fields:**
| Field | Type | Required |
|-------|------|----------|
| iconMaterial | Material | Manual |
| iconText | string | Manual |
| poiName | string | Manual |
| faceCamera | bool | Manual |
| iconScale | Vector3 | Manual |
| showTextLabel | bool | Manual |

**Optional Components:**
- Renderer (auto-created if null - uses Quad)
- TMPro.TextMeshPro for label

---

#### Room
```
Room GameObject
├── Room.cs (script)
├── Transform
└── BoxCollider (auto-created)
```

**Script Fields:**
| Field | Type | Required |
|-------|------|----------|
| roomTitle | string | Manual |
| floorText | string | Manual |
| roomType | RoomType (enum) | Manual |
| size | Vector2 | Manual (or auto-calculate) |
| height | float | Manual |
| mapSegment | Transform | Optional |

**Auto-created Components:**
| Component | Setting | Value |
|-----------|---------|-------|
| BoxCollider | isTrigger | true |

---

#### Marker (Prefab or Instance)
```
Marker GameObject
├── Marker.cs (script)
└── Renderer (auto-created if null)
```

**Script Fields:**
| Field | Type | Required |
|-------|------|----------|
| markerName | string | Manual |
| markerMaterial | Material | Optional |
| markerColor | Color | Manual |
| markerScale | Vector3 | Manual |

**Auto-created Components:**
| Component | Setting | Value |
|-----------|---------|-------|
| Renderer | (creates Cube) | if no mesh |
| Collider | (creates BoxCollider) | if no collider |

---

### Summary: Minimum Components Per GameObject

| GameObject | Min Unity Components | Min Scripts |
|------------|---------------------|-------------|
| XROrigin | Transform | - |
| + ARRaycastManager | 1 |
| + ARPlaneManager | 1 |
| + ARAnchorManager | 1 |
| ARSession | ARSession | 1 |
| ARSessionManager | - | 1 |
| ARPlayerController | - | 1 |
| ARWorldManager | - | 1 |
| PathfindingManager | - | 1 |
| PathVisualizer | LineRenderer* | 1 |
| MinimapRenderer | - | 1 |
| MinimapCamera | Camera, MinimapCamera | 2 |
| MapSegment | - | 1 |
| NavigationVertex | SphereCollider* | 1 |
| FloatingIcon | - | 1 |
| Room | BoxCollider* | 1 |
| Marker | - | 1 |

*Auto-created if null

---

## Scene Hierarchy Overview

```
AR Navigation Scene
├── XROrigin (ARSessionOrigin in AR Foundation 5.x, XROrigin in 6.x)
│   ├── Camera Offset
│   │   └── AR Camera (Main Camera with ARCameraManager)
│   ├── AR Plane Manager (detects planes)
│   ├── AR Raycast Manager (hit testing)
│   └── AR Anchor Manager (anchors)
├── AR Session (ARSession)
├── Directional Light
├── ARSessionManager (GameObject with script)
├── ARPlayerController (GameObject with script)
├── ARWorldManager (GameObject with script)
├── Navigation
│   ├── PathfindingManager (GameObject with script)
│   ├── PathVisualizer (GameObject with script)
│   └── MinimapRenderer (GameObject with script)
│       └── MinimapCamera (Camera with MinimapCamera component)
│   └── Minimap UI (in Canvas)
├── World Mapping
│   ├── ARWorldManager
│   ├── MapSegment (multiple GameObjects)
│   │   ├── NavigationVertex (children)
│   │   └── FloatingIcon (children)
│   └── Room (multiple GameObjects)
└── UI
    └── Canvas
        └── [Your UI prefabs]
```

> **Notes:** 
> - `XROrigin` in AR Foundation 6.x (requires `com.unity.xr.coreutils` package), or `ARSessionOrigin` in 5.x
> - `NavigationConfig` is a **ScriptableObject**, not a GameObject - see [Section 4](#4-navigationconfig-scriptableobject) for creation instructions

---

## Core GameObjects and Scripts

### 1. ARSessionManager

**GameObject Name:** `ARSessionManager`

**Parent:** Root (Scene)

**Scripts to Add:**
- `ARSessionManager.cs`

**Components Required:**
- None (script handles component creation)

**Inspector Assignments:**
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| arSession | ARSession | AR Session component | Auto-find |
| xrOrigin | ARSessionOrigin | XR Origin (XROrigin in 6.x) | Auto-find |
| detectHorizontalPlanes | bool | Enable horizontal plane detection | Yes (default: true) |
| detectVerticalPlanes | bool | Enable vertical plane detection | Yes (default: true) |
| autoStartPlaneDetection | bool | Start plane detection on AR start | Yes (default: true) |
| referenceImageLibrary | XRReferenceImageLibrary | QR code/image tracking library | No |

**Child GameObjects:** None required

---

### 2. ARPlayerController

**GameObject Name:** `ARPlayerController`

**Parent:** Root (Scene) or under XROrigin

**Scripts to Add:**
- `ARPlayerController.cs`

**Components Required:**
- None (script handles component creation)

**Inspector Assignments:**
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| arSessionOrigin | Transform | XR Origin (XROrigin/ARSessionOrigin) transform | Auto-find |
| arCamera | Transform | AR Camera transform | Auto-find |
| movementSpeed | float | Player movement speed | Yes (default: 1.0) |
| heightOffset | float | Height offset above planes | Yes (default: 0.0) |
| markerPrefab | GameObject | Prefab for marker placement | **See below** |
| enableMarkerPlacement | bool | Enable tap-to-place markers | Yes (default: true) |
| **currentDestination** | NavigationVertex | Current navigation target | **See below** |
| destinationThreshold | float | Distance to consider destination reached | Yes (default: 0.5) |

**Child GameObjects:** None required

#### Marker Prefab

The **Marker Prefab** is used when users tap on AR planes to place markers. The script has built-in fallback - if no prefab is assigned, it will auto-create a basic cube marker.

**Recommended Prefab Setup:**
```
MarkerPrefab (GameObject)
├── Marker (script)
└── [Mesh Filter + Mesh Renderer]
```

**Minimal Setup (auto-created if prefab is null):**
- The script will auto-create a Cube primitive
- Default scale: 0.2 x 0.2 x 0.2
- Default color: Red

**Custom Prefab:**
1. Create a new GameObject (or use any mesh)
2. Add the `Marker.cs` script
3. Optionally add a Collider (BoxCollider, SphereCollider, or MeshCollider)
4. Make it a prefab by dragging to Assets folder
5. Assign to ARPlayerController's **Marker Prefab** field

**Prefab Settings:**
| Setting | Recommended Value |
|---------|------------------|
| Mesh | Sphere, Cube, or custom pin model |
| Material | URP/Unlit with bright color (red, blue, yellow) |
| Scale | 0.1 - 0.3 meters |
| Collider | BoxCollider (for touch detection) |

---

#### Current Destination

The **Current Destination** field is used for navigation - it marks where the player is trying to go.

| Field | Value |
|-------|-------|
| Type | NavigationVertex |
| Required | **No** (set at runtime) |

**How it works:**
- Leave this **EMPTY** in the inspector
- It's set programmatically by your navigation UI or PathfindingManager
- When the player gets within **Destination Threshold** distance (default 0.5m), the destination is considered "reached"

**How to set it:**
```csharp
// From any script:
ARPlayerController.Instance.SetDestination(someNavigationVertex);
```

**What happens:**
1. Set destination via `SetDestination(vertex)`
2. Player moves toward that vertex
3. When within 0.5m (or destinationThreshold), triggers `onDestinationReached` event
4. Destination is cleared automatically

---

### 3. ARWorldManager

**GameObject Name:** `ARWorldManager`

**Parent:** Root (Scene)

**Scripts to Add:**
- `ARWorldManager.cs`

**Inspector Assignments:**
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| worldTrueNorthAngle | float (0-360) | Rotation relative to true north | Yes (default: 0) |
| navigationHeightOffset | float | Height offset for nav elements | Yes (default: 0.1) |
| mapSegments | List<MapSegment> | All map segments in world | No |
| arSessionManager | ARSessionManager | Reference to session manager | Auto-find |
| trackedImageManager | ARTrackedImageManager | Image tracking manager | Auto-find |
| enableImageTracking | bool | Enable QR code detection | Yes (default: true) |

**Child GameObjects:**
- MapSegment GameObjects (add as children, assign to mapSegments list)
- Room GameObjects (optional)

---

### 4. NavigationConfig (ScriptableObject)

**Note:** NavigationConfig is a ScriptableObject, not a MonoBehaviour. It is created as an asset file (.asset), not added to a GameObject.

**How to Create:**
1. In Unity Editor, go to: **Assets > Create > AR Navigation > Configuration**
2. This creates a new .asset file (e.g., `NavigationConfig.asset`)
3. Configure the settings in the Inspector
4. Reference this asset in other scripts that need it

**Inspector Assignments:** (on the .asset file)
| Field | Type | Description | Default |
|-------|------|-------------|----------|
| pathBuildingType | SearchType | Pathfinding algorithm | Regular |
| initialPositions | List<InitialPosition> | Initial user positions | Empty |
| quickDestinations | List<QuickDestinationButton> | Quick destination buttons | Empty |
| showDebugByDefault | bool | Show debug mode | false |
| destinationThreshold | float | Distance to reach destination | 0.5 |
| minimapZoom | float | Minimap zoom level | 1.0 |
| navigationHeightOffset | float | Height above planes | 0.1 |
| autoStartAR | bool | Auto-start AR on launch | true |
| enableQRCodeScanning | bool | Enable QR scanning | true |
| defaultMarkerColor | Color | Default marker color | Red |
| maxCustomMarkers | int | Max custom markers | 10 |

**Used By:** Scripts that reference this via `[Tooltip("Configuration asset")] public NavigationConfig config;`

---

### 5. PathfindingManager

**GameObject Name:** `PathfindingManager`

**Parent:** Root (Scene) or under Navigation folder

**Scripts to Add:**
- `PathfindingManager.cs`

**Inspector Assignments:**
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| arWorldManager | ARWorldManager | Reference to world manager | Auto-find |
| maxPathLength | int | Maximum vertices in path | Yes (default: 100) |
| enableDynamicPathfinding | bool | Recalculate paths dynamically | Yes (default: true) |

---

### 6. PathVisualizer

**GameObject Name:** `PathVisualizer`

**Parent:** Root (Scene) or under Navigation folder

**Scripts to Add:**
- `PathVisualizer.cs`

**Inspector Assignments:**
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| lineRenderer | LineRenderer | Line renderer for path | **See below** |
| lineMaterial | Material | Material for path line | Optional |
| lineColor | Color | Path line color | Yes (Blue) |
| lineWidth | float | Width of path line | Yes (0.05) |
| arrowPrefab | GameObject | Prefab for direction arrows | Optional |
| arrowSpacing | float | Distance between arrows | Yes (0.5) |
| showLine | bool | Show path line | Yes (true) |
| showArrows | bool | Show direction arrows | Yes (true) |
| arrowScale | float | Size of arrows | Yes (0.2) |
| animateArrows | bool | Animate arrows along path | Yes (true) |
| animationSpeed | float | Arrow animation speed | Yes (1.0) |

---

#### Line Renderer Setup

**Option 1: Auto-Created (Recommended)**
- Leave **Line Renderer** field empty
- The script will auto-create a LineRenderer component

**Option 2: Manual Setup**
1. Add a LineRenderer component to the PathVisualizer GameObject
2. Configure:
| Setting | Value |
|---------|-------|
| Use World Space | Checked |
| Width | 0.05 |
| Material | Assign Line Material |
| Color | Blue |

---

#### Line Material Setup

**Option 1: Default (No setup needed)**
- Leave **Line Material** empty
- Script uses "Sprites/Default" shader automatically

**Option 2: Custom Material**
1. Create new Material: **Assets > Create > Material**
2. Name: "PathLineMaterial"
3. Shader: **Universal Render Pipeline/Unlit** or **Sprites/Default**
4. Color: Blue or desired path color
5. Assign to **Line Material** field

---

#### Arrow Prefab Setup

**Option 1: No Arrows (Simplest)**
- Leave **Arrow Prefab** empty
- Set **Show Arrows** to false
- Only the line will be displayed

**Option 2: Custom Arrow Prefab**
1. Create a GameObject (or use existing model)
2. Recommended: Simple 3D arrow mesh pointing +Z forward
3. Make it a prefab: drag to Assets folder
4. Assign to **Arrow Prefab** field

**Prefab Requirements:**
| Setting | Value |
|---------|-------|
| Mesh | Arrow/triangle pointing forward (+Z) |
| Material | URP Unlit with bright color |
| Scale | ~0.2 meters |
| Rotation | Default rotation = forward |

---

#### PathVisualizer Full Inspector Settings

| Section | Field | Recommended Value |
|---------|-------|------------------|
| Line Renderer | Line Renderer | (leave empty for auto-create) |
| | Line Material | Custom or empty for default |
| | Line Color | Blue (#0000FF) |
| | Line Width | 0.05 |
| Arrow Prefab | Arrow Prefab | Your arrow prefab or empty |
| | Arrow Spacing | 0.5 |
| Path Settings | Show Line | Checked |
| | Show Arrows | Checked |
| | Arrow Scale | 0.2 |
| Animation | Animate Arrows | Checked |
| | Animation Speed | 1.0 |

---

### 7. MinimapRenderer

**GameObject Name:** `MinimapRenderer`

**Parent:** Root (Scene) or under Navigation folder

**Scripts to Add:**
- `MinimapRenderer.cs`

**Inspector Assignments:**
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| minimapCamera | Camera | Orthographic camera for minimap | Yes |
| minimapDisplay | RawImage | UI RawImage to display minimap | Yes |
| minimapTexture | RenderTexture | Render texture (auto-created) | Auto |
| textureSize | Vector2Int | Texture resolution | Yes (512x512) |
| playerIndicator | Transform | Player position marker | No |
| rotationIndicator | Transform | Rotation arrow marker | No |
| zoom | float | Minimap zoom level | Yes (1.0) |
| followPlayer | bool | Camera follows player | Yes (true) |
| rotateWithPlayer | bool | Rotate minimap with player | Yes (false) |
| useCompassHeading | bool | Use device compass | Yes (true) |
| mapCenter | Vector3 | Center of map in world | Yes |
| mapSize | Vector2 | Size of map area | Yes (80x60) |

**Child GameObjects:**
- `MinimapCamera` (Camera with MinimapCamera component)

---

### 7b. MinimapCamera (Child of MinimapRenderer)

**GameObject Name:** `MinimapCamera`

**Parent:** MinimapRenderer

**Components to Add:**
- Camera (set to Orthographic)
- `MinimapCamera` (script - acts as tag)

**Camera Settings:**
| Setting | Value |
|---------|-------|
| Projection | Orthographic |
| Size | 40 (adjust based on mapSize) |
| Position | (mapCenter.x, 50, mapCenter.z) |
| Rotation | (90, 0, 0) - looking down |
| Clear Flags | Solid Color |
| Background | Transparent (0,0,0,0) |
| Culling Mask | Everything except UI |

---

### 7c. Minimap UI Setup

Create a UI Canvas with the following hierarchy:

```
Canvas
└── MinimapContainer (RectTransform)
    └── MinimapDisplay (RawImage)
        ├── PlayerIndicator (Image) - centered
        └── RotationIndicator (Image) - arrow
```

**MinimapDisplay Settings:**
| Setting | Value |
|---------|-------|
| Source Image | None (uses texture) |
| Color | White |
| Width/Height | 200x200 (or desired size) |

---

### 7d. Player Indicator Setup

The Player Indicator shows the player's position on the minimap.

**Hierarchy:**
```
MinimapDisplay (RawImage)
└── PlayerIndicator (Image)
```

**PlayerIndicator Settings:**
| Setting | Value |
|---------|-------|
| RectTransform Position | (0, 0, 0) - center of RawImage |
| Width/Height | 20x20 |
| Image Type | Simple |
| Image Source | Circle sprite or triangle |
| Color | Blue or user-chosen |

**In MinimapRenderer Inspector:**
- Drag `PlayerIndicator` GameObject to **Player Indicator** field

---

### 7e. Rotation Indicator Setup

The Rotation Indicator shows the player's facing direction on the minimap.

**Hierarchy:**
```
MinimapDisplay (RawImage)
├── PlayerIndicator (Image)
└── RotationIndicator (Image)
```

**RotationIndicator Settings:**
| Setting | Value |
|---------|-------|
| RectTransform Position | (0, 0, 0) - same position as player |
| Width/Height | 30x15 |
| Pivot | (0.5, 0) - bottom center (so it rotates from center-bottom) |
| Image Type | Simple |
| Image Source | Arrow/triangle sprite pointing up |
| Color | Yellow or user-chosen |

**In MinimapRenderer Inspector:**
- Drag `RotationIndicator` GameObject to **Rotation Indicator** field

---

### 7f. MinimapRenderer Inspector Full Setup

**Components Section:**
| Field | Value |
|-------|-------|
| Minimap Camera | Drag MinimapCamera GameObject |
| Minimap Display | Drag MinimapDisplay (RawImage) GameObject |
| **Minimap Texture** | **Leave EMPTY - auto-creates at runtime** |

> **Note:** The Minimap Texture field can be left empty. The script will automatically create a RenderTexture at runtime using the Texture Size values. If you want a custom RenderTexture, create one in Assets (right-click > Create > Render Texture) and assign it here.

**Render Texture Section:**
| Field | Value |
|-------|-------|
| Texture Size | 512x512 |

**Player Indicator:**
| Field | Value |
|-------|-------|
| Player Indicator | Drag PlayerIndicator GameObject |
| Rotation Indicator | Drag RotationIndicator GameObject |

**Settings:**
| Field | Value |
|-------|-------|
| Zoom | 1.0 |
| Follow Player | Checked |
| Rotate with Player | Unchecked (map stays north-up) |
| Use Compass Heading | Checked |

**Map Bounds:**
| Field | Value |
|-------|-------|
| Map Center | Set to world center of your building |
| Map Size | (80, 60) or adjust to fit your building |

---

### 8. MapSegment

**GameObject Name:** `MapSegment_FirstFloor` (example)

**Parent:** `World Mapping` folder (empty GameObject for organization)

**Scripts to Add:**
- `MapSegment.cs`

**What is a MapSegment?**

A **MapSegment** is a container component that represents an **entire floor** of a building. It groups together:
- The visual floor plan (segmentMesh)
- All navigation waypoints on that floor (NavigationVertex children)
- Points of interest icons (FloatingIcon children)
- Rooms on that floor (Room children)

**Structure:**
```
MapSegment (one per floor)
├── segmentMesh (3D floor plan model - Plane, Quad, or custom mesh)
├── NavigationVertex[] (waypoints on this floor)
├── FloatingIcon[] (points of interest - canteen, stairs icons)
└── Room[] (rooms on this floor)
```

**Inspector Assignments:**
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| floor | int | Floor number (0, 1, 2...) | Yes |
| floorDisplayedText | string | "First Floor", "Ground Floor" | Yes |
| segmentMesh | GameObject | 3D model of the floor plan | **See below** |
| segmentOffset | Vector2 | Position offset of mesh | No |
| segmentHeight | float | Height of the floor mesh | Yes (default: 1.0) |
| segmentScale | float | Scale multiplier | Yes (default: 1.0) |
| arWorldBase | Transform | Parent ARWorldManager transform | **See below** |
| showInMinimap | bool | Show in minimap | Yes (default: true) |
| showMeshInAR | bool | Show mesh in AR view | Yes (default: true) |

---

#### 8b. segmentMesh - What It Is and How to Set Up

The `segmentMesh` is the **visual floor plan** - the 3D representation of the floor that users see in AR and on the minimap.

**NOT just a box collider** - it should be a visual mesh with a floor plan texture/image.

**Recommended Setup:**

1. **Create a Plane or Quad:**
   - Right-click in Hierarchy > 3D Object > Plane (or Quad)
   - Scale it to match your building dimensions

2. **Apply Floor Plan Texture:**
   - Create a Material with the floor plan image
   - Assign to the Plane's Mesh Renderer
   - Use "Unlit/Texture" or "URP/Unlit" shader for best results

3. **Assign to MapSegment:**
   - Drag the Plane GameObject to the **segmentMesh** field

**Alternative - Import a 3D Model:**
- Import a .fbx or .obj of your floor plan
- Assign to segmentMesh
- Adjust segmentOffset, segmentHeight, and segmentScale as needed

**segmentMesh Inspector Fields:**

| Field | Purpose |
|-------|---------|
| segmentOffset | X,Y offset of mesh relative to segment origin |
| segmentHeight | Height of the mesh (usually 1.0 for flat planes) |
| segmentScale | Overall scale multiplier |

---

#### 8c. arWorldBase - Connecting to ARWorldManager

Since MapSegment is under "World Mapping" (not as a child of ARWorldManager), you need to manually link them.

**The `arWorldBase` field** is a reference to the ARWorldManager GameObject - it connects the MapSegment to the world manager.

**How to Set:**
1. Select the MapSegment GameObject
2. In the Inspector, find the **arWorldBase** field
3. Drag the **ARWorldManager** GameObject to this field

**Why This Matters:**
| Feature | Without arWorldBase | With arWorldBase |
|---------|-------------------|------------------|
| Auto-registration | Won't register | Registers on Start |
| True north alignment | Won't work | Works via `AlignToTrueNorth()` |
| World offset | Won't apply | All segments offset together |
| Minimap display | May not show | Properly tracked |

---

#### 8d. MapSegment Example Hierarchy

```
World Mapping
├── ARWorldManager
├── MapSegment_Floor0  ← arWorldBase = ARWorldManager
│   ├── segmentMesh: FloorPlan_F0 (Plane with floor plan texture)
│   ├── NavigationVertex_MainDoor
│   │   └── FloatingIcon_DoorIcon
│   ├── NavigationVertex_Canteen
│   │   └── FloatingIcon_CanteenIcon
│   └── Room_Classroom101
└── MapSegment_Floor1  ← arWorldBase = ARWorldManager
    ├── segmentMesh: FloorPlan_F1
    ├── NavigationVertex_Stairs_Top
    └── Room_Classroom201
```

---

### 9. NavigationVertex

**GameObject Name:** `Vertex_Entrance` (example)

**Parent:** MapSegment

**Scripts to Add:**
- `NavigationVertex.cs`

**Inspector Assignments:**
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| vertexId | string | Unique identifier | Yes |
| vertexType | VertexType enum | Type: Entry, Exit, Junction, etc. | Yes |
| connectedVertices | List<NavigationVertex> | Adjacent vertices | No |
| floor | int | Floor number | Yes (auto-set by parent) |

**Child GameObjects:** None required

---

### 9b. What is a NavigationVertex?

A **NavigationVertex** represents a single **waypoint or node** in the navigation graph for indoor AR navigation. Think of it like a GPS waypoint - it's a point on the floor where a person can stand and navigate to other points.

#### Key Properties

| Property | Purpose |
|----------|---------|
| **vertexId** | Unique identifier for searching (e.g., "canteen", "stairs_f1") |
| **displayName** | Human-readable name shown in UI |
| **floor** | Which floor this vertex is on (0 = ground, 1 = first floor, etc.) |
| **bilateralConnections** | Bidirectional paths (corridors, stairs) - can go both ways |
| **lateralConnections** | Unidirectional paths (escalators, turnstiles) - one way only |

#### Visual Representation

```
    [Vertex A] -------- [Vertex B] -------- [Vertex C]
    (Floor 0)           (Floor 0)           (Floor 1)
         |                                        |
    [Vertex D]                               [Vertex E]
    (Floor 0)                                (Floor 1)
    
    Green lines = bilateral connections (bidirectional)
    Yellow lines = lateral connections (one-way)
```

#### Usage in Pathfinding

The `PathfindingManager` uses A* algorithm to find routes between vertices:

- **gScore** - Cost from start to this vertex
- **hScore** - Heuristic (estimated distance to destination)
- **parentVertex** - Previous vertex in the optimal path

#### Inspector Fields

| Field | Description |
|-------|-------------|
| Vertex ID | Unique ID (e.g., "canteen_1") |
| Is Unique | Whether ID is unique across entire map |
| Display Name | UI-friendly name (e.g., "Main Canteen") |
| Bilateral Connections | Two-way paths (corridors) |
| Lateral Connections | One-way paths (escalators) |
| Floor | Floor number (0, 1, 2...) |
| Show Debug Axis | Visualize axes in Scene view |
| Vertex Color | Color of sphere in Scene view |

---

### 9c. Where to Place NavigationVertices

NavigationVertex GameObjects are added to your **Unity scene** (not a prefab) - they represent physical locations in the real AR space.

#### Placement in Scene Hierarchy

```
Hierarchy
├── ARWorldManager
│   ├── MapSegment_Floor0
│   │   ├── NavigationVertex_Canteen
│   │   ├── NavigationVertex_MainDoor
│   │   ├── NavigationVertex_Stairs
│   │   └── FloatingIcon_CanteenIcon
│   └── MapSegment_Floor1
│       ├── NavigationVertex_Stairs_Top
│       └── NavigationVertex_Classroom101
```

#### Positioning Tips

| Guideline | Value |
|-----------|-------|
| Y Position | Floor level (0 or real-world height) |
| Spacing | Every 2-5 meters along paths |
| Decision Points | Add at turns, intersections |
| Connections | Use bilateralConnections for hallways |
| One-way | Use lateralConnections for escalators |

#### Example Vertex Placements

| Location Type | Example vertexId |
|---------------|------------------|
| Entrances | "main_entrance", "side_door" |
| Hallway junctions | "junction_1", "corridor_center" |
| Stairs/escalators | "stairs_0_1", "escalator_up" |
| Room doors | "classroom_101_door", "canteen_door" |
| Elevators | "elevator_floor_0", "elevator_floor_1" |

---

### 10. FloatingIcon

**GameObject Name:** `Icon_Stairs` (example)

**Parent:** MapSegment or NavigationVertex

**Scripts to Add:**
- `FloatingIcon.cs`

**Inspector Assignments:**
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| iconType | IconType enum | Type: Stairs, Elevator, Exit, etc. | Yes |
| iconMesh | Mesh | 3D mesh for icon | Yes |
| iconMaterial | Material | Material for icon | Yes |
| bobSpeed | float | Animation speed | Yes (default: 1.0) |
| bobAmplitude | float | Animation amplitude | Yes (default: 0.1) |
| alwaysFaceCamera | bool | Billboard rotation | Yes (default: true) |

**Child GameObjects:** None required

---

### 11. Marker

**GameObject Name:** Custom marker instances

**Parent:** Root or under Navigation

**Scripts to Add:**
- `Marker.cs`

**Inspector Assignments:**
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| markerName | string | Name of marker | Yes |
| markerType | MarkerType enum | Type of marker | Yes |
| associatedVertex | NavigationVertex | Linked vertex (optional) | No |

---

## Mobile AR Template Integration

### Available Prefabs

#### /Assets/MobileARTemplateAssets/Prefabs/

| Prefab | Use Case | Recommended Integration |
|--------|----------|------------------------|
| ARFeatheredOcclusionPlane | Occlusion for AR planes | Add as child of ARSessionOrigin |
| ArchVariant | AR placement test object | Use for testing |
| CubeVariant | Simple AR object | Use for testing |
| CylinderVariant | Simple AR object | Use for testing |
| DebugCubeVariant | Debug visualization | Development only |
| PyramidVariant | Simple AR object | Use for testing |
| TorusVariant | Simple AR object | Use for testing |
| WedgeVariant | Simple AR object | Use for testing |

#### /Assets/MobileARTemplateAssets/UI/Prefabs/

| Prefab | Use Case | Recommended Integration |
|--------|----------|------------------------|
| GreetingCTA | Initial greeting screen | Add to Canvas |
| Prompt_InputHints | Input hints display | Add to Canvas |
| Prompt_MoveObject | Move object prompt | Add to Canvas |
| Prompt_RotateObject | Rotate object prompt | Add to Canvas |
| Prompt_ScaleObject | Scale object prompt | Add to Canvas |
| Prompt_ScanSurfaces | Surface scanning prompt | **Recommended: Add to Canvas** |
| Prompt_TapToPlace | Tap to place prompt | **Recommended: Add to Canvas** |

### Recommended Integration

1. **AR Plane Visualization:**
   - Add `ARFeatheredOcclusionPlane` prefab to scene
   - The ARPlaneManager will auto-generate plane visualizations

2. **UI Prompts:**
   - Add `Prompt_ScanSurfaces` to your main Canvas for AR guidance
   - Add `Prompt_TapToPlace` when marker placement is enabled
   - Customize or replace with your own UI

3. **Testing Objects:**
   - Keep `CubeVariant` or other simple shapes for testing AR placement
   - Remove before production build

---

## Unity Editor Settings

### Project Settings > Player

#### Android Tab
| Setting | Value | Notes |
|---------|-------|-------|
| Minimum API Level | Android 6.0 (API 23)+ | ARCore requirement |
| Target API Level | Android 12L (API 32)+ | Recommended |
| Architecture | ARM64 | Required for ARCore |
| Graphics API | OpenGLES3 or Vulkan | Vulkan recommended |
| Install Location | Automatic | |

#### iOS Tab
| Setting | Value | Notes |
|---------|-------|-------|
| Target Device | iPhone only | ARKit requirement |
| Minimum OS Version | iOS 15.0+ | ARKit 5+ requirement |
| Camera Usage Description | "AR Camera" | Required for AR |
| ARKit Required | Checked | |
| Architecture | ARM64 | |

### Project Settings > XR Plugin Management

#### Android Tab
| Setting | Value |
|---------|-------|
| ARCore | Checked |
| ARKit | Unchecked |

#### iOS Tab
| Setting | Value |
|---------|-------|
| ARCore | Unchecked |
| ARKit | Checked |

> **Note on XROrigin:** In AR Foundation 6.x, `XROrigin` replaces `ARSessionOrigin`. To use XROrigin, you need to install the `com.unity.xr.coreutils` package. The Mobile AR Template in Unity 2022+ uses XROrigin by default.

### Quality Settings

| Setting | Value |
|---------|-------|
| VSync | Every V Blank |
| Anti Aliasing | 2x or 4x |
| Shadow Quality | Low (mobile) |
| Shadow Distance | 20 |

### Editor Settings

| Setting | Value |
|---------|-------|
| Asset Serialization | Force Text |
| Line Endings | OS Native |
| Enter Play Mode Options | Checked (faster iteration) |

---

## AR Foundation Configuration

### Required Components on XROrigin (or ARSessionOrigin)

Add these components to the XROrigin GameObject (or ARSessionOrigin for AR Foundation 5.x):

1. **ARRaycastManager**
   - Enable: Checked
   - No configuration needed

2. **ARPlaneManager**
   - Enable: Checked
   - Plane Prefab: Optional (ARFeatheredOcclusionPlane recommended)
   - Detection Mode: Horizontal + Vertical

3. **ARAnchorManager**
   - Enable: Checked (optional, for persistent anchors)

4. **ARTrackedImageManager** (if using QR codes)
   - Enable: If using image tracking
   - Reference Image Library: Assign your XRReferenceImageLibrary
   - Max Number of Moving Images: 4 (recommended)

### XROrigin vs ARSessionOrigin

| Feature | XROrigin (6.x) | ARSessionOrigin (5.x) |
|---------|----------------|----------------------|
| Package | AR Foundation 6.x | AR Foundation 5.x |
| Additional Package | Unity.XR.CoreUtils | Not required |
| Camera Setup | Camera Offset child | Direct child |
| Transform | Origin + Camera Offset | Direct transform |

> **Recommendation:** For AR Foundation 6.x, install `com.unity.xr.coreutils` and use XROrigin. The Mobile AR Template uses XROrigin by default.

### Camera Configuration

The AR Camera (usually as child of ARSessionOrigin) should have:
- **Clear Flags:** Solid Color (with alpha for AR passthrough)
- **Culling Mask:** Everything except UI
- **Near Clip Plane:** 0.01
- **Far Clip Plane:** 1000

---

## Step-by-Step Setup Checklist

### 1. Scene Setup
- [ ] Create new Scene or use SampleScene
- [ ] Add XROrigin GameObject (or ARSessionOrigin for AR Foundation 5.x)
- [ ] Add ARSession GameObject
- [ ] Configure AR Camera as child of Camera Offset (XROrigin) or directly (ARSessionOrigin)

### 2. Core Scripts
- [ ] Add ARSessionManager to scene
- [ ] Add ARPlayerController to scene
- [ ] Add ARWorldManager to scene

### 3. Navigation System
- [ ] Add PathfindingManager
- [ ] Add PathVisualizer
- [ ] Add MinimapRenderer
  - [ ] Create MinimapCamera child (orthographic camera)
  - [ ] Create Minimap UI Canvas with RawImage
  - [ ] Configure mapCenter and mapSize

### 4. World Building
- [ ] Create MapSegment GameObjects
- [ ] Add NavigationVertex GameObjects as children
- [ ] Connect vertices in PathfindingManager

### 5. UI
- [ ] Add Canvas
- [ ] Add Mobile AR Template UI prefabs (optional)
- [ ] Configure UI to work with ARPlayerController

### 6. Testing
- [ ] Switch to Android/iOS build target
- [ ] Add testing prefab (CubeVariant)
- [ ] Build and test on device

---

## Troubleshooting

### AR Session Not Starting
- Check XR Plugin Management settings
- Verify ARCore/ARKit is installed
- Check Camera Usage Description in Player Settings

### Raycast Not Working
- Ensure ARRaycastManager is on ARSessionOrigin
- Check that planes are being detected
- Verify TrackableType is correct (PlaneWithinPolygon)

### Planes Not Detected
- Enable PlaneDetectionMode on ARPlaneManager
- Check lighting conditions
- Ensure surface has sufficient texture/contrast

---

## Related Documentation

- [API Reference](../API_REFERENCE.md)
- [Graphics Assets](../Graphics/GRAPHICS_ASSETS.md)
- [Migration Plan](../Unity_Migration_Plan.md)
