using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// MaterialCreator creates materials and placeholder textures for the AR Navigation project.
/// Run this in Unity Editor to generate basic materials.
/// </summary>
public class MaterialCreator : MonoBehaviour
{
    [Header("Material Settings")]
    [Tooltip("Create navigation materials")]
    public bool createNavigationMaterials = true;

    [Tooltip("Create icon materials")]
    public bool createIconMaterials = true;

    [Tooltip("Create UI materials")]
    public bool createUIMaterials = true;

    [Header("Colors")]
    public Color arrowColor = Color.blue;
    public Color boxRedColor = Color.red;
    public Color boxGreenColor = Color.green;
    public Color boxBlueColor = Color.blue;
    public Color wallsColor = Color.gray;
    public Color clearColor = new Color(1, 1, 1, 0.3f);

    /// <summary>
    /// Create all materials
    /// </summary>
    [ContextMenu("Create All Materials")]
    public void CreateAllMaterials()
    {
        if (createNavigationMaterials)
            CreateNavigationMaterials();
        
        if (createIconMaterials)
            CreateIconMaterials();
        
        if (createUIMaterials)
            CreateUIMaterials();

        Debug.Log("MaterialCreator: All materials created!");
    }

    /// <summary>
    /// Create navigation-related materials
    /// </summary>
    [ContextMenu("Create Navigation Materials")]
    public void CreateNavigationMaterials()
    {
        // Arrow Material
        CreateMaterial("Arrow_Mat", "Sprites/Default", arrowColor, true);

        // Box Materials
        CreateMaterial("BoxRed_Mat", "Standard", boxRedColor, false);
        CreateMaterial("BoxGreen_Mat", "Standard", boxGreenColor, false);
        CreateMaterial("BoxBlue_Mat", "Standard", boxBlueColor, false);

        // Wall Material
        CreateMaterial("Walls_Mat", "Standard", wallsColor, false);

        // Clear/Transparent Material
        CreateMaterial("Clear_Mat", "Sprites/Default", clearColor, true);

        // Floating Shape Material
        CreateMaterial("FloatingShape_Mat", "Sprites/Default", Color.white, true);

        Debug.Log("MaterialCreator: Navigation materials created");
    }

    /// <summary>
    /// Create icon materials
    /// </summary>
    [ContextMenu("Create Icon Materials")]
    public void CreateIconMaterials()
    {
        // Create placeholder textures for icons
        Texture2D canteenTex = CreatePlaceholderTexture("Canteen", Color.yellow);
        Texture2D cashboxTex = CreatePlaceholderTexture("Cashbox", Color.cyan);
        Texture2D stairsTex = CreatePlaceholderTexture("Stairs", Color.magenta);

        // Create materials with textures
        CreateMaterialWithTexture("CanteenIcon_Mat", canteenTex, true);
        CreateMaterialWithTexture("CashboxIcon_Mat", cashboxTex, true);
        CreateMaterialWithTexture("StairsIcon_Mat", stairsTex, true);

        Debug.Log("MaterialCreator: Icon materials created");
    }

    /// <summary>
    /// Create UI materials
    /// </summary>
    [ContextMenu("Create UI Materials")]
    public void CreateUIMaterials()
    {
        // Create placeholder UI textures
        Texture2D buttonTex = CreatePlaceholderTexture("Button", new Color(0.3f, 0.3f, 0.3f));
        Texture2D bottomBarTex = CreatePlaceholderTexture("BottomBar", new Color(0.1f, 0.1f, 0.1f));
        Texture2D searchTex = CreatePlaceholderTexture("Search", Color.white);
        Texture2D roundedMaskTex = CreateRoundedMaskTexture();

        // Create materials
        CreateMaterialWithTexture("Button_Mat", buttonTex, true);
        CreateMaterialWithTexture("BottomBar_Mat", bottomBarTex, true);
        CreateMaterialWithTexture("SearchIcon_Mat", searchTex, true);
        CreateMaterialWithTexture("RoundedMask_Mat", roundedMaskTex, true);

        Debug.Log("MaterialCreator: UI materials created");
    }

    /// <summary>
    /// Create a basic material
    /// </summary>
    private Material CreateMaterial(string name, string shader, Color color, bool transparent)
    {
        // Try to find the shader
        Shader shaderObj = Shader.Find(shader);
        if (shaderObj == null)
        {
            // Fallback to standard shaders
            shaderObj = Shader.Find("Universal Render Pipeline/Lit");
            if (shaderObj == null)
                shaderObj = Shader.Find("Standard");
            if (shaderObj == null)
                shaderObj = Shader.Find("Sprites/Default");
        }

        if (shaderObj == null)
        {
            Debug.LogWarning($"MaterialCreator: Could not find shader '{shader}' for material '{name}'");
            return null;
        }

        Material material = new Material(shaderObj);
        material.color = color;

        if (transparent)
        {
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }

        // Save to Assets folder
        #if UNITY_EDITOR
        string path = $"Assets/Materials/{name}.mat";
        UnityEditor.AssetDatabase.CreateAsset(material, path);
        #endif

        return material;
    }

    /// <summary>
    /// Create material with texture
    /// </summary>
    private Material CreateMaterialWithTexture(string name, Texture2D texture, bool transparent)
    {
        string shader = transparent ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit";
        
        Shader shaderObj = Shader.Find(shader);
        if (shaderObj == null)
            shaderObj = Shader.Find("Sprites/Default");

        if (shaderObj == null)
        {
            Debug.LogWarning($"MaterialCreator: Could not find shader for material '{name}'");
            return null;
        }

        Material material = new Material(shaderObj);
        material.mainTexture = texture;

        if (transparent)
        {
            material.SetFloat("_Mode", 3);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.renderQueue = 3000;
        }

        #if UNITY_EDITOR
        string path = $"Assets/Materials/{name}.mat";
        UnityEditor.AssetDatabase.CreateAsset(material, path);
        #endif

        return material;
    }

    /// <summary>
    /// Create a placeholder texture
    /// </summary>
    public static Texture2D CreatePlaceholderTexture(string text, Color color, int size = 256)
    {
        Texture2D texture = new Texture2D(size, size);
        
        // Fill with color
        Color[] colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }
        texture.SetPixels(colors);

        // Add simple text representation (first letter)
        // Note: Real text rendering requires TextMeshPro
        texture.Apply();

        // Create readable version
        return CreateReadableTexture(texture);
    }

    /// <summary>
    /// Create a rounded mask texture for UI
    /// </summary>
    public static Texture2D CreateRoundedMaskTexture(int size = 128)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];

        float centerX = size / 2f;
        float centerY = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                
                if (dist < radius * 0.8f)
                {
                    colors[y * size + x] = Color.white;
                }
                else if (dist < radius)
                {
                    // Anti-aliased edge
                    float alpha = Mathf.Lerp(1f, 0f, (dist - radius * 0.8f) / (radius * 0.2f));
                    colors[y * size + x] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return CreateReadableTexture(texture);
    }

    /// <summary>
    /// Create a readable copy of texture (for import)
    /// </summary>
    private static Texture2D CreateReadableTexture(Texture2D source)
    {
        // For runtime, just return the source
        // In Editor, this would mark it for import
        return source;
    }

    /// <summary>
    /// Create floor segment texture
    /// </summary>
    public static Texture2D CreateFloorSegmentTexture(int width = 1024, int height = 1024)
    {
        Texture2D texture = new Texture2D(width, height);
        
        // Create a simple floor plan pattern
        Color wallColor = Color.black;
        Color floorColor = Color.white;
        Color gridColor = new Color(0.9f, 0.9f, 0.9f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Add grid lines every 100 pixels (representing meters)
                bool isGridLine = (x % 100 == 0) || (y % 100 == 0);
                
                if (isGridLine)
                {
                    texture.SetPixel(x, y, gridColor);
                }
                else
                {
                    texture.SetPixel(x, y, floorColor);
                }
            }
        }

        texture.Apply();
        return CreateReadableTexture(texture);
    }

    /// <summary>
    /// Create arrow texture for path visualization
    /// </summary>
    public static Texture2D CreateArrowTexture(int size = 128)
    {
        Texture2D texture = new Texture2D(size, size);
        
        // Clear to transparent
        Color[] colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.clear;
        
        texture.SetPixels(colors);

        // Draw arrow pointing up
        int centerX = size / 2;
        int tipY = size - 20;
        int baseY = 20;
        int arrowWidth = size / 3;

        // Draw arrow shape
        for (int y = baseY; y < tipY; y++)
        {
            float progress = (float)(y - baseY) / (tipY - baseY);
            int halfWidth = Mathf.RoundToInt(arrowWidth * (1 - progress));
            
            for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }
        }

        // Draw arrowhead
        for (int y = tipY - 20; y < tipY; y++)
        {
            float progress = (float)(y - (tipY - 20)) / 20f;
            int halfWidth = Mathf.RoundToInt(arrowWidth * progress);
            
            for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }
        }

        texture.Apply();
        return CreateReadableTexture(texture);
    }
}
