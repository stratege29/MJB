using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float laneDistance = 2f;
    public float laneChangeSpeed = 10f;
    public float jumpForce = 8f;
    public float doubleJumpForce = 6f;
    public int maxJumps = 2; // Ground jump + air jump
    public float slideHeight = 0.5f;
    public float slideDuration = 1f;
    
    [Header("Physics")]
    public LayerMask groundLayer = -1; // All layers
    public float groundCheckDistance = 0.5f;
    
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private UnityEventInputManager inputManager;
    private ShootingSystem shootingSystem;
    
    private int currentLane = 0; // -1 = left, 0 = center, 1 = right
    private Vector3 targetPosition;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isSliding;
    private float slideTimer;
    private float originalHeight;
    private float originalCenterY;
    private int jumpsRemaining;
    
    private Vector3 startPosition;
    
    public delegate void PlayerCollision();
    public static event PlayerCollision OnPlayerCollision;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        inputManager = FindObjectOfType<UnityEventInputManager>();
        shootingSystem = GetComponent<ShootingSystem>();
        jumpsRemaining = maxJumps;
        
        startPosition = transform.position;
        targetPosition = transform.position;
        
        // Store original collider dimensions
        originalHeight = capsuleCollider.height;
        originalCenterY = capsuleCollider.center.y;
        
        // Subscribe to input events
        if (inputManager != null)
        {
            inputManager.OnSwipeLeft += MoveLeft;
            inputManager.OnSwipeRight += MoveRight;
            inputManager.OnSwipeUp += Jump;
            inputManager.OnSwipeDown += StartSlide;
            inputManager.OnTap += Shoot;
            inputManager.OnTapHold += ChargedShoot;
            Debug.Log("✓ PlayerController subscribed to input events");
        }
        else
        {
            Debug.LogError("× UnityEventInputManager not found! Controls will not work.");
        }
        
        // Subscribe to game state changes
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (inputManager != null)
        {
            inputManager.OnSwipeLeft -= MoveLeft;
            inputManager.OnSwipeRight -= MoveRight;
            inputManager.OnSwipeUp -= Jump;
            inputManager.OnSwipeDown -= StartSlide;
            inputManager.OnTap -= Shoot;
            inputManager.OnTapHold -= ChargedShoot;
        }
        
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
    
    void Update()
    {
        if (!GameManager.Instance.IsGameActive) return;
        
        UpdateMovement();
        UpdateSlide();
        CheckGrounded();
        MoveForward();
    }
    
    void UpdateMovement()
    {
        // Smooth lane changing
        Vector3 newPosition = Vector3.Lerp(
            transform.position, 
            new Vector3(targetPosition.x, transform.position.y, transform.position.z), 
            laneChangeSpeed * Time.deltaTime
        );
        
        transform.position = newPosition;
    }
    
    void UpdateSlide()
    {
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                EndSlide();
            }
        }
    }
    
    void CheckGrounded()
    {
        RaycastHit hit;
        wasGrounded = isGrounded;
        
        // Check for ground using multiple methods for reliability
        bool raycastGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f, 
            Vector3.down, 
            out hit, 
            groundCheckDistance + 0.1f
        );
        
        // Also check with a small sphere cast for better ground detection
        bool spherecastGrounded = Physics.CheckSphere(
            transform.position + Vector3.down * 0.4f,
            0.3f
        );
        
        isGrounded = raycastGrounded || spherecastGrounded || transform.position.y <= 1.2f;
        
        // Additional check - if we hit something tagged as Ground
        if (raycastGrounded && hit.collider != null)
        {
            isGrounded = hit.collider.CompareTag("Ground") || hit.collider.name.Contains("Ground");
        }
        
        // Reset jumps when landing
        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
            Debug.Log("Landed - jumps reset to " + jumpsRemaining);
        }
    }
    
    void MoveForward()
    {
        // Auto-run forward
        if (GameManager.Instance != null)
        {
            transform.Translate(Vector3.forward * GameManager.Instance.CurrentSpeed * Time.deltaTime);
        }
    }
    
    void MoveLeft()
    {
        Debug.Log($"MoveLeft called - GameActive: {GameManager.Instance?.IsGameActive}, CurrentLane: {currentLane}");
        if (!GameManager.Instance.IsGameActive || currentLane <= -1) return;
        
        currentLane--;
        UpdateTargetPosition();
        Debug.Log($"Moved to lane {currentLane}, target position: {targetPosition}");
    }
    
    void MoveRight()
    {
        Debug.Log($"MoveRight called - GameActive: {GameManager.Instance?.IsGameActive}, CurrentLane: {currentLane}");
        if (!GameManager.Instance.IsGameActive || currentLane >= 1) return;
        
        currentLane++;
        UpdateTargetPosition();
        Debug.Log($"Moved to lane {currentLane}, target position: {targetPosition}");
    }
    
    void UpdateTargetPosition()
    {
        targetPosition = new Vector3(currentLane * laneDistance, transform.position.y, transform.position.z);
    }
    
    void Jump()
    {
        Debug.Log($"Jump called - GameActive: {GameManager.Instance?.IsGameActive}, isGrounded: {isGrounded}, jumpsRemaining: {jumpsRemaining}, isSliding: {isSliding}");
        
        if (!GameManager.Instance.IsGameActive || isSliding) return;
        
        // Can jump if grounded OR have jumps remaining in air
        if (jumpsRemaining > 0)
        {
            // Reduce remaining jumps
            jumpsRemaining--;
            
            // Reset vertical velocity for consistent jumps
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            
            // Use different force for ground vs air jump
            float forceToUse = isGrounded ? jumpForce : doubleJumpForce;
            rb.AddForce(Vector3.up * forceToUse, ForceMode.Impulse);
            
            string jumpType = isGrounded ? "Ground" : "Air";
            Debug.Log($"{jumpType} jump executed! Jumps remaining: {jumpsRemaining}");
            
            // Reset combo on successful action
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetCombo();
            }
        }
        else
        {
            Debug.Log("Cannot jump - no jumps remaining");
        }
    }
    
    void StartSlide()
    {
        if (!GameManager.Instance.IsGameActive || !isGrounded || isSliding) return;
        
        isSliding = true;
        slideTimer = slideDuration;
        
        // Reduce collider height
        capsuleCollider.height = slideHeight;
        capsuleCollider.center = new Vector3(0, slideHeight * 0.5f, 0);
        
        // Reset combo on successful action
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetCombo();
        }
    }
    
    void EndSlide()
    {
        isSliding = false;
        
        // Restore original collider dimensions
        capsuleCollider.height = originalHeight;
        capsuleCollider.center = new Vector3(0, originalCenterY, 0);
    }
    
    void Shoot()
    {
        Debug.Log($"Shoot called - GameActive: {GameManager.Instance?.IsGameActive}");
        if (!GameManager.Instance.IsGameActive) 
        {
            Debug.LogWarning("Game not active - cannot shoot");
            return;
        }
        
        if (shootingSystem != null)
        {
            Debug.Log("Calling QuickShot on ShootingSystem");
            shootingSystem.QuickShot();
        }
        else
        {
            Debug.LogError("ShootingSystem is null!");
        }
    }
    
    void ChargedShoot()
    {
        if (!GameManager.Instance.IsGameActive) return;
        
        if (shootingSystem != null)
        {
            shootingSystem.ChargedShot();
        }
    }
    
    void OnGameStateChanged(bool isActive)
    {
        if (!isActive)
        {
            // Reset player position when game ends
            transform.position = startPosition;
            currentLane = 0;
            UpdateTargetPosition();
            
            // Reset slide state
            if (isSliding)
            {
                EndSlide();
            }
            
            // Reset physics
            rb.linearVelocity = Vector3.zero;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            // Player hit obstacle - game over
            OnPlayerCollision?.Invoke();
            GameManager.Instance?.EndGame();
        }
        else if (other.CompareTag("Collectible"))
        {
            // Collect coin
            GameManager.Instance?.CollectCoin();
            Destroy(other.gameObject);
        }
    }
}