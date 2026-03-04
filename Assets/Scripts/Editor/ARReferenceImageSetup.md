# AR Reference Image Library Setup Guide

## Overview

This guide explains how to set up AR Reference Image Libraries in Unity for QR code/image tracking in the AR Navigation project.

---

## Creating a Reference Image Library

### Method 1: Unity Editor (Recommended)

1. **Open Unity Project**
   - Navigate to your Unity project in Unity Hub
   - Open the project

2. **Create Reference Image Library**
   - In the Project window, right-click
   - Select **Create** > **XR** > **AR Reference Image Library**
   - Name it `ARReferenceImageLibrary`

3. **Configure Reference Images**
   - Select the created library
   - In the Inspector, click **Add Image**
   - For each reference image:
     - Drag your QR code/reference image texture into the Texture field
     - Enter a name (for example: InitialQRCode, QRCode)
     - Set the physical width in meters (for example: 0.1 for a 10cm QR code)

4. **Assign to ARTrackedImageManager**
   - Find your ARSessionOrigin in the scene
   - Add or select the **AR Tracked Image Manager** component
   - Drag your Reference Image Library into the Reference Library field

---

## Reference Images from Unreal Project

The original Unreal project has these reference images:

| Unreal Asset | Recommended Name | Width (meters) |
|--------------|-----------------|----------------|
| T_InitialQRCode | InitialQRCode | 0.1 (10cm) |
| QR Code texture | QRCode | 0.1 (10cm) |

### Exporting Textures from Unreal

1. **Option A: Screenshot in Unreal**
   - Open the level in Unreal Editor
   - Take screenshots of the QR code textures
   - Export as PNG

2. **Option B: Texture File Location**
   - Navigate to the Content folder
   - Find the texture files (PNG, JPG)
   - Copy to Unity project

3. **Option C: Create New QR Codes**
   - Use an online QR code generator
   - Create QR codes for your vertices (for example: Canteen, Stairs)
   - Save as PNG files
   - Import to Unity

---

## Required QR Codes for AR Navigation

Based on the documentation, you need:

| QR Code Name | Vertex ID | Purpose |
|--------------|-----------|---------|
| QRCode | QRCode | Initial positioning |
| Canteen | Canteen | Quick destination |
| Cashbox | Cashbox | Quick destination |
| Stairs | Stairs | Quick destination |

### Recommended QR Code Generation

1. **For Testing**: Use any QR code scanner app to generate test codes
2. **For Production**: Generate QR codes with your vertex IDs encoded

Example QR code content:
```
Canteen
```

---

## Setting Up in ARWorldManager

1. **Create Reference Image Library** (steps above)

2. **Assign to ARWorldManager**
   - Select ARWorldManager in scene
   - Drag library to Reference Image Library field

3. **Handle Detection**
   - The ARWorldManager script already handles reference image detection
   - Implement onReferenceImageDetected callback to teleport player

---

## Code Integration Example

```csharp
// In your initialization script
void Start()
{
    ARWorldManager worldManager = FindObjectOfType<ARWorldManager>();
    
    if (worldManager != null)
    {
        worldManager.onReferenceImageDetected += OnReferenceImageDetected;
    }
}

void OnReferenceImageDetected(string imageName, Pose pose)
{
    Debug.Log($"Detected: {imageName}");
    
    // Teleport to vertex matching the image name
    if (imageName == "QRCode")
    {
        worldManager.SetInitialPositionFromQRCode("QRCode", pose);
    }
    else
    {
        // Teleport to vertex with matching ID
        NavigationVertex vertex = ARWorldManager.Instance.FindVertexById(imageName);
        if (vertex != null)
        {
            ARPlayerController.Instance.TeleportToVertex(vertex);
        }
    }
}
```

---

## Testing Checklist

- [ ] Created AR Reference Image Library
- [ ] Added reference images (textures plus names plus widths)
- [ ] Assigned library to AR Tracked Image Manager
- [ ] Enabled image tracking in ARSessionManager
- [ ] Tested QR code detection on device
- [ ] Verified player teleportation on detection

---

## Troubleshooting

### Image Not Detected
- Check texture format (PNG/JPG recommended)
- Ensure texture is readable in Unity
- Verify physical width is correct
- Ensure adequate lighting

### Tracking Lost
- Keep reference image in view
- Ensure good lighting
- Avoid motion blur

### Build Errors
- Verify ARKit/ARCore is enabled in XR Plugin Settings
- Check iOS deployment target (12.0+)
- Ensure camera permissions are requested

---

*AR Reference Image Setup Guide v1.0*
