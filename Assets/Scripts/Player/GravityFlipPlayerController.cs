using UnityEngine;
using System.Collections;
using UnityEditor.U2D;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityFlipPlayerController : MonoBehaviour
{
    [Header("References")]
    public MinigameConfig config;
    
    [Header("Camera Bounds")]
    public float leftBound = -8f;
    public float rightBound = 8f;
    
    private Rigidbody2D rb;
    private bool dashActive = false;
    private float dashCooldownTimer = 0f;

    private SpriteRenderer sprite;
    
    // Gravity cycle tracking for Quick Recovery card
    private bool hasDashedThisCycle = false;
    private bool isQuickRecoveryActive = false;
    
    // Sprite rotation for gravity feedback
    private Coroutine spriteRotationCoroutine;
    
    // Input
    private float horizontalInput;
    private bool dashInput;
    
    // Events
    public System.Action OnDashUsed;
    public System.Action OnHit;
    
    public bool CanDash => (isQuickRecoveryActive ? CanDashThisCycle : dashCooldownTimer <= 0f) && !dashActive;
    public float DashCooldownProgress => config != null ? 1f - (dashCooldownTimer / config.dashCooldown) : 0f;
    public bool CanDashThisCycle => !isQuickRecoveryActive || !hasDashedThisCycle;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        // Find config if not assigned
        if (config == null)
        {
            GravityFlipGameController gameController = FindFirstObjectByType<GravityFlipGameController>();
            if (gameController != null)
            {
                config = gameController.config;
            }
        }
        
        // Set up camera bounds automatically if not set
        if (leftBound == rightBound)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float cameraWidth = cam.orthographicSize * cam.aspect;
                leftBound = -cameraWidth + 0.5f; // add margin
                rightBound = cameraWidth - 0.5f;
            }
        }
    }
    
    void Start()
    {
        // Check if Quick Recovery card is active
        CheckForQuickRecovery();
        
        // Subscribe to gravity flip events (all players need this for sprite flipping)
        GravityFlipController gravityController = FindFirstObjectByType<GravityFlipController>();
        if (gravityController != null)
        {
            gravityController.OnGravityFlipped += OnGravityFlipped;
            
            // Set initial sprite orientation based on current gravity
            if (sprite != null)
            {
                float initialRotation = gravityController.IsGravityDown ? 0f : 180f;
                sprite.transform.rotation = Quaternion.Euler(0, 0, initialRotation);
                Debug.Log($"Initial sprite rotation set to {initialRotation}° (gravity {(gravityController.IsGravityDown ? "down" : "up")})");
            }
        }
    }
    
    void Update()
    {
        // Handle input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        dashInput = Input.GetKeyDown(KeyCode.Space);
        
        // Update dash cooldown
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
        
        // Handle dash input
        if (dashInput && CanDash)
        {
            StartCoroutine(PerformDash());
        }
    }
    
    void FixedUpdate()
    {
        // Apply horizontal movement
        if (!dashActive && config != null)
        {
            rb.linearVelocity = new Vector2(horizontalInput * config.playerSpeed, rb.linearVelocity.y);
        }
        SpriteFlip();
    }

    void SpriteFlip()
    {
        if (horizontalInput > 0)
        {
            sprite.flipX = true;
        }
        else if (horizontalInput < 0)
        {
            sprite.flipX = false;
        }
    }
    IEnumerator PerformDash()
    {
        if (config == null) yield break;
        
        dashActive = true;
        dashCooldownTimer = config.dashCooldown;
        
        // Track dash usage for Quick Recovery card
        if (isQuickRecoveryActive)
        {
            hasDashedThisCycle = true;
        }
        
        OnDashUsed?.Invoke();
        
        // Apply dash velocity (horizontal only)
        float dashDirection = horizontalInput != 0 ? Mathf.Sign(horizontalInput) : 1f;
        rb.linearVelocity = new Vector2(dashDirection * config.dashSpeed, rb.linearVelocity.y);
        
        // Wait for dash duration
        yield return new WaitForSeconds(config.dashTime);
        
        dashActive = false;
        
        // Return to normal horizontal movement
        if (config != null)
        {
            rb.linearVelocity = new Vector2(horizontalInput * config.playerSpeed, rb.linearVelocity.y);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Item item = other.GetComponent<Item>();
        if (item != null)
        {
            item.OnPlayerHit(this);
        }
    }
    
    public void TakeHit(Vector2 knockbackDirection)
    {
        OnHit?.Invoke();
        
        // Apply knockback impulse
        rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
    }
    
    // Public methods for external control
    public void SetInputEnabled(bool enabled)
    {
        if (!enabled)
        {
            horizontalInput = 0f;
            dashInput = false;
        }
    }
    
    public void ResetPosition(Vector3 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
    }
    
    void CheckForQuickRecovery()
    {
        // Check if Quick Recovery card is in active rules
        if (GameStateManager.I != null && GameStateManager.I.activeRules != null)
        {
            foreach (RuleCard rule in GameStateManager.I.activeRules)
            {
                if (rule.id == "quick_recovery" && rule.IsCompatibleWith(MinigameType.GravityFlipDodge))
                {
                    isQuickRecoveryActive = true;
                    Debug.Log("Quick Recovery card detected - dash limited to once per gravity cycle");
                    break;
                }
            }
        }
    }
    
    void OnGravityFlipped(bool isGravityDown)
    {
        // Smoothly rotate sprite based on gravity direction
        StartSmoothSpriteRotation(isGravityDown);
        
        // Reset dash availability when gravity flips (Quick Recovery only)
        if (isQuickRecoveryActive)
        {
            hasDashedThisCycle = false;
            Debug.Log("Gravity flipped - dash availability reset for Quick Recovery");
        }
    }
    
    void StartSmoothSpriteRotation(bool isGravityDown)
    {
        // Stop any existing rotation
        if (spriteRotationCoroutine != null)
        {
            StopCoroutine(spriteRotationCoroutine);
        }
        
        // Start smooth rotation to new orientation
        spriteRotationCoroutine = StartCoroutine(SmoothRotateSprite(isGravityDown));
    }
    
    IEnumerator SmoothRotateSprite(bool isGravityDown)
    {
        if (sprite == null) yield break;
        
        // Target rotation: (0°,0°,0°) for gravity down, (0°,180°,180°) for gravity up
        Vector3 targetRotation = isGravityDown ? Vector3.zero : new Vector3(0f, 180f, 180f);
        Vector3 currentRotation = sprite.transform.eulerAngles;
        
        // Handle angle wrapping for each axis
        for (int i = 0; i < 3; i++)
        {
            if (currentRotation[i] > 180f) currentRotation[i] -= 360f;
            if (targetRotation[i] - currentRotation[i] > 180f) currentRotation[i] += 360f;
            if (currentRotation[i] - targetRotation[i] > 180f) targetRotation[i] += 360f;
        }
        
        float rotationSpeed = 450f; // degrees per second (0.8 seconds for full 360° flip)
        float maxRotationDistance = Mathf.Max(
            Mathf.Abs(targetRotation.y - currentRotation.y),
            Mathf.Abs(targetRotation.z - currentRotation.z)
        );
        float rotationTime = maxRotationDistance / rotationSpeed;
        
        float elapsed = 0f;
        
        while (elapsed < rotationTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationTime;
            
            // Smooth easing
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic
            
            Vector3 rotation = Vector3.Lerp(currentRotation, targetRotation, easedT);
            sprite.transform.rotation = Quaternion.Euler(rotation);
            
            yield return null;
        }
        
        // Ensure exact final rotation
        sprite.transform.rotation = Quaternion.Euler(targetRotation);
        spriteRotationCoroutine = null;
        
        Debug.Log($"Sprite rotated to {targetRotation}° (gravity {(isGravityDown ? "down" : "up")})");
    }
    
    void OnDestroy()
    {
        // Stop rotation coroutine
        if (spriteRotationCoroutine != null)
        {
            StopCoroutine(spriteRotationCoroutine);
        }
        
        // Unsubscribe from events
        GravityFlipController gravityController = FindFirstObjectByType<GravityFlipController>();
        if (gravityController != null)
        {
            gravityController.OnGravityFlipped -= OnGravityFlipped;
        }
    }
}