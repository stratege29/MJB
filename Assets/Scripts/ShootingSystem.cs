using UnityEngine;

public class ShootingSystem : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject ballPrefab;
    public Transform shootPoint;
    public float ballSpeed = 15f;
    public float ballLifetime = 5f; // Increased for boomerang return
    public float shotCooldown = 0.3f; // Cooldown between shots
    public float chargedShotMultiplier = 2f;
    public float autoTargetRange = 10f;
    public LayerMask obstacleLayer = 1;
    
    [Header("Charged Shot")]
    public float chargedShotForce = 25f;
    public float chargedShotRadius = 2f;
    public int maxChargedTargets = 3;
    
    private Transform playerTransform;
    private float lastShotTime = -1f; // Track last shot time
    
    void Start()
    {
        playerTransform = transform;
        
        // If no shoot point is assigned, create one in front of player
        if (shootPoint == null)
        {
            GameObject shootPointObj = new GameObject("ShootPoint");
            shootPointObj.transform.SetParent(transform);
            shootPointObj.transform.localPosition = new Vector3(0, 1f, 1f); // Further in front
            shootPoint = shootPointObj.transform;
        }
    }
    
    public void QuickShot()
    {
        Debug.Log("QuickShot called");
        if (ballPrefab == null) 
        {
            Debug.LogError("Ball prefab is null!");
            return;
        }
        
        // Check cooldown
        if (Time.time - lastShotTime < shotCooldown)
        {
            Debug.Log($"Shot on cooldown - {shotCooldown - (Time.time - lastShotTime):F1}s remaining");
            return;
        }
        
        // Always shoot forward relative to player's forward direction
        Vector3 shootDirection = transform.forward;
        
        Debug.Log($"Creating ball, direction: {shootDirection}");
        CreateBall(shootDirection, ballSpeed, false);
        lastShotTime = Time.time;
    }
    
    public void ChargedShot()
    {
        Debug.Log("ChargedShot called");
        if (ballPrefab == null) 
        {
            Debug.LogError("Ball prefab is null!");
            return;
        }
        
        // Check cooldown (charged shots have longer cooldown)
        float chargedCooldown = shotCooldown * 2f;
        if (Time.time - lastShotTime < chargedCooldown)
        {
            Debug.Log($"Charged shot on cooldown - {chargedCooldown - (Time.time - lastShotTime):F1}s remaining");
            return;
        }
        
        // Always shoot forward with charged power
        Vector3 shootDirection = transform.forward;
        CreateBall(shootDirection, chargedShotForce, true);
        lastShotTime = Time.time;
    }
    
    void CreateBall(Vector3 direction, float speed, bool isChargedShot)
    {
        // Ensure ball spawns in front of player
        Vector3 spawnPosition = transform.position + transform.forward * 1.5f + Vector3.up * 1f;
        GameObject ball = Instantiate(ballPrefab, spawnPosition, Quaternion.LookRotation(direction));
        
        // Ensure ball is visible
        ball.SetActive(true);
        
        // Add ball physics
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb == null)
        {
            ballRb = ball.AddComponent<Rigidbody>();
        }
        
        // Don't set velocity directly, let the Ball script handle movement
        ballRb.useGravity = false;
        ballRb.isKinematic = true; // Kinematic for controlled movement
        
        // Add ball script
        Ball ballScript = ball.GetComponent<Ball>();
        if (ballScript == null)
        {
            ballScript = ball.AddComponent<Ball>();
        }
        
        ballScript.Initialize(ballLifetime, isChargedShot ? chargedShotRadius : 0f);
        ballScript.SetSpeed(speed);
        
        // Destroy ball after lifetime (as backup)
        Destroy(ball, ballLifetime * 2);
        
        Debug.Log($"Ball created at {spawnPosition} moving {direction}");
    }
    
    Transform FindNearestObstacle()
    {
        Collider[] obstacles = Physics.OverlapSphere(playerTransform.position, autoTargetRange, obstacleLayer);
        Transform nearestObstacle = null;
        float nearestDistance = float.MaxValue;
        
        foreach (Collider obstacle in obstacles)
        {
            // Only target obstacles in front of the player
            Vector3 directionToObstacle = obstacle.transform.position - playerTransform.position;
            if (Vector3.Dot(directionToObstacle.normalized, playerTransform.forward) > 0.5f)
            {
                float distance = directionToObstacle.magnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestObstacle = obstacle.transform;
                }
            }
        }
        
        return nearestObstacle;
    }
    
    Transform[] FindMultipleObstacles()
    {
        Collider[] obstacles = Physics.OverlapSphere(playerTransform.position, autoTargetRange, obstacleLayer);
        System.Collections.Generic.List<Transform> validTargets = new System.Collections.Generic.List<Transform>();
        
        foreach (Collider obstacle in obstacles)
        {
            // Only target obstacles in front of the player
            Vector3 directionToObstacle = obstacle.transform.position - playerTransform.position;
            if (Vector3.Dot(directionToObstacle.normalized, playerTransform.forward) > 0.5f)
            {
                validTargets.Add(obstacle.transform);
                
                if (validTargets.Count >= maxChargedTargets)
                    break;
            }
        }
        
        // Sort by distance
        validTargets.Sort((a, b) => 
        {
            float distA = Vector3.Distance(playerTransform.position, a.position);
            float distB = Vector3.Distance(playerTransform.position, b.position);
            return distA.CompareTo(distB);
        });
        
        return validTargets.ToArray();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw auto-target range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, autoTargetRange);
        
        // Draw shoot point
        if (shootPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(shootPoint.position, 0.1f);
        }
    }
}