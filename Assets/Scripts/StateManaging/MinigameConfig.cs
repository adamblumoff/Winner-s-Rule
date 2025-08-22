using UnityEngine;

[CreateAssetMenu(menuName = "WR/MinigameConfig")]
public class MinigameConfig : ScriptableObject
{
    [Header("Game Duration")]
    public float durationSeconds = 25f;
    
    [Header("Gravity Settings")]
    public bool startGravity = true; // true = down, false = up
    public float gravityDown = -18f;
    public float gravityUp = 18f;
    
    [Header("Player Movement")]
    public float playerSpeed = 9f;
    public float dashSpeed = 18f;
    public float dashTime = 0.12f;
    public float dashCooldown = 1.5f;
    
    [Header("Spawning")]
    public float hazardSpawnRate = 0.8f; // per second
    public float goodSpawnRate = 0.5f; // per second
    public int maxSimultaneous = 10;
    
    [Header("Gravity Flip Timing")]
    public float flipIntervalMin = 3.0f;
    public float flipIntervalMax = 6.0f;
    public float flipWarningTime = 0.6f;
    
    [Header("Health System")]
    public int hitsToFail = 1;
    
    [Header("Scoring")]
    public int scoreGoodPickup = 3;
    public int scorePerSecond = 1;
    public float rewardScalar = 0.15f; // converts score to draft points
    
    [Header("Difficulty Ramp")]
    public float spawnRateRampFactor = 0.6f; // multiplier increase over duration
    public float speedRampFactor = 0.2f; // speed increase by end
    public float flipIntervalRampFactor = 0.15f; // interval decrease by end
    
    [Header("Burst Patterns")]
    public bool useBurstWaves = true;
    public float burstInterval = 7f; // every X seconds
    public float burstDuration = 1.5f;
    public float burstRateMultiplier = 2.5f;
}