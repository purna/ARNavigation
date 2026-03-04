# AR Navigation - Required Graphics Assets

This document lists all graphics assets needed for the Unity AR Navigation project, organized by category, with recommended sources for free/purchasable alternatives.

---

## 1. Materials Needed

### 1.1 Navigation Materials

| Material Name | Type | Description | Recommended Source |
|--------------|------|-------------|-------------------|
| `Arrow_Mat` | Unlit/Transparent | Direction arrows for path visualization | Unity Asset Store - Free Arrow Icons |
| `BoxRed_Mat` | Lit/Opaque | Red marker boxes | Unity built-in (create material) |
| `BoxGreen_Mat` | Lit/Opaque | Green marker boxes | Unity built-in (create material) |
| `BoxBlue_Mat` | Lit/Opaque | Blue marker boxes | Unity built-in (create material) |
| `Walls_Mat` | Lit/Opaque | Building walls | Unity built-in (create material) |
| `Clear_Mat` | Transparent | Transparent debug overlay | Unity built-in (create material) |
| `FloatingShape_Mat` | Unlit/Transparent | Floating icons | Unity Asset Store - Free UI packs |

### 1.2 Floor Segment Materials

| Material Name | Type | Description | Recommended Source |
|--------------|------|-------------|-------------------|
| `SegmentFirstFloor_Mat` | Lit/Opaque | First floor map texture | Export from Unreal project |
| `FloorPlanOverlay_Mat` | Unlit/Transparent | Floor plan overlay texture | Export from Unreal project |

### 1.3 Icon Materials

| Material Name | Type | Description | Recommended Source |
|--------------|------|-------------|-------------------|
| `CanteenIcon_Mat` | Unlit/Transparent | Canteen/restaurant icon | Unity Asset Store - Icons |
| `CashboxIcon_Mat` | Unlit/Transparent | Payment/cashier icon | Unity Asset Store - Icons |
| `StairsIcon_Mat` | Unlit/Transparent | Stairs/escalator icon | Unity Asset Store - Icons |
| `ElevatorIcon_Mat` | Unlit/Transparent | Elevator icon (if needed) | Unity Asset Store - Icons |
| `ExitIcon_Mat` | Unlit/Transparent | Exit/door icon | Unity Asset Store - Icons |
| `RestroomIcon_Mat` | Unlit/Transparent | Restroom icon | Unity Asset Store - Icons |

### 1.4 UI Materials

| Material Name | Type | Description | Recommended Source |
|--------------|------|-------------|-------------------|
| `BottomBar_Mat` | Unlit/Transparent | Bottom UI bar background | Unity Asset Store - UI |
| `Button_Mat` | Unlit/Transparent | Button background | Unity Asset Store - UI |
| `RoundedMask_Mat` | Unlit/Transparent | Rounded corners mask | Unity Asset Store - UI |
| `SearchIcon_Mat` | Unlit/Transparent | Search icon | Unity Asset Store - Icons |

---

## 2. 3D Meshes Needed

### 2.1 Navigation Meshes

| Mesh Name | Description | Recommended Source |
|-----------|-------------|-------------------|
| `M_Arrow` | 3D Arrow for path direction | Blender - Create from primitives |
| `M_ArrowPointer` | Pointer arrow for minimap | Blender - Create from primitives |

### 2.2 Building Meshes

| Mesh Name | Description | Recommended Source |
|-----------|-------------|-------------------|
| `M_FirstFloorMapSegment` | 3D model of first floor | Export from Unreal project |
| `M_Wall` | Wall segment mesh | Blender - Simple cube |
| `M_Floor` | Floor segment mesh | Blender - Simple plane |

### 2.3 Icon Meshes

| Mesh Name | Description | Recommended Source |
|-----------|-------------|-------------------|
| `M_Icon` | Billboard quad for icons | Unity - Plane primitive |
| `M_FloatingIcon` | Floating icon mesh | Blender - Create plane with icon texture |

---

## 3. Textures Needed

### 3.1 Icon Textures

| Texture Name | Size | Format | Recommended Source |
|--------------|------|--------|-------------------|
| `T_CanteenIcon` | 512x512 | PNG | Unity Asset Store - Free Icons |
| `T_CashboxIcon` | 512x512 | PNG | Unity Asset Store - Free Icons |
| `T_StairsIcon` | 512x512 | PNG | Unity Asset Store - Free Icons |
| `T_SearchIcon` | 256x256 | PNG | Unity Asset Store - Free Icons |
| `T_ArrowShadow` | 256x256 | PNG | Unity built-in - Default particle |

### 3.2 UI Textures

| Texture Name | Size | Format | Recommended Source |
|--------------|------|--------|-------------------|
| `T_BottomBar` | 1024x256 | PNG | Unity Asset Store - UI |
| `T_Button` | 512x128 | PNG | Unity Asset Store - UI |
| `T_ButtonWithoutInset` | 512x128 | PNG | Unity Asset Store - UI |
| `T_MainUI` | 512x512 | PNG | Unity Asset Store - UI |
| `T_RoundedMask` | 128x128 | PNG | Unity Asset Store - UI |
| `T_RoundedMaskShadow` | 256x256 | PNG | Unity Asset Store - UI |

### 3.3 Floor Textures

| Texture Name | Size | Format | Recommended Source |
|--------------|------|--------|-------------------|
| `T_FirstFloorSegment` | 2048x2048 | PNG | Export from Unreal project |

---

## 4. Recommended Free Asset Sources

### Unity Asset Store (Free)
1. **Kenney Assets**
   - URL: https://kenney.nl/assets
   - Contains: Icons, UI elements, 3D models
   
2. **Unity Technologies - AR Foundation Samples**
   - URL: Built into Unity (XR Interaction Toolkit)
   - Contains: AR sample scenes, basic meshes

3. **Simple Icons**
   - URL: Unity Asset Store - Search "Simple Icons"
   - Contains: Basic icon set

### Free Icon Libraries
1. **Flaticon**
   - URL: https://www.flaticon.com
   - Search: "navigation", "arrow", "building", "room"

2. **Icons8**
   - URL: https://icons8.com
   - Contains: Icons, illustrations

3. **Material Design Icons**
   - URL: https://fonts.google.com/icons
   - Contains: Material icons

---

## 5. Asset Import Checklist

### Textures
- [ ] Export all textures from Unreal project as PNG/JPG
- [ ] Import at 2x resolution for high-DPI displays
- [ ] Set texture type to "Sprite (2D and UI)" for icons
- [ ] Set texture type to "Default" for floor textures

### Materials (URP)
- [ ] Create URP/Lit materials for 3D objects
- [ ] Create URP/Unlit materials for icons and UI
- [ ] Enable "Alpha Clipping" for transparent icons
- [ ] Set "Surface Type" to "Transparent" for transparent materials

### Meshes
- [ ] Export meshes from Unreal as FBX
- [ ] Check "Import Materials" option
- [ ] Apply scale factor of 0.01 (cm to meters)
- [ ] Generate colliders where needed

---

## 6. Asset Organization

```
Assets/
├── Materials/
│   ├── Navigation/
│   │   ├── Arrow_Mat.mat
│   │   ├── BoxRed_Mat.mat
│   │   ├── BoxGreen_Mat.mat
│   │   └── BoxBlue_Mat.mat
│   ├── Icons/
│   │   ├── CanteenIcon_Mat.mat
│   │   ├── CashboxIcon_Mat.mat
│   │   └── StairsIcon_Mat.mat
│   └── UI/
│       ├── Button_Mat.mat
│       └── BottomBar_Mat.mat
├── Meshes/
│   ├── Navigation/
│   │   ├── M_Arrow.fbx
│   │   └── M_ArrowPointer.fbx
│   ├── Building/
│   │   ├── M_FirstFloorMapSegment.fbx
│   │   └── M_Wall.fbx
│   └── Icons/
│       └── M_Icon.fbx
├── Textures/
│   ├── Icons/
│   │   ├── T_CanteenIcon.png
│   │   ├── T_CashboxIcon.png
│   │   └── T_StairsIcon.png
│   ├── UI/
│   │   ├── T_Button.png
│   │   └── T_BottomBar.png
│   └── Segments/
│       └── T_FirstFloorSegment.png
└── Sprites/
    ├── Icons/
    └── UI/
```

---

## 7. Next Steps

1. **Export from Unreal**: Use FBX exporter to get meshes from Unreal project
2. **Screenshot textures**: Take screenshots of materials in Unreal for reference
3. **Download free icons**: Get replacement icons from Flaticon or Unity Asset Store
4. **Import and configure**: Import into Unity, create materials, apply to prefabs

---

*Graphics Assets Documentation v1.0*
