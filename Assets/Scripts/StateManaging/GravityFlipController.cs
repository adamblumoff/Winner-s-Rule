using UnityEngine;
using System.Collections;

public class GravityFlipController : MonoBehaviour
{
    [Header("Configuration")]
    public MinigameConfig config;
    
    [Header("Visual Effects")]
    public Transform gravityIndicator; // Arrow that shows current gravity direction
    
    // Current state
    private bool currentGravityDown = true;
    private float nextFlipTime;
    private bool warningActive = false;
    
    // Events
    public System.Action<bool> OnGravityFlipped; // bool = isGravityDown
    public System.Action OnFlipWarning;
    public System.Action OnFlipWarningEnd;
    
    // Properties
    public bool IsGravityDown => currentGravityDown;
    public float TimeToNextFlip => Mathf.Max(0f, nextFlipTime - Time.time);
    public bool IsWarningActive => warningActive;
    
    void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        currentGravityDown = config.startGravity;
        ApplyGravity();
        ScheduleNextFlip();
        UpdateGravityIndicator();
    }
    
    void Update()
    {
        // Check for flip warning
        if (!warningActive && Time.time >= nextFlipTime - config.flipWarningTime)
        {
            StartFlipWarning();
        }
        
        // Check for actual flip
        if (Time.time >= nextFlipTime)
        {
            FlipGravity();
        }
    }
    
    void StartFlipWarning()
    {
        warningActive = true;
        OnFlipWarning?.Invoke();
        
        // Start visual warning effects
        if (gravityIndicator != null)
        {
            StartCoroutine(PulseGravityIndicator());
        }
    }
    
    void FlipGravity()
    {
        // End warning state
        if (warningActive)
        {
            warningActive = false;
            OnFlipWarningEnd?.Invoke();
        }
        
        // Flip gravity
        currentGravityDown = !currentGravityDown;
        ApplyGravity();
        UpdateGravityIndicator();
        
        // Notify systems
        OnGravityFlipped?.Invoke(currentGravityDown);
        
        // Schedule next flip
        ScheduleNextFlip();
    }
    
    void ApplyGravity()
    {
        float gravityValue = currentGravityDown ? config.gravityDown : config.gravityUp;
        Physics2D.gravity = new Vector2(0, gravityValue);
    }
    
    void UpdateGravityIndicator()
    {
        if (gravityIndicator != null)
        {
            // Point arrow in gravity direction
            float rotation = currentGravityDown ? 180f : 0f;
            gravityIndicator.rotation = Quaternion.Euler(0, 0, rotation);
        }
    }
    
    void ScheduleNextFlip()
    {
        // Calculate time-based difficulty ramp
        float gameProgress = Time.time / config.durationSeconds;
        float rampedMin = config.flipIntervalMin * (1f - config.flipIntervalRampFactor * gameProgress);
        float rampedMax = config.flipIntervalMax * (1f - config.flipIntervalRampFactor * gameProgress);
        
        // Ensure minimum intervals
        rampedMin = Mathf.Max(rampedMin, 1.5f);
        rampedMax = Mathf.Max(rampedMax, 2.5f);
        
        float interval = Random.Range(rampedMin, rampedMax);
        nextFlipTime = Time.time + interval;
    }
    
    IEnumerator PulseGravityIndicator()
    {
        if (gravityIndicator == null) yield break;
        
        Vector3 originalScale = gravityIndicator.localScale;
        float pulseTime = 0f;
        
        while (warningActive && pulseTime < config.flipWarningTime)
        {
            // Pulse between 1.0 and 1.3 scale
            float pulse = 1f + 0.3f * Mathf.Sin(pulseTime * 10f);
            gravityIndicator.localScale = originalScale * pulse;
            
            pulseTime += Time.deltaTime;
            yield return null;
        }
        
        // Reset scale
        if (gravityIndicator != null)
        {
            gravityIndicator.localScale = originalScale;
        }
    }
    
    // Public methods for external control
    public void PauseFlips()
    {
        enabled = false;
    }
    
    public void ResumeFlips()
    {
        enabled = true;
    }
    
    public void ForceFlip()
    {
        FlipGravity();
    }
    
    public Vector2 GetSpawnDirection()
    {
        // Returns direction items should move when spawned
        return currentGravityDown ? Vector2.down : Vector2.up;
    }
    
    public Vector2 GetKnockbackDirection()
    {
        // Returns direction for player knockback (opposite to gravity)
        return currentGravityDown ? Vector2.up : Vector2.down;
    }
}