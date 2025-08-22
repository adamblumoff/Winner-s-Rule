using UnityEngine;

public class PlayerStats2D : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseMoveSpeed = 7f;
    public float baseJumpForce = 12f;
    public float baseGravity = 1f;
    public float baseFriction = 1f;
    public int baseShield = 0;
    public float basePlatformSpeed = 1f;
    public float baseStaminaDrain = 1f;
    public float baseKnockbackTaken = 1f;
    public float baseRespawnDelay = 1f;

    [Header("Current Modified Stats")]
    public float moveSpeed = 7f;
    public float jumpForce = 12f;
    public float gravity = 1f;
    public float friction = 1f;
    public int shieldHits = 0;
    public float platformSpeed = 1f;
    public float staminaDrainRate = 1f;
    public float knockbackTakenMul = 1f;
    public float respawnDelay = 1f;

    [Header("Player Info")]
    public int playerId;
    public string playerName = "Player";

    void Awake()
    {
        ResetToBaseStats();
    }

    public void ResetToBaseStats()
    {
        moveSpeed = baseMoveSpeed;
        jumpForce = baseJumpForce;
        gravity = baseGravity;
        friction = baseFriction;
        shieldHits = baseShield;
        platformSpeed = basePlatformSpeed;
        staminaDrainRate = baseStaminaDrain;
        knockbackTakenMul = baseKnockbackTaken;
        respawnDelay = baseRespawnDelay;
    }

    public void ModifyStat(Stat stat, float value, bool isMultiplier = false)
    {
        switch (stat)
        {
            case Stat.MoveSpeed:
                moveSpeed = isMultiplier ? moveSpeed * value : moveSpeed + value;
                break;
            // Note: Other 2D platformer stats (Jump, Gravity, Friction, Shield, PlatformSpeed, 
            // StaminaDrain, KnockbackTaken, RespawnDelay) removed from cleaned enum 
            // as they're not currently implemented
            default:
                Debug.LogWarning($"Stat {stat} not supported in current PlayerStats2D implementation");
                break;
        }
    }

    public float GetStat(Stat stat)
    {
        return stat switch
        {
            Stat.MoveSpeed => moveSpeed,
            // Note: Other 2D platformer stats removed from cleaned enum
            _ => 1f
        };
    }
}
