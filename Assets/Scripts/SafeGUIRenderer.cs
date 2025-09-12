using UnityEngine;

/// <summary>
/// SafeGUIRenderer provides additional safety checks and validation for GUI rendering
/// to prevent "Screen position out of view frustum" errors
/// </summary>
[DefaultExecutionOrder(-500)] // Execute early but after SceneBootstrapper
public class SafeGUIRenderer : MonoBehaviour
{
    private static SafeGUIRenderer instance;
    public static SafeGUIRenderer Instance => instance;
    
    private bool isRenderingSafe = false;
    private int framesSinceStart = 0;
    private const int SAFETY_FRAME_DELAY = 5; // Wait 5 frames before allowing GUI
    
    public bool IsGUIRenderingSafe => isRenderingSafe;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Validate initial setup
        ValidateRenderingEnvironment();
    }
    
    void Update()
    {
        framesSinceStart++;
        
        // Only allow GUI rendering after a few frames to ensure everything is initialized
        if (framesSinceStart >= SAFETY_FRAME_DELAY && !isRenderingSafe)
        {
            if (ValidateRenderingEnvironment())
            {
                isRenderingSafe = true;
                Debug.Log($"GUI rendering enabled after {framesSinceStart} frames");
            }
        }
        
        // Periodic re-validation
        if (framesSinceStart % 60 == 0) // Check every second at 60 FPS
        {
            ValidateRenderingEnvironment();
        }
    }
    
    bool ValidateRenderingEnvironment()
    {
        // Check screen dimensions
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            Debug.LogWarning($"Invalid screen dimensions: {Screen.width}x{Screen.height}");
            return false;
        }
        
        // Check main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main camera not found");
            return false;
        }
        
        // Check camera viewport
        if (mainCamera.pixelWidth <= 0 || mainCamera.pixelHeight <= 0)
        {
            Debug.LogWarning($"Invalid camera viewport: {mainCamera.pixelWidth}x{mainCamera.pixelHeight}");
            
            // Try to fix it
            mainCamera.ResetAspect();
            return false;
        }
        
        // Check camera rect
        Rect camRect = mainCamera.rect;
        if (camRect.width <= 0 || camRect.height <= 0)
        {
            Debug.LogWarning($"Invalid camera rect: {camRect}");
            mainCamera.rect = new Rect(0, 0, 1, 1);
            return false;
        }
        
        // Validate frustum
        if (!ValidateFrustum(mainCamera))
        {
            return false;
        }
        
        return true;
    }
    
    bool ValidateFrustum(Camera camera)
    {
        try
        {
            // Test viewport to world point conversion
            Vector3 testPoint = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.nearClipPlane));
            
            // Test screen to world point conversion
            Vector3 screenTest = camera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, camera.nearClipPlane));
            
            // If we get here without exceptions, frustum is valid
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Frustum validation failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Safe wrapper for GUI operations that checks if rendering is safe
    /// </summary>
    public static bool CanRenderGUI()
    {
        if (instance == null) return false;
        return instance.isRenderingSafe;
    }
    
    /// <summary>
    /// Creates a safe rect within screen bounds
    /// </summary>
    public static Rect CreateSafeRect(float x, float y, float width, float height)
    {
        // Clamp values to screen bounds
        x = Mathf.Max(0, x);
        y = Mathf.Max(0, y);
        width = Mathf.Min(width, Screen.width - x);
        height = Mathf.Min(height, Screen.height - y);
        
        // Ensure minimum size
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        
        return new Rect(x, y, width, height);
    }
    
    /// <summary>
    /// Validates if a screen position is within valid bounds
    /// </summary>
    public static bool IsScreenPositionValid(Vector2 position)
    {
        return position.x >= 0 && position.x <= Screen.width &&
               position.y >= 0 && position.y <= Screen.height;
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // Re-validate when app regains focus
            framesSinceStart = 0;
            isRenderingSafe = false;
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // Re-validate when app resumes
            framesSinceStart = 0;
            isRenderingSafe = false;
        }
    }
}