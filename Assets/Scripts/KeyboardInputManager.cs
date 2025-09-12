using UnityEngine;

public class KeyboardInputManager : MonoBehaviour
{
    [Header("Keyboard Controls")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;
    public KeyCode slideKey = KeyCode.S;
    public KeyCode shootKey = KeyCode.Space;
    
    // Events for input actions
    public delegate void SwipeAction();
    public delegate void TapAction();
    
    public event SwipeAction OnSwipeLeft;
    public event SwipeAction OnSwipeRight;
    public event SwipeAction OnSwipeUp;
    public event SwipeAction OnSwipeDown;
    public event TapAction OnTap;
    public event TapAction OnTapHold;
    
    private float shootHoldTime = 0f;
    private bool isHoldingShoot = false;
    
    void Start()
    {
        Debug.Log("✓ Keyboard Input Manager initialized (Unity 6 compatible)");
        Debug.Log("Controls: A/D = lanes, W = jump, S = slide, SPACE = shoot");
    }
    
    void Update()
    {
        // Handle keyboard input - safe and doesn't conflict with Input System
        HandleKeyboardInput();
    }
    
    void HandleKeyboardInput()
    {
        // Lane switching
        if (UnityEngine.Input.GetKeyDown(leftKey))
        {
            OnSwipeLeft?.Invoke();
            Debug.Log("Move Left (A key)");
        }
        
        if (UnityEngine.Input.GetKeyDown(rightKey))
        {
            OnSwipeRight?.Invoke();
            Debug.Log("Move Right (D key)");
        }
        
        // Jumping
        if (UnityEngine.Input.GetKeyDown(jumpKey))
        {
            OnSwipeUp?.Invoke();
            Debug.Log("Jump (W key)");
        }
        
        // Sliding
        if (UnityEngine.Input.GetKeyDown(slideKey))
        {
            OnSwipeDown?.Invoke();
            Debug.Log("Slide (S key)");
        }
        
        // Shooting
        if (UnityEngine.Input.GetKeyDown(shootKey))
        {
            isHoldingShoot = true;
            shootHoldTime = 0f;
        }
        
        if (UnityEngine.Input.GetKey(shootKey) && isHoldingShoot)
        {
            shootHoldTime += Time.deltaTime;
        }
        
        if (UnityEngine.Input.GetKeyUp(shootKey) && isHoldingShoot)
        {
            if (shootHoldTime >= 0.5f)
            {
                OnTapHold?.Invoke();
                Debug.Log("Charged Shot (SPACE held)");
            }
            else
            {
                OnTap?.Invoke();
                Debug.Log("Quick Shot (SPACE tap)");
            }
            
            isHoldingShoot = false;
            shootHoldTime = 0f;
        }
    }
    
    void OnGUI()
    {
        if (!Application.isEditor) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Keyboard Input Manager (Unity 6)");
        GUILayout.Label("CONTROLS:");
        GUILayout.Label("A = Move Left");
        GUILayout.Label("D = Move Right");
        GUILayout.Label("W = Jump");
        GUILayout.Label("S = Slide");
        GUILayout.Label("SPACE = Shoot (hold for charged)");
        
        if (isHoldingShoot)
        {
            GUILayout.Label($"Charging shot: {shootHoldTime:F1}s");
        }
        
        GUILayout.Label("(No Input System conflicts!)");
        GUILayout.EndArea();
    }
}