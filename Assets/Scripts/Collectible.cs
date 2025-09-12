using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Animation Settings")]
    public float rotationSpeed = 180f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    
    private float startY;
    private bool isCollected = false;
    
    public void Initialize(float rotation, float bob, float bobRange, float startHeight)
    {
        rotationSpeed = rotation;
        bobSpeed = bob;
        bobHeight = bobRange;
        startY = startHeight;
    }
    
    void Start()
    {
        startY = transform.position.y;
        
        // Ensure proper tag
        if (!gameObject.CompareTag("Collectible"))
        {
            gameObject.tag = "Collectible";
        }
        
        // Ensure trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    void Update()
    {
        if (isCollected) return;
        
        // Rotate around Y axis
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        // Bob up and down
        float newY = startY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        
        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }
    
    void CollectCoin()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Add score through GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectCoin();
        }
        
        // Create collection effect
        CreateCollectionEffect();
        
        // Optional: Play sound effect
        // AudioSource.PlayClipAtPoint(collectSound, transform.position);
        
        // Destroy the collectible
        Destroy(gameObject);
    }
    
    void CreateCollectionEffect()
    {
        // Simple sparkle effect
        int sparkleCount = Random.Range(4, 8);
        
        for (int i = 0; i < sparkleCount; i++)
        {
            GameObject sparkle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sparkle.transform.position = transform.position + Random.insideUnitSphere * 0.3f;
            sparkle.transform.localScale = Vector3.one * Random.Range(0.05f, 0.15f);
            sparkle.transform.rotation = Random.rotation;
            
            // Add upward movement
            Rigidbody sparkleRb = sparkle.AddComponent<Rigidbody>();
            sparkleRb.useGravity = false;
            Vector3 sparkleDirection = Random.insideUnitSphere.normalized;
            sparkleDirection.y = Mathf.Abs(sparkleDirection.y) + 1f; // Ensure upward movement
            
            sparkleRb.linearVelocity = sparkleDirection * Random.Range(2f, 4f);
            sparkleRb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            
            // Set sparkle material using Built-in shader
            Material sparkleMaterial = new Material(Shader.Find("Standard"));
            sparkleMaterial.color = Color.yellow;
            sparkleMaterial.SetFloat("_Metallic", 1f);
            sparkleMaterial.SetFloat("_Glossiness", 1f);
            sparkle.GetComponent<MeshRenderer>().material = sparkleMaterial;
            
            // Destroy sparkles after animation
            Destroy(sparkle, Random.Range(0.5f, 1f));
        }
    }
    
    // Visual indicator in scene view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}