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
    }
    
    void SetupVisuals()
    {
        // Add mesh filter and renderer for basic sphere
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        // Use built-in sphere mesh
        meshFilter.mesh = CreateSphereMesh();
        
        // Create a basic material using URP shader for Unity 6
        Material ballMaterial = new Material(Shader.Find("Standard"));
        ballMaterial.color = isChargedShot ? Color.red : Color.cyan;
        ballMaterial.SetFloat("_Metallic", 0.5f);
        ballMaterial.SetFloat("_Glossiness", 0.8f);
        meshRenderer.material = ballMaterial;
        
        // Scale the ball - make it bigger so it's more visible
        transform.localScale = Vector3.one * (isChargedShot ? 0.5f : 0.4f);
    }
    
    Mesh CreateSphereMesh()
    {
        // Create a simple sphere mesh procedurally
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh sphereMesh = tempSphere.GetComponent<MeshFilter>().mesh;
        DestroyImmediate(tempSphere);
        return sphereMesh;
    }
    
    void Update()
    {
        if (playerTransform == null) return;
        
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
        
        // Rotate the ball for visual effect
        transform.Rotate(Vector3.up * 360f * Time.deltaTime);
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
        
        if (isChargedShot && explosionRadius > 0f)
        {
            // Charged shot - affect multiple obstacles
            Collider[] nearbyObstacles = Physics.OverlapSphere(transform.position, explosionRadius);
            
            foreach (Collider obstacle in nearbyObstacles)
            {
                if (obstacle.CompareTag("Obstacle"))
                {
                    DestroyObstacle(obstacle.gameObject);
                }
            }
            
            // Create explosion effect
            CreateExplosionEffect();
        }
        else
        {
            // Regular shot - affect only the hit obstacle
            DestroyObstacle(hitObstacle.gameObject);
        }
        
        DestroyBall();
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
        // Simple particle-like explosion effect
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.transform.position = transform.position;
            particle.transform.localScale = Vector3.one * 0.1f;
            
            Rigidbody particleRb = particle.AddComponent<Rigidbody>();
            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            particleRb.AddForce(randomDirection * 5f, ForceMode.Impulse);
            
            // Change color to explosion color using URP shader
            Material particleMat = new Material(Shader.Find("Standard"));
            particleMat.color = Color.red;
            particle.GetComponent<MeshRenderer>().material = particleMat;
            
            // Destroy particles after short time
            Destroy(particle, 1f);
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