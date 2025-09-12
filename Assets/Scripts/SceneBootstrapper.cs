using UnityEngine;

[DefaultExecutionOrder(-1000)] // Execute very early to ensure proper initialization
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
            ValidateAndInitializeCamera();
            BootstrapScene();
        }
    }
    
    void ValidateAndInitializeCamera()
    {
        // Ensure camera exists and is properly configured before anything else
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = GameObject.Find("Main Camera");
            if (cameraObj == null)
            {
                cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.AddComponent<AudioListener>();
                cameraObj.tag = "MainCamera";
            }
            else
            {
                mainCamera = cameraObj.GetComponent<Camera>();
                if (mainCamera == null)
                {
                    mainCamera = cameraObj.AddComponent<Camera>();
                }
            }
        }
        
        // Ensure camera viewport is valid
        if (mainCamera.pixelWidth <= 0 || mainCamera.pixelHeight <= 0)
        {
            mainCamera.ResetAspect();
            mainCamera.rect = new Rect(0, 0, 1, 1);
        }
        
        Debug.Log($"✓ Camera validated - Viewport: {mainCamera.pixelWidth}x{mainCamera.pixelHeight}");
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
        // MouseEventSanitizer - MUST be created first to prevent frustum errors
        if (FindObjectOfType<MouseEventSanitizer>() == null)
        {
            GameObject mouseSanitizer = new GameObject("MouseEventSanitizer");
            mouseSanitizer.AddComponent<MouseEventSanitizer>();
            Debug.Log("✓ Created MouseEventSanitizer");
        }
        
        // SafeGUIRenderer - Created second for GUI safety
        if (FindObjectOfType<SafeGUIRenderer>() == null)
        {
            GameObject safeGui = new GameObject("SafeGUIRenderer");
            safeGui.AddComponent<SafeGUIRenderer>();
            Debug.Log("✓ Created SafeGUIRenderer");
        }
        
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
        ball.name = "BallPrefab";
        ball.transform.localScale = Vector3.one * 0.4f; // Bigger for visibility
        ball.tag = "Ball";
        
        // Add components
        Rigidbody ballRb = ball.AddComponent<Rigidbody>();
        ballRb.useGravity = false;
        ballRb.isKinematic = true; // Kinematic for controlled movement
        
        SphereCollider ballCol = ball.GetComponent<SphereCollider>();
        ballCol.isTrigger = true;
        ballCol.radius = 0.5f; // Match visual size
        
        Ball ballScript = ball.AddComponent<Ball>();
        
        // Set material - make it bright and emissive for visibility
        Material ballMat = new Material(Shader.Find("Standard"));
        ballMat.color = new Color(0f, 1f, 1f, 1f); // Bright cyan
        ballMat.SetFloat("_Metallic", 0.7f);
        ballMat.SetFloat("_Glossiness", 0.9f);
        ballMat.EnableKeyword("_EMISSION");
        ballMat.SetColor("_EmissionColor", new Color(0f, 0.5f, 0.5f)); // Slight glow
        ball.GetComponent<MeshRenderer>().material = ballMat;
        
        // Add trail for better visibility
        TrailRenderer trail = ball.AddComponent<TrailRenderer>();
        trail.time = 0.5f;
        trail.startWidth = 0.3f;
        trail.endWidth = 0.05f;
        trail.material = ballMat;
        trail.startColor = Color.cyan;
        trail.endColor = new Color(0, 1, 1, 0);
        
        // Deactivate it initially (will be instantiated when needed)
        ball.SetActive(false);
        
        Debug.Log("✓ Created Ball Prefab with boomerang behavior");
        return ball;
    }
    
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            // This should not happen as we validate camera in Awake
            Debug.LogError("Main camera missing after validation!");
            return;
        }
        
        // Verify camera is still valid
        if (mainCamera.pixelWidth <= 0 || mainCamera.pixelHeight <= 0)
        {
            mainCamera.ResetAspect();
            mainCamera.rect = new Rect(0, 0, 1, 1);
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
        
        // Force camera to update its matrices
        mainCamera.Render();
        
        Debug.Log($"✓ Camera configured - Position: {mainCamera.transform.position}, Viewport: {mainCamera.pixelWidth}x{mainCamera.pixelHeight}");
    }
    
    void ConfigureGameSettings()
    {
        // Mobile optimization
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        
        // Validate screen dimensions
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            Debug.LogWarning($"Invalid screen dimensions detected: {Screen.width}x{Screen.height}");
            // Force screen refresh
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, Screen.fullScreen);
        }
        
        // Start the game automatically
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // Increased delay to ensure everything is properly initialized
            Invoke(nameof(StartGame), 0.2f);
        }
        
        Debug.Log($"✓ Game settings configured - Screen: {Screen.width}x{Screen.height}");
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