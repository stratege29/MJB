using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float baseSpeed = 5f;
    public float speedIncreaseRate = 0.1f;
    public float maxSpeed = 15f;
    
    [Header("Scoring")]
    public int coinPoints = 1;
    public int obstacleDestroyPoints = 5;
    public float comboMultiplierIncrease = 0.1f;
    public float maxComboMultiplier = 3f;
    
    public static GameManager Instance { get; private set; }
    
    public bool IsGameActive { get; private set; }
    public float CurrentSpeed { get; private set; }
    public int Score { get; private set; }
    public float ComboMultiplier { get; private set; }
    public int ComboCount { get; private set; }
    
    private float gameTime;
    private UIManager uiManager;
    
    public delegate void GameStateChanged(bool isActive);
    public static event GameStateChanged OnGameStateChanged;
    
    public delegate void ScoreChanged(int newScore, float multiplier);
    public static event ScoreChanged OnScoreChanged;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        ResetGame();
    }
    
    void Update()
    {
        if (IsGameActive)
        {
            UpdateGameSpeed();
            gameTime += Time.deltaTime;
        }
    }
    
    void UpdateGameSpeed()
    {
        CurrentSpeed = Mathf.Min(baseSpeed + (gameTime * speedIncreaseRate), maxSpeed);
    }
    
    public void StartGame()
    {
        IsGameActive = true;
        ResetGameStats();
        OnGameStateChanged?.Invoke(true);
        
        if (uiManager != null)
        {
            uiManager.ShowGameUI();
        }
    }
    
    public void EndGame()
    {
        IsGameActive = false;
        OnGameStateChanged?.Invoke(false);
        
        if (uiManager != null)
        {
            uiManager.ShowGameOverScreen(Score);
        }
    }
    
    public void RestartGame()
    {
        ResetGame();
        StartGame();
    }
    
    void ResetGame()
    {
        IsGameActive = false;
        ResetGameStats();
    }
    
    void ResetGameStats()
    {
        gameTime = 0f;
        CurrentSpeed = baseSpeed;
        Score = 0;
        ComboMultiplier = 1f;
        ComboCount = 0;
        OnScoreChanged?.Invoke(Score, ComboMultiplier);
    }
    
    public void AddScore(int points, bool isComboAction = false)
    {
        if (!IsGameActive) return;
        
        if (isComboAction)
        {
            ComboCount++;
            ComboMultiplier = Mathf.Min(1f + (ComboCount * comboMultiplierIncrease), maxComboMultiplier);
        }
        
        int finalPoints = Mathf.RoundToInt(points * ComboMultiplier);
        Score += finalPoints;
        
        OnScoreChanged?.Invoke(Score, ComboMultiplier);
    }
    
    public void ResetCombo()
    {
        ComboCount = 0;
        ComboMultiplier = 1f;
        OnScoreChanged?.Invoke(Score, ComboMultiplier);
    }
    
    public void CollectCoin()
    {
        AddScore(coinPoints, true);
    }
    
    public void DestroyObstacle()
    {
        AddScore(obstacleDestroyPoints, true);
    }
}