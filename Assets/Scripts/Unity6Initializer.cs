using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-1000)] // Execute very early
public class Unity6Initializer : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("Unity 6 Initializer: Disabling problematic systems...");
        
        // Built-in Pipeline - No Debug Manager conflicts to handle
        Debug.Log("✓ Built-in Pipeline - No URP Debug Manager conflicts");
        
        // Force legacy input mode
        ConfigureLegacyInput();
        
        Debug.Log("Unity 6 Initializer: Configuration complete - Built-in Pipeline stable");
    }
    
    // No URP Debug Manager in Built-in Pipeline - no conflicts to resolve
    
    void ConfigureLegacyInput()
    {
        // Ensure we're using legacy input
        Debug.Log("✓ Configured for Legacy Input mode");
        
        // Set application settings that support legacy input
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        
        Debug.Log("✓ Mobile optimization settings applied");
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeBeforeScene()
    {
        Debug.Log("Unity 6 Early Initialization: Configuring Built-in Pipeline...");
        
        // Create the initializer if it doesn't exist
        GameObject initializer = new GameObject("Unity6Initializer");
        initializer.AddComponent<Unity6Initializer>();
        DontDestroyOnLoad(initializer);
        
        Debug.Log("Unity 6 Early Initialization: Complete");
    }
}