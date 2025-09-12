using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    [Header("Collectible Settings")]
    public float spawnDistance = 20f;
    public float despawnDistance = 10f;
    public float spawnChance = 0.3f;
    public float spawnHeight = 1f;
    public float laneDistance = 2f;
    
    [Header("Collectible Movement")]
    public float rotationSpeed = 180f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    
    private Transform playerTransform;
    private float lastSpawnZ;
    private float spawnInterval = 3f;
    
    void Start()
    {
        playerTransform = FindObjectOfType<PlayerController>()?.transform;
        
        if (playerTransform == null)
        {
            Debug.LogError("PlayerController not found! CollectibleManager needs a player reference.");
            return;
        }
        
        lastSpawnZ = playerTransform.position.z;
        
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
        
        CheckForSpawn();
        CleanupOldCollectibles();
    }
    
    void CheckForSpawn()
    {
        if (playerTransform == null) return;
        
        float distanceTraveled = playerTransform.position.z - lastSpawnZ;
        
        if (distanceTraveled >= spawnInterval)
        {
            if (Random.value <= spawnChance)
            {
                SpawnCollectible();
            }
            
            lastSpawnZ = playerTransform.position.z;
        }
    }
    
    void SpawnCollectible()
    {
        // Choose random lane
        int lane = Random.Range(-1, 2); // -1, 0, or 1
        Vector3 spawnPosition = new Vector3(
            lane * laneDistance,
            spawnHeight,
            playerTransform.position.z + spawnDistance
        );
        
        // Create collectible
        GameObject collectible = CreateCollectible(spawnPosition);
        
        // Add collectible component
        Collectible collectibleComponent = collectible.AddComponent<Collectible>();
        collectibleComponent.Initialize(rotationSpeed, bobSpeed, bobHeight, spawnHeight);
    }
    
    GameObject CreateCollectible(Vector3 position)
    {
        // Create a simple coin-like collectible
        GameObject collectible = new GameObject("Coin");
        collectible.transform.position = position;
        collectible.tag = "Collectible";
        
        // Add visual components
        MeshFilter meshFilter = collectible.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = collectible.AddComponent<MeshRenderer>();
        
        // Create a cylinder mesh for coin shape
        meshFilter.mesh = CreateCoinMesh();
        
        // Create material using Built-in shader
        Material coinMaterial = new Material(Shader.Find("Standard"));
        coinMaterial.color = Color.yellow;
        coinMaterial.SetFloat("_Metallic", 0.7f);
        coinMaterial.SetFloat("_Glossiness", 0.9f);
        meshRenderer.material = coinMaterial;
        
        // Add trigger collider
        CapsuleCollider coinCollider = collectible.AddComponent<CapsuleCollider>();
        coinCollider.isTrigger = true;
        coinCollider.radius = 0.5f;
        coinCollider.height = 0.2f;
        
        // Scale the collectible
        collectible.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        
        return collectible;
    }
    
    Mesh CreateCoinMesh()
    {
        // Create a simple cylinder mesh for the coin
        GameObject tempCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Mesh coinMesh = tempCylinder.GetComponent<MeshFilter>().mesh;
        DestroyImmediate(tempCylinder);
        return coinMesh;
    }
    
    void CleanupOldCollectibles()
    {
        if (playerTransform == null) return;
        
        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        
        foreach (GameObject collectible in collectibles)
        {
            float distanceBehindPlayer = playerTransform.position.z - collectible.transform.position.z;
            
            if (distanceBehindPlayer > despawnDistance)
            {
                Destroy(collectible);
            }
        }
    }
    
    void OnGameStateChanged(bool isActive)
    {
        if (isActive)
        {
            // Reset spawning when game starts
            if (playerTransform != null)
            {
                lastSpawnZ = playerTransform.position.z;
            }
        }
        else
        {
            // Clean up all collectibles when game ends
            GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
            foreach (GameObject collectible in collectibles)
            {
                Destroy(collectible);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            // Draw spawn line
            Gizmos.color = Color.yellow;
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