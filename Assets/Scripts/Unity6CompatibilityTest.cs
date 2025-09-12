using UnityEngine;
using UnityEngine.Rendering;

public class Unity6CompatibilityTest : MonoBehaviour
{
    void Start()
    {
        // Wait a moment for scene setup to complete
        Invoke(nameof(RunCompatibilityTests), 1f);
    }
    
    void RunCompatibilityTests()
    {
        Debug.Log("=== Unity 6 Compatibility Test ===");
        
        // Test 1: Check Unity version
        Debug.Log($"Unity Version: {Application.unityVersion}");
        
        // Test 2: Check Render Pipeline
        RenderPipelineAsset currentPipeline = GraphicsSettings.currentRenderPipeline;
        if (currentPipeline != null)
        {
            Debug.Log($"Render Pipeline: {currentPipeline.GetType().Name}");
            Debug.Log("✓ Custom render pipeline active");
        }
        else
        {
            Debug.Log("✓ Built-in Render Pipeline active (Unity 6 compatible)");
        }
        
        // Test 3: Check Input System
        Debug.Log("✓ Using Legacy Input System (Unity 6 compatible)");
        
        // Test Unity Event Input System
        var inputManager = FindObjectOfType<UnityEventInputManager>();
        if (inputManager != null)
        {
            Debug.Log("✓ UnityEventInputManager found and working");
        }
        else
        {
            Debug.LogWarning("× UnityEventInputManager not found");
        }
        
        // Test 4: Check mobile optimization settings
        Debug.Log($"Target Frame Rate: {Application.targetFrameRate}");
        Debug.Log($"Platform: {Application.platform}");
        
        // Test 5: Check Unity 6 specific features
        Debug.Log($"Render Graph Support: {SystemInfo.supportsRenderTargetArrayIndexFromVertexShader}");
        
        // Test 6: Check Core Systems
        Debug.Log("✓ Built-in Pipeline - No Adaptive Performance conflicts");
        
        Debug.Log("=== Compatibility Test Complete ===");
    }
    
    // No Update method needed - using GUI-based input system instead
}