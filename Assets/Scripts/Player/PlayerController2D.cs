using UnityEngine;

[RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(PlayerStats2D))]
public class PlayerController2D : MonoBehaviour
{
    Rigidbody2D rb; PlayerStats2D stats;
    public Transform groundCheck; public LayerMask groundMask; public float groundRadius = 0.15f;
    bool grounded;
    void Awake() { rb = GetComponent<Rigidbody2D>(); stats = GetComponent<PlayerStats2D>(); }
    void Update()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);
        float h = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(h * stats.moveSpeed, rb.linearVelocity.y);
        if (grounded && Input.GetButtonDown("Jump")) rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);
    }
}
