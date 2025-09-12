using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float followSpeed = 5f;
    public float rotationSpeed = 2f;
    
    [Header("Look Settings")]
    public bool lookAtTarget = true;
    public Vector3 lookOffset = Vector3.zero;
    
    [Header("Smoothing")]
    public bool useSmoothing = true;
    public float positionSmoothing = 5f;
    public float rotationSmoothing = 3f;
    
    private Vector3 velocity = Vector3.zero;
    private bool isInitialized = false;
    
    void Start()
    {
        // Initialize camera immediately
        InitializeCamera();
    }
    
    void InitializeCamera()
    {
        // Ensure camera component is properly set up
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            // Ensure camera viewport is valid
            if (cam.pixelWidth <= 0 || cam.pixelHeight <= 0)
            {
                // Force refresh camera dimensions
                cam.ResetAspect();
            }
        }
        
        // Auto-find player if no target assigned
        if (target == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // Set initial position if target exists
        if (target != null)
        {
            transform.position = target.position + offset;
            
            if (lookAtTarget)
            {
                LookAtTarget();
            }
        }
        
        isInitialized = true;
        Debug.Log($"✓ Camera initialized at position: {transform.position}");
    }
    
    void LateUpdate()
    {
        if (!isInitialized || target == null) return;
        
        UpdateCameraPosition();
        
        if (lookAtTarget)
        {
            UpdateCameraRotation();
        }
    }
    
    void UpdateCameraPosition()
    {
        Vector3 targetPosition = target.position + offset;
        
        if (useSmoothing)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref velocity, 
                1f / positionSmoothing
            );
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position, 
                targetPosition, 
                followSpeed * Time.deltaTime
            );
        }
    }
    
    void UpdateCameraRotation()
    {
        Vector3 lookPosition = target.position + lookOffset;
        Vector3 direction = lookPosition - transform.position;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            if (useSmoothing)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    rotationSmoothing * Time.deltaTime
                );
            }
            else
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
    
    void LookAtTarget()
    {
        if (target != null)
        {
            Vector3 lookPosition = target.position + lookOffset;
            transform.LookAt(lookPosition);
        }
    }
    
    // Method to set new target (useful for scene transitions)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        velocity = Vector3.zero; // Reset velocity for smooth transition
        
        // Re-initialize if needed
        if (!isInitialized)
        {
            InitializeCamera();
        }
    }
    
    // Method to set offset (useful for different camera angles)
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    // Shake effect for impacts
    public void ShakeCamera(float intensity = 0.5f, float duration = 0.2f)
    {
        StartCoroutine(CameraShake(intensity, duration));
    }
    
    private System.Collections.IEnumerator CameraShake(float intensity, float duration)
    {
        Vector3 originalOffset = offset;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float strength = Mathf.Lerp(intensity, 0f, elapsed / duration);
            Vector3 shakeOffset = Random.insideUnitSphere * strength;
            shakeOffset.z = 0f; // Don't shake forward/backward
            
            offset = originalOffset + shakeOffset;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        offset = originalOffset;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw camera target position
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position, 0.5f);
            
            // Draw offset line
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(target.position, target.position + offset);
            
            // Draw look target
            if (lookAtTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(target.position + lookOffset, 0.2f);
            }
        }
    }
    
    void OnEnable()
    {
        // Re-initialize when enabled
        if (Application.isPlaying && !isInitialized)
        {
            InitializeCamera();
        }
    }
    
    void OnDisable()
    {
        isInitialized = false;
    }
}