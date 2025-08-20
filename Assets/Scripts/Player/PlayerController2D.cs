using UnityEngine;

[RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(PlayerStats2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Ground Detection")]
    public Transform groundCheck;
    public LayerMask groundMask = 1;
    public float groundRadius = 0.15f;

    [Header("Input Settings")]
    public bool usePlayerInput = true;
    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Jump";

    private Rigidbody2D rb;
    private PlayerStats2D stats;
    private bool grounded;
    private float coyoteTimer = 0f;
    private const float coyoteTime = 0.1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats2D>();
        
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }

    void Start()
    {
        if (rb != null)
        {
            rb.gravityScale = stats.gravity;
        }
    }

    void Update()
    {
        CheckGrounded();
        
        if (usePlayerInput)
        {
            HandleInput();
        }
        
        ApplyGravityModifier();
    }

    void CheckGrounded()
    {
        bool wasGrounded = grounded;
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);
        
        if (grounded)
        {
            coyoteTimer = coyoteTime;
        }
        else if (wasGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    void HandleInput()
    {
        float horizontalInput = Input.GetAxisRaw(horizontalAxis);
        
        rb.linearVelocity = new Vector2(horizontalInput * stats.moveSpeed, rb.linearVelocity.y);
        
        if (Input.GetButtonDown(jumpButton) && CanJump())
        {
            Jump();
        }
    }

    bool CanJump()
    {
        return coyoteTimer > 0f;
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);
        coyoteTimer = 0f;
    }

    void ApplyGravityModifier()
    {
        if (rb != null && stats != null)
        {
            rb.gravityScale = stats.gravity;
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        usePlayerInput = enabled;
    }

    public void MoveHorizontal(float input)
    {
        if (rb != null && stats != null)
        {
            rb.linearVelocity = new Vector2(input * stats.moveSpeed, rb.linearVelocity.y);
        }
    }

    public void TryJump()
    {
        if (CanJump())
        {
            Jump();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
