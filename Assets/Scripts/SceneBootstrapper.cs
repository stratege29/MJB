using UnityEngine;

[DefaultExecutionOrder(-100)] // Execute before other scripts
public class SceneBootstrapper : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool setupOnAwake = true;
    public bool createPlayerIfMissing = true;
    public bool createManagersIfMissing = true;
    public bool createEnvironmentIfMissing = true;
    
    void Awake()
    {
        if (setupOnAwake)
        {
            Debug.Log("=== Scene Bootstrapper Starting ===");
            BootstrapScene();
        }
    }
    
    void BootstrapScene()
    {
        // 1. Create essential managers first
        if (createManagersIfMissing)
        {
            CreateEssentialManagers();
        }
        
        // 2. Create environment
        if (createEnvironmentIfMissing)
        {
            CreateEnvironment();
        }
        
        // 3. Create player
        if (createPlayerIfMissing)
        {
            CreatePlayer();
        }
        
        // 4. Setup camera
        SetupCamera();
        
        // 5. Configure game settings
        ConfigureGameSettings();
        
        Debug.Log("=== Scene Bootstrap Complete ===");
    }
    
    void CreateEssentialManagers()
    {
        // Unity6Initializer and InputSystemBypass (ensure they exist)
        if (FindObjectOfType<Unity6Initializer>() == null)
        {
            GameObject initializer = new GameObject("Unity6Initializer");
            initializer.AddComponent<Unity6Initializer>();
            Debug.Log("✓ Created Unity6Initializer");
        }
        
        if (FindObjectOfType<InputSystemBypass>() == null)
        {
            GameObject bypass = new GameObject("InputSystemBypass");
            bypass.AddComponent<InputSystemBypass>();
            Debug.Log("✓ Created InputSystemBypass");
        }
        
        // GameManager
        if (FindObjectOfType<GameManager>() == null)
        {
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameManager>();
            Debug.Log("✓ Created GameManager");
        }
        
        // UnityEventInputManager
        if (FindObjectOfType<UnityEventInputManager>() == null)
        {
            GameObject inputManager = new GameObject("UnityEventInputManager");
            inputManager.AddComponent<UnityEventInputManager>();
            Debug.Log("✓ Created UnityEventInputManager (Zero Input System conflicts)");
        }
        
        // UIManager
        if (FindObjectOfType<UIManager>() == null)
        {
            GameObject uiManager = new GameObject("UIManager");
            uiManager.AddComponent<UIManager>();
            Debug.Log("✓ Created UIManager");
        }
        
        // No Adaptive Performance Manager needed (using Built-in Pipeline)
        Debug.Log("✓ Built-in Pipeline - No Adaptive Performance needed");
        
        // ObstacleSpawner
        if (FindObjectOfType<ObstacleSpawner>() == null)
        {
            GameObject obstacleSpawner = new GameObject("ObstacleSpawner");
            obstacleSpawner.AddComponent<ObstacleSpawner>();
            Debug.Log("✓ Created ObstacleSpawner");
        }
        
        // CollectibleManager
        if (FindObjectOfType<CollectibleManager>() == null)
        {
            GameObject collectibleManager = new GameObject("CollectibleManager");
            collectibleManager.AddComponent<CollectibleManager>();
            Debug.Log("✓ Created CollectibleManager");
        }
    }
    
    void CreateEnvironment()
    {
        // Create ground/path
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0, -0.5f, 0);
            ground.transform.localScale = new Vector3(6, 1, 100);
            ground.tag = "Ground";
            
            // Use Built-in material (no URP dependencies)
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.8f, 0.7f, 0.6f); // Sandy color
            ground.GetComponent<MeshRenderer>().material = groundMat;
            
            Debug.Log("✓ Created Ground");
        }
        
        // Create walls/lanes
        CreateLaneMarkers();
        
        // Add infinite ground system
        CreateInfiniteGround();
        
        // Ensure lighting
        Light directionalLight = FindObjectOfType<Light>();
        if (directionalLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            directionalLight = lightObj.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            directionalLight.intensity = 1.5f;
            Debug.Log("✓ Created Directional Light");
        }
    }
    
    void CreateLaneMarkers()
    {
        // Create visual lane markers so player can see the lanes
        for (int i = -1; i <= 1; i++)
        {
            GameObject laneMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            laneMarker.name = $"LaneMarker_{i}";
            laneMarker.transform.position = new Vector3(i * 2f, 0.1f, 5f);
            laneMarker.transform.localScale = new Vector3(0.2f, 0.2f, 50f);
            
            Material markerMat = new Material(Shader.Find("Standard"));
            markerMat.color = Color.white;
            laneMarker.GetComponent<MeshRenderer>().material = markerMat;
            
            // Remove collider so it doesn't interfere with gameplay
            DestroyImmediate(laneMarker.GetComponent<BoxCollider>());
        }
        Debug.Log("✓ Created Lane Markers");
    }
    
    void CreateInfiniteGround()
    {
        // Add infinite ground component to an empty GameObject
        GameObject infiniteGroundObj = new GameObject("InfiniteGround");
        infiniteGroundObj.AddComponent<InfiniteGround>();
        Debug.Log("✓ Created Infinite Ground System");
    }
    
    void CreatePlayer()
    {
        if (FindObjectOfType<PlayerController>() == null)
        {
            // Create player capsule
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0, 1f, 0);
            player.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            player.tag = "Player";
            
            // Add Rigidbody
            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.mass = 1f;
            
            // Add PlayerController
            PlayerController playerController = player.AddComponent<PlayerController>();
            
            // Add ShootingSystem
            ShootingSystem shootingSystem = player.AddComponent<ShootingSystem>();
            
            // Create and assign ball prefab
            GameObject ballPrefab = CreateBallPrefab();
            shootingSystem.ballPrefab = ballPrefab;
            
            // Set player material (African-inspired colors)
            Material playerMat = new Material(Shader.Find("Standard"));
            playerMat.color = new Color(0.4f, 0.2f, 0.1f); // Brown skin tone
            player.GetComponent<MeshRenderer>().material = playerMat;
            
            Debug.Log("✓ Created Player with components");
        }
    }
    
    GameObject CreateBallPrefab()
    {
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Ball";
        ball.transform.localScale = Vector3.one * 0.3f;
        ball.tag = "Ball";
        
        // Add components
        Rigidbody ballRb = ball.AddComponent<Rigidbody>();
        ballRb.useGravity = false;
        
        SphereCollider ballCol = ball.GetComponent<SphereCollider>();
        ballCol.isTrigger = true;
        
        ball.AddComponent<Ball>();
        
        // Set material
        Material ballMat = new Material(Shader.Find("Standard"));
        ballMat.color = Color.cyan;
        ballMat.SetFloat("_Metallic", 0.3f);
        ballMat.SetFloat("_Glossiness", 0.7f);
        ball.GetComponent<MeshRenderer>().material = ballMat;
        
        // Deactivate it initially (will be instantiated when needed)
        ball.SetActive(false);
        
        Debug.Log("✓ Created Ball Prefab");
        return ball;
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
        
        // Add CameraFollow if missing
        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
        }
        
        // Set camera position for third-person view
        mainCamera.transform.position = new Vector3(0, 3f, -6f);
        mainCamera.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
        
        Debug.Log("✓ Camera configured");
    }
    
    void ConfigureGameSettings()
    {
        // Mobile optimization
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        
        // Start the game automatically
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // Small delay to ensure everything is initialized
            Invoke(nameof(StartGame), 0.1f);
        }
        
        Debug.Log("✓ Game settings configured");
    }
    
    void StartGame()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.StartGame();
            Debug.Log("✓ Game started automatically");
        }
    }
}