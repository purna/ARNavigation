# Unity Migration Checklist - AR Navigation

## Pre-Migration Checklist
- [ ] Unity Hub installed (version 3.x+)
- [ ] Unity Editor installed (2021.3 LTS or 2022.3 LTS)
- [ ] Android Studio installed (for Android builds)
- [ ] Xcode installed (for iOS builds, macOS only)
- [ ] Reviewed Unreal Engine project structure
- [ ] Reviewed ARDocs.pdf documentation

---

## Phase 1: Project Setup ✅ COMPLETED

### 1.1 Project Created ✅
- [x] Unity project folder structure created
- [x] Package manifest with AR Foundation dependencies
- [x] Project settings configured
- [x] XR Plugin settings configured

### 1.2 Package Verification
- [ ] Open project in Unity
- [ ] Wait for package import to complete
- [ ] Verify XR Plugin Management installed
- [ ] Verify ARCore XR Plugin installed
- [ ] Verify ARKit XR Plugin installed
- [ ] Verify Universal RP installed

### 1.3 XR Configuration
- [ ] Configure XR Plug-in Management (Android tab)
- [ ] Configure XR Plug-in Management (iOS tab)
- [ ] Set Android minimum SDK to API 21
- [ ] Set iOS deployment target to 12.0

---

## Phase 2: Core AR Foundation Implementation

### 2.1 Scene Setup
- [ ] Create main scene `Assets/Scenes/ARNavigation.unity`
- [ ] Add AR Session Origin to scene
- [ ] Add AR Session component to AR Session Origin
- [ ] Configure AR Camera with XROrigin
- [ ] Set up Directional Light
- [ ] Configure AR Session (ARCore/ARKit)

### 2.2 AR Components Required
| Component | Script Name | Priority |
|-----------|-------------|----------|
| AR Session | ARSession | Required |
| AR Session Origin | ARSessionOrigin | Required |
| AR Camera | ARCameraBackground | Required |
| Plane Detection | ARPlaneManager | Required |
| Raycast Manager | ARRaycastManager | Required |
| Image Tracking | ARTrackedImageManager | Optional |
| Anchors | ARAnchorManager | Optional |

### 2.3 Camera Setup Checklist
- [ ] Position: (0, 0, 0) relative to ARSessionOrigin
- [ ] Clear Flags: Solid Color (for AR passthrough)
- [ ] Background: Black (000000)
- [ ] Culling Mask: Everything
- [ ] Add ARCameraBackground component
- [ ] Add ARCameraManager component

---

## Phase 3: World Mapping System

### 3.1 Core Scripts Required
| Script | File | Purpose |
|--------|------|----------|
| ARWorldManager | `Scripts/Core/ARWorldManager.cs` | Root container for all segments |
| MapSegment | `Scripts/WorldMapping/MapSegment.cs` | Floor/area segment |
| NavigationVertex | `Scripts/WorldMapping/NavigationVertex.cs` | Graph nodes |
| FloatingIcon | `Scripts/WorldMapping/FloatingIcon.cs` | POI markers |
| Room | `Scripts/WorldMapping/Room.cs` | Room triggers |

### 3.2 ARWorldManager Implementation
- [ ] Create ARWorldManager.cs script
- [ ] Add WorldTrueNorthAngle property (float)
- [ ] Add MapSegments array (List<MapSegment>)
- [ ] Add Reference Image Library reference
- [ ] Implement segment rotation on start

### 3.3 MapSegment Implementation
- [ ] Create MapSegment.cs script
- [ ] Add Floor property (int)
- [ ] Add FloorDisplayedText property (string)
- [ ] Add SegmentMesh property (GameObject)
- [ ] Add SegmentOffset property (Vector2)
- [ ] Add SegmentHeight property (float)
- [ ] Add SegmentScale property (float)
- [ ] Add NavigationVertex children list

### 3.4 NavigationVertex Implementation
- [ ] Create NavigationVertex.cs script
- [ ] Add VertexID property (string)
- [ ] Add IsUnique property (bool)
- [ ] Add BilateralConnections (List<NavigationVertex>)
- [ ] Add LateralConnections (List<NavigationVertex>)
- [ ] Add Floor property (int)
- [ ] Add gScore, hScore, fScore (for A*)
- [ ] Add Parent vertex reference

### 3.5 FloatingIcon Implementation
- [ ] Create FloatingIcon.cs script
- [ ] Add IconMaterial property (Material)
- [ ] Add Text property (string)
- [ ] Implement billboard effect (face camera)
- [ ] Add WorldToScreenPosition logic

### 3.6 Room Implementation
- [ ] Create Room.cs script
- [ ] Add RoomTitle property (string)
- [ ] Add Floor property (string)
- [ ] Add RoomType enum (Room, Corridor, Staircase, Other)
- [ ] Add Size property (Vector2)
- [ ] Add Height property (float)
- [ ] Add BoxCollider (Trigger)
- [ ] Implement OnTriggerEnter/Exit for location tracking

---

## Phase 4: Pathfinding System

### 4.1 PathfindingManager Implementation
- [ ] Create PathfindingManager.cs (Singleton)
- [ ] Implement Dijkstra algorithm
- [ ] Implement FastSearch algorithm (tree-based)
- [ ] Add SearchType enum (Regular, Fast)
- [ ] Implement FindPath(startVertex, targetVertex) method

### 4.2 Algorithm Requirements
| Algorithm | Use Case | Implementation |
|-----------|----------|----------------|
| Dijkstra | Graphs (multiple paths) | Visit all vertices |
| Fast | Trees (single path) | Stop when target found |

### 4.3 Path Visualization
- [ ] Create PathArrow prefab
- [ ] Add Arrow mesh to prefab
- [ ] Add material with arrow texture
- [ ] Implement arrow spawning along path
- [ ] Add LineRenderer for path line
- [ ] Configure arrow spacing property

---

## Phase 5: Minimap System

### 5.1 Minimap Components
| Component | Script | Purpose |
|-----------|--------|---------|
| Minimap Camera | MinimapCamera.cs | Orthographic top-down view |
| Minimap Renderer | MinimapRenderer.cs | Render to texture |
| Player Indicator | MinimapPlayerIndicator.cs | Shows position/rotation |

### 5.2 Minimap Setup
- [ ] Create separate Camera (Orthographic)
- [ ] Set Clear Flags to Depth Only
- [ ] Set Culling Mask to Minimap layer
- [ ] Create RenderTexture (512x512 or 1024x1024)
- [ ] Create RawImage in UI to display texture
- [ ] Position camera above map center

### 5.3 Minimap Player Tracking
- [ ] Get player position from ARSessionOrigin
- [ ] Get player rotation from ARSessionOrigin
- [ ] Update minimap camera position
- [ ] Update player indicator rotation
- [ ] Implement compass heading adjustment

---

## Phase 6: UI System

### 6.1 Required UI Screens
| Screen | Canvas Element | Purpose |
|--------|----------------|---------|
| Start Screen | Panel | App launch/welcome |
| Main UI | Panel | Navigation controls |
| QR Scanner | Panel | Scan reference image |
| Manual Input | Panel | Enter vertex ID |
| Place Marker | Panel | Place custom markers |
| Instructions | Panel | How to use guide |
| Unsupported | Panel | Device not supported |

### 6.2 Main UI Components
- [ ] Destination dropdown/list
- [ ] Quick destination buttons
- [ ] Current destination display
- [ ] Distance remaining text
- [ ] Debug toggle button
- [ ] Minimap display area

### 6.3 UI Scripts Required
- [ ] UIManager.cs - Main UI controller
- [ ] QRCodeScanner.cs - Reference image scanning
- [ ] ManualInputHandler.cs - Vertex ID input
- [ ] MarkerPlacer.cs - Custom marker placement

---

## Phase 7: Configuration & Data

### 7.1 ScriptableObjects Required
| ScriptableObject | Purpose |
|------------------|---------|
| NavigationConfig | Quick destinations, algorithm type |
| InitialPositionData | Predefined start positions |
| ReferenceImageLibrary | QR code / reference images |

### 7.2 Configuration Properties
- [ ] Path Building Type (Regular/Fast)
- [ ] InitialPositionPage array
- [ ] MainUIText (QuickButtons) array

### 7.3 Data Import
- [ ] Export vertex positions from Unreal
- [ ] Export vertex connections from Unreal
- [ ] Create JSON configuration file
- [ ] Import JSON at runtime or editor time

---

## Phase 8: Materials & Assets

### 8.1 Asset Import
- [ ] Export textures from Unreal (PNG/JPG)
- [ ] Export meshes from Unreal (FBX format)
- [ ] Import to Unity Assets folder

### 8.2 Material Setup (URP)
- [ ] Create URP/Lit materials for meshes
- [ ] Create URP/Unlit materials for icons
- [ ] Create Transparent materials for UI

### 8.3 Required Materials
| Unreal Material | Unity Equivalent |
|-----------------|------------------|
| Arrow_Mat | URP Unlit (Arrow texture) |
| BoxRed/Green/Blue_Mat | URP Lit (Color) |
| FloatingShape_Mat | URP Unlit (Transparent) |
| SegmentFirstFloor_Mat | URP Lit (Map texture) |

---

## Phase 9: Testing & Build

### 9.1 Editor Testing
- [ ] AR Session starts in Editor (simulated)
- [ ] Plane detection works
- [ ] UI responds to input

### 9.2 Android Testing
- [ ] Build APK successfully
- [ ] Install on ARCore device
- [ ] AR session starts
- [ ] Plane detection works
- [ ] Pathfinding calculates path

### 9.3 iOS Testing
- [ ] Build Xcode project
- [ ] Sign with development certificate
- [ ] Install on ARKit device
- [ ] AR session starts
- [ ] Plane detection works

---

## Completion Sign-Off

### Final Checklist
- [ ] All scripts implemented
- [ ] All scenes configured
- [ ] All materials created
- [ ] Android build successful
- [ ] iOS build successful
- [ ] QR code detection works
- [ ] Pathfinding returns optimal path
- [ ] Minimap displays correctly
- [ ] UI navigation works

### Known Issues / Notes
_________________________________________
_________________________________________
_________________________________________

---

## API Reference Quick Lookup

### Essential ARFoundation Classes
```csharp
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.Management;
```

### Key Methods
| Method | Class | Purpose |
|--------|-------|---------|
| Start() | ARSession | Begin AR |
| Stop() | ARSession | End AR |
| Raycast() | ARRaycastManager | Hit test |
| GetPlanes() | ARPlaneManager | Get detected planes |
| GetTrackables() | ARTrackedImageManager | Get tracked images |

---

*Migration Checklist v1.0 - AR Navigation v3.0.0*
