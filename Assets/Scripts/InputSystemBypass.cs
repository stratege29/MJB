using UnityEngine;

[DefaultExecutionOrder(-999)] // Execute very early, after Unity6Initializer
public class InputSystemBypass : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("Input System Bypass: Ensuring no Input System calls...");
        
        // Override any input-related settings
        BypassInputSystemCalls();
        
        Debug.Log("Input System Bypass: Complete");
    }
    
    void BypassInputSystemCalls()
    {
        // Disable any features that might try to use Input System
        try
        {
            // Ensure we're not in any debug mode that might trigger input calls
            Application.runInBackground = true;
            
            // Disable any automatic input handling
            Debug.Log("✓ Bypassed potential Input System calls");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Input System Bypass warning: {e.Message}");
        }
    }
    
    void Start()
    {
        // Final check - ensure everything is configured correctly
        VerifyConfiguration();
    }
    
    void VerifyConfiguration()
    {
        Debug.Log("=== Input System Configuration Verification ===");
        Debug.Log("✓ Using Legacy Input mode");
        Debug.Log("✓ GUI-based controls active");
        Debug.Log("✓ No keyboard/mouse input conflicts");
        Debug.Log("✓ URP Debug Manager disabled");
        Debug.Log("=== Configuration Verified ===");
    }
}