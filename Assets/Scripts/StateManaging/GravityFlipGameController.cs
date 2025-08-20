using UnityEngine;
using System.Collections;

public enum GravityFlipGameState
{
    Init,
    Countdown,
    Running,
    Paused,
    Ending,
    Results
}

public class GravityFlipGameController : MonoBehaviour
{
    [Header("Configuration")]
    public MinigameConfig config;
    
    [Header("References")]
    public GravityFlipPlayerController player;
    public GravityFlipController gravityController;
    public SpawnerController spawnerController;
    public GravityFlipUI gameUI;
    
    // Game state
    private GravityFlipGameState currentState = GravityFlipGameState.Init;
    private float gameTimer;
    private int score = 0;
    private int timeScore = 0;
    private int pickupScore = 0;
    private int hitsRemaining;
    private bool isPaused = false;
    
    // Statistics for results
    private int totalGoodsCollected = 0;
    private int totalGoodsSpawned = 0;
    private int hitsTaken = 0;
    
    // Events
    public System.Action<int, GameResultBreakdown> OnGameComplete;
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        currentState = GravityFlipGameState.Init;
        
        // Initialize game state
        gameTimer = config.durationSeconds;
        score = 0;
        hitsRemaining = config.hitsToFail;
        
        // Subscribe to events
        if (player != null)
        {
            player.OnHit += OnPlayerHit;
        }
        
        if (spawnerController != null)
        {
            spawnerController.OnItemCollected += OnItemCollected;
        }
        
        // Start countdown
        StartCoroutine(GameSequence());
    }
    
    void Update()
    {
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
        // Update game timer
        if (currentState == GravityFlipGameState.Running && !isPaused)
        {
            gameTimer -= Time.deltaTime;
            
            // Add time score
            float previousTime = gameTimer + Time.deltaTime;
            if (Mathf.FloorToInt(previousTime) > Mathf.FloorToInt(gameTimer))
            {
                // Full second passed
                timeScore += config.scorePerSecond;
                score = timeScore + pickupScore;
                
                if (gameUI != null)
                {
                    gameUI.UpdateScore(score);
                }
            }
            
            // Check for game end
            if (gameTimer <= 0f)
            {
                EndGame();
            }
            
            // Update UI
            if (gameUI != null)
            {
                gameUI.UpdateTimer(gameTimer, config.durationSeconds);
            }
        }
    }
    
    IEnumerator GameSequence()
    {
        // Init phase
        yield return StartCoroutine(InitPhase());
        
        // Countdown phase
        yield return StartCoroutine(CountdownPhase());
        
        // Running phase
        StartRunningPhase();
    }
    
    IEnumerator InitPhase()
    {
        currentState = GravityFlipGameState.Init;
        
        // Setup initial positions and states
        if (player != null)
        {
            player.SetInputEnabled(false);
            player.ResetPosition(Vector3.zero);
        }
        
        if (gravityController != null)
        {
            gravityController.Initialize();
        }
        
        if (spawnerController != null)
        {
            spawnerController.PauseSpawning();
        }
        
        // Initialize UI
        if (gameUI != null)
        {
            gameUI.Initialize(config);
            gameUI.UpdateScore(score);
            gameUI.UpdateTimer(gameTimer, config.durationSeconds);
            gameUI.UpdateHitsRemaining(hitsRemaining);
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    IEnumerator CountdownPhase()
    {
        currentState = GravityFlipGameState.Countdown;
        
        // 3, 2, 1 countdown
        for (int i = 3; i > 0; i--)
        {
            if (gameUI != null)
            {
                gameUI.ShowCountdown(i);
            }
            yield return new WaitForSeconds(1f);
        }
        
        // "GO!" message
        if (gameUI != null)
        {
            gameUI.ShowCountdown(0); // 0 = "GO!"
        }
        yield return new WaitForSeconds(0.5f);
        
        if (gameUI != null)
        {
            gameUI.HideCountdown();
        }
    }
    
    void StartRunningPhase()
    {
        currentState = GravityFlipGameState.Running;
        
        // Enable player input
        if (player != null)
        {
            player.SetInputEnabled(true);
        }
        
        // Start spawning
        if (spawnerController != null)
        {
            spawnerController.ResumeSpawning();
        }
        
        // Start gravity flips
        if (gravityController != null)
        {
            gravityController.ResumeFlips();
        }
    }
    
    void OnPlayerHit()
    {
        if (currentState != GravityFlipGameState.Running) return;
        
        hitsTaken++;
        hitsRemaining--;
        
        // Update UI
        if (gameUI != null)
        {
            gameUI.UpdateHitsRemaining(hitsRemaining);
            gameUI.ShowHitFeedback();
        }
        
        // Check for game over
        if (hitsRemaining <= 0)
        {
            EndGame();
        }
    }
    
    void OnItemCollected(Item item, GravityFlipPlayerController player)
    {
        if (currentState != GravityFlipGameState.Running) return;
        
        if (item.itemType == ItemType.Good)
        {
            totalGoodsCollected++;
            pickupScore += config.scoreGoodPickup;
            score = timeScore + pickupScore;
            
            // Update UI
            if (gameUI != null)
            {
                gameUI.UpdateScore(score);
                gameUI.ShowPickupFeedback(config.scoreGoodPickup);
            }
        }
        
        // Track total goods spawned for accuracy calculation
        if (item.itemType == ItemType.Good)
        {
            totalGoodsSpawned++;
        }
    }
    
    void EndGame()
    {
        if (currentState == GravityFlipGameState.Ending || currentState == GravityFlipGameState.Results)
            return;
        
        currentState = GravityFlipGameState.Ending;
        StartCoroutine(EndGameSequence());
    }
    
    IEnumerator EndGameSequence()
    {
        // Stop all game systems
        if (player != null)
        {
            player.SetInputEnabled(false);
        }
        
        if (spawnerController != null)
        {
            spawnerController.PauseSpawning();
            spawnerController.ClearAllItems();
        }
        
        if (gravityController != null)
        {
            gravityController.PauseFlips();
        }
        
        // Brief pause for impact
        yield return new WaitForSeconds(1f);
        
        // Calculate final results
        ShowResults();
    }
    
    void ShowResults()
    {
        currentState = GravityFlipGameState.Results;
        
        // Calculate final scores and stats
        float timeSurvived = config.durationSeconds - Mathf.Max(0f, gameTimer);
        float accuracy = totalGoodsSpawned > 0 ? (float)totalGoodsCollected / totalGoodsSpawned : 0f;
        int draftBonus = Mathf.RoundToInt(score * config.rewardScalar);
        
        // Create result breakdown
        GameResultBreakdown breakdown = new GameResultBreakdown
        {
            finalScore = score,
            timeScore = timeScore,
            pickupScore = pickupScore,
            timeSurvived = timeSurvived,
            goodsCollected = totalGoodsCollected,
            totalGoods = totalGoodsSpawned,
            accuracy = accuracy * 100f,
            hitsTaken = hitsTaken,
            draftBonus = draftBonus
        };
        
        // Show results UI
        if (gameUI != null)
        {
            gameUI.ShowResults(breakdown);
        }
        
        // Notify game loop
        OnGameComplete?.Invoke(draftBonus, breakdown);
    }
    
    void TogglePause()
    {
        if (currentState != GravityFlipGameState.Running && !isPaused) return;
        
        isPaused = !isPaused;
        
        if (isPaused)
        {
            Time.timeScale = 0f;
            if (gameUI != null)
            {
                gameUI.ShowPauseMenu();
            }
        }
        else
        {
            Time.timeScale = 1f;
            if (gameUI != null)
            {
                gameUI.HidePauseMenu();
            }
        }
    }
    
    public void ResumeFromPause()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }
    
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        // This would be handled by the main game loop
        GameStateManager.I?.LoadLobby();
    }
    
    public void ContinueToResults()
    {
        // Continue to next phase in game loop
        if (currentState == GravityFlipGameState.Results)
        {
            GameStateManager.I?.OnRoundEnd(0); // Winner doesn't matter for single-player minigame
        }
    }
}

[System.Serializable]
public class GameResultBreakdown
{
    public int finalScore;
    public int timeScore;
    public int pickupScore;
    public float timeSurvived;
    public int goodsCollected;
    public int totalGoods;
    public float accuracy; // percentage
    public int hitsTaken;
    public int draftBonus;
}