using UnityEngine;
using System.Collections.Generic;

public class Ball : MonoBehaviour
{
    [Header("Ball Settings")]
    public float explosionRadius = 0f;
    public bool isChargedShot = false;
    
    [Header("Boomerang Settings")]
    public float maxDistance = 10f;
    public float returnSpeed = 20f;
    public bool isReturning = false;
    
    [Header("Auto-Aim Settings")]
    public float aimRange = 20f; // Portée étendue pour détection précoce
    public float maxAimAngle = 75f; // Augmenté pour angles descendants marqués
    public float aimSpeed = 5f; // Base plus rapide pour réactivité
    public float closeRangeSpeed = 10f; // Vitesse pour obstacles proches
    public LayerMask obstacleLayerMask = -1;
    
    [Header("Lane System")]
    public float laneDistance = 2f; // Distance entre les lanes
    public float laneTolerance = 1.0f; // Tolérance augmentée pour détection
    
    private float lifetime;
    private bool hasExploded = false;
    private Transform playerTransform;
    private Vector3 startPosition;
    private Vector3 forwardDirection;
    private Vector3 currentDirection; // Current movement direction (can change for auto-aim)
    private float travelDistance = 0f;
    private ShootingSystem shootingSystem;
    private GameObject currentTarget; // Currently targeted obstacle
    private List<GameObject> obstaclesInLane = new List<GameObject>(); // NOUVEAU: Tous les obstacles de la lane
    private int shootingLane; // Lane du joueur au moment du tir (-1, 0, 1)
    
    public void Initialize(float ballLifetime, float chargedRadius, int playerLane)
    {
        lifetime = ballLifetime;
        explosionRadius = chargedRadius;
        isChargedShot = explosionRadius > 0f;
        shootingLane = playerLane;
        
        Debug.Log($"🎯 Balle initialisée - Lane: {shootingLane}, Chargée: {isChargedShot}");
        
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
        currentDirection = forwardDirection; // Initialize current direction
        maxDistance = isChargedShot ? 15f : 10f;
        
        // NOUVEAU: Scanner immédiatement pour orienter vers le premier obstacle
        StartCoroutine(InitialTargetScan());
        
        // Ensure the ball has a collider avec rayon élargi pour capture maximale
        if (GetComponent<Collider>() == null)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 0.4f; // Augmenté de 0.1f à 0.4f pour capturer plus d'obstacles
        }
        else
        {
            // Ajuster le collider existant
            SphereCollider existingCollider = GetComponent<SphereCollider>();
            if (existingCollider != null && existingCollider.radius < 0.4f)
            {
                existingCollider.radius = 0.4f;
                Debug.Log("🎯 Collider balle élargi pour capture maximale");
            }
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
        
        // Boomerang behavior with auto-aim
        if (!isReturning)
        {
            // Update auto-aim targeting
            UpdateAutoAim();
            
            // Move in current direction (which may be adjusted by auto-aim)
            transform.position += currentDirection * ballSpeed * Time.deltaTime;
            travelDistance += ballSpeed * Time.deltaTime;
            
            // Start returning when max distance reached
            if (travelDistance >= maxDistance)
            {
                isReturning = true;
                currentTarget = null; // Clear target when returning
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
        int obstaclesInLane = 0;
        
        foreach (Collider obstacle in obstacles)
        {
            if (!obstacle.CompareTag("Obstacle")) continue;
            if (obstacle.gameObject == currentTarget) continue;
            
            // NOUVEAU: Filtrer par lane avant tout autre test
            if (!IsObstacleInSameLane(obstacle.gameObject)) continue;
            
            obstaclesInLane++;
            
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
        
        if (obstaclesInLane == 0)
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