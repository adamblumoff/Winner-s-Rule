using UnityEngine;
using System.Collections;

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
    
    // Input
    private float horizontalInput;
    private bool dashInput;
    
    // Events
    public System.Action OnDashUsed;
    public System.Action OnHit;
    
    public bool CanDash => dashCooldownTimer <= 0f && !dashActive;
    public float DashCooldownProgress => config != null ? 1f - (dashCooldownTimer / config.dashCooldown) : 0f;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
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
        
        // Clamp position within bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, leftBound, rightBound);
        transform.position = pos;
    }
    
    IEnumerator PerformDash()
    {
        if (config == null) yield break;
        
        dashActive = true;
        dashCooldownTimer = config.dashCooldown;
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
}