using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameUIPanel;
    public GameObject gameOverPanel;
    
    [Header("Game UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI speedText;
    public Slider speedSlider;
    
    [Header("Main Menu Elements")]
    public Button playButton;
    public TextMeshProUGUI highScoreText;
    
    [Header("Game Over Elements")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI newHighScoreText;
    public Button retryButton;
    public Button menuButton;
    
    private int highScore = 0;
    
    void Start()
    {
        // Load high score
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // Subscribe to game events
        GameManager.OnGameStateChanged += OnGameStateChanged;
        GameManager.OnScoreChanged += OnScoreChanged;
        
        // Setup button events
        SetupButtons();
        
        // Initialize UI
        ShowMainMenu();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        GameManager.OnGameStateChanged -= OnGameStateChanged;
        GameManager.OnScoreChanged -= OnScoreChanged;
    }
    
    void SetupButtons()
    {
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
        
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);
        
        if (menuButton != null)
            menuButton.onClick.AddListener(ShowMainMenu);
    }
    
    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameActive)
        {
            UpdateGameUI();
        }
    }
    
    void UpdateGameUI()
    {
        // Update speed display
        if (speedText != null)
        {
            speedText.text = $"Speed: {GameManager.Instance.CurrentSpeed:F1}";
        }
        
        if (speedSlider != null)
        {
            float maxSpeed = GameManager.Instance.maxSpeed;
            float currentSpeed = GameManager.Instance.CurrentSpeed;
            speedSlider.value = currentSpeed / maxSpeed;
        }
    }
    
    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
        
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }
    
    public void ShowGameUI()
    {
        SetActivePanel(gameUIPanel);
    }
    
    public void ShowGameOverScreen(int finalScore)
    {
        SetActivePanel(gameOverPanel);
        
        bool isNewHighScore = finalScore > highScore;
        
        if (isNewHighScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }
        
        if (newHighScoreText != null)
        {
            newHighScoreText.gameObject.SetActive(isNewHighScore);
            if (isNewHighScore)
            {
                newHighScoreText.text = "NEW HIGH SCORE!";
            }
        }
    }
    
    void SetActivePanel(GameObject activePanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(activePanel == mainMenuPanel);
        if (gameUIPanel != null) gameUIPanel.SetActive(activePanel == gameUIPanel);
        if (gameOverPanel != null) gameOverPanel.SetActive(activePanel == gameOverPanel);
    }
    
    void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }
    
    void RetryGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    
    void OnGameStateChanged(bool isActive)
    {
        if (isActive)
        {
            ShowGameUI();
        }
    }
    
    void OnScoreChanged(int newScore, float multiplier)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {newScore}";
        }
        
        if (multiplierText != null)
        {
            multiplierText.text = multiplier > 1f ? $"x{multiplier:F1}" : "";
        }
    }
    
    // Create UI elements if they don't exist (for testing without Canvas setup)
    void CreateDefaultUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create simple text elements for testing
        if (scoreText == null)
        {
            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(canvas.transform);
            scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.text = "Score: 0";
            scoreText.fontSize = 24;
            scoreText.color = Color.white;
            
            RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(0, 1);
            scoreRect.anchoredPosition = new Vector2(10, -10);
            scoreRect.sizeDelta = new Vector2(200, 50);
        }
    }
    
    void OnValidate()
    {
        // Auto-create UI elements if missing (editor only)
        if (Application.isPlaying && scoreText == null)
        {
            CreateDefaultUI();
        }
    }
}