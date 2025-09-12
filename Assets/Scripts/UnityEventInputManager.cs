using UnityEngine;
using UnityEngine.Events;

public class UnityEventInputManager : MonoBehaviour
{
    [Header("Unity Event Input (No Input System Conflicts)")]
    public bool enableGUIControls = true;
    public bool showControlsGUI = true;
    
    [Header("Unity Events")]
    public UnityEvent OnMoveLeft;
    public UnityEvent OnMoveRight;
    public UnityEvent OnJump;
    public UnityEvent OnSlide;
    public UnityEvent OnShoot;
    public UnityEvent OnChargedShoot;
    
    // Events for other systems to subscribe to
    public delegate void InputAction();
    public event InputAction OnSwipeLeft;
    public event InputAction OnSwipeRight;
    public event InputAction OnSwipeUp;
    public event InputAction OnSwipeDown;
    public event InputAction OnTap;
    public event InputAction OnTapHold;
    
    private bool isCharging = false;
    private float chargeStartTime;
    
    void Start()
    {
        Debug.Log("✓ Unity Event Input Manager initialized (Zero Input System conflicts)");
        Debug.Log("Using GUI buttons and Unity Events - completely safe in Unity 6");
    }
    
    void Update()
    {
        // Handle charging logic
        if (isCharging)
        {
            if (Time.time - chargeStartTime >= 0.5f)
            {
                // Auto-release charged shot after 0.5 seconds
                PerformChargedShoot();
                isCharging = false;
            }
        }
        
        // Handle keyboard input (Legacy Input System - safe in Unity 6)
        // TEMPORARILY DISABLED: HandleKeyboardInput(); // Causing Input System conflicts
        // Unity may need restart to switch input modes properly
    }
    
    void HandleKeyboardInput()
    {
        // Movement controls
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TriggerMoveLeft();
        }
        
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            TriggerMoveRight();
        }
        
        // Action controls
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            TriggerJump();
        }
        
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            TriggerSlide();
        }
        
        // Shooting controls
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerShoot();
        }
        
        // Charged shot (hold Space for longer than 0.3 seconds)
        if (Input.GetKey(KeyCode.Space))
        {
            if (!isCharging)
            {
                StartChargedShoot();
            }
        }
        else if (isCharging)
        {
            PerformChargedShoot();
        }
    }
    
    // Public methods that can be called from GUI or other sources
    public void TriggerMoveLeft()
    {
        var gameManager = FindObjectOfType<GameManager>();
        Debug.Log($"Move Left triggered - GameActive: {gameManager?.IsGameActive}");
        OnSwipeLeft?.Invoke();
        OnMoveLeft?.Invoke();
    }
    
    public void TriggerMoveRight()
    {
        OnSwipeRight?.Invoke();
        OnMoveRight?.Invoke();
        Debug.Log("Move Right triggered");
    }
    
    public void TriggerJump()
    {
        OnSwipeUp?.Invoke();
        OnJump?.Invoke();
        Debug.Log("Jump triggered");
    }
    
    public void TriggerSlide()
    {
        OnSwipeDown?.Invoke();
        OnSlide?.Invoke();
        Debug.Log("Slide triggered");
    }
    
    public void TriggerShoot()
    {
        OnTap?.Invoke();
        OnShoot?.Invoke();
        Debug.Log("Quick Shot triggered");
    }
    
    public void StartChargedShoot()
    {
        if (!isCharging)
        {
            isCharging = true;
            chargeStartTime = Time.time;
            Debug.Log("Charging shot...");
        }
    }
    
    public void PerformChargedShoot()
    {
        OnTapHold?.Invoke();
        OnChargedShoot?.Invoke();
        Debug.Log("Charged Shot triggered");
        isCharging = false;
    }
    
    void OnGUI()
    {
        if (!enableGUIControls) return;
        
        // Style setup for better visibility
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fixedHeight = 40;
        
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 12;
        
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        
        // Main control panel with background
        GUI.Box(new Rect(5, 5, 280, 450), "");
        GUILayout.BeginArea(new Rect(15, 15, 260, 430));
        
        if (showControlsGUI)
        {
            GUILayout.Label("Just Play Mariam Controls", titleStyle);
            GUILayout.Space(10);
            
            // Movement section
            GUILayout.Label("MOVEMENT:", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("← LEFT (A)", buttonStyle, GUILayout.Width(115)))
            {
                TriggerMoveLeft();
            }
            if (GUILayout.Button("RIGHT (D) →", buttonStyle, GUILayout.Width(115)))
            {
                TriggerMoveRight();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(8);
            
            // Jump and slide section
            GUILayout.Label("ACTIONS:", labelStyle);
            if (GUILayout.Button("↑ JUMP (W)", buttonStyle))
            {
                TriggerJump();
            }
            
            if (GUILayout.Button("↓ SLIDE (S)", buttonStyle))
            {
                TriggerSlide();
            }
            
            GUILayout.Space(8);
            
            // Shooting section
            GUILayout.Label("SHOOTING:", labelStyle);
            if (GUILayout.Button("SHOOT (SPACE)", buttonStyle))
            {
                TriggerShoot();
            }
            
            if (GUILayout.RepeatButton("HOLD FOR CHARGED", buttonStyle))
            {
                if (!isCharging)
                {
                    StartChargedShoot();
                }
            }
            else if (isCharging)
            {
                PerformChargedShoot();
            }
            
            GUILayout.Space(8);
            
            // Charging status - ALWAYS reserve space to prevent layout changes
            GUILayout.Label("STATUS:", labelStyle);
            string chargeText = isCharging ? $"Charging: {(Time.time - chargeStartTime):F1}s" : "Ready to shoot";
            GUILayout.Label(chargeText, labelStyle);
            
            GUILayout.Space(8);
            
            // Debug info - ALWAYS show fixed number of labels
            GUILayout.Label("DEBUG INFO:", labelStyle);
            var gameManager = FindObjectOfType<GameManager>();
            string gameActiveText = gameManager != null ? $"Game Active: {gameManager.IsGameActive}" : "Game Active: UNKNOWN";
            string speedText = gameManager != null ? $"Speed: {gameManager.CurrentSpeed:F1}" : "Speed: UNKNOWN";
            string scoreText = gameManager != null ? $"Score: {gameManager.Score}" : "Score: UNKNOWN";
            
            GUILayout.Label(gameActiveText, labelStyle);
            GUILayout.Label(speedText, labelStyle);
            GUILayout.Label(scoreText, labelStyle);
            
            GUILayout.Space(8);
            GUILayout.Label("CONTROLS:", labelStyle);
            GUILayout.Label("• Click buttons above", labelStyle);
            GUILayout.Label("• GUI controls only", labelStyle);
            GUILayout.Label("• No Input System conflicts!", labelStyle);
            GUILayout.Label("Unity 6 Compatible!", labelStyle);
        }
        
        GUILayout.EndArea();
    }
}