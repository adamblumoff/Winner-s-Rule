using UnityEngine;
public class PingPong2D : MonoBehaviour
{
    public Vector2 axis = Vector2.right; public float distance = 3f, speed = 1f; Vector3 start;
    void Start() { start = transform.position; }
    void Update()
    {
        float s = speed * RuleApplier2D.platformSpeedMul;
        transform.position = start + (Vector3)(axis.normalized * Mathf.Sin(Time.time * s) * distance);
    }
}
