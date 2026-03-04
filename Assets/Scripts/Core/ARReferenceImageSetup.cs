using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_XR_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

/// <summary>
/// ReferenceImageManager helps manage AR Reference Images for QR code detection.
/// This is a runtime helper that works with Reference Image Libraries created in the Unity Editor.
///
/// In Unity Editor: Right-click > Create > XR > AR Reference Image Library
/// </summary>
public class ReferenceImageManager : MonoBehaviour
{
    [Header("Configuration")]
#if UNITY_XR_ARFOUNDATION
    [Tooltip("Reference image library created in Unity Editor")]
    public ARReferenceImageLibrary referenceImageLibrary;
#endif

    [Header("Runtime Images")]
    [Tooltip("List of runtime reference image data")]
    public List<ReferenceImageData> runtimeImages = new List<ReferenceImageData>();

#if UNITY_XR_ARFOUNDATION
    private ARTrackedImageManager trackedImageManager;
#endif

    private void Start()
    {
        SetupTrackedImageManager();
    }

    private void SetupTrackedImageManager()
    {
#if UNITY_XR_ARFOUNDATION
        // Try to find ARTrackedImageManager
        if (trackedImageManager == null)
        {
            ARSessionOrigin origin = FindObjectOfType<ARSessionOrigin>();
            if (origin != null)
            {
                trackedImageManager = origin.GetComponent<ARTrackedImageManager>();
            }
        }

        if (trackedImageManager != null)
        {
            // Assign reference library if set
            if (referenceImageLibrary != null)
            {
                trackedImageManager.referenceLibrary = referenceImageLibrary;
            }
            
            // Enable image tracking
            trackedImageManager.enabled = true;
            
            Debug.Log("ReferenceImageManager: Image tracking enabled");
        }
        else
        {
            Debug.LogWarning("ReferenceImageManager: No ARTrackedImageManager found");
        }
#else
        Debug.LogWarning("ReferenceImageManager: ARFoundation not installed. Install XR ARFoundation package.");
#endif
    }

    /// <summary>
    /// Add a reference image at runtime
    /// </summary>
    public void AddReferenceImage(string name, Texture2D texture, float widthMeters)
    {
        Debug.Log($"ReferenceImageManager: Would add image '{name}' ({widthMeters}m width)");
    }

    /// <summary>
    /// Create a placeholder QR code texture for testing
    /// </summary>
    public static Texture2D CreatePlaceholderQRTexture(string data, int size = 512)
    {
        Texture2D texture = new Texture2D(size, size);
        
        // Create a simple colored pattern
        Color[] colors = new Color[size * size];
        int hash = data.GetHashCode();
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float value = ((x + y + hash) % 256) / 255f;
                colors[y * size + x] = new Color(value, value * 0.5f, 1f - value, 1f);
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return texture;
    }

    /// <summary>
    /// Check if image tracking is supported
    /// </summary>
    public static bool IsImageTrackingSupported()
    {
#if UNITY_XR_ARFOUNDATION && UNITY_ANDROID
        return ARCoreSessionSubsystem.CheckAvailability() == ArAvailability.Supported;
#elif UNITY_XR_ARFOUNDATION && UNITY_IOS
        return ARKitSessionSubsystem.IsSupported();
#else
        return false;
#endif
    }
}

/// <summary>
/// Data structure for reference image information
/// </summary>
[Serializable]
public class ReferenceImageData
{
    [Tooltip("Name/identifier for this reference image")]
    public string name = "";

    [Tooltip("Texture for the reference image")]
    public Texture2D texture;

    [Tooltip("Physical width of the image in meters")]
    public float widthInMeters = 0.1f;

    [Tooltip("Use raw texture descriptor")]
    public bool allowRawDescriptor = false;

    public ReferenceImageData()
    {
    }

    public ReferenceImageData(string imageName, Texture2D imageTexture, float width)
    {
        name = imageName;
        texture = imageTexture;
        widthInMeters = width;
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor-only helper - creates reference image library
/// </summary>
public class ARReferenceImageEditorHelper
{
    /// <summary>
    /// Menu item to create reference image library
    /// </summary>
    [UnityEditor.MenuItem("AR Navigation/Create Reference Image Library")]
    public static void CreateReferenceImageLibrary()
    {
        // In Unity Editor with ARFoundation installed:
        // 1. Right-click in Project window
        // 2. Select Create > XR > AR Reference Image Library
        Debug.Log("ARReferenceImageSetup: Please create AR Reference Image Library from Unity Editor menu");
    }
}
#endif
