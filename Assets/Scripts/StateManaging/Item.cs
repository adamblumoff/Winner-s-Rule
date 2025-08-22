using UnityEngine;

public enum ItemType
{
    Good,
    Hazard
}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Item : MonoBehaviour
{
    [Header("Item Settings")]
    public ItemType itemType = ItemType.Good;
    public int scoreValue = 3;
    public float offScreenPadding = 2f;
    
    private Rigidbody2D rb;
    private Camera mainCamera;
    private SpawnerController spawnerController;
    private bool hasTriggered = false;
    
    // Properties set by spawner
    public float Speed { get; set; } = 5f;
    public Vector2 Direction { get; set; } = Vector2.down;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        // Ensure trigger collider
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void OnEnable()
    {
        hasTriggered = false;
        
        // Set velocity when enabled
        if (rb != null)
        {
            rb.linearVelocity = Direction.normalized * Speed;
        }
    }
    
    void Update()
    {
        // Check if item is off screen and should be despawned
        if (IsOffScreen())
        {
            ReturnToPool();
        }
    }
    
    bool IsOffScreen()
    {
        if (mainCamera == null) return false;
        
        Vector3 screenPos = mainCamera.WorldToViewportPoint(transform.position);
        
        // Check if completely outside viewport with padding
        float padding = offScreenPadding / 10f; // Convert padding to viewport units
        return screenPos.x < -padding || screenPos.x > 1f + padding ||
               screenPos.y < -padding || screenPos.y > 1f + padding;
    }
    
    public void OnPlayerHit(GravityFlipPlayerController player)
    {
        if (hasTriggered) return;
        hasTriggered = true;
        
        // Find the spawner controller to handle scoring
        if (spawnerController == null)
        {
            spawnerController = FindFirstObjectByType<SpawnerController>();
        }
        
        if (spawnerController != null)
        {
            spawnerController.OnItemCollected(this, player);
        }
        
        // Handle item-specific effects
        switch (itemType)
        {
            case ItemType.Good:
                HandleGoodPickup(player);
                break;
            case ItemType.Hazard:
                HandleHazardHit(player);
                break;
        }
        
        ReturnToPool();
    }
    
    void HandleGoodPickup(GravityFlipPlayerController player)
    {
        // TODO: Add pickup sound effect
        // TODO: Add pickup particle effect
        
        // Small visual feedback - could add a pop animation here
        // For now, just immediate collection
    }
    
    void HandleHazardHit(GravityFlipPlayerController player)
    {
        // Apply knockback in direction opposite to current gravity
        GravityFlipController gravityController = FindFirstObjectByType<GravityFlipController>();
        if (gravityController != null)
        {
            Vector2 knockbackDirection = gravityController.GetKnockbackDirection();
            player.TakeHit(knockbackDirection);
        }
        
        // TODO: Add hit sound effect
        // TODO: Add hit particle effect
    }
    
    void ReturnToPool()
    {
        // Find spawner controller if not cached
        if (spawnerController == null)
        {
            spawnerController = FindFirstObjectByType<SpawnerController>();
        }
        
        if (spawnerController != null)
        {
            spawnerController.ReturnItemToPool(this);
        }
        else
        {
            // Fallback: just deactivate
            gameObject.SetActive(false);
        }
    }
    
    // Method to set up item when spawned
    public void Initialize(ItemType type, Vector2 direction, float speed, float size = 1f)
    {
        itemType = type;
        Direction = direction;
        Speed = speed;
        
        // Apply size scaling
        transform.localScale = Vector3.one * size;
        
        // Set velocity
        if (rb != null)
        {
            rb.linearVelocity = Direction.normalized * Speed;
        }
    }
    
    // Method to reset item for pooling
    public void ResetForPool()
    {
        hasTriggered = false;
        transform.localScale = Vector3.one;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}