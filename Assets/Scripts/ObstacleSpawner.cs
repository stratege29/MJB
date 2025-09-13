using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public float spawnDistance = 20f;
    public float despawnDistance = 10f;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;
    public float intervalDecreaseRate = 0.05f;
    public float minInterval = 0.5f;
    
    [Header("Obstacle Settings")]
    public float laneDistance = 2f;
    public float obstacleHeight = 1f;
    
    [Header("Obstacle Types")]
    public ObstacleType[] obstacleTypes;
    
    [Header("Urban Obstacle Types")]
    public UrbanObstacleConfiguration[] urbanObstacleTypes;
    
    private Transform playerTransform;
    private float nextSpawnTime;
    private float currentSpawnInterval;
    private float gameStartTime;
    
    [System.Serializable]
    public class ObstacleType
    {
        public string name;
        public GameObject prefab;
        public float spawnWeight = 1f;
        public bool canBeDestroyed = true;
        public float height = 1f;
    }
    
    [System.Serializable]
    public class UrbanObstacleConfiguration
    {
        public string name;
        public UrbanObstacleType urbanType;
        public GameObject prefab;
        public float spawnWeight = 1f;
        public Vector3 scale = Vector3.one;
        public MovementPattern movementPattern = MovementPattern.Static;
        public CollisionBehavior collisionBehavior = CollisionBehavior.Destroyable;
        public bool preferredLane = false; // If true, spawns in specific lanes only
        public int[] allowedLanes = { 0, 1, 2 }; // Which lanes this obstacle can spawn in
        public int minDifficultyLevel = 0; // Minimum difficulty to spawn this obstacle
    }
    
    void Start()
    {
        playerTransform = FindObjectOfType<PlayerController>()?.transform;
        
        if (playerTransform == null)
        {
            Debug.LogError("PlayerController not found! ObstacleSpawner needs a player reference.");
            return;
        }
        
        // Initialize spawning
        currentSpawnInterval = maxSpawnInterval;
        nextSpawnTime = Time.time + currentSpawnInterval;
        gameStartTime = Time.time;
        
        // Create default obstacle types if none assigned
        if (obstacleTypes == null || obstacleTypes.Length == 0)
        {
            CreateDefaultObstacleTypes();
        }
        
        // Create default urban obstacle types if none assigned
        if (urbanObstacleTypes == null || urbanObstacleTypes.Length == 0)
        {
            CreateDefaultUrbanObstacleTypes();
        }
        
        // Subscribe to game state changes
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }
    
    void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
    
    void Update()
    {
        if (!GameManager.Instance.IsGameActive) return;
        
        UpdateSpawnTiming();
        
        if (Time.time >= nextSpawnTime)
        {
            SpawnObstacle();
            ScheduleNextSpawn();
        }
        
        CleanupOldObstacles();
    }
    
    void UpdateSpawnTiming()
    {
        float gameTime = Time.time - gameStartTime;
        currentSpawnInterval = Mathf.Max(
            maxSpawnInterval - (gameTime * intervalDecreaseRate), 
            minInterval
        );
    }
    
    void SpawnObstacle()
    {
        if (playerTransform == null) return;
        
        // Decide whether to spawn urban or regular obstacle (80% urban, 20% regular)
        bool spawnUrban = Random.Range(0f, 1f) < 0.8f;
        
        if (spawnUrban && urbanObstacleTypes != null && urbanObstacleTypes.Length > 0)
        {
            SpawnUrbanObstacle();
        }
        else
        {
            SpawnRegularObstacle();
        }
    }
    
    void SpawnUrbanObstacle()
    {
        // Get current difficulty level based on game time
        int difficultyLevel = GetCurrentDifficultyLevel();
        
        // Filter urban obstacles by difficulty
        var availableObstacles = System.Array.FindAll(urbanObstacleTypes, 
            o => o.minDifficultyLevel <= difficultyLevel);
        
        if (availableObstacles.Length == 0)
        {
            // Fallback to regular obstacle
            SpawnRegularObstacle();
            return;
        }
        
        // Choose random urban obstacle type
        UrbanObstacleConfiguration urbanConfig = GetRandomUrbanObstacleType(availableObstacles);
        
        // Choose appropriate lane
        int lane = ChooseLaneForObstacle(urbanConfig);
        
        Vector3 spawnPosition = new Vector3(
            lane * laneDistance,
            obstacleHeight * 0.5f,
            playerTransform.position.z + spawnDistance
        );
        
        if (urbanConfig.prefab != null)
        {
            GameObject obstacle = Instantiate(urbanConfig.prefab, spawnPosition, Quaternion.identity);
            SetupUrbanObstacle(obstacle, urbanConfig);
        }
        else
        {
            CreateDefaultUrbanObstacle(spawnPosition, urbanConfig);
        }
    }
    
    void SpawnRegularObstacle()
    {
        // Choose random lane
        int lane = Random.Range(-1, 2); // -1, 0, or 1
        Vector3 spawnPosition = new Vector3(
            lane * laneDistance,
            obstacleHeight * 0.5f,
            playerTransform.position.z + spawnDistance
        );
        
        // Choose random obstacle type
        ObstacleType obstacleType = GetRandomObstacleType();
        
        if (obstacleType.prefab != null)
        {
            GameObject obstacle = Instantiate(obstacleType.prefab, spawnPosition, Quaternion.identity);
            
            // Ensure obstacle has proper tag and components
            SetupObstacle(obstacle, obstacleType);
        }
        else
        {
            // Create default obstacle if no prefab
            CreateDefaultObstacle(spawnPosition, obstacleType);
        }
    }
    
    void SetupObstacle(GameObject obstacle, ObstacleType obstacleType)
    {
        // Ensure proper tag
        obstacle.tag = "Obstacle";
        
        // Add collider if missing
        if (obstacle.GetComponent<Collider>() == null)
        {
            BoxCollider boxCollider = obstacle.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }
        
        // Add obstacle component if missing
        Obstacle obstacleComponent = obstacle.GetComponent<Obstacle>();
        if (obstacleComponent == null)
        {
            obstacleComponent = obstacle.AddComponent<Obstacle>();
        }
        
        // No need to set canBeDestroyed - obstacle type is set in CreateDefaultObstacle
    }
    
    void CreateDefaultObstacle(Vector3 position, ObstacleType obstacleType)
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.transform.position = position;
        obstacle.transform.localScale = new Vector3(1f, obstacleType.height, 1f);
        
        SetupObstacle(obstacle, obstacleType);
        
        // Configure obstacle type based on name
        Obstacle obstacleComponent = obstacle.GetComponent<Obstacle>();
        if (obstacleComponent != null)
        {
            if (obstacleType.name.Contains("Weak"))
            {
                obstacleComponent.obstacleType = global::ObstacleType.Weak;
            }
            else if (obstacleType.name.Contains("Strong") || obstacleType.name.Contains("Metal"))
            {
                obstacleComponent.obstacleType = global::ObstacleType.Strong;
            }
            else if (obstacleType.name.Contains("Reinforced"))
            {
                obstacleComponent.obstacleType = global::ObstacleType.Reinforced;
            }
            
            // This will trigger the visual setup
            obstacle.SetActive(false);
            obstacle.SetActive(true);
        }
    }
    
    ObstacleType GetRandomObstacleType()
    {
        if (obstacleTypes == null || obstacleTypes.Length == 0)
            return new ObstacleType { name = "Default", canBeDestroyed = true, height = 1f };
        
        float totalWeight = 0f;
        foreach (var type in obstacleTypes)
        {
            totalWeight += type.spawnWeight;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var type in obstacleTypes)
        {
            currentWeight += type.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return type;
            }
        }
        
        return obstacleTypes[0];
    }
    
    int GetCurrentDifficultyLevel()
    {
        float gameTime = Time.time - gameStartTime;
        // Increase difficulty every 30 seconds
        return Mathf.FloorToInt(gameTime / 30f);
    }
    
    UrbanObstacleConfiguration GetRandomUrbanObstacleType(UrbanObstacleConfiguration[] availableObstacles)
    {
        if (availableObstacles == null || availableObstacles.Length == 0)
            return null;
        
        float totalWeight = 0f;
        foreach (var config in availableObstacles)
        {
            totalWeight += config.spawnWeight;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var config in availableObstacles)
        {
            currentWeight += config.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return config;
            }
        }
        
        return availableObstacles[0];
    }
    
    int ChooseLaneForObstacle(UrbanObstacleConfiguration config)
    {
        if (config.preferredLane && config.allowedLanes != null && config.allowedLanes.Length > 0)
        {
            int laneIndex = config.allowedLanes[Random.Range(0, config.allowedLanes.Length)];
            return laneIndex - 1; // Convert from 0,1,2 to -1,0,1
        }
        else
        {
            return Random.Range(-1, 2); // -1, 0, or 1
        }
    }
    
    void SetupUrbanObstacle(GameObject obstacle, UrbanObstacleConfiguration config)
    {
        // Ensure proper tag
        obstacle.tag = "Obstacle";
        
        // Apply scale
        obstacle.transform.localScale = config.scale;
        
        // Add collider if missing
        if (obstacle.GetComponent<Collider>() == null)
        {
            BoxCollider boxCollider = obstacle.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }
        
        // Add urban obstacle component if missing
        UrbanObstacle urbanObstacleComponent = obstacle.GetComponent<UrbanObstacle>();
        if (urbanObstacleComponent == null)
        {
            urbanObstacleComponent = obstacle.AddComponent<UrbanObstacle>();
        }
        
        // Configure the urban obstacle
        urbanObstacleComponent.urbanType = config.urbanType;
        urbanObstacleComponent.movementPattern = config.movementPattern;
        urbanObstacleComponent.collisionBehavior = config.collisionBehavior;
        
        // Add movement component for dynamic obstacles
        if (config.movementPattern != MovementPattern.Static)
        {
            ObstacleMovement movementComponent = obstacle.GetComponent<ObstacleMovement>();
            if (movementComponent == null)
            {
                movementComponent = obstacle.AddComponent<ObstacleMovement>();
            }
            ConfigureMovementComponent(movementComponent, config);
        }
        
        // Add effects component
        ObstacleEffects effectsComponent = obstacle.GetComponent<ObstacleEffects>();
        if (effectsComponent == null)
        {
            effectsComponent = obstacle.AddComponent<ObstacleEffects>();
        }
    }
    
    void ConfigureMovementComponent(ObstacleMovement movement, UrbanObstacleConfiguration config)
    {
        switch (config.movementPattern)
        {
            case MovementPattern.CrossingLanes:
                movement.movementType = MovementType.Linear;
                movement.direction = Vector3.right;
                movement.speed = 3f;
                movement.destroyOnReachEnd = true;
                movement.crossLanes = true;
                break;
                
            case MovementPattern.Patrol:
                movement.movementType = MovementType.Linear;
                movement.direction = Vector3.right;
                movement.speed = 1.5f;
                movement.loopMovement = true;
                movement.destroyOnReachEnd = false;
                break;
                
            case MovementPattern.Rolling:
                // Rolling obstacles use physics, configured in UrbanObstacle
                movement.enabled = false;
                break;
        }
    }
    
    void CreateDefaultUrbanObstacle(Vector3 position, UrbanObstacleConfiguration config)
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.transform.position = position;
        obstacle.transform.localScale = config.scale;
        
        SetupUrbanObstacle(obstacle, config);
    }
    
    void ScheduleNextSpawn()
    {
        float randomizedInterval = Random.Range(currentSpawnInterval * 0.8f, currentSpawnInterval * 1.2f);
        nextSpawnTime = Time.time + randomizedInterval;
    }
    
    void CleanupOldObstacles()
    {
        if (playerTransform == null) return;
        
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        
        foreach (GameObject obstacle in obstacles)
        {
            float distanceBehindPlayer = playerTransform.position.z - obstacle.transform.position.z;
            
            if (distanceBehindPlayer > despawnDistance)
            {
                Destroy(obstacle);
            }
        }
    }
    
    void CreateDefaultObstacleTypes()
    {
        obstacleTypes = new ObstacleType[]
        {
            // Weak obstacles - common, destroyed by any shot
            new ObstacleType 
            { 
                name = "WeakCrate", 
                spawnWeight = 3f, 
                canBeDestroyed = true, 
                height = 0.8f 
            },
            new ObstacleType 
            { 
                name = "WeakBarrel", 
                spawnWeight = 2f, 
                canBeDestroyed = true, 
                height = 1f 
            },
            
            // Strong obstacles - less common, need charged shots
            new ObstacleType 
            { 
                name = "StrongBlock", 
                spawnWeight = 1f, 
                canBeDestroyed = true, 
                height = 1.2f 
            },
            new ObstacleType 
            { 
                name = "MetalWall", 
                spawnWeight = 0.8f, 
                canBeDestroyed = true, 
                height = 1.5f 
            },
            
            // Reinforced obstacles - rare, need multiple charged shots
            new ObstacleType 
            { 
                name = "ReinforcedFortress", 
                spawnWeight = 0.2f, 
                canBeDestroyed = true, 
                height = 2f 
            }
        };
    }
    
    void CreateDefaultUrbanObstacleTypes()
    {
        urbanObstacleTypes = new UrbanObstacleConfiguration[]
        {
            // Static Urban Obstacles
            new UrbanObstacleConfiguration
            {
                name = "Plastic Trash Bin",
                urbanType = UrbanObstacleType.TrashBin,
                spawnWeight = 3f,
                scale = new Vector3(0.8f, 1f, 0.8f),
                movementPattern = MovementPattern.Static,
                collisionBehavior = CollisionBehavior.Destroyable,
                minDifficultyLevel = 0
            },
            new UrbanObstacleConfiguration
            {
                name = "Metal Trash Bin",
                urbanType = UrbanObstacleType.TrashBin,
                spawnWeight = 2f,
                scale = new Vector3(0.9f, 1.2f, 0.9f),
                movementPattern = MovementPattern.Static,
                collisionBehavior = CollisionBehavior.Destroyable,
                minDifficultyLevel = 1
            },
            new UrbanObstacleConfiguration
            {
                name = "Street Sign",
                urbanType = UrbanObstacleType.StreetSign,
                spawnWeight = 2.5f,
                scale = new Vector3(0.3f, 1.5f, 0.3f),
                movementPattern = MovementPattern.Wobbling,
                collisionBehavior = CollisionBehavior.Destroyable,
                minDifficultyLevel = 0
            },
            new UrbanObstacleConfiguration
            {
                name = "Vendor Cart",
                urbanType = UrbanObstacleType.VendorCart,
                spawnWeight = 1.5f,
                scale = new Vector3(1.2f, 0.8f, 2f),
                movementPattern = MovementPattern.Static,
                collisionBehavior = CollisionBehavior.Destroyable,
                minDifficultyLevel = 0
            },
            new UrbanObstacleConfiguration
            {
                name = "Fire Hydrant",
                urbanType = UrbanObstacleType.FireHydrant,
                spawnWeight = 1f,
                scale = new Vector3(0.6f, 1f, 0.6f),
                movementPattern = MovementPattern.Static,
                collisionBehavior = CollisionBehavior.Destroyable,
                minDifficultyLevel = 1
            },
            new UrbanObstacleConfiguration
            {
                name = "Parked Car",
                urbanType = UrbanObstacleType.ParkedCar,
                spawnWeight = 0.8f,
                scale = new Vector3(1.8f, 1.2f, 4f),
                movementPattern = MovementPattern.Static,
                collisionBehavior = CollisionBehavior.Indestructible,
                preferredLane = true,
                allowedLanes = new int[] { 0, 2 }, // Side lanes only
                minDifficultyLevel = 2
            },
            new UrbanObstacleConfiguration
            {
                name = "Flower Pot",
                urbanType = UrbanObstacleType.FlowerPot,
                spawnWeight = 2f,
                scale = new Vector3(0.5f, 0.6f, 0.5f),
                movementPattern = MovementPattern.Static,
                collisionBehavior = CollisionBehavior.Destroyable,
                minDifficultyLevel = 0
            },
            
            // Dynamic Urban Obstacles
            new UrbanObstacleConfiguration
            {
                name = "Delivery Scooter",
                urbanType = UrbanObstacleType.DeliveryScooter,
                spawnWeight = 1f,
                scale = new Vector3(0.8f, 1.2f, 1.5f),
                movementPattern = MovementPattern.CrossingLanes,
                collisionBehavior = CollisionBehavior.Avoidable,
                minDifficultyLevel = 1
            },
            new UrbanObstacleConfiguration
            {
                name = "Stray Cat",
                urbanType = UrbanObstacleType.StrayCat,
                spawnWeight = 1.5f,
                scale = new Vector3(0.4f, 0.3f, 0.7f),
                movementPattern = MovementPattern.Patrol,
                collisionBehavior = CollisionBehavior.Avoidable,
                minDifficultyLevel = 0
            },
            new UrbanObstacleConfiguration
            {
                name = "Shopping Cart",
                urbanType = UrbanObstacleType.ShoppingCart,
                spawnWeight = 1.2f,
                scale = new Vector3(0.8f, 1f, 1.2f),
                movementPattern = MovementPattern.Rolling,
                collisionBehavior = CollisionBehavior.Destroyable,
                minDifficultyLevel = 1
            },
            new UrbanObstacleConfiguration
            {
                name = "Skateboarder",
                urbanType = UrbanObstacleType.Skateboarder,
                spawnWeight = 0.8f,
                scale = new Vector3(0.6f, 1.7f, 0.3f),
                movementPattern = MovementPattern.CrossingLanes,
                collisionBehavior = CollisionBehavior.Avoidable,
                minDifficultyLevel = 2
            },
            new UrbanObstacleConfiguration
            {
                name = "Pigeons",
                urbanType = UrbanObstacleType.Pigeon,
                spawnWeight = 1f,
                scale = new Vector3(0.3f, 0.2f, 0.3f),
                movementPattern = MovementPattern.Flying,
                collisionBehavior = CollisionBehavior.Avoidable,
                minDifficultyLevel = 0
            }
        };
    }
    
    void OnGameStateChanged(bool isActive)
    {
        if (isActive)
        {
            // Reset spawning when game starts
            gameStartTime = Time.time;
            currentSpawnInterval = maxSpawnInterval;
            nextSpawnTime = Time.time + currentSpawnInterval;
        }
        else
        {
            // Clean up all obstacles when game ends
            GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
            foreach (GameObject obstacle in obstacles)
            {
                Destroy(obstacle);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            // Draw spawn line
            Gizmos.color = Color.green;
            Vector3 spawnLine = playerTransform.position + Vector3.forward * spawnDistance;
            Gizmos.DrawLine(
                spawnLine + Vector3.left * laneDistance,
                spawnLine + Vector3.right * laneDistance
            );
            
            // Draw despawn line
            Gizmos.color = Color.red;
            Vector3 despawnLine = playerTransform.position - Vector3.forward * despawnDistance;
            Gizmos.DrawLine(
                despawnLine + Vector3.left * laneDistance,
                despawnLine + Vector3.right * laneDistance
            );
        }
    }
}