using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Ball Settings")]
    public float explosionRadius = 0f;
    public bool isChargedShot = false;
    
    [Header("Boomerang Settings")]
    public float maxDistance = 10f;
    public float returnSpeed = 20f;
    public bool isReturning = false;
    
    private float lifetime;
    private bool hasExploded = false;
    private Transform playerTransform;
    private Vector3 startPosition;
    private Vector3 forwardDirection;
    private float travelDistance = 0f;
    private ShootingSystem shootingSystem;
    
    public void Initialize(float ballLifetime, float chargedRadius)
    {
        lifetime = ballLifetime;
        explosionRadius = chargedRadius;
        isChargedShot = explosionRadius > 0f;
        
        // Find the player for return trajectory
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            shootingSystem = player.GetComponent<ShootingSystem>();
            Debug.Log($"Ball found player: {player.name}");
        }
        else
        {
            Debug.LogWarning("Ball could not find player with 'Player' tag");
        }
        
        startPosition = transform.position;
        forwardDirection = transform.forward;
        maxDistance = isChargedShot ? 15f : 10f;
        
        // Ensure the ball has a collider
        if (GetComponent<Collider>() == null)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 0.1f;
        }
        
        // Add visual component if missing
        if (GetComponent<MeshRenderer>() == null)
        {
            SetupVisuals();
        }
        
        // Configure trail for shot type
        SetupTrail();
    }
    
    void SetupVisuals()
    {
        // Add mesh filter and renderer for basic sphere
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        // Use built-in sphere mesh
        meshFilter.mesh = CreateSphereMesh();
        
        // Create enhanced material for charged vs normal shots
        Material ballMaterial = new Material(Shader.Find("Standard"));
        
        if (isChargedShot)
        {
            // Golden charged shot with emission
            ballMaterial.color = new Color(1f, 0.84f, 0f); // Gold
            ballMaterial.SetFloat("_Metallic", 0.8f);
            ballMaterial.SetFloat("_Glossiness", 1f);
            ballMaterial.EnableKeyword("_EMISSION");
            ballMaterial.SetColor("_EmissionColor", new Color(1f, 0.6f, 0f) * 0.5f); // Orange glow
        }
        else
        {
            // Cyan normal shot
            ballMaterial.color = Color.cyan;
            ballMaterial.SetFloat("_Metallic", 0.3f);
            ballMaterial.SetFloat("_Glossiness", 0.7f);
            ballMaterial.EnableKeyword("_EMISSION");
            ballMaterial.SetColor("_EmissionColor", Color.cyan * 0.2f);
        }
        
        meshRenderer.material = ballMaterial;
        
        // Scale the ball - charged shots are bigger and more visible
        transform.localScale = Vector3.one * (isChargedShot ? 0.6f : 0.4f);
    }
    
    Mesh CreateSphereMesh()
    {
        // Create a simple sphere mesh procedurally
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh sphereMesh = tempSphere.GetComponent<MeshFilter>().mesh;
        DestroyImmediate(tempSphere);
        return sphereMesh;
    }
    
    void SetupTrail()
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null) return;
        
        if (isChargedShot)
        {
            // Golden charged trail - thicker and longer
            trail.time = 0.8f;
            trail.startWidth = 0.5f;
            trail.endWidth = 0.1f;
            trail.startColor = new Color(1f, 0.84f, 0f); // Gold
            trail.endColor = new Color(1f, 0.6f, 0f, 0f); // Orange fade
        }
        else
        {
            // Cyan normal trail
            trail.time = 0.5f;
            trail.startWidth = 0.3f;
            trail.endWidth = 0.05f;
            trail.startColor = Color.cyan;
            trail.endColor = new Color(0, 1, 1, 0);
        }
    }
    
    void Update()
    {
        // Find player if not found yet
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = FindObjectOfType<PlayerController>()?.gameObject;
            }
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                return; // Still no player found
            }
        }
        
        // Boomerang behavior
        if (!isReturning)
        {
            // Move forward
            transform.position += forwardDirection * ballSpeed * Time.deltaTime;
            travelDistance += ballSpeed * Time.deltaTime;
            
            // Start returning when max distance reached
            if (travelDistance >= maxDistance)
            {
                isReturning = true;
            }
        }
        else
        {
            // Return to player
            Vector3 directionToPlayer = (playerTransform.position + Vector3.up - transform.position).normalized;
            transform.position += directionToPlayer * returnSpeed * Time.deltaTime;
            
            // Check if close to player
            if (Vector3.Distance(transform.position, playerTransform.position) < 1f)
            {
                DestroyBall();
            }
        }
        
        // Rotate the ball for visual effect - charged shots spin faster
        float rotationSpeed = isChargedShot ? 720f : 360f;
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
    
    private float ballSpeed = 15f; // Default speed
    
    public void SetSpeed(float speed)
    {
        ballSpeed = speed;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;
        
        if (other.CompareTag("Obstacle"))
        {
            HitObstacle(other);
        }
        else if (other.CompareTag("Ground"))
        {
            // Don't destroy on ground hit, let it return
            // Ball will bounce off ground
        }
        else if (other.CompareTag("Player") && isReturning)
        {
            // Ball returned to player
            DestroyBall();
        }
    }
    
    void HitObstacle(Collider hitObstacle)
    {
        hasExploded = true;
        
        bool obstacleDestroyed = false;
        
        if (isChargedShot && explosionRadius > 0f)
        {
            // Charged shot - affect multiple obstacles
            Collider[] nearbyObstacles = Physics.OverlapSphere(transform.position, explosionRadius);
            
            foreach (Collider obstacle in nearbyObstacles)
            {
                if (obstacle.CompareTag("Obstacle"))
                {
                    Obstacle obstacleComponent = obstacle.GetComponent<Obstacle>();
                    if (obstacleComponent != null)
                    {
                        bool destroyed = obstacleComponent.TakeDamage(2, true); // Charged shots do 2 damage
                        if (destroyed) obstacleDestroyed = true;
                    }
                }
            }
            
            // Create explosion effect
            CreateExplosionEffect();
        }
        else
        {
            // Regular shot - affect only the hit obstacle
            Obstacle obstacleComponent = hitObstacle.GetComponent<Obstacle>();
            if (obstacleComponent != null)
            {
                obstacleDestroyed = obstacleComponent.TakeDamage(1, false); // Normal shots do 1 damage
            }
        }
        
        // Only destroy ball if we actually destroyed an obstacle or it's a charged shot
        if (obstacleDestroyed || isChargedShot)
        {
            DestroyBall();
        }
        else
        {
            // Ball bounces off if it didn't destroy the obstacle
            Debug.Log("Ball bounced off strong obstacle - continuing trajectory");
            // Could add bounce physics here if desired
        }
    }
    
    void DestroyObstacle(GameObject obstacle)
    {
        // Add score for destroying obstacle
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DestroyObstacle();
        }
        
        // Create destruction effect
        CreateDestructionEffect(obstacle.transform.position);
        
        // Destroy the obstacle
        Destroy(obstacle);
    }
    
    void CreateExplosionEffect()
    {
        // Enhanced explosion effect for charged shots
        int particleCount = isChargedShot ? 15 : 8;
        Color explosionColor = isChargedShot ? new Color(1f, 0.84f, 0f) : Color.red; // Gold for charged, red for normal
        
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
            particle.transform.localScale = Vector3.one * Random.Range(0.08f, 0.15f);
            particle.transform.rotation = Random.rotation;
            
            Rigidbody particleRb = particle.AddComponent<Rigidbody>();
            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            float force = isChargedShot ? 8f : 5f;
            particleRb.AddForce(randomDirection * force, ForceMode.Impulse);
            particleRb.AddTorque(Random.insideUnitSphere * force, ForceMode.Impulse);
            
            // Enhanced explosion material with emission
            Material particleMat = new Material(Shader.Find("Standard"));
            particleMat.color = explosionColor;
            particleMat.EnableKeyword("_EMISSION");
            particleMat.SetColor("_EmissionColor", explosionColor * 0.5f);
            particleMat.SetFloat("_Metallic", 0.3f);
            particleMat.SetFloat("_Glossiness", 0.8f);
            particle.GetComponent<MeshRenderer>().material = particleMat;
            
            // Destroy particles after time with some variation
            Destroy(particle, Random.Range(1f, 2f));
        }
        
        // Add screen shake effect for charged explosions
        if (isChargedShot)
        {
            Debug.Log("BOOM! Charged explosion with screen shake!");
            // Screen shake would be implemented in camera script
        }
    }
    
    void CreateDestructionEffect(Vector3 position)
    {
        // Simple destruction effect
        for (int i = 0; i < 4; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.transform.position = position + Random.insideUnitSphere * 0.5f;
            particle.transform.localScale = Vector3.one * 0.05f;
            
            Rigidbody particleRb = particle.AddComponent<Rigidbody>();
            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            particleRb.AddForce(randomDirection * 3f, ForceMode.Impulse);
            
            // Change color to debris color using URP shader
            Material particleMat = new Material(Shader.Find("Standard"));
            particleMat.color = Color.gray;
            particle.GetComponent<MeshRenderer>().material = particleMat;
            
            // Destroy particles after short time
            Destroy(particle, 2f);
        }
    }
    
    void DestroyBall()
    {
        // Notify shooting system before destroying
        if (shootingSystem != null)
        {
            shootingSystem.OnBallDestroyed(gameObject);
        }
        
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        if (isChargedShot && explosionRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}