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
    
    // Original config values for resetting after rounds
    private float originalPlayerSpeed;
    private float originalDashSpeed;
    private float originalDashCooldown;
    private int originalHitsToFail;
    private float originalHazardSpawnRate;
    private float originalGoodSpawnRate;
    private float originalDurationSeconds;
    
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
        
        // Save original config values first
        SaveOriginalConfigValues();
        
        // Apply rule effects to config values
        ApplyRuleEffects();
        
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
            spawnerController.OnItemCollectedEvent += OnItemCollected;
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
            // Trigger player death animation and end game after delay
            if (player != null)
            {
                player.Die();
                StartCoroutine(EndGameAfterDeathAnimation());
            }
            else
            {
                EndGame();
            }
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
    
    IEnumerator EndGameAfterDeathAnimation()
    {
        // Prevent multiple calls
        if (currentState == GravityFlipGameState.Ending || currentState == GravityFlipGameState.Results)
            yield break;
            
        currentState = GravityFlipGameState.Ending;
        
        // Wait for death animation to play (adjust time as needed for your animation length)
        yield return new WaitForSeconds(1.5f);
        
        // Restore original config values when game ends
        RestoreOriginalConfigValues();
        
        StartCoroutine(EndGameSequence());
    }
    
    void EndGame()
    {
        if (currentState == GravityFlipGameState.Ending || currentState == GravityFlipGameState.Results)
            return;
        
        currentState = GravityFlipGameState.Ending;
        
        // Restore original config values when game ends
        RestoreOriginalConfigValues();
        
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
        // Restore original config values before ending round
        RestoreOriginalConfigValues();
        
        // Continue to next phase in game loop
        if (currentState == GravityFlipGameState.Results)
        {
            GameStateManager.I?.OnRoundEnd(0); // Winner doesn't matter for single-player minigame
        }
    }
    
    void SaveOriginalConfigValues()
    {
        originalPlayerSpeed = config.playerSpeed;
        originalDashSpeed = config.dashSpeed;
        originalDashCooldown = config.dashCooldown;
        originalHitsToFail = config.hitsToFail;
        originalHazardSpawnRate = config.hazardSpawnRate;
        originalGoodSpawnRate = config.goodSpawnRate;
        originalDurationSeconds = config.durationSeconds;
        
        Debug.Log("Original config values saved for reset after round");
    }
    
    void RestoreOriginalConfigValues()
    {
        config.playerSpeed = originalPlayerSpeed;
        config.dashSpeed = originalDashSpeed;
        config.dashCooldown = originalDashCooldown;
        config.hitsToFail = originalHitsToFail;
        config.hazardSpawnRate = originalHazardSpawnRate;
        config.goodSpawnRate = originalGoodSpawnRate;
        config.durationSeconds = originalDurationSeconds;
        
        Debug.Log("Config values restored to original defaults");
    }
    
    void ApplyRuleEffects()
    {
        if (GameStateManager.I == null || GameStateManager.I.activeRules == null) return;
        
        Debug.Log($"Applying rule effects from {GameStateManager.I.activeRules.Count} active rules");
        
        foreach (RuleCard rule in GameStateManager.I.activeRules)
        {
            if (!rule.IsCompatibleWith(MinigameType.GravityFlipDodge)) continue;
            
            Debug.Log($"Applying effects from rule: {rule.title}");
            
            // Apply positive effects
            if (rule.effects != null)
            {
                foreach (RuleEffect effect in rule.effects)
                {
                    ApplyRuleEffect(effect, rule.title, "effect");
                }
            }
            
            // Apply negative effects (drawbacks)
            if (rule.drawbacks != null)
            {
                foreach (RuleEffect drawback in rule.drawbacks)
                {
                    ApplyRuleEffect(drawback, rule.title, "drawback");
                }
            }
        }
    }
    
    void ApplyRuleEffect(RuleEffect effect, string cardName, string effectType)
    {
        switch (effect.stat)
        {
            case Stat.MoveSpeed:
                float oldMoveSpeed = config.playerSpeed;
                config.playerSpeed += effect.addFlat;
                config.playerSpeed *= (1f + effect.addPct);
                Debug.Log($"{cardName} {effectType}: MoveSpeed {oldMoveSpeed:F1} → {config.playerSpeed:F1}");
                break;
                
            case Stat.HitPoints:
                int oldHits = config.hitsToFail;
                config.hitsToFail += Mathf.RoundToInt(effect.addFlat);
                config.hitsToFail = Mathf.RoundToInt(config.hitsToFail * (1f + effect.addPct));
                Debug.Log($"{cardName} {effectType}: HitPoints {oldHits} → {config.hitsToFail}");
                break;
                
            case Stat.DashSpeed:
                float oldDashSpeed = config.dashSpeed;
                config.dashSpeed += effect.addFlat;
                config.dashSpeed *= (1f + effect.addPct);
                Debug.Log($"{cardName} {effectType}: DashSpeed {oldDashSpeed:F1} → {config.dashSpeed:F1}");
                break;
                
            case Stat.DashCooldown:
                float oldDashCooldown = config.dashCooldown;
                config.dashCooldown += effect.addFlat;
                config.dashCooldown *= (1f + effect.addPct);
                Debug.Log($"{cardName} {effectType}: DashCooldown {oldDashCooldown:F1} → {config.dashCooldown:F1}");
                break;
                
            case Stat.ItemSpawnRate:
                float oldHazardRate = config.hazardSpawnRate;
                config.hazardSpawnRate += effect.addFlat;
                config.hazardSpawnRate *= (1f + effect.addPct);
                Debug.Log($"{cardName} {effectType}: ItemSpawnRate {oldHazardRate:F1} → {config.hazardSpawnRate:F1}");
                break;
                
            case Stat.GameDuration:
                float oldDuration = config.durationSeconds;
                config.durationSeconds += effect.addFlat;
                config.durationSeconds *= (1f + effect.addPct);
                Debug.Log($"{cardName} {effectType}: GameDuration {oldDuration:F1} → {config.durationSeconds:F1}");
                break;
                
            default:
                Debug.LogWarning($"Unhandled stat in rule effect: {effect.stat} from {cardName}");
                break;
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