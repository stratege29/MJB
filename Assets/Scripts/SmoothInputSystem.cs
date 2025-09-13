using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SmoothInputSystem : MonoBehaviour
{
    [Header("Touch & Mouse Input")]
    public bool enableTouchInput = true;
    public bool enableMouseInput = true;
    public float tapThreshold = 0.2f; // Time threshold for tap vs hold
    public float swipeThreshold = 50f; // Pixel threshold for swipe detection
    
    [Header("Charging System")]
    public float chargeTime = 1.5f;
    public AnimationCurve chargeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool enableHaptics = true;
    
    [Header("Visual Feedback")]
    public GameObject chargingIndicator;
    public RectTransform chargeProgressRing;
    public CanvasGroup inputUI;
    
    [Header("Audio Feedback")]
    public AudioClip tapSound;
    public AudioClip chargeStartSound;
    public AudioClip chargeCompleteSound;
    public AudioClip shootSound;
    
    [Header("Events")]
    public UnityEvent OnMoveLeft;
    public UnityEvent OnMoveRight;
    public UnityEvent OnJump;
    public UnityEvent OnSlide;
    public UnityEvent OnQuickShoot;
    public UnityEvent OnChargedShoot;
    public UnityEvent<float> OnChargingUpdate; // Float = charge progress (0-1)
    
    // Input state
    private bool isCharging = false;
    private float chargeStartTime;
    private float currentChargeLevel = 0f;
    private bool isFullyCharged = false;
    
    // Touch tracking
    private Vector2 touchStartPos;
    private float touchStartTime;
    private bool isTouching = false;
    
    // Keyboard charging tracking
    private bool isShiftCharging = false;
    private float shiftChargeStartTime;
    
    // Audio
    private AudioSource audioSource;
    
    // References
    private ShootingSystem shootingSystem;
    
    void Start()
    {
        // Get components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        shootingSystem = FindObjectOfType<ShootingSystem>();
        
        // Setup UI
        SetupChargingIndicator();
        
        Debug.Log("✨ Smooth Input System initialized - Touch and responsive controls ready!");
    }
    
    void SetupChargingIndicator()
    {
        if (chargingIndicator == null)
        {
            // Create charging indicator UI
            GameObject canvasGO = GameObject.Find("Canvas");
            if (canvasGO == null)
            {
                canvasGO = new GameObject("InputCanvas");
                Canvas canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            // Create charging ring
            CreateChargingRing(canvasGO);
        }
    }
    
    void CreateChargingRing(GameObject canvas)
    {
        chargingIndicator = new GameObject("ChargingIndicator");
        chargingIndicator.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = chargingIndicator.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.2f); // Bottom center
        rectTransform.anchorMax = new Vector2(0.5f, 0.2f);
        rectTransform.sizeDelta = new Vector2(100, 100);
        
        // Add visual components
        UnityEngine.UI.Image backgroundRing = chargingIndicator.AddComponent<UnityEngine.UI.Image>();
        backgroundRing.color = new Color(1, 1, 1, 0.2f);
        backgroundRing.type = UnityEngine.UI.Image.Type.Filled;
        backgroundRing.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
        
        // Create progress ring
        GameObject progressObj = new GameObject("ProgressRing");
        progressObj.transform.SetParent(chargingIndicator.transform, false);
        chargeProgressRing = progressObj.AddComponent<RectTransform>();
        chargeProgressRing.anchorMin = Vector2.zero;
        chargeProgressRing.anchorMax = Vector2.one;
        chargeProgressRing.offsetMin = Vector2.zero;
        chargeProgressRing.offsetMax = Vector2.zero;
        
        UnityEngine.UI.Image progressImage = progressObj.AddComponent<UnityEngine.UI.Image>();
        progressImage.color = new Color(1f, 0.8f, 0f, 0.8f); // Golden
        progressImage.type = UnityEngine.UI.Image.Type.Filled;
        progressImage.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
        progressImage.fillAmount = 0f;
        
        chargingIndicator.SetActive(false);
    }
    
    void Update()
    {
        HandleInput();
        UpdateChargingSystem();
        UpdateVisualFeedback();
    }
    
    void HandleInput()
    {
        // Handle touch input
        if (enableTouchInput && Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        // Fallback to mouse input
        else if (enableMouseInput)
        {
            HandleMouseInput();
        }
        
        // Handle keyboard shortcuts (for testing)
        HandleKeyboardShortcuts();
    }
    
    void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);
        
        switch (touch.phase)
        {
            case TouchPhase.Began:
                StartTouch(touch.position);
                break;
                
            case TouchPhase.Moved:
                // Could handle directional swipes here
                break;
                
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndTouch(touch.position);
                break;
        }
    }
    
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartTouch(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndTouch(Input.mousePosition);
        }
    }
    
    void StartTouch(Vector2 screenPosition)
    {
        touchStartPos = screenPosition;
        touchStartTime = Time.time;
        isTouching = true;
        
        // Start charging
        StartCharging();
        
        // Play tap feedback
        PlaySound(tapSound);
        
        // Haptic feedback
        if (enableHaptics)
        {
            TriggerHaptic(HapticFeedbackType.LightImpact);
        }
    }
    
    void EndTouch(Vector2 screenPosition)
    {
        if (!isTouching) return;
        
        float touchDuration = Time.time - touchStartTime;
        Vector2 touchDelta = screenPosition - touchStartPos;
        float touchDistance = touchDelta.magnitude;
        
        isTouching = false;
        
        // Determine input type based on duration and distance
        if (touchDistance > swipeThreshold)
        {
            // Handle swipe
            HandleSwipe(touchDelta);
        }
        else if (touchDuration < tapThreshold)
        {
            // Quick tap
            HandleTap();
        }
        else
        {
            // Long press / charged shot
            HandleLongPress();
        }
        
        StopCharging();
    }
    
    void HandleSwipe(Vector2 swipeDelta)
    {
        Vector2 swipeDirection = swipeDelta.normalized;
        
        if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
        {
            // Horizontal swipe
            if (swipeDirection.x > 0)
            {
                TriggerMoveRight();
            }
            else
            {
                TriggerMoveLeft();
            }
        }
        else
        {
            // Vertical swipe
            if (swipeDirection.y > 0)
            {
                TriggerJump();
            }
            else
            {
                TriggerSlide();
            }
        }
        
        Debug.Log($"🎮 Swipe detected: {swipeDirection}");
    }
    
    void HandleTap()
    {
        TriggerQuickShoot();
        Debug.Log("🎮 Quick tap - Normal shot");
    }
    
    void HandleLongPress()
    {
        if (isFullyCharged)
        {
            TriggerChargedShoot();
            Debug.Log("🎮 Long press - Charged shot");
        }
        else
        {
            TriggerQuickShoot();
            Debug.Log("🎮 Long press incomplete - Normal shot");
        }
    }
    
    void HandleKeyboardShortcuts()
    {
        // Keyboard shortcuts for testing
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TriggerMoveLeft();
        }
        
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            TriggerMoveRight();
        }
        
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            TriggerJump();
        }
        
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            TriggerSlide();
        }
        
        // Quick shoot with Space (tap only)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerQuickShoot();
        }
        
        // Charged shot with LeftShift (hold and release)
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (!isShiftCharging)
            {
                // Start charging when LeftShift is pressed
                StartShiftCharging();
            }
            else
            {
                // Update charging while holding
                UpdateShiftCharging();
            }
        }
        else if (isShiftCharging)
        {
            // Release LeftShift - fire charged shot
            EndShiftCharging();
        }
    }
    
    void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        currentChargeLevel = 0f;
        isFullyCharged = false;
        
        PlaySound(chargeStartSound);
        chargingIndicator?.SetActive(true);
        
        // Show trajectory preview for charged shot
        if (shootingSystem != null)
        {
            shootingSystem.ShowTrajectoryPreview(true);
        }
        
        Debug.Log("⚡ Charging started");
    }
    
    void StopCharging()
    {
        isCharging = false;
        currentChargeLevel = 0f;
        isFullyCharged = false;
        
        chargingIndicator?.SetActive(false);
        
        // Hide trajectory preview
        if (shootingSystem != null)
        {
            shootingSystem.HideTrajectoryPreview();
        }
        
        Debug.Log("⚡ Charging stopped");
    }
    
    void UpdateChargingSystem()
    {
        if (!isCharging) return;
        
        float chargeDuration = Time.time - chargeStartTime;
        float normalizedCharge = Mathf.Clamp01(chargeDuration / chargeTime);
        currentChargeLevel = chargeCurve.Evaluate(normalizedCharge);
        
        // Trigger events
        OnChargingUpdate.Invoke(currentChargeLevel);
        
        // Check if fully charged
        if (currentChargeLevel >= 1f && !isFullyCharged)
        {
            isFullyCharged = true;
            PlaySound(chargeCompleteSound);
            
            if (enableHaptics)
            {
                TriggerHaptic(HapticFeedbackType.MediumImpact);
            }
            
            Debug.Log("⚡ Charge complete!");
        }
    }
    
    void UpdateVisualFeedback()
    {
        if (chargeProgressRing != null)
        {
            UnityEngine.UI.Image progressImage = chargeProgressRing.GetComponent<UnityEngine.UI.Image>();
            if (progressImage != null)
            {
                progressImage.fillAmount = currentChargeLevel;
                
                // Color feedback
                Color chargeColor = Color.Lerp(
                    new Color(1f, 1f, 0f, 0.6f), // Yellow
                    new Color(1f, 0.3f, 0f, 0.9f), // Orange-red
                    currentChargeLevel
                );
                progressImage.color = chargeColor;
                
                // Pulse effect when fully charged
                if (isFullyCharged)
                {
                    float pulse = Mathf.Sin(Time.time * 5f) * 0.1f + 0.9f;
                    progressImage.color = chargeColor * pulse;
                }
            }
        }
        
        // Show/hide charging indicator based on any charging state
        if (chargingIndicator != null)
        {
            bool shouldShowIndicator = isCharging || isShiftCharging;
            chargingIndicator.SetActive(shouldShowIndicator);
        }
    }
    
    // Event triggers
    void TriggerMoveLeft()
    {
        OnMoveLeft.Invoke();
        PlaySound(tapSound);
        if (enableHaptics) TriggerHaptic(HapticFeedbackType.LightImpact);
    }
    
    void TriggerMoveRight()
    {
        OnMoveRight.Invoke();
        PlaySound(tapSound);
        if (enableHaptics) TriggerHaptic(HapticFeedbackType.LightImpact);
    }
    
    void TriggerJump()
    {
        OnJump.Invoke();
        PlaySound(tapSound);
        if (enableHaptics) TriggerHaptic(HapticFeedbackType.MediumImpact);
    }
    
    void TriggerSlide()
    {
        OnSlide.Invoke();
        PlaySound(tapSound);
        if (enableHaptics) TriggerHaptic(HapticFeedbackType.MediumImpact);
    }
    
    void TriggerQuickShoot()
    {
        OnQuickShoot.Invoke();
        PlaySound(shootSound);
        if (enableHaptics) TriggerHaptic(HapticFeedbackType.HeavyImpact);
    }
    
    void TriggerChargedShoot()
    {
        OnChargedShoot.Invoke();
        PlaySound(shootSound);
        if (enableHaptics) TriggerHaptic(HapticFeedbackType.HeavyImpact);
    }
    
    // LeftShift charging methods
    void StartShiftCharging()
    {
        isShiftCharging = true;
        shiftChargeStartTime = Time.time;
        
        // Start visual charging
        StartCharging();
        
        Debug.Log("⚡ LeftShift charging started");
    }
    
    void UpdateShiftCharging()
    {
        // Calculate charge level
        float chargeDuration = Time.time - shiftChargeStartTime;
        float normalizedCharge = Mathf.Clamp01(chargeDuration / chargeTime);
        currentChargeLevel = chargeCurve.Evaluate(normalizedCharge);
        
        // Update visual feedback
        OnChargingUpdate.Invoke(currentChargeLevel);
        
        // Check if fully charged
        if (currentChargeLevel >= 1f && !isFullyCharged)
        {
            isFullyCharged = true;
            PlaySound(chargeCompleteSound);
            Debug.Log("⚡ LeftShift fully charged!");
        }
    }
    
    void EndShiftCharging()
    {
        float chargeDuration = Time.time - shiftChargeStartTime;
        
        if (chargeDuration >= 0.3f) // Minimum charge time for charged shot
        {
            TriggerChargedShoot();
            Debug.Log($"⚡ LeftShift charged shot fired (held for {chargeDuration:F1}s)");
        }
        else
        {
            TriggerQuickShoot();
            Debug.Log($"⚡ LeftShift released too early - normal shot (held for {chargeDuration:F1}s)");
        }
        
        // Reset charging state
        isShiftCharging = false;
        StopCharging();
    }
    
    // Audio system
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Haptic feedback
    public enum HapticFeedbackType
    {
        LightImpact,
        MediumImpact,
        HeavyImpact
    }
    
    void TriggerHaptic(HapticFeedbackType type)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android haptics
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        
        switch (type)
        {
            case HapticFeedbackType.LightImpact:
                vibrator.Call("vibrate", 25);
                break;
            case HapticFeedbackType.MediumImpact:
                vibrator.Call("vibrate", 50);
                break;
            case HapticFeedbackType.HeavyImpact:
                vibrator.Call("vibrate", 75);
                break;
        }
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS haptics would be implemented via native plugins
        UnityEngine.iOS.Handheld.Vibrate();
#endif
        
        // Editor simulation
#if UNITY_EDITOR
        Debug.Log($"🎮 Haptic feedback: {type}");
#endif
    }
}