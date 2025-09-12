using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [Header("Scene Setup")]
    public bool autoSetupOnStart = true;
    public GameObject playerPrefab;
    public GameObject ballPrefab;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupGameScene();
        }
    }
    
    [ContextMenu("Setup Game Scene")]
    public void SetupGameScene()
    {
        Debug.Log("Setting up Just Play Mariam game scene...");
        
        // Setup player
        SetupPlayer();
        
        // Setup camera
        SetupCamera();
        
        // Setup managers
        SetupGameManager();
        SetupUIManager();
        SetupInputManager();
        // No Adaptive Performance Manager needed (using Built-in Pipeline)
        SetupObstacleSpawner();
        SetupCollectibleManager();
        
        // Setup environment
        SetupEnvironment();
        
        // Setup tags and layers
        SetupTagsAndLayers();
        
        Debug.Log("Game scene setup complete!");
    }
    
    void SetupPlayer()
    {
        PlayerController existingPlayer = FindObjectOfType<PlayerController>();
        
        if (existingPlayer == null)
        {
            GameObject player;
            
            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab);
            }
            else
            {
                // Create default player
                player = CreateDefaultPlayer();
            }
            
            player.name = "Player";
            player.transform.position = new Vector3(0, 1, 0);
            player.tag = "Player";
            
            Debug.Log("Player created and configured.");
        }
    }
    
    GameObject CreateDefaultPlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        
        // Add required components
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        PlayerController playerController = player.AddComponent<PlayerController>();
        ShootingSystem shootingSystem = player.AddComponent<ShootingSystem>();
        
        // Configure shooting system
        if (ballPrefab != null)
        {
            shootingSystem.ballPrefab = ballPrefab;
        }
        
        // Set material using Built-in shader
        Material playerMat = new Material(Shader.Find("Standard"));
        playerMat.color = Color.blue;
        player.GetComponent<MeshRenderer>().material = playerMat;
        
        return player;
    }
    
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            cameraObj.tag = "MainCamera";
        }
        
        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
        }
        
        // Configure camera position for third-person view
        mainCamera.transform.position = new Vector3(0, 2, -5);
        cameraFollow.offset = new Vector3(0, 2, -5);
        
        Debug.Log("Camera configured for third-person view.");
    }
    
    void SetupGameManager()
    {
        GameManager existingManager = FindObjectOfType<GameManager>();
        
        if (existingManager == null)
        {
            GameObject managerObj = new GameObject("GameManager");
            managerObj.AddComponent<GameManager>();
            Debug.Log("GameManager created.");
        }
    }
    
    void SetupUIManager()
    {
        UIManager existingUI = FindObjectOfType<UIManager>();
        
        if (existingUI == null)
        {
            GameObject uiObj = new GameObject("UIManager");
            uiObj.AddComponent<UIManager>();
            Debug.Log("UIManager created.");
        }
    }
    
    void SetupInputManager()
    {
        Unity6InputManager existingInput = FindObjectOfType<Unity6InputManager>();
        
        if (existingInput == null)
        {
            GameObject inputObj = new GameObject("Unity6InputManager");
            inputObj.AddComponent<Unity6InputManager>();
            Debug.Log("Unity6InputManager created - Legacy Input System for Unity 6 compatibility.");
        }
    }
    
    // AdaptivePerformanceManager not needed for Built-in Pipeline
    // Unity 6 Built-in Pipeline provides sufficient performance optimization
    
    void SetupObstacleSpawner()
    {
        ObstacleSpawner existingSpawner = FindObjectOfType<ObstacleSpawner>();
        
        if (existingSpawner == null)
        {
            GameObject spawnerObj = new GameObject("ObstacleSpawner");
            spawnerObj.AddComponent<ObstacleSpawner>();
            Debug.Log("ObstacleSpawner created.");
        }
    }
    
    void SetupCollectibleManager()
    {
        CollectibleManager existingCollectible = FindObjectOfType<CollectibleManager>();
        
        if (existingCollectible == null)
        {
            GameObject collectibleObj = new GameObject("CollectibleManager");
            collectibleObj.AddComponent<CollectibleManager>();
            Debug.Log("CollectibleManager created.");
        }
    }
    
    void SetupEnvironment()
    {
        // Create ground if it doesn't exist
        GameObject ground = GameObject.Find("Ground");
        
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0, 0, 0);
            ground.transform.localScale = new Vector3(6, 0.1f, 100);
            ground.tag = "Ground";
            
            // Set ground material using Built-in shader
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = Color.gray;
            ground.GetComponent<MeshRenderer>().material = groundMat;
            
            Debug.Log("Ground created.");
        }
        
        // Ensure directional light exists
        Light directionalLight = FindObjectOfType<Light>();
        
        if (directionalLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            directionalLight = lightObj.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Debug.Log("Directional light created.");
        }
    }
    
    void SetupTagsAndLayers()
    {
        // Note: In a real Unity project, you would set up tags and layers in the Editor
        // This is just documentation of what tags need to be created:
        
        Debug.Log("Required Tags to create in Editor:");
        Debug.Log("- Player");
        Debug.Log("- Obstacle");
        Debug.Log("- Collectible");
        Debug.Log("- Ball");
        Debug.Log("- Ground");
        
        Debug.Log("Required Layers to create in Editor:");
        Debug.Log("- Ground (layer 8)");
        Debug.Log("- Obstacles (layer 9)");
    }
    
    // Helper method to create basic ball prefab if none is assigned
    void CreateDefaultBallPrefab()
    {
        if (ballPrefab == null)
        {
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Ball";
            ball.transform.localScale = Vector3.one * 0.2f;
            ball.tag = "Ball";
            
            // Add components
            Rigidbody ballRb = ball.AddComponent<Rigidbody>();
            ballRb.useGravity = false;
            
            SphereCollider ballCol = ball.GetComponent<SphereCollider>();
            ballCol.isTrigger = true;
            
            ball.AddComponent<Ball>();
            
            // Set material using Built-in shader
            Material ballMat = new Material(Shader.Find("Standard"));
            ballMat.color = Color.cyan;
            ballMat.SetFloat("_Metallic", 0.5f);
            ballMat.SetFloat("_Glossiness", 0.8f);
            ball.GetComponent<MeshRenderer>().material = ballMat;
            
            ballPrefab = ball;
            
            // Update shooting systems
            ShootingSystem[] shootingSystems = FindObjectsOfType<ShootingSystem>();
            foreach (var system in shootingSystems)
            {
                if (system.ballPrefab == null)
                {
                    system.ballPrefab = ballPrefab;
                }
            }
        }
    }
    
    void OnValidate()
    {
        if (ballPrefab == null && Application.isPlaying)
        {
            CreateDefaultBallPrefab();
        }
    }
}