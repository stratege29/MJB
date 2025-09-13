using UnityEngine;
using System.Collections;

public enum UrbanObstacleType
{
    // Static obstacles
    TrashBin,
    StreetSign,
    VendorCart,
    FireHydrant,
    ConstructionBarrier,
    ParkedCar,
    FlowerPot,
    BusStop,
    StreetLamp,
    
    // Dynamic obstacles
    DeliveryScooter,
    StrayCat,
    ShoppingCart,
    Skateboarder,
    Pigeon,
    
    // Environmental
    OpenManhole,
    WaterPuddle,
    MarketStall
}

public enum MovementPattern
{
    Static,
    CrossingLanes,
    Rolling,
    Patrol,
    Flying,
    Wobbling
}

public enum CollisionBehavior
{
    Destroyable,
    Indestructible,
    Bouncy,
    Passthrough,
    Avoidable
}

[System.Serializable]
public class ObstacleVisualState
{
    public Material pristineMaterial;
    public Material damagedMaterial;
    public Material criticalMaterial;
    public GameObject destructionParticles;
    public AudioClip impactSound;
    public AudioClip destructionSound;
}

public class UrbanObstacle : MonoBehaviour
{
    [Header("Urban Obstacle Settings")]
    public UrbanObstacleType urbanType = UrbanObstacleType.TrashBin;
    public MovementPattern movementPattern = MovementPattern.Static;
    public CollisionBehavior collisionBehavior = CollisionBehavior.Destroyable;
    
    [Header("Movement Settings")]
    public float movementSpeed = 2f;
    public float patrolDistance = 5f;
    public float crossingSpeed = 3f;
    public float wobbleAmount = 0.5f;
    public float wobbleSpeed = 2f;
    
    [Header("Special Effects")]
    public bool dropCoins = false;
    public int coinDropChance = 30; // Percentage
    public int coinDropAmount = 3;
    public GameObject coinPrefab;
    
    [Header("Chain Reaction")]
    public bool canTriggerChainReaction = false;
    public float chainReactionRadius = 3f;
    public float chainReactionDelay = 0.1f;
    
    [Header("Visual States")]
    public ObstacleVisualState visualState;
    
    // Movement variables
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool movingForward = true;
    private float wobbleTimer = 0f;
    private Rigidbody rb;
    private bool isRolling = false;
    
    // Audio
    private AudioSource audioSource;
    
    // Base obstacle properties 
    [Header("Base Obstacle Properties")]
    public ObstacleType obstacleType = ObstacleType.Weak;
    public int maxHealth = 1;
    public int scoreValue = 5;
    public float destructionForce = 5f;
    
    protected int currentHealth;
    protected bool isDestroyed = false;
    protected Material originalMaterial;
    protected MeshRenderer meshRenderer;
    
    void Start()
    {
        // Initialize base obstacle properties
        currentHealth = maxHealth;
        
        // Setup based on obstacle type  
        SetupObstacleType();
        
        // Ensure proper setup
        if (GetComponent<Collider>() == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }
        
        // Ensure proper tag
        if (!gameObject.CompareTag("Obstacle"))
        {
            gameObject.tag = "Obstacle";
        }
        
        // Get mesh renderer for visual effects
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }
        
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        SetupUrbanObstacleType();
        InitializeMovement();
        
        // Initialize visual state if not set
        if (visualState == null)
        {
            visualState = new ObstacleVisualState();
        }
    }
    
    void SetObstacleColor(Color color)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return;
        
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        
        // Add metallic effect for strong obstacles
        if (obstacleType == ObstacleType.Strong)
        {
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Glossiness", 0.9f);
        }
        else if (obstacleType == ObstacleType.Reinforced)
        {
            mat.SetFloat("_Metallic", 0.6f);
            mat.SetFloat("_Glossiness", 0.8f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.3f);
        }
        
        renderer.material = mat;
    }
    
    void SetupObstacleType()
    {
        switch (obstacleType)
        {
            case ObstacleType.Weak:
                maxHealth = 1;
                scoreValue = 5;
                transform.localScale = Vector3.one * 0.8f;
                SetObstacleColor(Color.green);
                break;
                
            case ObstacleType.Strong:
                maxHealth = 2;
                scoreValue = 15;
                transform.localScale = Vector3.one * 1.2f;
                SetObstacleColor(Color.red);
                break;
                
            case ObstacleType.Reinforced:
                maxHealth = 3;
                scoreValue = 30;
                transform.localScale = Vector3.one * 1.5f;
                SetObstacleColor(new Color(0.5f, 0f, 1f)); // Purple
                break;
        }
        
        currentHealth = maxHealth;
    }
    
    void SetupUrbanObstacleType()
    {
        switch (urbanType)
        {
            case UrbanObstacleType.TrashBin:
                SetupTrashBin();
                break;
            case UrbanObstacleType.StreetSign:
                SetupStreetSign();
                break;
            case UrbanObstacleType.VendorCart:
                SetupVendorCart();
                break;
            case UrbanObstacleType.FireHydrant:
                SetupFireHydrant();
                break;
            case UrbanObstacleType.ParkedCar:
                SetupParkedCar();
                break;
            case UrbanObstacleType.DeliveryScooter:
                SetupDeliveryScooter();
                break;
            case UrbanObstacleType.StrayCat:
                SetupStrayCat();
                break;
            case UrbanObstacleType.ShoppingCart:
                SetupShoppingCart();
                break;
            default:
                SetupDefaultUrbanObstacle();
                break;
        }
    }
    
    void SetupTrashBin()
    {
        if (obstacleType == ObstacleType.Weak)
        {
            // Plastic trash bin
            maxHealth = 1;
            scoreValue = 5;
            dropCoins = true;
            coinDropChance = 20;
            SetObstacleColor(new Color(0.2f, 0.8f, 0.2f)); // Green
        }
        else
        {
            // Metal trash bin
            maxHealth = 2;
            scoreValue = 10;
            dropCoins = true;
            coinDropChance = 30;
            SetObstacleColor(Color.gray);
        }
        movementPattern = MovementPattern.Static;
        collisionBehavior = CollisionBehavior.Destroyable;
    }
    
    void SetupStreetSign()
    {
        obstacleType = ObstacleType.Strong;
        maxHealth = 2;
        scoreValue = 15;
        movementPattern = MovementPattern.Wobbling;
        collisionBehavior = CollisionBehavior.Destroyable;
        wobbleAmount = 0.3f;
        SetObstacleColor(new Color(1f, 1f, 0f)); // Yellow
    }
    
    void SetupVendorCart()
    {
        obstacleType = ObstacleType.Weak;
        maxHealth = 1;
        scoreValue = 20;
        dropCoins = true;
        coinDropChance = 50;
        coinDropAmount = 5;
        movementPattern = MovementPattern.Static;
        collisionBehavior = CollisionBehavior.Destroyable;
        canTriggerChainReaction = true;
        SetObstacleColor(new Color(0.8f, 0.5f, 0.2f)); // Brown
    }
    
    void SetupFireHydrant()
    {
        obstacleType = ObstacleType.Strong;
        maxHealth = 2;
        scoreValue = 25;
        movementPattern = MovementPattern.Static;
        collisionBehavior = CollisionBehavior.Destroyable;
        SetObstacleColor(Color.red);
    }
    
    void SetupParkedCar()
    {
        obstacleType = ObstacleType.Reinforced;
        maxHealth = 999; // Essentially indestructible
        scoreValue = 0;
        movementPattern = MovementPattern.Static;
        collisionBehavior = CollisionBehavior.Indestructible;
        transform.localScale = new Vector3(1.5f, 1f, 2f);
        SetObstacleColor(new Color(0.2f, 0.3f, 0.8f)); // Blue car
    }
    
    void SetupDeliveryScooter()
    {
        obstacleType = ObstacleType.Strong;
        maxHealth = 999; // Can't be destroyed
        scoreValue = 10; // Points for avoiding
        movementPattern = MovementPattern.CrossingLanes;
        collisionBehavior = CollisionBehavior.Avoidable;
        crossingSpeed = 5f;
        SetObstacleColor(new Color(1f, 0.5f, 0f)); // Orange
    }
    
    void SetupStrayCat()
    {
        obstacleType = ObstacleType.Weak;
        maxHealth = 999; // Can't be destroyed
        scoreValue = 5; // Points for avoiding
        movementPattern = MovementPattern.Patrol;
        collisionBehavior = CollisionBehavior.Avoidable;
        movementSpeed = 1.5f;
        patrolDistance = 3f;
        transform.localScale = Vector3.one * 0.5f;
        SetObstacleColor(new Color(0.3f, 0.3f, 0.3f)); // Gray cat
    }
    
    void SetupShoppingCart()
    {
        obstacleType = ObstacleType.Strong;
        maxHealth = 2;
        scoreValue = 15;
        movementPattern = MovementPattern.Rolling;
        collisionBehavior = CollisionBehavior.Destroyable;
        SetObstacleColor(Color.gray);
        
        // Add rigidbody for rolling physics
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.mass = 2f;
        rb.linearDamping = 0.5f;
    }
    
    void SetupDefaultUrbanObstacle()
    {
        // Default setup for any unspecified urban obstacle
        maxHealth = 1;
        scoreValue = 10;
        movementPattern = MovementPattern.Static;
        collisionBehavior = CollisionBehavior.Destroyable;
    }
    
    void InitializeMovement()
    {
        switch (movementPattern)
        {
            case MovementPattern.Patrol:
                targetPosition = startPosition + Vector3.right * patrolDistance;
                break;
            case MovementPattern.CrossingLanes:
                targetPosition = startPosition + Vector3.right * 6f; // Cross all lanes
                break;
        }
    }
    
    void Update()
    {
        
        // Handle movement patterns
        switch (movementPattern)
        {
            case MovementPattern.Patrol:
                HandlePatrolMovement();
                break;
            case MovementPattern.CrossingLanes:
                HandleCrossingMovement();
                break;
            case MovementPattern.Wobbling:
                HandleWobbling();
                break;
            case MovementPattern.Rolling:
                HandleRolling();
                break;
        }
    }
    
    void HandlePatrolMovement()
    {
        Vector3 direction = movingForward ? targetPosition - transform.position : startPosition - transform.position;
        
        if (direction.magnitude > 0.1f)
        {
            transform.position += direction.normalized * movementSpeed * Time.deltaTime;
        }
        else
        {
            movingForward = !movingForward;
        }
    }
    
    void HandleCrossingMovement()
    {
        transform.position += Vector3.right * crossingSpeed * Time.deltaTime;
        
        // Destroy when out of bounds
        if (Mathf.Abs(transform.position.x) > 10f)
        {
            Destroy(gameObject);
        }
    }
    
    void HandleWobbling()
    {
        wobbleTimer += Time.deltaTime * wobbleSpeed;
        float wobble = Mathf.Sin(wobbleTimer) * wobbleAmount;
        transform.rotation = Quaternion.Euler(0, 0, wobble * 10f);
    }
    
    void HandleRolling()
    {
        if (isRolling && rb != null)
        {
            // Apply continuous forward force when rolling
            rb.AddForce(Vector3.forward * 2f, ForceMode.Force);
            
            // Add rotation for visual effect
            transform.Rotate(Vector3.right * 100f * Time.deltaTime);
        }
    }
    
    public bool TakeDamage(int damage, bool isChargedShot)
    {
        // Check if obstacle can be damaged
        if (collisionBehavior == CollisionBehavior.Indestructible || 
            collisionBehavior == CollisionBehavior.Avoidable)
        {
            if (collisionBehavior == CollisionBehavior.Avoidable)
            {
                // Make avoidable obstacles dodge
                StartCoroutine(DodgeAnimation());
            }
            else
            {
                // Bounce effect for indestructible
                CreateBounceEffect();
            }
            return false;
        }
        
        // Apply damage
        currentHealth -= damage;
        Debug.Log($"Urban obstacle took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Update visual state based on health
        UpdateVisualState();
        
        // Start rolling if hit and is a rolling type
        if (movementPattern == MovementPattern.Rolling && !isRolling)
        {
            StartRolling();
        }
        
        // Visual damage feedback
        bool destroyed = false;
        if (currentHealth > 0)
        {
            ShowDamageEffect();
        }
        else
        {
            destroyed = true;
            // Trigger chain reaction if destroyed
            if (canTriggerChainReaction)
            {
                StartCoroutine(TriggerChainReaction());
            }
            DestroyObstacle();
        }
        
        return destroyed;
    }
    
    void ShowDamageEffect()
    {
        // Flash effect when damaged but not destroyed
        if (meshRenderer != null)
        {
            StartCoroutine(DamageFlash());
        }
    }
    
    System.Collections.IEnumerator DamageFlash()
    {
        Color originalColor = meshRenderer.material.color;
        meshRenderer.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        meshRenderer.material.color = originalColor;
    }
    
    void UpdateVisualState()
    {
        if (visualState != null && visualState.damagedMaterial != null && visualState.criticalMaterial != null)
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                float healthPercent = (float)currentHealth / maxHealth;
                
                if (healthPercent <= 0.33f)
                {
                    renderer.material = visualState.criticalMaterial;
                }
                else if (healthPercent <= 0.66f)
                {
                    renderer.material = visualState.damagedMaterial;
                }
            }
        }
    }
    
    void StartRolling()
    {
        isRolling = true;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector3.forward * 5f, ForceMode.Impulse);
        }
    }
    
    IEnumerator DodgeAnimation()
    {
        // Quick dodge to the side
        Vector3 dodgeDirection = Random.Range(0, 2) == 0 ? Vector3.right : Vector3.left;
        float dodgeDistance = 1f;
        float dodgeTime = 0.3f;
        
        Vector3 originalPos = transform.position;
        Vector3 dodgePos = originalPos + dodgeDirection * dodgeDistance;
        
        float timer = 0;
        while (timer < dodgeTime)
        {
            transform.position = Vector3.Lerp(originalPos, dodgePos, timer / dodgeTime);
            timer += Time.deltaTime;
            yield return null;
        }
        
        // Return to original position
        timer = 0;
        while (timer < dodgeTime)
        {
            transform.position = Vector3.Lerp(dodgePos, originalPos, timer / dodgeTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }
    
    void CreateBounceEffect()
    {
        // Visual feedback for hitting indestructible object
        for (int i = 0; i < 3; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
            spark.transform.localScale = Vector3.one * 0.1f;
            
            Material sparkMat = new Material(Shader.Find("Standard"));
            sparkMat.color = Color.yellow;
            sparkMat.EnableKeyword("_EMISSION");
            sparkMat.SetColor("_EmissionColor", Color.yellow);
            spark.GetComponent<MeshRenderer>().material = sparkMat;
            
            Rigidbody sparkRb = spark.AddComponent<Rigidbody>();
            sparkRb.AddForce(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            
            Destroy(spark, 1f);
        }
    }
    
    public void DestroyObstacle()
    {
        // Drop coins if applicable
        if (dropCoins && Random.Range(0, 100) < coinDropChance)
        {
            SpawnCoins();
        }
        
        // Play destruction sound
        if (visualState != null && visualState.destructionSound != null && audioSource != null)
        {
            AudioSource.PlayClipAtPoint(visualState.destructionSound, transform.position);
        }
        
        // Create special destruction effects based on type
        CreateSpecialDestructionEffect();
        
        // Base destruction logic
        isDestroyed = true;
        
        // Add score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue, true);
        }
        
        // Create debris effect
        CreateDebrisEffect();
        
        // Destroy the game object
        Destroy(gameObject);
    }
    
    void CreateDebrisEffect()
    {
        // Simple destruction effect with debris
        int debrisCount = Random.Range(3, 6);
        
        for (int i = 0; i < debrisCount; i++)
        {
            GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debris.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
            debris.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
            debris.transform.rotation = Random.rotation;
            
            // Add physics to debris
            Rigidbody debrisRb = debris.AddComponent<Rigidbody>();
            Vector3 explosionDirection = Random.insideUnitSphere.normalized;
            explosionDirection.y = Mathf.Abs(explosionDirection.y); // Ensure upward component
            
            debrisRb.AddForce(explosionDirection * destructionForce, ForceMode.Impulse);
            debrisRb.AddTorque(Random.insideUnitSphere * destructionForce, ForceMode.Impulse);
            
            // Set debris material
            Material debrisMaterial = new Material(Shader.Find("Standard"));
            debrisMaterial.color = Color.gray;
            debris.GetComponent<MeshRenderer>().material = debrisMaterial;
            
            // Destroy debris after some time
            Destroy(debris, Random.Range(2f, 4f));
        }
    }
    
    void SpawnCoins()
    {
        if (coinPrefab == null)
        {
            // Create simple coin placeholder
            for (int i = 0; i < coinDropAmount; i++)
            {
                GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                coin.transform.position = transform.position + Random.insideUnitSphere * 1f;
                coin.transform.position = new Vector3(coin.transform.position.x, 0.5f, coin.transform.position.z);
                coin.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
                
                Material coinMat = new Material(Shader.Find("Standard"));
                coinMat.color = Color.yellow;
                coinMat.SetFloat("_Metallic", 0.9f);
                coinMat.SetFloat("_Glossiness", 0.9f);
                coin.GetComponent<MeshRenderer>().material = coinMat;
                
                // Add simple rotation
                coin.AddComponent<CoinPickup>();
                
                // Auto-destroy after 5 seconds if not collected
                Destroy(coin, 5f);
            }
        }
    }
    
    void CreateSpecialDestructionEffect()
    {
        switch (urbanType)
        {
            case UrbanObstacleType.FireHydrant:
                CreateWaterSprayEffect();
                break;
            case UrbanObstacleType.VendorCart:
                CreateFruitScatterEffect();
                break;
            case UrbanObstacleType.TrashBin:
                CreateTrashScatterEffect();
                break;
        }
    }
    
    void CreateWaterSprayEffect()
    {
        // Create water spray particles
        GameObject waterSpray = new GameObject("WaterSpray");
        waterSpray.transform.position = transform.position;
        
        // Simple water droplets
        for (int i = 0; i < 20; i++)
        {
            GameObject droplet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            droplet.transform.position = transform.position;
            droplet.transform.localScale = Vector3.one * 0.1f;
            
            Material waterMat = new Material(Shader.Find("Standard"));
            waterMat.color = new Color(0.5f, 0.7f, 1f, 0.7f);
            droplet.GetComponent<MeshRenderer>().material = waterMat;
            
            Rigidbody dropletRb = droplet.AddComponent<Rigidbody>();
            Vector3 sprayDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 3f), Random.Range(-1f, 1f));
            dropletRb.AddForce(sprayDirection * 5f, ForceMode.Impulse);
            
            Destroy(droplet, 2f);
        }
        
        Destroy(waterSpray, 3f);
    }
    
    void CreateFruitScatterEffect()
    {
        // Create scattered fruit pieces
        string[] fruitColors = { "red", "yellow", "orange", "green" };
        
        for (int i = 0; i < 8; i++)
        {
            GameObject fruit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fruit.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
            fruit.transform.localScale = Vector3.one * Random.Range(0.2f, 0.4f);
            
            Material fruitMat = new Material(Shader.Find("Standard"));
            
            // Random fruit colors
            switch (Random.Range(0, 4))
            {
                case 0: fruitMat.color = Color.red; break;
                case 1: fruitMat.color = Color.yellow; break;
                case 2: fruitMat.color = new Color(1f, 0.5f, 0f); break; // Orange
                case 3: fruitMat.color = Color.green; break;
            }
            
            fruit.GetComponent<MeshRenderer>().material = fruitMat;
            
            Rigidbody fruitRb = fruit.AddComponent<Rigidbody>();
            fruitRb.AddForce(Random.insideUnitSphere * 3f, ForceMode.Impulse);
            
            Destroy(fruit, 3f);
        }
    }
    
    void CreateTrashScatterEffect()
    {
        // Create scattered trash pieces
        for (int i = 0; i < 5; i++)
        {
            GameObject trash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trash.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
            trash.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
            trash.transform.rotation = Random.rotation;
            
            Material trashMat = new Material(Shader.Find("Standard"));
            trashMat.color = new Color(Random.Range(0.3f, 0.6f), Random.Range(0.3f, 0.6f), Random.Range(0.3f, 0.6f));
            trash.GetComponent<MeshRenderer>().material = trashMat;
            
            Rigidbody trashRb = trash.AddComponent<Rigidbody>();
            trashRb.AddForce(Random.insideUnitSphere * 2f, ForceMode.Impulse);
            
            Destroy(trash, 2f);
        }
    }
    
    IEnumerator TriggerChainReaction()
    {
        yield return new WaitForSeconds(chainReactionDelay);
        
        // Find nearby obstacles
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, chainReactionRadius);
        
        foreach (Collider col in nearbyColliders)
        {
            if (col.gameObject != gameObject && col.CompareTag("Obstacle"))
            {
                Obstacle nearbyObstacle = col.GetComponent<Obstacle>();
                if (nearbyObstacle != null)
                {
                    nearbyObstacle.TakeDamage(1, true);
                }
            }
        }
    }
}

// Simple coin pickup component
public class CoinPickup : MonoBehaviour
{
    void Update()
    {
        // Rotate the coin
        transform.Rotate(Vector3.up * 100f * Time.deltaTime);
        
        // Gentle floating motion
        transform.position += Vector3.up * Mathf.Sin(Time.time * 2f) * 0.001f;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add score for coin collection
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(1, false);
            }
            
            // Create collection effect
            CreateCollectionEffect();
            
            Destroy(gameObject);
        }
    }
    
    void CreateCollectionEffect()
    {
        // Simple sparkle effect
        for (int i = 0; i < 5; i++)
        {
            GameObject sparkle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sparkle.transform.position = transform.position;
            sparkle.transform.localScale = Vector3.one * 0.05f;
            
            Material sparkleMat = new Material(Shader.Find("Standard"));
            sparkleMat.color = Color.yellow;
            sparkleMat.EnableKeyword("_EMISSION");
            sparkleMat.SetColor("_EmissionColor", Color.yellow * 2f);
            sparkle.GetComponent<MeshRenderer>().material = sparkleMat;
            
            Rigidbody sparkleRb = sparkle.AddComponent<Rigidbody>();
            sparkleRb.AddForce(Random.insideUnitSphere * 2f, ForceMode.Impulse);
            
            Destroy(sparkle, 0.5f);
        }
    }
}