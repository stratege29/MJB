using UnityEngine;
using System.Collections;

public enum MovementType
{
    Linear,
    Curved,
    Zigzag,
    Circular,
    Random
}

public class ObstacleMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public MovementType movementType = MovementType.Linear;
    public Vector3 direction = Vector3.right;
    public float speed = 2f;
    public bool loopMovement = false;
    public bool destroyOnReachEnd = true;
    
    [Header("Boundaries")]
    public float leftBoundary = -4f;
    public float rightBoundary = 4f;
    public float frontBoundary = 20f;
    public float backBoundary = -10f;
    
    [Header("Curved Movement")]
    public float curveRadius = 3f;
    public float curveSpeed = 1f;
    
    [Header("Zigzag Movement")]
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 1f;
    
    [Header("Random Movement")]
    public float randomChangeInterval = 2f;
    public float randomDirectionRange = 90f;
    
    [Header("Lane Crossing")]
    public bool crossLanes = false;
    public float[] lanePositions = { -2f, 0f, 2f };
    public float laneChangeSpeed = 5f;
    
    // Private variables
    private Vector3 startPosition;
    private float movementTimer = 0f;
    private Vector3 currentDirection;
    private float nextRandomChange = 0f;
    private bool movingForward = true;
    
    // Lane crossing variables
    private int currentLane = 1; // Start in middle lane
    private int targetLane = 1;
    private bool isChangingLanes = false;
    
    void Start()
    {
        startPosition = transform.position;
        currentDirection = direction.normalized;
        nextRandomChange = Time.time + randomChangeInterval;
        
        // Initialize lane position if crossing lanes
        if (crossLanes && lanePositions.Length > 0)
        {
            // Find closest lane
            FindClosestLane();
        }
    }
    
    void Update()
    {
        movementTimer += Time.deltaTime;
        
        switch (movementType)
        {
            case MovementType.Linear:
                MoveLinear();
                break;
            case MovementType.Curved:
                MoveCurved();
                break;
            case MovementType.Zigzag:
                MoveZigzag();
                break;
            case MovementType.Circular:
                MoveCircular();
                break;
            case MovementType.Random:
                MoveRandom();
                break;
        }
        
        // Handle lane crossing
        if (crossLanes)
        {
            HandleLaneCrossing();
        }
        
        // Check boundaries
        CheckBoundaries();
    }
    
    void MoveLinear()
    {
        transform.position += currentDirection * speed * Time.deltaTime;
    }
    
    void MoveCurved()
    {
        // Move in a curved path
        float angle = movementTimer * curveSpeed;
        Vector3 curveOffset = new Vector3(
            Mathf.Sin(angle) * curveRadius,
            0,
            Mathf.Cos(angle) * curveRadius
        );
        
        Vector3 targetPosition = startPosition + curveOffset + currentDirection * speed * movementTimer;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }
    
    void MoveZigzag()
    {
        // Base linear movement
        Vector3 linearMovement = currentDirection * speed * Time.deltaTime;
        
        // Add zigzag motion perpendicular to direction
        Vector3 perpendicularDirection = Vector3.Cross(currentDirection, Vector3.up).normalized;
        float zigzagOffset = Mathf.Sin(movementTimer * zigzagFrequency * Mathf.PI * 2) * zigzagAmplitude;
        Vector3 zigzagMovement = perpendicularDirection * zigzagOffset * Time.deltaTime;
        
        transform.position += linearMovement + zigzagMovement;
    }
    
    void MoveCircular()
    {
        // Move in a circle around start position
        float angle = movementTimer * speed;
        Vector3 circularPosition = new Vector3(
            startPosition.x + Mathf.Cos(angle) * curveRadius,
            transform.position.y,
            startPosition.z + Mathf.Sin(angle) * curveRadius
        );
        
        transform.position = circularPosition;
        
        // Face movement direction
        Vector3 tangent = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle));
        if (tangent != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(tangent);
        }
    }
    
    void MoveRandom()
    {
        // Change direction randomly at intervals
        if (Time.time >= nextRandomChange)
        {
            ChangeRandomDirection();
            nextRandomChange = Time.time + randomChangeInterval;
        }
        
        // Move in current random direction
        transform.position += currentDirection * speed * Time.deltaTime;
    }
    
    void ChangeRandomDirection()
    {
        // Generate random direction within specified range
        float randomAngle = Random.Range(-randomDirectionRange * 0.5f, randomDirectionRange * 0.5f);
        Quaternion rotation = Quaternion.Euler(0, randomAngle, 0);
        currentDirection = rotation * direction.normalized;
    }
    
    void HandleLaneCrossing()
    {
        // Randomly decide to change lanes
        if (!isChangingLanes && Random.Range(0f, 1f) < 0.01f) // 1% chance per frame
        {
            StartLaneChange();
        }
        
        // Execute lane change
        if (isChangingLanes)
        {
            float targetX = lanePositions[targetLane];
            Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);
            
            transform.position = Vector3.MoveTowards(transform.position, targetPos, laneChangeSpeed * Time.deltaTime);
            
            // Check if lane change is complete
            if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
            {
                currentLane = targetLane;
                isChangingLanes = false;
            }
        }
    }
    
    void StartLaneChange()
    {
        // Choose a different lane
        int newTargetLane = currentLane;
        while (newTargetLane == currentLane)
        {
            newTargetLane = Random.Range(0, lanePositions.Length);
        }
        
        targetLane = newTargetLane;
        isChangingLanes = true;
    }
    
    void FindClosestLane()
    {
        float closestDistance = float.MaxValue;
        int closestLane = 0;
        
        for (int i = 0; i < lanePositions.Length; i++)
        {
            float distance = Mathf.Abs(transform.position.x - lanePositions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLane = i;
            }
        }
        
        currentLane = closestLane;
        targetLane = closestLane;
    }
    
    void CheckBoundaries()
    {
        Vector3 pos = transform.position;
        
        // Check horizontal boundaries
        if (pos.x < leftBoundary || pos.x > rightBoundary)
        {
            if (loopMovement)
            {
                // Wrap around
                if (pos.x < leftBoundary)
                    pos.x = rightBoundary;
                else if (pos.x > rightBoundary)
                    pos.x = leftBoundary;
                
                transform.position = pos;
            }
            else if (destroyOnReachEnd)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                // Reverse direction
                currentDirection.x = -currentDirection.x;
                movingForward = !movingForward;
            }
        }
        
        // Check forward/backward boundaries
        if (pos.z < backBoundary || pos.z > frontBoundary)
        {
            if (loopMovement)
            {
                // Wrap around
                if (pos.z < backBoundary)
                    pos.z = frontBoundary;
                else if (pos.z > frontBoundary)
                    pos.z = backBoundary;
                
                transform.position = pos;
            }
            else if (destroyOnReachEnd)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                // Reverse direction
                currentDirection.z = -currentDirection.z;
                movingForward = !movingForward;
            }
        }
    }
    
    // Public methods for external control
    public void SetDirection(Vector3 newDirection)
    {
        currentDirection = newDirection.normalized;
        direction = currentDirection;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public void PauseMovement()
    {
        enabled = false;
    }
    
    public void ResumeMovement()
    {
        enabled = true;
    }
    
    public void ReverseDirection()
    {
        currentDirection = -currentDirection;
        movingForward = !movingForward;
    }
    
    // Gizmos for visualization in editor
    void OnDrawGizmosSelected()
    {
        // Draw boundaries
        Gizmos.color = Color.red;
        
        // Horizontal boundaries
        Vector3 leftLine = new Vector3(leftBoundary, transform.position.y, transform.position.z - 5f);
        Vector3 rightLine = new Vector3(rightBoundary, transform.position.y, transform.position.z - 5f);
        Gizmos.DrawLine(leftLine, leftLine + Vector3.forward * 10f);
        Gizmos.DrawLine(rightLine, rightLine + Vector3.forward * 10f);
        
        // Forward/backward boundaries
        Vector3 frontLine = new Vector3(transform.position.x - 5f, transform.position.y, frontBoundary);
        Vector3 backLine = new Vector3(transform.position.x - 5f, transform.position.y, backBoundary);
        Gizmos.DrawLine(frontLine, frontLine + Vector3.right * 10f);
        Gizmos.DrawLine(backLine, backLine + Vector3.right * 10f);
        
        // Draw lane positions if crossing lanes
        if (crossLanes)
        {
            Gizmos.color = Color.yellow;
            foreach (float lanePos in lanePositions)
            {
                Vector3 laneStart = new Vector3(lanePos, transform.position.y, transform.position.z - 10f);
                Vector3 laneEnd = new Vector3(lanePos, transform.position.y, transform.position.z + 10f);
                Gizmos.DrawLine(laneStart, laneEnd);
            }
        }
        
        // Draw movement direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, currentDirection * 2f);
        
        // Draw movement path preview based on type
        if (movementType == MovementType.Curved)
        {
            DrawCurvedPath();
        }
        else if (movementType == MovementType.Circular)
        {
            DrawCircularPath();
        }
    }
    
    void DrawCurvedPath()
    {
        Gizmos.color = Color.green;
        Vector3 previousPoint = transform.position;
        
        for (int i = 1; i <= 20; i++)
        {
            float t = i / 20f;
            float angle = t * curveSpeed * 2f;
            Vector3 curveOffset = new Vector3(
                Mathf.Sin(angle) * curveRadius,
                0,
                Mathf.Cos(angle) * curveRadius
            );
            
            Vector3 point = startPosition + curveOffset + currentDirection * speed * t * 2f;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
    
    void DrawCircularPath()
    {
        Gizmos.color = Color.green;
        Vector3 previousPoint = Vector3.zero;
        
        for (int i = 0; i <= 36; i++)
        {
            float angle = i * 10f * Mathf.Deg2Rad;
            Vector3 point = new Vector3(
                startPosition.x + Mathf.Cos(angle) * curveRadius,
                transform.position.y,
                startPosition.z + Mathf.Sin(angle) * curveRadius
            );
            
            if (i > 0)
            {
                Gizmos.DrawLine(previousPoint, point);
            }
            previousPoint = point;
        }
    }
}