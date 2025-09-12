using UnityEngine;

[DefaultExecutionOrder(-50)] // Execute early but after SceneBootstrapper
public class AutoGameStarter : MonoBehaviour
{
    void Start()
    {
        // Give the bootstrapper a moment to create everything
        Invoke(nameof(StartGameAndShowInstructions), 0.5f);
    }
    
    void StartGameAndShowInstructions()
    {
        Debug.Log("=== Auto Game Starter ===");
        
        // List all created objects for debugging
        ListCreatedObjects();
        
        // Start the game
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.StartGame();
            Debug.Log("✓ Game started!");
            Debug.Log($"GameManager.IsGameActive = {gameManager.IsGameActive}");
        }
        else
        {
            Debug.LogError("× GameManager not found!");
        }
        
        // Show controls
        ShowControls();
    }
    
    void ListCreatedObjects()
    {
        Debug.Log("=== Scene Objects ===");
        
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Debug.Log($"✓ Player found at: {player.transform.position}");
        }
        else
        {
            Debug.LogError("× Player not found!");
        }
        
        var camera = Camera.main;
        if (camera != null)
        {
            Debug.Log($"✓ Camera found at: {camera.transform.position}");
        }
        
        var ground = GameObject.Find("Ground");
        if (ground != null)
        {
            Debug.Log($"✓ Ground found at: {ground.transform.position}");
        }
        
        var inputManager = FindObjectOfType<UnityEventInputManager>();
        if (inputManager != null)
        {
            Debug.Log("✓ UnityEventInputManager found");
        }
        else
        {
            Debug.LogError("× UnityEventInputManager not found!");
        }
        
        Debug.Log("=== End Scene Objects ===");
    }
    
    void ShowControls()
    {
        Debug.Log("=== GAME CONTROLS ===");
        Debug.Log("GUI Button Controls:");
        Debug.Log("• Click LEFT/RIGHT buttons = Change lanes");
        Debug.Log("• Click JUMP button = Jump");
        Debug.Log("• Click SLIDE button = Slide");
        Debug.Log("• Click SHOOT button = Shoot");
        Debug.Log("• Hold CHARGED button = Charged shot");
        Debug.Log("==================");
    }
    
    void OnGUI()
    {
        if (!Application.isEditor) return;
        
        // Show instructions on screen
        GUI.Box(new Rect(10, 10, 300, 150), "");
        GUILayout.BeginArea(new Rect(15, 15, 290, 140));
        
        GUILayout.Label("Just Play Mariam - Unity 6");
        GUILayout.Space(5);
        GUILayout.Label("GUI BUTTON CONTROLS:");
        GUILayout.Label("Click the buttons on the left");
        GUILayout.Label("to control the game!");
        GUILayout.Label("No Input System conflicts!");
        GUILayout.Label("• Hold = Charged shot");
        
        GUILayout.EndArea();
    }
}