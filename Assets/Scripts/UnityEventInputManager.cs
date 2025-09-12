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
    private float chargeTime = 1.5f; // Time required to fully charge
    private bool isFullyCharged = false;
    private bool isGUIReady = false;
    private float initializationDelay = 0.1f; // Delay to ensure camera is ready
    
    void Start()
    {
        Debug.Log("✓ Unity Event Input Manager initialized (Zero Input System conflicts)");
        Debug.Log("Using GUI buttons and Unity Events - completely safe in Unity 6");
        
        // Delay GUI initialization to ensure camera is ready
        StartCoroutine(InitializeGUIWithDelay());
    }
    
    System.Collections.IEnumerator InitializeGUIWithDelay()
    {
        // Wait for initialization delay
        yield return new WaitForSeconds(initializationDelay);
        
        // Verify camera is ready
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.pixelWidth > 0 && mainCam.pixelHeight > 0)
        {
            isGUIReady = true;
            Debug.Log($"✓ GUI Ready - Camera viewport: {mainCam.pixelWidth}x{mainCam.pixelHeight}");
        }
        else
        {
            // If camera not ready, wait a bit more
            yield return new WaitForSeconds(0.5f);
            isGUIReady = true;
            Debug.LogWarning("GUI enabled after extended wait");
        }
    }
    
    void Update()
    {
        // Handle charging logic
        if (isCharging)
        {
            float chargeDuration = Time.time - chargeStartTime;
            
            // Check if fully charged
            if (chargeDuration >= chargeTime && !isFullyCharged)
            {
                isFullyCharged = true;
                Debug.Log("Shot fully charged!");
                // Could add charge complete sound effect here
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
            isFullyCharged = false;
            chargeStartTime = Time.time;
            Debug.Log("Charging shot...");
        }
    }
    
    public void PerformChargedShoot()
    {
        if (isCharging)
        {
            // Only perform charged shot if minimum charge time is met
            float chargeDuration = Time.time - chargeStartTime;
            
            if (chargeDuration >= 0.3f) // Minimum charge time
            {
                OnTapHold?.Invoke();
                OnChargedShoot?.Invoke();
                Debug.Log($"Charged Shot triggered (charged for {chargeDuration:F1}s)");
            }
            else
            {
                // Not charged enough, perform normal shot
                OnTap?.Invoke();
                OnShoot?.Invoke();
                Debug.Log("Insufficient charge - performing normal shot");
            }
            
            isCharging = false;
            isFullyCharged = false;
        }
    }
    
    void OnGUI()
    {
        if (!enableGUIControls || !isGUIReady) return;
        
        // Use SafeGUIRenderer for additional safety
        if (!SafeGUIRenderer.CanRenderGUI()) return;
        
        // Validate screen dimensions before rendering
        if (!ValidateScreenDimensions()) return;
        
        // Style setup for better visibility
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fixedHeight = 40;
        
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 12;
        
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        
        // Main control panel with background - use safe rect creation
        Rect boxRect = SafeGUIRenderer.CreateSafeRect(5, 5, 280, 450);
        Rect areaRect = SafeGUIRenderer.CreateSafeRect(15, 15, 260, 430);
        
        GUI.Box(boxRect, "");
        GUILayout.BeginArea(areaRect);
        
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
            
            // Charging status with visual gauge
            GUILayout.Label("CHARGE STATUS:", labelStyle);
            
            if (isCharging)
            {
                float chargeDuration = Time.time - chargeStartTime;
                float chargePercent = Mathf.Clamp01(chargeDuration / chargeTime);
                
                // Create charging gauge
                Rect gaugeRect = GUILayoutUtility.GetRect(200, 20);
                
                // Background
                GUI.color = Color.black;
                GUI.DrawTexture(gaugeRect, Texture2D.whiteTexture);
                
                // Charge fill
                Rect fillRect = new Rect(gaugeRect.x + 2, gaugeRect.y + 2, (gaugeRect.width - 4) * chargePercent, gaugeRect.height - 4);
                
                // Color progression: yellow → orange → red → gold when full
                Color chargeColor;
                if (chargePercent < 0.33f)
                    chargeColor = Color.yellow;
                else if (chargePercent < 0.66f)
                    chargeColor = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), (chargePercent - 0.33f) / 0.33f); // Orange
                else if (chargePercent < 1f)
                    chargeColor = Color.Lerp(new Color(1f, 0.5f, 0f), Color.red, (chargePercent - 0.66f) / 0.34f);
                else
                    chargeColor = new Color(1f, 0.84f, 0f); // Gold when fully charged
                
                GUI.color = chargeColor;
                if (fillRect.width > 0)
                    GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
                
                // Reset color
                GUI.color = Color.white;
                
                // Charge text
                string chargeText = isFullyCharged ? "FULLY CHARGED!" : $"Charging: {chargePercent * 100:F0}%";
                GUIStyle chargeStyle = new GUIStyle(labelStyle);
                if (isFullyCharged)
                {
                    chargeStyle.normal.textColor = Color.yellow;
                }
                GUILayout.Label(chargeText, chargeStyle);
            }
            else
            {
                GUILayout.Label("Ready to shoot", labelStyle);
                // Reserve space for gauge when not charging
                GUILayoutUtility.GetRect(200, 20);
            }
            
            GUILayout.Space(8);
            
            // Debug info - ALWAYS show fixed number of labels
            GUILayout.Label("DEBUG INFO:", labelStyle);
            var gameManager = FindObjectOfType<GameManager>();
            var playerController = FindObjectOfType<PlayerController>();
            string gameActiveText = gameManager != null ? $"Game Active: {gameManager.IsGameActive}" : "Game Active: UNKNOWN";
            string speedText = gameManager != null ? $"Speed: {gameManager.CurrentSpeed:F1}" : "Speed: UNKNOWN";
            string scoreText = gameManager != null ? $"Score: {gameManager.Score}" : "Score: UNKNOWN";
            
            // Add jump status info
            string jumpText = "Jumps: UNKNOWN";
            if (playerController != null)
            {
                // Use reflection to get private field values for debugging
                var jumpField = typeof(PlayerController).GetField("jumpsRemaining", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var groundedField = typeof(PlayerController).GetField("isGrounded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (jumpField != null && groundedField != null)
                {
                    int jumpsRemaining = (int)jumpField.GetValue(playerController);
                    bool isGrounded = (bool)groundedField.GetValue(playerController);
                    jumpText = $"Jumps: {jumpsRemaining}/2 (Grounded: {isGrounded})";
                }
            }
            
            GUILayout.Label(gameActiveText, labelStyle);
            GUILayout.Label(speedText, labelStyle);
            GUILayout.Label(scoreText, labelStyle);
            GUILayout.Label(jumpText, labelStyle);
            
            GUILayout.Space(8);
            GUILayout.Label("CONTROLS:", labelStyle);
            GUILayout.Label("• Click buttons above", labelStyle);
            GUILayout.Label("• GUI controls only", labelStyle);
            GUILayout.Label("• No Input System conflicts!", labelStyle);
            GUILayout.Label("Unity 6 Compatible!", labelStyle);
        }
        
        GUILayout.EndArea();
    }
    
    bool ValidateScreenDimensions()
    {
        // Check if screen dimensions are valid
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return false;
        }
        
        // Check if camera is properly initialized
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            return false;
        }
        
        // Verify camera viewport is valid
        if (mainCam.pixelWidth <= 0 || mainCam.pixelHeight <= 0)
        {
            return false;
        }
        
        // Additional safety check for GUI rendering area
        try
        {
            // Test if we can create a rect without issues
            Rect testRect = new Rect(0, 0, Mathf.Min(280, Screen.width), Mathf.Min(450, Screen.height));
            if (testRect.width <= 0 || testRect.height <= 0)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
        
        return true;
    }
    
    void OnDisable()
    {
        // Clean up when disabled
        isGUIReady = false;
    }
    
    void OnEnable()
    {
        // Re-initialize when enabled
        if (Application.isPlaying)
        {
            StartCoroutine(InitializeGUIWithDelay());
        }
    }
}