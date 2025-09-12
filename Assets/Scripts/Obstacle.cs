using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public bool canBeDestroyed = true;
    public int scoreValue = 5;
    public float destructionForce = 5f;
    
    private bool isDestroyed = false;
    
    void Start()
    {
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
    }
    
    public void DestroyObstacle()
    {
        if (isDestroyed || !canBeDestroyed) return;
        
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