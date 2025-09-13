using UnityEngine;

public class CharacterTest : MonoBehaviour
{
    void Start()
    {
        // Test loading the character assets
        TestCharacterAssetLoading();
    }
    
    void TestCharacterAssetLoading()
    {
        Debug.Log("=== Testing Character Asset Loading ===");
        
        // Test Base Mesh
        GameObject baseMesh = Resources.Load<GameObject>("ithappy/Creative_Characters_FREE/Prefabs/Base_Mesh");
        Debug.Log(baseMesh != null ? "✓ Base_Mesh loaded successfully" : "✗ Base_Mesh failed to load");
        
        // Test Shorts
        GameObject shorts = Resources.Load<GameObject>("ithappy/Creative_Characters_FREE/Prefabs/Shorts/Shorts_003");
        Debug.Log(shorts != null ? "✓ Shorts loaded successfully" : "✗ Shorts failed to load");
        
        // Test Sneakers
        GameObject sneakers = Resources.Load<GameObject>("ithappy/Creative_Characters_FREE/Prefabs/Shoes/Shoe_Sneakers_009");
        Debug.Log(sneakers != null ? "✓ Sneakers loaded successfully" : "✗ Sneakers failed to load");
        
        // Test Face
        GameObject face = Resources.Load<GameObject>("ithappy/Creative_Characters_FREE/Prefabs/Faces/Male_emotion_happy_002");
        Debug.Log(face != null ? "✓ Face loaded successfully" : "✗ Face failed to load");
        
        // Test Material
        Material colorMat = Resources.Load<Material>("ithappy/Creative_Characters_FREE/Materials/Color");
        Debug.Log(colorMat != null ? "✓ Color material loaded successfully" : "✗ Color material failed to load");
        
        Debug.Log("=== Character Asset Loading Test Complete ===");
    }
}