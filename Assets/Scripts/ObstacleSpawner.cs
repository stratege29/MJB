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