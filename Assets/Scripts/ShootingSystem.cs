using UnityEngine;

public class ShootingSystem : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject ballPrefab;
    public Transform shootPoint;
    public float ballSpeed = 15f;
    public float ballLifetime = 3f;
    public float chargedShotMultiplier = 2f;
    public float autoTargetRange = 10f;
    public LayerMask obstacleLayer = 1;
    
    [Header("Charged Shot")]
    public float chargedShotForce = 25f;
    public float chargedShotRadius = 2f;
    public int maxChargedTargets = 3;
    
    private Transform playerTransform;
    
    void Start()
    {
        playerTransform = transform;
        
        // If no shoot point is assigned, create one
        if (shootPoint == null)
        {
            GameObject shootPointObj = new GameObject("ShootPoint");
            shootPointObj.transform.SetParent(transform);
            shootPointObj.transform.localPosition = new Vector3(0, 1f, 0.5f);
            shootPoint = shootPointObj.transform;
        }
    }
    
    public void QuickShot()
    {
        if (ballPrefab == null) return;
        
        Transform target = FindNearestObstacle();
        Vector3 shootDirection = target != null ? 
            (target.position - shootPoint.position).normalized : 
            Vector3.forward;
        
        CreateBall(shootDirection, ballSpeed, false);
    }
    
    public void ChargedShot()
    {
        if (ballPrefab == null) return;
        
        Transform[] targets = FindMultipleObstacles();
        
        if (targets.Length > 0)
        {
            // Shoot at primary target with extra force
            Vector3 primaryDirection = (targets[0].position - shootPoint.position).normalized;
            CreateBall(primaryDirection, chargedShotForce, true);
        }
        else
        {
            // Shoot forward with charged power
            CreateBall(Vector3.forward, chargedShotForce, true);
        }
    }
    
    void CreateBall(Vector3 direction, float speed, bool isChargedShot)
    {
        GameObject ball = Instantiate(ballPrefab, shootPoint.position, Quaternion.LookRotation(direction));
        
        // Add ball physics
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb == null)
        {
            ballRb = ball.AddComponent<Rigidbody>();
        }
        
        ballRb.linearVelocity = direction * speed;
        ballRb.useGravity = false;
        
        // Add ball script
        Ball ballScript = ball.GetComponent<Ball>();
        if (ballScript == null)
        {
            ballScript = ball.AddComponent<Ball>();
        }
        
        ballScript.Initialize(ballLifetime, isChargedShot ? chargedShotRadius : 0f);
        
        // Destroy ball after lifetime
        Destroy(ball, ballLifetime);
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