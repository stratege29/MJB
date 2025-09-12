using UnityEngine;
using System.Collections;

/// <summary>
/// MouseEventSanitizer prevents "Screen position out of view frustum" errors
/// by intercepting and validating mouse events before they're processed
/// </summary>
[DefaultExecutionOrder(-2000)] // Execute very early
public class MouseEventSanitizer : MonoBehaviour
{
    private static MouseEventSanitizer instance;
    private Camera mainCamera;
    private bool isInitialized = false;
    private Coroutine validationCoroutine;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Start validation immediately
            validationCoroutine = StartCoroutine(ContinuousValidation());
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeCamera();
    }
    
    void InitializeCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("MouseEventSanitizer: Main camera not found, will retry...");
            Invoke(nameof(InitializeCamera), 0.1f);
            return;
        }
        
        // Validate camera settings
        if (mainCamera.pixelWidth <= 0 || mainCamera.pixelHeight <= 0)
        {
            Debug.LogWarning($"MouseEventSanitizer: Invalid camera viewport ({mainCamera.pixelWidth}x{mainCamera.pixelHeight}), fixing...");
            mainCamera.ResetAspect();
            mainCamera.rect = new Rect(0, 0, 1, 1);
        }
        
        isInitialized = true;
        Debug.Log($"MouseEventSanitizer initialized - Camera viewport: {mainCamera.pixelWidth}x{mainCamera.pixelHeight}");
    }
    
    IEnumerator ContinuousValidation()
    {
        // Wait a frame to ensure scene is loaded
        yield return null;
        
        while (true)
        {
            // Validate camera setup periodically
            if (Time.timeSinceLevelLoad < 1f)
            {
                ValidateCameraSetup();
                yield return null;
            }
            else
            {
                // Then validate every 0.5 seconds
                ValidateCameraSetup();
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
    
    void ValidateCameraSetup()
    {
        if (!isInitialized || mainCamera == null) return;
        
        // Just validate camera setup without using Input.mousePosition
        // This avoids Input System conflicts
        
        // Check camera viewport is valid
        if (mainCamera.pixelWidth <= 0 || mainCamera.pixelHeight <= 0)
        {
            Debug.LogWarning($"MouseEventSanitizer: Invalid camera viewport, fixing...");
            mainCamera.ResetAspect();
            mainCamera.rect = new Rect(0, 0, 1, 1);
        }
        
        // Validate screen dimensions
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            Debug.LogWarning("MouseEventSanitizer: Invalid screen dimensions detected");
        }
    }
    
    void OnGUI()
    {
        // Early validation in OnGUI to catch GUI-related mouse events
        if (!isInitialized) return;
        
        Event currentEvent = Event.current;
        if (currentEvent == null) return;
        
        // Validate mouse position for GUI events
        if (currentEvent.isMouse)
        {
            Vector2 mousePosition = currentEvent.mousePosition;
            
            // Check if position is valid
            if (mousePosition.x < 0 || mousePosition.x > Screen.width ||
                mousePosition.y < 0 || mousePosition.y > Screen.height)
            {
                // Consume invalid event to prevent it from causing errors
                currentEvent.Use();
                return;
            }
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // Re-initialize when app regains focus
            isInitialized = false;
            InitializeCamera();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // Re-initialize when app resumes
            isInitialized = false;
            InitializeCamera();
        }
    }
    
    void OnDestroy()
    {
        if (validationCoroutine != null)
        {
            StopCoroutine(validationCoroutine);
        }
    }
}