# AR Navigation - Unity Migration Instructions

## Prerequisites

Before starting the migration, ensure you have the following installed:

### Required Software
| Software | Version | Purpose |
|----------|---------|---------|
| Unity Hub | 3.x+ | Unity version management |
| Unity Editor | 2021.3 LTS or 2022.3 LTS | Game engine |
| Xcode | 14.x+ | iOS development (macOS only) |
| Android Studio | 2022.x+ | Android development |
| Visual Studio Code | Latest | Code editor |

### Required Unity Modules (via Unity Hub)
- Android Build Support
- iOS Build Support
- WebGL Build Support (optional, for testing)

---

## Phase 1: Project Setup

### Step 1.1: Open the Unity Project

1. Navigate to `/Users/nigelmorris/Downloads/ARNavigation/Unity/ARNavigation/`
2. Open Unity Hub
3. Click **Add** → Select the `ARNavigation` folder
4. Click the project to open it
5. Wait for Unity to import packages (this may take several minutes)

### Step 1.2: Verify XR Packages Installed

After opening the project, verify packages are installed:

1. Go to **Window** → **Package Manager**
2. Check that the following packages are installed:
   - `XR Plugin Management` (com.unity.xr.management)
   - `ARCore XR Plugin` (com.unity.xr.arcore)
   - `ARKit XR Plugin` (com.unity.xr.arkit)
   - `Universal RP` (com.unity.render-pipelines.universal)

### Step 1.3: Configure XR Settings

1. Go to **Edit** → **Project Settings** → **XR Plug-in Management**
2. For **Android** tab:
   - Check **ARCore XR Plugin**
3. For **iOS** tab:
   - Check **ARKit XR Plugin**

---

## Phase 2: Import AR Foundation Packages

### Step 2.1: Install via Package Manager

The required packages have been pre-configured in `Packages/manifest.json`:

```json
{
  "com.unity.xr.arcore": "5.1.2",
  "com.unity.xr.arkit": "5.1.2",
  "com.unity.xr.management": "4.4.0",
  "com.unity.render-pipelines.universal": "14.0.9"
}
```

To install/update:
1. Open **Window** → **Package Manager**
2. Select **My Assets** (if packages were purchased) or use **Add package from registry**
3. Search for each package and install

### Step 2.2: Verify Package Versions

| Package | Minimum Version | Recommended Version |
|---------|-----------------|---------------------|
| ARFoundation | 5.1.x | 5.1.2 |
| ARCore XR Plugin | 5.1.x | 5.1.2 |
| ARKit XR Plugin | 5.1.x | 5.1.2 |
| XR Management | 4.2.x | 4.4.0 |
| Universal RP | 14.0.x | 14.0.9 |

---

## Phase 3: Project Structure

The Unity project has been created with the following structure:

```
ARNavigation/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # AR Session, Player Controller
│   │   ├── WorldMapping/  # MapSegment, Vertex, Room, FloatingIcon
│   │   ├── Navigation/   # Pathfinding, Arrows
│   │   ├── UI/             # UI Screens and Components
│   │   └── Configuration/  # Settings and Config
│   ├── Materials/         # URP Materials
│   ├── Meshes/            # 3D Models
│   ├── Textures/          # Images and Icons
│   ├── Prefabs/           # Reusable GameObjects
│   ├── Scenes/            # Unity Scenes
│   └── Editor/            # Custom Editors
├── Packages/
│   └── manifest.json      # Package dependencies
└── ProjectSettings/
    ├── ProjectSettings.asset
    └── XRPluginSettings.asset
```

---

## Phase 4: Checklist for Migration

### Core AR Foundation
- [ ] Create AR Session Origin in scene
- [ ] Add AR Camera to AR Session Origin
- [ ] Add AR Session component
- [ ] Configure AR Session Origin for ARCore/ARKit

### World Mapping System
- [x] Create `ARWorldManager` script
- [x] Create `MapSegment` script
- [x] Create `NavigationVertex` script with connection arrays
- [x] Create `FloatingIcon` script with billboard effect
- [x] Create `Room` script with trigger colliders
- [x] Create `ARReferenceImageSetup` script for QR code detection

### Navigation
- [x] Implement Dijkstra pathfinding algorithm
- [x] Implement Fast pathfinding algorithm (tree-based)
- [x] Create `PathfindingManager` singleton
- [x] Create path visualization with LineRenderer
- [x] Create `PathVisualizer` script for arrows
- [x] Create `Marker` script for custom markers
- [ ] Create `PathArrow` prefab for direction indicators

### Minimap
- [x] Create orthographic camera for top-down view
- [x] Set up RenderTexture
- [x] Create `MinimapRenderer` script
- [x] Add player position indicator
- [x] Implement rotation based on compass heading

### UI System
- [ ] Create MainUI Canvas
- [ ] Create Start Screen
- [ ] Create QR Code Scanner Screen
- [ ] Create Manual Input Screen
- [ ] Create Marker Placement UI
- [ ] Implement navigation button logic

### Configuration
- [x] Create ScriptableObject for `NavigationConfig`
- [x] Implement quick destination buttons
- [x] Set up initial position list
- [x] Configure pathfinding algorithm selection

### Materials & Assets
- [x] Create material creation script (`MaterialCreator.cs`)
- [ ] Import textures from Unreal project
- [ ] Recreate materials using URP
- [ ] Import 3D meshes (export from UE4 as FBX)
- [ ] Set up shaders

### AR Reference Images
- [x] Create `ARReferenceImageSetup` script
- [ ] Create AR Reference Image Library in Editor
- [ ] Add QR code textures
- [ ] Configure AR Tracked Image Manager

---

## Phase 5: Running the Application

### iOS Build
1. Connect iOS device via USB
2. Go to **File** → **Build Settings**
3. Select **iOS** platform
4. Click **Build And Run**
5. Select output directory
6. Wait for Xcode to open
7. Select your development team in Xcode
8. Build and run on device

### Android Build
1. Connect Android device via USB (enable Developer Mode + USB Debugging)
2. Go to **File** → **Build Settings**
3. Select **Android** platform
4. Click **Build And Run**
5. Select output APK location
6. Wait for Unity to build and install

---

## Required APIs Summary

### Unity APIs Used
| API | Namespace | Purpose |
|-----|-----------|---------|
| ARFoundation | `UnityEngine.XR.ARFoundation` | AR session management |
| ARCore | `UnityEngine.XR.ARCore` | Android AR |
| ARKit | `UnityEngine.XR.ARKit` | iOS AR |
| XR Management | `UnityEngine.XR.Management` | XR plugin management |
| URP | `UnityEngine.Rendering` | Rendering pipeline |

### Key Classes Required
| Class | Purpose |
|-------|---------|
| `ARSession` | Manages AR session lifecycle |
| `ARSessionOrigin` | Transforms AR coordinates |
| `ARCamera` | AR camera component |
| `ARPlaneManager` | Detects planes |
| `ARReferenceImageLibrary` | Reference image database |
| `ARRaycastManager` | Raycasting against AR planes |

### External Dependencies
| Package | Purpose | Source |
|---------|---------|--------|
| ZXing.Net | QR Code scanning | NuGet/Unity Package Manager |
| TextMeshPro | Advanced text | Unity Package Manager |

---

## Troubleshooting

### Common Issues

1. **Packages not installing**
   - Check Unity version compatibility
   - Verify internet connection
   - Try reinstalling from Package Manager directly

2. **AR Session not starting**
   - Check device supports ARCore/ARKit
   - Verify camera permissions in manifest/settings
   - Ensure ARCore/ARKit XR plugins are enabled

3. **Build errors**
   - Check minimum SDK version (Android: API 21+, iOS: 12+)
   - Verify development certificates (iOS)
   - Enable USB debugging (Android)

---

## Next Steps

After completing the initial setup:
1. Begin implementing core scripts in `Assets/Scripts/`
2. Follow the checklist in each phase
3. Test incrementally on device
4. Import assets from Unreal project as needed

---

*Migration Instructions v1.0 - For AR Navigation v3.0.0*
