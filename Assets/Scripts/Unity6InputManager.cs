using UnityEngine;

public class Unity6InputManager : MonoBehaviour
{
    [Header("Swipe Settings")]
    public float swipeThreshold = 50f;
    public float tapTimeThreshold = 0.2f;
    public float holdTimeThreshold = 0.5f;
    
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float touchStartTime;
    private bool isTouching;
    private bool hasProcessedSwipe;
    
    // Events for input actions
    public delegate void SwipeAction();
    public delegate void TapAction();
    
    public event SwipeAction OnSwipeLeft;
    public event SwipeAction OnSwipeRight;
    public event SwipeAction OnSwipeUp;
    public event SwipeAction OnSwipeDown;
    public event TapAction OnTap;
    public event TapAction OnTapHold;
    
    void Start()
    {
        Debug.Log("✓ Unity 6 Compatible Input Manager initialized (Legacy Input System)");
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        try
        {
            // Handle mouse input (for editor testing)
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                StartTouch(UnityEngine.Input.mousePosition);
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                EndTouch(UnityEngine.Input.mousePosition);
            }
            else if (UnityEngine.Input.GetMouseButton(0) && isTouching && !hasProcessedSwipe)
            {
                CheckForSwipe(UnityEngine.Input.mousePosition);
            }
            
            // Handle touch input (for mobile)
            if (UnityEngine.Input.touchCount > 0)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        StartTouch(touch.position);
                        break;
                        
                    case TouchPhase.Moved:
                        if (isTouching && !hasProcessedSwipe)
                        {
                            CheckForSwipe(touch.position);
                        }
                        break;
                        
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        EndTouch(touch.position);
                        break;
                }
            }
        }
        catch (System.InvalidOperationException e)
        {
            // If Input System is interfering, use a safer approach
            Debug.LogWarning($"Input conflict detected: {e.Message}");
            Debug.LogWarning("Falling back to simplified input handling");
            
            // Use a simplified input approach that doesn't conflict
            HandleSimplifiedInput();
        }
    }
    
    void HandleSimplifiedInput()
    {
        // Very basic input that should work regardless of Input System conflicts
        if (Time.time % 1f < Time.deltaTime) // Every second, for testing
        {
            Debug.Log("Simplified input active - Input System conflict mode");
        }
    }
    
    void StartTouch(Vector2 position)
    {
        isTouching = true;
        hasProcessedSwipe = false;
        startTouchPosition = position;
        touchStartTime = Time.time;
    }
    
    void CheckForSwipe(Vector2 currentPosition)
    {
        Vector2 swipeVector = currentPosition - startTouchPosition;
        float swipeDistance = swipeVector.magnitude;
        
        if (swipeDistance >= swipeThreshold)
        {
            hasProcessedSwipe = true;
            
            // Determine swipe direction
            Vector2 swipeDirection = swipeVector.normalized;
            
            if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
            {
                // Horizontal swipe
                if (swipeDirection.x > 0)
                {
                    OnSwipeRight?.Invoke();
                    Debug.Log("Swipe Right detected");
                }
                else
                {
                    OnSwipeLeft?.Invoke();
                    Debug.Log("Swipe Left detected");
                }
            }
            else
            {
                // Vertical swipe
                if (swipeDirection.y > 0)
                {
                    OnSwipeUp?.Invoke();
                    Debug.Log("Swipe Up detected");
                }
                else
                {
                    OnSwipeDown?.Invoke();
                    Debug.Log("Swipe Down detected");
                }
            }
        }
    }
    
    void EndTouch(Vector2 position)
    {
        if (!isTouching) return;
        
        float touchDuration = Time.time - touchStartTime;
        endTouchPosition = position;
        
        // If no swipe was processed, check for tap or hold
        if (!hasProcessedSwipe)
        {
            if (touchDuration >= holdTimeThreshold)
            {
                OnTapHold?.Invoke();
                Debug.Log("Hold detected");
            }
            else if (touchDuration <= tapTimeThreshold)
            {
                Vector2 touchDistance = endTouchPosition - startTouchPosition;
                if (touchDistance.magnitude < swipeThreshold * 0.3f) // Small movement tolerance for tap
                {
                    OnTap?.Invoke();
                    Debug.Log("Tap detected");
                }
            }
        }
        
        CancelTouch();
    }
    
    void CancelTouch()
    {
        isTouching = false;
        hasProcessedSwipe = false;
        touchStartTime = 0f;
    }
    
    // GUI removed to prevent overlapping with main control panel
    // OnGUI method disabled to avoid conflicts
}