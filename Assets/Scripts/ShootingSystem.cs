using UnityEngine;

public class ShootingSystem : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject ballPrefab;
    public Transform shootPoint;
    public float ballSpeed = 30f; // Match Ball.cs launchForce
    public float ballLifetime = 5f;
    public float chargedShotMultiplier = 1.5f; // 45 speed for charged shots
    public float autoTargetRange = 10f;
    public LayerMask obstacleLayer = 1;
    
    [Header("Trajectory Preview")]
    public bool showTrajectoryPreview = true;
    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 50;
    public float trajectoryTimeStep = 0.1f;
    public Material trajectoryMaterial;
    
    [Header("Charged Shot")]
    public float chargedShotForce = 45f; // 3x player max speed
    public float chargedShotRadius = 2f;
    public int maxChargedTargets = 3;
    
    private Transform playerTransform;
    private PlayerController playerController; // Pour accéder à la lane
    private GameObject activeBall; // Track current active ball
    
    void Start()
    {
        playerTransform = transform;
        playerController = GetComponent<PlayerController>();
        
        if (playerController == null)
        {
            Debug.LogError("ShootingSystem: PlayerController non trouvé!");
        }
        
        // If no shoot point is assigned, create one at player's feet
        if (shootPoint == null)
        {
            GameObject shootPointObj = new GameObject("ShootPoint");
            shootPointObj.transform.SetParent(transform);
            shootPointObj.transform.localPosition = new Vector3(0, -0.5f, 1f); // At foot level
            shootPoint = shootPointObj.transform;
        }
        
        // Setup trajectory preview
        SetupTrajectoryPreview();
    }
    
    public void QuickShot()
    {
        Debug.Log("QuickShot called");
        if (ballPrefab == null) 
        {
            Debug.LogError("Ball prefab is null!");
            return;
        }
        
        // Check if there's already an active ball
        if (activeBall != null)
        {
            Debug.Log("Ball already active - cannot shoot another");
            return;
        }
        
        // Always shoot forward relative to player's forward direction
        Vector3 shootDirection = transform.forward;
        
        Debug.Log($"Creating ball, direction: {shootDirection}");
        CreateBall(shootDirection, ballSpeed, false);
    }
    
    public void ChargedShot()
    {
        Debug.Log("ChargedShot called");
        if (ballPrefab == null) 
        {
            Debug.LogError("Ball prefab is null!");
            return;
        }
        
        // Check if there's already an active ball
        if (activeBall != null)
        {
            Debug.Log("Ball already active - cannot shoot charged shot");
            return;
        }
        
        // Always shoot forward with charged power
        Vector3 shootDirection = transform.forward;
        CreateBall(shootDirection, chargedShotForce, true);
    }
    
    void CreateBall(Vector3 direction, float speed, bool isChargedShot)
    {
        // Spawn ball at player's feet (ground level) in front
        Vector3 spawnPosition = transform.position + transform.forward * 1.5f - Vector3.up * 0.5f;
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
        
        // Obtenir la lane actuelle du joueur
        int currentPlayerLane = playerController != null ? playerController.CurrentLane : 0;
        
        ballScript.Initialize(ballLifetime, isChargedShot ? chargedShotRadius : 0f, currentPlayerLane);
        ballScript.SetSpeed(speed);
        
        // Clear reference when ball is destroyed
        StartCoroutine(ClearBallReference(ball, ballLifetime * 2));
        
        // Track the active ball
        activeBall = ball;
        
        Debug.Log($"Ball created at {spawnPosition} moving {direction}");
    }
    
    System.Collections.IEnumerator ClearBallReference(GameObject ball, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activeBall == ball)
        {
            activeBall = null;
            Debug.Log("Ball reference cleared - can shoot again");
        }
    }
    
    // Called by Ball script when it's destroyed
    public void OnBallDestroyed(GameObject ball)
    {
        if (activeBall == ball)
        {
            activeBall = null;
            Debug.Log("Ball destroyed - can shoot again");
        }
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
    
    void SetupTrajectoryPreview()
    {
        if (trajectoryLine == null)
        {
            GameObject trajectoryObj = new GameObject("TrajectoryPreview");
            trajectoryObj.transform.SetParent(transform);
            trajectoryLine = trajectoryObj.AddComponent<LineRenderer>();
        }
        
        trajectoryLine.positionCount = trajectoryPoints;
        trajectoryLine.startWidth = 0.05f;
        trajectoryLine.endWidth = 0.02f;
        trajectoryLine.useWorldSpace = true;
        trajectoryLine.enabled = false;
        
        // Create trajectory material if not assigned
        if (trajectoryMaterial == null)
        {
            trajectoryMaterial = new Material(Shader.Find("Sprites/Default"));
            trajectoryMaterial.color = new Color(1f, 1f, 0f, 0.7f); // Semi-transparent yellow
        }
        
        trajectoryLine.material = trajectoryMaterial;
    }
    
    public void ShowTrajectoryPreview(bool isCharged = false)
    {
        if (!showTrajectoryPreview || trajectoryLine == null) return;
        
        Vector3 startPos = shootPoint.position;
        Vector3 direction = transform.forward;
        float force = isCharged ? chargedShotForce : ballSpeed;
        
        // Calculate trajectory points
        Vector3[] points = new Vector3[trajectoryPoints];
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * trajectoryTimeStep;
            points[i] = CalculateTrajectoryPoint(startPos, direction, force, time);
            
            // Stop at obstacles
            if (CheckTrajectoryCollision(points[i]))
            {
                // Truncate trajectory at collision point
                System.Array.Resize(ref points, i + 1);
                trajectoryLine.positionCount = i + 1;
                break;
            }
        }
        
        trajectoryLine.SetPositions(points);
        trajectoryLine.enabled = true;
    }
    
    public void HideTrajectoryPreview()
    {
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
    }
    
    Vector3 CalculateTrajectoryPoint(Vector3 startPos, Vector3 direction, float force, float time)
    {
        // Simple ballistic trajectory (no gravity for now since balls don't use gravity)
        return startPos + direction * force * time;
    }
    
    bool CheckTrajectoryCollision(Vector3 point)
    {
        // Check if trajectory point hits an obstacle
        Collider[] hits = Physics.OverlapSphere(point, 0.2f, obstacleLayer);
        return hits.Length > 0;
    }
    
    void Update()
    {
        // Update trajectory preview when aiming (could be triggered by input)
        if (showTrajectoryPreview && activeBall == null)
        {
            // Show preview when not shooting
            ShowTrajectoryPreview(false);
        }
        else
        {
            HideTrajectoryPreview();
        }
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