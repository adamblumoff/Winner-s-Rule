using UnityEngine;

[RequireComponent(typeof(PlayerController2D))]
public class AIPlayerController2D : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveDirection = 1f;
    public float jumpChance = 0.02f;
    public float directionChangeChance = 0.005f;
    public float obstacleAvoidanceDistance = 2f;
    public LayerMask obstacleLayer = 1;

    private PlayerController2D controller;
    private float currentDirection = 1f;
    private float decisionTimer = 0f;
    private const float decisionInterval = 0.1f;

    void Awake()
    {
        controller = GetComponent<PlayerController2D>();
    }

    void Start()
    {
        if (controller != null)
        {
            controller.SetInputEnabled(false);
        }
        
        currentDirection = moveDirection;
    }

    void Update()
    {
        if (controller == null) return;

        decisionTimer += Time.deltaTime;
        
        if (decisionTimer >= decisionInterval)
        {
            MakeDecisions();
            decisionTimer = 0f;
        }

        controller.MoveHorizontal(currentDirection);
    }

    void MakeDecisions()
    {
        CheckForObstacles();
        
        if (Random.value < directionChangeChance)
        {
            currentDirection = Random.value > 0.5f ? 1f : -1f;
        }
        
        if (Random.value < jumpChance)
        {
            controller.TryJump();
        }
    }

    void CheckForObstacles()
    {
        Vector2 rayOrigin = transform.position;
        Vector2 rayDirection = Vector2.right * currentDirection;
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, obstacleAvoidanceDistance, obstacleLayer);
        
        if (hit.collider != null && !hit.collider.CompareTag("Player"))
        {
            currentDirection *= -1f;
            
            if (Random.value < 0.5f)
            {
                controller.TryJump();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 rayStart = transform.position;
        Vector3 rayEnd = rayStart + Vector3.right * currentDirection * obstacleAvoidanceDistance;
        Gizmos.DrawLine(rayStart, rayEnd);
    }
}