using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Ball Settings")]
    public float explosionRadius = 0f;
    public bool isChargedShot = false;
    
    private float lifetime;
    private bool hasExploded = false;
    
    public void Initialize(float ballLifetime, float chargedRadius)
    {
        lifetime = ballLifetime;
        explosionRadius = chargedRadius;
        isChargedShot = explosionRadius > 0f;
        
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
        
        // Scale the ball
        transform.localScale = Vector3.one * (isChargedShot ? 0.3f : 0.2f);
    }
    
    Mesh CreateSphereMesh()
    {
        // Create a simple sphere mesh procedurally
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh sphereMesh = tempSphere.GetComponent<MeshFilter>().mesh;
        DestroyImmediate(tempSphere);
        return sphereMesh;
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
            // Ball hit ground, destroy it
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