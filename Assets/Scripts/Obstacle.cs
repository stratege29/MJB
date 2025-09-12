using UnityEngine;

public enum ObstacleType
{
    Weak,      // Détruit par tir simple et chargé
    Strong,    // Détruit seulement par tir chargé  
    Reinforced // Nécessite plusieurs tirs chargés
}

public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public ObstacleType obstacleType = ObstacleType.Weak;
    public int maxHealth = 1;
    public int scoreValue = 5;
    public float destructionForce = 5f;
    
    private int currentHealth;
    private bool isDestroyed = false;
    private Material originalMaterial;
    private MeshRenderer meshRenderer;
    
    void Start()
    {
        // Initialize health
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
    
    public bool TakeDamage(int damage, bool isChargedShot)
    {
        if (isDestroyed) return false;
        
        // Check if this shot can damage this obstacle type
        bool canDamage = false;
        
        switch (obstacleType)
        {
            case ObstacleType.Weak:
                canDamage = true; // Both normal and charged shots work
                break;
            case ObstacleType.Strong:
            case ObstacleType.Reinforced:
                canDamage = isChargedShot; // Only charged shots work
                break;
        }
        
        if (!canDamage)
        {
            // Create ricochet effect for ineffective shots
            CreateRicochetEffect();
            return false;
        }
        
        // Apply damage
        currentHealth -= damage;
        Debug.Log($"Obstacle took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Visual damage feedback
        if (currentHealth > 0)
        {
            ShowDamageEffect();
            return false;
        }
        else
        {
            DestroyObstacle();
            return true;
        }
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
    
    void CreateRicochetEffect()
    {
        // Create sparks for ineffective shots
        for (int i = 0; i < 5; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spark.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
            spark.transform.localScale = Vector3.one * 0.05f;
            
            Material sparkMat = new Material(Shader.Find("Standard"));
            sparkMat.color = Color.white;
            sparkMat.EnableKeyword("_EMISSION");
            sparkMat.SetColor("_EmissionColor", Color.white);
            spark.GetComponent<MeshRenderer>().material = sparkMat;
            
            Rigidbody sparkRb = spark.AddComponent<Rigidbody>();
            sparkRb.AddForce(Random.insideUnitSphere * 3f, ForceMode.Impulse);
            
            Destroy(spark, 0.5f);
        }
        
        Debug.Log("Shot ricocheted - need charged shot for this obstacle!");
    }
    
    public void DestroyObstacle()
    {
        if (isDestroyed) return;
        
        isDestroyed = true;
        
        // Add score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue, true);
        }
        
        // Create destruction effect
        CreateDestructionEffect();
        
        // Destroy the game object
        Destroy(gameObject);
    }
    
    void CreateDestructionEffect()
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
            
            // Set debris material using Built-in shader
            Material debrisMaterial = new Material(Shader.Find("Standard"));
            debrisMaterial.color = Color.gray;
            debris.GetComponent<MeshRenderer>().material = debrisMaterial;
            
            // Destroy debris after some time
            Destroy(debris, Random.Range(2f, 4f));
        }
        
        // Optional: Add sound effect here
        // AudioSource.PlayClipAtPoint(destructionSound, transform.position);
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Handle ball collision
        if (other.CompareTag("Ball"))
        {
            Ball ball = other.GetComponent<Ball>();
            if (ball != null && canBeDestroyed)
            {
                DestroyObstacle();
            }
        }
    }
    
    // Visual indicator for destroyable obstacles
    void OnDrawGizmos()
    {
        Gizmos.color = canBeDestroyed ? Color.red : Color.gray;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}