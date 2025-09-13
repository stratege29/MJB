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
        
        // Handle keyboard input - with Input System conflict protection
        if (CanUseLegacyInput())
        {
            HandleKeyboardInput();
        }
    }
    
    private static bool? legacyInputAvailable = null; // Cache pour éviter tests répétés
    
    bool CanUseLegacyInput()
    {
        // Retourne le résultat mis en cache si déjà testé
        if (legacyInputAvailable.HasValue)
        {
            return legacyInputAvailable.Value;
        }
        
        try
        {
            // Test simple mais robuste pour Legacy Input
            bool testInput = UnityEngine.Input.inputString != null;
            // Si on arrive ici, Legacy Input fonctionne
            legacyInputAvailable = true;
            Debug.Log("✅ Legacy Input disponible - contrôles clavier activés");
            return true;
        }
        catch (System.InvalidOperationException)
        {
            legacyInputAvailable = false;
            Debug.LogWarning("⚠️ Input System conflict - contrôles clavier désactivés. Utilisez GUI ou boutons de test.");
            return false;
        }
        catch (System.Exception e)
        {
            legacyInputAvailable = false;
            Debug.LogWarning($"⚠️ Erreur input inattendue: {e.Message}");
            return false;
        }
    }
    
    void HandleKeyboardInput()
    {
        // Movement controls - avec debug pour tests
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TriggerMoveLeft();
            Debug.Log("🎮 CLAVIER: Déplacer gauche (A/←)");
        }
        
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            TriggerMoveRight();
            Debug.Log("🎮 CLAVIER: Déplacer droite (D/→)");
        }
        
        // Action controls - avec debug pour tests
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            TriggerJump();
            Debug.Log("🎮 CLAVIER: Saut (W/↑)");
        }
        
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            TriggerSlide();
            Debug.Log("🎮 CLAVIER: Glisser (S/↓)");
        }
        
        // Shooting controls - améliorés pour cohérence avec GUI
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isCharging)
            {
                // Tir simple immédiat si pas en train de charger
                TriggerShoot();
                Debug.Log("🎮 CLAVIER: Tir simple (Espace)");
            }
        }
        
        // Charged shot - gestion améliorée du maintien
        if (Input.GetKey(KeyCode.Space))
        {
            if (!isCharging)
            {
                StartChargedShoot();
                Debug.Log("🎮 CLAVIER: Début charge tir (maintenir Espace)");
            }
        }
        else if (isCharging)
        {
            // Relâcher la touche = exécuter le tir chargé
            PerformChargedShoot();
            Debug.Log("🎮 CLAVIER: Tir chargé exécuté (relâcher Espace)");
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
            
            Rect chargeButtonRect = GUILayoutUtility.GetRect(new GUIContent("HOLD FOR CHARGED"), buttonStyle);
            
            // Handle mouse events for hold-to-charge
            if (Event.current.type == EventType.MouseDown && chargeButtonRect.Contains(Event.current.mousePosition))
            {
                if (!isCharging)
                {
                    StartChargedShoot();
                    Event.current.Use();
                }
            }
            else if (Event.current.type == EventType.MouseUp && isCharging)
            {
                PerformChargedShoot();
                Event.current.Use();
            }
            
            // Visual button state
            GUI.Button(chargeButtonRect, isCharging ? "CHARGING..." : "HOLD FOR CHARGED", buttonStyle);
            
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
            GUILayout.Label("• GUI: Click buttons above", labelStyle);
            
            // Show keyboard status
            if (CanUseLegacyInput())
            {
                GUILayout.Label("• CLAVIER: A/D = lanes, W/S = saut/glisser", labelStyle);
                GUILayout.Label("• ESPACE: tir simple ou maintenu = chargé", labelStyle);
                GUILayout.Label("✅ Contrôles clavier actifs!", labelStyle);
            }
            else
            {
                GUILayout.Label("⚠️ Contrôles clavier indisponibles", labelStyle);
                GUILayout.Label("→ Redémarrez Unity après config", labelStyle);
                GUILayout.Label("→ Boutons TEST disponibles →", labelStyle);
            }
            GUILayout.Label("Unity 6 Compatible!", labelStyle);
        }
        
        GUILayout.EndArea();
        
        // Boutons de test flottants si Legacy Input indisponible
        if (!CanUseLegacyInput())
        {
            DrawTestingButtons();
        }
    }
    
    void DrawTestingButtons()
    {
        // Position des boutons de test à droite de l'écran
        float buttonSize = 60f;
        float spacing = 10f;
        float rightMargin = 10f;
        float startX = Screen.width - buttonSize - rightMargin;
        float startY = 100f;
        
        GUIStyle testButtonStyle = new GUIStyle(GUI.skin.button);
        testButtonStyle.fontSize = 18;
        testButtonStyle.fontStyle = FontStyle.Bold;
        
        // Titre des contrôles de test
        Rect titleRect = new Rect(startX - 50, startY - 30, buttonSize + 100, 25);
        GUI.Label(titleRect, "CONTRÔLES TEST", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });
        
        // Boutons de déplacement
        Rect leftButtonRect = new Rect(startX - 35, startY, buttonSize, buttonSize);
        Rect rightButtonRect = new Rect(startX + 35, startY, buttonSize, buttonSize);
        
        if (GUI.Button(leftButtonRect, "A\n←", testButtonStyle))
        {
            TriggerMoveLeft();
        }
        
        if (GUI.Button(rightButtonRect, "D\n→", testButtonStyle))
        {
            TriggerMoveRight();
        }
        
        // Boutons d'action
        float actionY = startY + buttonSize + spacing;
        Rect jumpButtonRect = new Rect(startX, actionY, buttonSize, buttonSize);
        Rect slideButtonRect = new Rect(startX, actionY + buttonSize + spacing, buttonSize, buttonSize);
        
        if (GUI.Button(jumpButtonRect, "W\n↑\nSAUT", testButtonStyle))
        {
            TriggerJump();
        }
        
        if (GUI.Button(slideButtonRect, "S\n↓\nGLISS", testButtonStyle))
        {
            TriggerSlide();
        }
        
        // Boutons de tir
        float shootY = actionY + (buttonSize + spacing) * 2;
        Rect shootButtonRect = new Rect(startX, shootY, buttonSize, buttonSize);
        Rect chargeButtonRect = new Rect(startX, shootY + buttonSize + spacing, buttonSize, buttonSize * 1.2f);
        
        if (GUI.Button(shootButtonRect, "TIR\nSIMPLE", testButtonStyle))
        {
            TriggerShoot();
        }
        
        // Bouton de charge avec détection MouseDown/Up
        if (Event.current.type == EventType.MouseDown && chargeButtonRect.Contains(Event.current.mousePosition))
        {
            if (!isCharging)
            {
                StartChargedShoot();
                Event.current.Use();
            }
        }
        else if (Event.current.type == EventType.MouseUp && isCharging)
        {
            PerformChargedShoot();
            Event.current.Use();
        }
        
        string chargeText = isCharging ? "CHARGE\n..." : "TIR\nCHARGÉ";
        GUI.Button(chargeButtonRect, chargeText, testButtonStyle);
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