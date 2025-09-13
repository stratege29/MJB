using UnityEngine;
using System.Collections.Generic;

public class Ball : MonoBehaviour
{
    [Header("Ball Settings")]
    public float explosionRadius = 0f;
    public bool isChargedShot = false;
    
    [Header("Physics Settings")]
    public float launchForce = 30f; // 2x player max speed
    public float acceleration = 8f; // 4x faster acceleration
    public float maxSpeed = 35f; // Higher than player max speed
    public float drag = 0.1f;
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Boomerang Settings")]
    public float maxDistance = 10f;
    public float returnForce = 20f;
    public bool isReturning = false;
    
    [Header("Smooth Auto-Aim Settings")]
    public float aimRange = 20f;
    public float maxAimAngle = 75f;
    public float aimSmoothing = 8f; // Smooth interpolation factor
    public float targetPrediction = 0.5f; // Predict target movement
    public LayerMask obstacleLayerMask = -1;
    
    [Header("Lane System")]
    public float laneDistance = 2f;
    public float laneTolerance = 1.0f;
    
    private float lifetime;
    private bool hasExploded = false;
    private Transform playerTransform;
    private Vector3 startPosition;
    private Vector3 targetDirection;
    private Vector3 currentVelocity;
    private float currentSpeed = 0f;
    private float travelDistance = 0f;
    private float launchTime;
    
    private ShootingSystem shootingSystem;
    private Rigidbody ballRigidbody;
    
    // Auto-aim system
    private GameObject currentTarget;
    private Vector3 predictedTargetPosition;
    private float lastTargetUpdateTime;
    private float targetUpdateInterval = 0.1f; // Update targeting every 0.1 seconds for performance
    
    // Lane system
    private int shootingLane;
    private List<GameObject> cachedObstacles = new List<GameObject>();
    
    // Missing variables that need to be declared
    private Vector3 currentDirection;
    private Vector3 forwardDirection;
    private List<GameObject> obstaclesInLane = new List<GameObject>();
    private float aimSpeed = 5f;
    private float closeRangeSpeed = 10f;
    
    public void Initialize(float ballLifetime, float chargedRadius, int playerLane)
    {
        lifetime = ballLifetime;
        explosionRadius = chargedRadius;
        isChargedShot = explosionRadius > 0f;
        shootingLane = playerLane;
        launchTime = Time.time;
        
        Debug.Log($"🎯 Ball initialized - Lane: {shootingLane}, Charged: {isChargedShot}");
        
        // Find player and shooting system
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            shootingSystem = player.GetComponent<ShootingSystem>();
        }
        else
        {
            Debug.LogWarning("Ball could not find player with 'Player' tag");
        }
        
        // Initialize physics
        ballRigidbody = GetComponent<Rigidbody>();
        if (ballRigidbody == null)
        {
            ballRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure rigidbody for smooth physics
        ballRigidbody.useGravity = false;
        ballRigidbody.isKinematic = false; // Use physics for smooth movement
        ballRigidbody.linearDamping = drag;
        ballRigidbody.mass = isChargedShot ? 2f : 1f;
        
        // Set initial values
        startPosition = transform.position;
        targetDirection = transform.forward;
        currentDirection = transform.forward;
        forwardDirection = transform.forward;
        currentVelocity = targetDirection * launchForce;
        maxDistance = isChargedShot ? 15f : 10f;
        
        // Launch the ball with initial force
        Launch();
        
        // Setup collision detection
        SetupCollider();
        
        // Setup visual components
        if (GetComponent<MeshRenderer>() == null)
        {
            SetupVisuals();
        }
        
        SetupTrail();
        
        // Initialize targeting system
        StartCoroutine(InitializeTargeting());
    }
    
    void Launch()
    {
        // Apply initial launch force
        float initialForce = isChargedShot ? launchForce * 1.5f : launchForce;
        ballRigidbody.linearVelocity = targetDirection * initialForce;
        currentSpeed = initialForce;
        
        Debug.Log($"⚽ Ball launched with force: {initialForce}, direction: {targetDirection}");
    }
    
    void SetupCollider()
    {
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        
        sphereCollider.isTrigger = true;
        sphereCollider.radius = isChargedShot ? 0.5f : 0.3f; // Larger for charged shots
    }
    
    System.Collections.IEnumerator InitializeTargeting()
    {
        // Wait one frame for physics to settle
        yield return null;
        
        // Initial target scan
        UpdateTargeting();
        lastTargetUpdateTime = Time.time;
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
                shootingSystem = player.GetComponent<ShootingSystem>();
            }
            else
            {
                return; // Still no player found
            }
        }
        
        // Check lifetime and destroy if expired
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f && !isReturning)
        {
            DestroyBall();
            return;
        }
        
        if (!isReturning)
        {
            // Update physics-based movement
            UpdatePhysicsMovement();
            
            // Update targeting system (performance optimized)
            if (Time.time - lastTargetUpdateTime >= targetUpdateInterval)
            {
                UpdateTargeting();
                lastTargetUpdateTime = Time.time;
            }
            
            // Apply auto-aim force smoothly
            ApplyAutoAim();
            
            // Track travel distance
            travelDistance += ballRigidbody.linearVelocity.magnitude * Time.deltaTime;
            
            // Start returning when max distance reached
            if (travelDistance >= maxDistance)
            {
                StartReturn();
            }
        }
        else
        {
            // Return to player with smooth physics
            ReturnToPlayer();
        }
        
        // Smooth visual rotation
        RotateBall();
    }
    
    private float ballSpeed = 15f; // Default speed (kept for compatibility)
    
    public void SetSpeed(float speed)
    {
        ballSpeed = speed;
        launchForce = speed; // Update launch force as well
    }
    
    void UpdatePhysicsMovement()
    {
        // Apply acceleration over time using animation curve - much faster now
        float timeSinceLaunch = Time.time - launchTime;
        float normalizedTime = Mathf.Clamp01(timeSinceLaunch / 0.5f); // 0.5 seconds to reach max speed
        float targetSpeed = speedCurve.Evaluate(normalizedTime) * maxSpeed;
        
        // Smoothly adjust speed with higher acceleration
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        
        // Maintain forward momentum
        Vector3 currentDir = ballRigidbody.linearVelocity.normalized;
        if (currentDir != Vector3.zero)
        {
            ballRigidbody.linearVelocity = currentDir * currentSpeed;
        }
    }
    
    void UpdateTargeting()
    {
        if (currentTarget == null || !IsTargetValid(currentTarget))
        {
            FindNewTarget();
        }
        
        if (currentTarget != null)
        {
            PredictTargetPosition();
        }
    }
    
    void ApplyAutoAim()
    {
        if (currentTarget == null || predictedTargetPosition == Vector3.zero) return;
        
        Vector3 directionToTarget = (predictedTargetPosition - transform.position).normalized;
        Vector3 currentVelocityDir = ballRigidbody.linearVelocity.normalized;
        
        // Calculate angle to target
        float angleToTarget = Vector3.Angle(currentVelocityDir, directionToTarget);
        
        if (angleToTarget <= maxAimAngle)
        {
            // Smooth steering towards target
            Vector3 steerForce = Vector3.Slerp(currentVelocityDir, directionToTarget, aimSmoothing * Time.deltaTime);
            ballRigidbody.linearVelocity = steerForce * ballRigidbody.linearVelocity.magnitude;
            
            Debug.Log($"🎯 Auto-aiming towards {currentTarget.name}, angle: {angleToTarget:F1}°");
        }
    }
    
    void StartReturn()
    {
        isReturning = true;
        currentTarget = null;
        Debug.Log("⚽ Ball starting return journey");
        
        // Add some upward arc for visual appeal
        Vector3 returnDirection = playerTransform != null ? 
            (playerTransform.position - transform.position).normalized : 
            -targetDirection;
        
        returnDirection.y += 0.3f; // Add slight upward arc
        ballRigidbody.linearVelocity = returnDirection.normalized * returnForce;
    }
    
    void ReturnToPlayer()
    {
        if (playerTransform == null) return;
        
        Vector3 directionToPlayer = (playerTransform.position + Vector3.up - transform.position).normalized;
        
        // Apply return force smoothly
        ballRigidbody.linearVelocity = Vector3.Lerp(ballRigidbody.linearVelocity, directionToPlayer * returnForce, 5f * Time.deltaTime);
        
        // Check if close enough to player to be caught
        if (Vector3.Distance(transform.position, playerTransform.position) < 1.5f)
        {
            DestroyBall();
        }
    }
    
    void RotateBall()
    {
        // Visual rotation based on velocity
        float rotationSpeed = isChargedShot ? 1080f : 720f;
        float velocityMagnitude = ballRigidbody.linearVelocity.magnitude;
        float normalizedSpeed = Mathf.Clamp01(velocityMagnitude / maxSpeed);
        
        transform.Rotate(Vector3.up * rotationSpeed * normalizedSpeed * Time.deltaTime);
    }
    
    bool IsTargetValid(GameObject target)
    {
        if (target == null) return false;
        
        // Check if target is still in the same lane
        float targetLaneX = target.transform.position.x;
        float expectedLaneX = shootingLane * laneDistance;
        
        return Mathf.Abs(targetLaneX - expectedLaneX) <= laneTolerance;
    }
    
    void FindNewTarget()
    {
        // Clear previous target
        currentTarget = null;
        cachedObstacles.Clear();
        
        // Find obstacles in sphere around ball
        Collider[] obstacles = Physics.OverlapSphere(transform.position, aimRange, obstacleLayerMask);
        
        foreach (Collider obstacle in obstacles)
        {
            if (obstacle.CompareTag("Obstacle") && IsInCorrectLane(obstacle.gameObject))
            {
                cachedObstacles.Add(obstacle.gameObject);
            }
        }
        
        // Sort by distance and pick closest
        if (cachedObstacles.Count > 0)
        {
            cachedObstacles.Sort((a, b) => 
            {
                float distA = Vector3.Distance(transform.position, a.transform.position);
                float distB = Vector3.Distance(transform.position, b.transform.position);
                return distA.CompareTo(distB);
            });
            
            currentTarget = cachedObstacles[0];
            Debug.Log($"🎯 New target acquired: {currentTarget.name}");
        }
    }
    
    bool IsInCorrectLane(GameObject obstacle)
    {
        float obstacleLaneX = obstacle.transform.position.x;
        float expectedLaneX = shootingLane * laneDistance;
        
        return Mathf.Abs(obstacleLaneX - expectedLaneX) <= laneTolerance;
    }
    
    void PredictTargetPosition()
    {
        if (currentTarget == null) return;
        
        // For static obstacles, prediction is just the current position
        predictedTargetPosition = currentTarget.transform.position;
        
        // For moving obstacles, predict future position
        Rigidbody targetRb = currentTarget.GetComponent<Rigidbody>();
        if (targetRb != null && targetRb.linearVelocity.magnitude > 0.1f)
        {
            float timeToReach = Vector3.Distance(transform.position, currentTarget.transform.position) / ballRigidbody.linearVelocity.magnitude;
            predictedTargetPosition += targetRb.linearVelocity * timeToReach * targetPrediction;
        }
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
        bool shouldBounce = false;
        
        if (isChargedShot && explosionRadius > 0f)
        {
            // Charged shot - affect multiple obstacles
            Collider[] nearbyObstacles = Physics.OverlapSphere(transform.position, explosionRadius);
            
            foreach (Collider obstacle in nearbyObstacles)
            {
                if (obstacle.CompareTag("Obstacle"))
                {
                    bool destroyed = DamageObstacle(obstacle.gameObject, 2, true);
                    if (destroyed) obstacleDestroyed = true;
                }
            }
            
            // Create explosion effect
            CreateExplosionEffect();
        }
        else
        {
            // Regular shot - affect only the hit obstacle
            bool destroyed = DamageObstacle(hitObstacle.gameObject, 1, false);
            obstacleDestroyed = destroyed;
            
            // Check if we should bounce off this obstacle
            UrbanObstacle urbanObstacle = hitObstacle.GetComponent<UrbanObstacle>();
            if (urbanObstacle != null)
            {
                shouldBounce = (urbanObstacle.collisionBehavior == CollisionBehavior.Bouncy || 
                               urbanObstacle.collisionBehavior == CollisionBehavior.Indestructible);
            }
        }
        
        // Handle post-collision behavior
        if (obstacleDestroyed || isChargedShot)
        {
            DestroyBall();
        }
        else if (shouldBounce)
        {
            // Ball bounces off certain urban obstacles
            HandleBounce(hitObstacle);
        }
        else
        {
            // Ball bounces off if it didn't destroy the obstacle
            Debug.Log("Ball bounced off strong obstacle - continuing trajectory");
        }
    }
    
    bool DamageObstacle(GameObject obstacleObject, int damage, bool isCharged)
    {
        // Try UrbanObstacle first (extends Obstacle)
        UrbanObstacle urbanObstacle = obstacleObject.GetComponent<UrbanObstacle>();
        if (urbanObstacle != null)
        {
            bool destroyed = urbanObstacle.TakeDamage(damage, isCharged);
            
            // Trigger special effects for urban obstacles
            ObstacleEffects effects = obstacleObject.GetComponent<ObstacleEffects>();
            if (effects != null)
            {
                if (destroyed)
                {
                    effects.PlayEffect(GetDestructionEffectType(urbanObstacle.urbanType), transform.position);
                }
                else
                {
                    effects.PlayEffect(EffectType.Impact, transform.position);
                }
            }
            
            return destroyed;
        }
        
        // Fallback to regular Obstacle
        Obstacle obstacle = obstacleObject.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            return obstacle.TakeDamage(damage, isCharged);
        }
        
        return false;
    }
    
    EffectType GetDestructionEffectType(UrbanObstacleType urbanType)
    {
        switch (urbanType)
        {
            case UrbanObstacleType.TrashBin: return EffectType.Plastic;
            case UrbanObstacleType.StreetSign: return EffectType.Metal;
            case UrbanObstacleType.VendorCart: return EffectType.Wood;
            case UrbanObstacleType.FireHydrant: return EffectType.WaterSpray;
            case UrbanObstacleType.ParkedCar: return EffectType.Metal;
            case UrbanObstacleType.FlowerPot: return EffectType.Stone;
            case UrbanObstacleType.ShoppingCart: return EffectType.Metal;
            case UrbanObstacleType.BusStop: return EffectType.Glass;
            case UrbanObstacleType.StreetLamp: return EffectType.Glass;
            case UrbanObstacleType.MarketStall: return EffectType.Fabric;
            default: return EffectType.Destruction;
        }
    }
    
    void HandleBounce(Collider hitObstacle)
    {
        // Calculate bounce direction
        Vector3 surfaceNormal = (transform.position - hitObstacle.transform.position).normalized;
        Vector3 bounceDirection = Vector3.Reflect(currentDirection, surfaceNormal);
        bounceDirection = bounceDirection.normalized;
        
        // Apply bounce
        Rigidbody ballRb = GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.linearVelocity = bounceDirection * 10f; // Bounce speed
        }
        
        // Create bounce visual effect
        ObstacleEffects effects = hitObstacle.GetComponent<ObstacleEffects>();
        if (effects != null)
        {
            effects.PlayEffect(EffectType.Ricochet, transform.position);
        }
        
        Debug.Log("Ball bounced off urban obstacle!");
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
    
    System.Collections.IEnumerator InitialTargetScan()
    {
        // Attendre 1 frame pour s'assurer que tout est initialisé
        yield return null;
        
        Debug.Log($"🎯 Scan initial - Lane: {shootingLane}");
        
        GameObject firstTarget = FindNearestObstacleInPath();
        if (firstTarget != null)
        {
            Vector3 targetDirection = CalculateTrajectoryToTarget(firstTarget);
            
            if (targetDirection != Vector3.zero)
            {
                Debug.Log($"🎯 Orientation immédiate vers {firstTarget.name} (Y: {firstTarget.transform.position.y:F1})");
                
                // Orienter immédiatement vers le premier obstacle
                currentDirection = targetDirection;
                currentTarget = firstTarget;
                
                // Optionnel: Orienter aussi la rotation visuelle de la balle
                transform.rotation = Quaternion.LookRotation(currentDirection);
            }
        }
        else
        {
            Debug.Log($"🎯 Aucun obstacle détecté dans la lane {shootingLane} - trajectoire droite");
        }
    }
    
    void UpdateAutoAim()
    {
        if (isReturning) return;
        
        // NOUVEAU: Utiliser ciblage multiple pour zéro obstacle manqué
        obstaclesInLane = FindAllObstaclesInLane();
        
        GameObject targetObstacle = null;
        
        // Essayer de cibler chaque obstacle dans l'ordre (plus proche en premier)
        foreach (GameObject obstacle in obstaclesInLane)
        {
            Vector3 targetDirection = CalculateTrajectoryToTarget(obstacle);
            
            if (targetDirection != Vector3.zero)
            {
                // Cet obstacle peut être ciblé !
                targetObstacle = obstacle;
                
                // Distance à l'obstacle pour ajustement adaptatif
                float distanceToTarget = Vector3.Distance(transform.position, obstacle.transform.position);
                
                // Ajustement adaptatif : plus proche = plus rapide
                float adaptiveSpeed = aimSpeed;
                
                if (distanceToTarget < 3f)
                {
                    // Très proche : ajustement quasi-immédiat
                    adaptiveSpeed = closeRangeSpeed * 2f; // 20f par défaut
                    Debug.Log($"🎯 Ajustement IMMÉDIAT - {obstacle.name} Distance: {distanceToTarget:F1}");
                }
                else if (distanceToTarget < 6f)
                {
                    // Proche : ajustement rapide
                    adaptiveSpeed = closeRangeSpeed; // 10f par défaut
                    Debug.Log($"🎯 Ajustement RAPIDE - {obstacle.name} Distance: {distanceToTarget:F1}");
                }
                else if (distanceToTarget < 10f)
                {
                    // Moyenne distance : ajustement accéléré
                    adaptiveSpeed = aimSpeed * 1.5f; // 7.5f par défaut
                }
                
                currentDirection = Vector3.Slerp(currentDirection, targetDirection, adaptiveSpeed * Time.deltaTime);
                currentTarget = obstacle;
                break; // Cibler ce premier obstacle valide
            }
        }
        
        // Si aucun obstacle ciblable, forcer trajectoire vers centre de lane
        if (targetObstacle == null && currentTarget != null)
        {
            currentDirection = Vector3.Slerp(currentDirection, forwardDirection, aimSpeed * 0.5f * Time.deltaTime);
            if (Vector3.Angle(currentDirection, forwardDirection) < 5f)
            {
                currentTarget = null;
            }
        }
        
        // NOUVEAU: Si aucun obstacle trouvé, forcer direction vers centre de lane
        if (obstaclesInLane.Count == 0 && !isReturning)
        {
            ForceCorrectLaneTrajectory();
        }
    }
    
    void ForceCorrectLaneTrajectory()
    {
        // Calculer la position du centre de la lane devant
        float expectedLaneX = shootingLane * laneDistance;
        Vector3 laneCenter = new Vector3(expectedLaneX, transform.position.y, transform.position.z + 15f);
        
        // Direction vers le centre de la lane
        Vector3 directionToLaneCenter = (laneCenter - transform.position).normalized;
        
        // Correction douce vers le centre
        float correctionSpeed = aimSpeed * 0.3f; // Plus doux que l'auto-aim
        currentDirection = Vector3.Slerp(currentDirection, directionToLaneCenter, correctionSpeed * Time.deltaTime);
        
        // Debug seulement si déviation significative
        float deviationAngle = Vector3.Angle(currentDirection, forwardDirection);
        if (deviationAngle > 10f)
        {
            Debug.Log($"🎯 Correction trajectoire lane {shootingLane} - Déviation: {deviationAngle:F1}°");
        }
    }
    
    GameObject FindNearestObstacleInPath()
    {
        Collider[] obstacles = Physics.OverlapSphere(transform.position, aimRange, obstacleLayerMask);
        GameObject nearest = null;
        float nearestZDistance = float.MaxValue;
        int obstacleCount = 0;
        
        foreach (Collider obstacle in obstacles)
        {
            if (!obstacle.CompareTag("Obstacle")) continue;
            if (obstacle.gameObject == currentTarget) continue;
            
            // NOUVEAU: Filtrer par lane avant tout autre test
            if (!IsObstacleInSameLane(obstacle.gameObject)) continue;
            
            obstacleCount++;
            
            // CHANGÉ: Utiliser distance Z (profondeur) au lieu de distance 3D
            // Cela permet de cibler les obstacles hauts/bas sans pénalité
            float zDistance = obstacle.transform.position.z - transform.position.z;
            
            // Ne considérer que les obstacles devant (Z positif)
            if (zDistance > 0 && zDistance < nearestZDistance)
            {
                // Vérifier toujours l'accessibilité
                if (IsObstacleReachable(obstacle.gameObject))
                {
                    nearest = obstacle.gameObject;
                    nearestZDistance = zDistance;
                }
            }
        }
        
        if (obstacleCount == 0)
        {
            Debug.Log($"🎯 Aucun obstacle trouvé dans la lane {shootingLane}");
        }
        else if (nearest != null)
        {
            float obstacleY = nearest.transform.position.y;
            Debug.Log($"🎯 Ciblage {nearest.name} - Lane: {shootingLane}, Hauteur Y: {obstacleY:F1}, Distance Z: {nearestZDistance:F1}");
        }
        
        return nearest;
    }
    
    List<GameObject> FindAllObstaclesInLane()
    {
        Collider[] obstacles = Physics.OverlapSphere(transform.position, aimRange, obstacleLayerMask);
        List<GameObject> validObstacles = new List<GameObject>();
        
        foreach (Collider obstacle in obstacles)
        {
            if (!obstacle.CompareTag("Obstacle")) continue;
            
            // Filtrer par lane et direction
            if (!IsObstacleInSameLane(obstacle.gameObject)) continue;
            
            // Check if this is an avoidable urban obstacle
            UrbanObstacle urbanObstacle = obstacle.GetComponent<UrbanObstacle>();
            if (urbanObstacle != null && urbanObstacle.collisionBehavior == CollisionBehavior.Avoidable)
            {
                // Skip avoidable obstacles for auto-aim (they will dodge)
                continue;
            }
            
            // Vérifier accessibilité avec critères assouplis
            if (!IsObstacleReachable(obstacle.gameObject)) continue;
            
            validObstacles.Add(obstacle.gameObject);
        }
        
        // Trier par distance Z (le plus proche en premier)
        validObstacles.Sort((a, b) => {
            float distA = a.transform.position.z - transform.position.z;
            float distB = b.transform.position.z - transform.position.z;
            return distA.CompareTo(distB);
        });
        
        if (validObstacles.Count > 0)
        {
            Debug.Log($"🎯 {validObstacles.Count} obstacles trouvés dans la lane {shootingLane}");
            for (int i = 0; i < Mathf.Min(validObstacles.Count, 3); i++)
            {
                Debug.Log($"  {i+1}. {validObstacles[i].name} (Y: {validObstacles[i].transform.position.y:F1}, Z: {validObstacles[i].transform.position.z:F1})");
            }
        }
        
        return validObstacles;
    }
    
    Vector3 CalculateTrajectoryToTarget(GameObject target)
    {
        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
        
        // Séparer les composantes horizontales et verticales
        Vector3 horizontalDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Vector3 forwardHorizontal = new Vector3(forwardDirection.x, 0, forwardDirection.z).normalized;
        
        // Calculer l'angle horizontal (pour rester dans la lane)
        float horizontalAngle = Vector3.Angle(forwardHorizontal, horizontalDirection);
        
        // Limite d'angle horizontal assouplie pour capturer plus d'obstacles (max 45°)
        if (horizontalAngle > 45f)
        {
            Debug.Log($"❌ Obstacle hors limite horizontale: {horizontalAngle:F1}°");
            return Vector3.zero; // Trop loin latéralement
        }
        
        // Permettre tout angle vertical pour viser haut/bas
        float verticalAngle = Mathf.Atan2(directionToTarget.y, 
            new Vector2(directionToTarget.x, directionToTarget.z).magnitude) * Mathf.Rad2Deg;
        
        Debug.Log($"✅ Trajectoire OK - Horizontal: {horizontalAngle:F1}°, Vertical: {verticalAngle:F1}°");
        
        // Retourner la direction complète (avec composante verticale)
        return directionToTarget;
    }
    
    bool IsObstacleInSameLane(GameObject obstacle)
    {
        // Calculer la position X attendue pour la lane de tir
        float expectedLaneX = shootingLane * laneDistance;
        
        // Vérifier si l'obstacle est dans la même lane avec tolérance
        float obstacleX = obstacle.transform.position.x;
        float distanceFromLane = Mathf.Abs(obstacleX - expectedLaneX);
        
        // Vérifier que l'obstacle est devant la balle (ignorer ceux derrière)
        bool isAhead = obstacle.transform.position.z > transform.position.z - 0.5f; // Petite marge
        
        bool inSameLane = distanceFromLane <= laneTolerance && isAhead;
        
        if (inSameLane)
        {
            float obstacleY = obstacle.transform.position.y;
            Debug.Log($"🎯 Obstacle {obstacle.name} dans lane {shootingLane} (X: {obstacleX:F1}, Y: {obstacleY:F1}, Z: {obstacle.transform.position.z:F1})");
        }
        
        return inSameLane;
    }
    
    bool IsObstacleReachable(GameObject obstacle)
    {
        // ANCIEN: Raycast restrictif qui bloquait les obstacles cachés
        // NOUVEAU: Vérification simple basée sur l'angle horizontal seulement
        
        Vector3 directionToObstacle = (obstacle.transform.position - transform.position).normalized;
        Vector3 horizontalDirection = new Vector3(directionToObstacle.x, 0, directionToObstacle.z).normalized;
        Vector3 forwardHorizontal = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        
        float horizontalAngle = Vector3.Angle(forwardHorizontal, horizontalDirection);
        
        // Très permissif : tant que l'obstacle est grossièrement devant, il est atteignable
        bool reachable = horizontalAngle <= 60f; // Très large pour capturer tous les obstacles de la lane
        
        if (!reachable)
        {
            Debug.Log($"⚠️ Obstacle {obstacle.name} non atteignable - Angle horizontal: {horizontalAngle:F1}°");
        }
        
        return reachable;
    }
    
    void OnDrawGizmosSelected()
    {
        if (isChargedShot && explosionRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
        
        // Zone de détection auto-aim (réduite)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aimRange);
        
        // Visualiser la lane de tir
        if (Application.isPlaying)
        {
            float expectedLaneX = shootingLane * laneDistance;
            Vector3 laneStart = new Vector3(expectedLaneX, transform.position.y - 1f, transform.position.z - 5f);
            Vector3 laneEnd = new Vector3(expectedLaneX, transform.position.y + 1f, transform.position.z + 20f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(laneStart, laneEnd);
            
            // Zone de tolérance de la lane
            Gizmos.color = new Color(0, 0, 1, 0.3f);
            Vector3 toleranceSize = new Vector3(laneTolerance * 2f, 2f, 25f);
            Vector3 toleranceCenter = new Vector3(expectedLaneX, transform.position.y, transform.position.z + 7.5f);
            Gizmos.DrawCube(toleranceCenter, toleranceSize);
        }
        
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}