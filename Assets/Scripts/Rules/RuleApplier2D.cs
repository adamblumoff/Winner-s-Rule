using UnityEngine;
public static class RuleApplier2D
{
    public static float platformSpeedMul = 1f, frictionAdd = 0f, gravityMul = 1f;
    public static void ResetGlobals() { platformSpeedMul = 1f; frictionAdd = 0f; gravityMul = 1f; }
    public static void ApplyAll()
    {
        ResetGlobals();
        foreach (var rc in GameStateManager.I.activeRules) { ApplySet(rc.effects); ApplySet(rc.drawbacks); }
        Physics2D.gravity = new Vector2(0, -9.81f * gravityMul);
    }
    static void ApplySet(RuleEffect[] set)
    {
        if (set == null) return;
        foreach (var e in set) { if (e.targetIsGlobal) ApplyGlobal(e); else ApplyPlayers(e); }
    }
    static void ApplyGlobal(RuleEffect e)
    {
        switch (e.stat)
        {
            // Note: Global stat effects for 2D platformer games not currently implemented
            // Original stats (PlatformSpeed, Friction, Gravity) removed from cleaned enum
            default:
                Debug.LogWarning($"Global stat {e.stat} not supported in current implementation");
                break;
        }
    }
    static void ApplyPlayers(RuleEffect e)
    {
        foreach (var ps in Object.FindObjectsOfType<PlayerStats2D>())
        {
            switch (e.stat)
            {
                case Stat.MoveSpeed: ps.moveSpeed = Mod(ps.moveSpeed, e); break;
                // Note: Other 2D platformer stats (Jump, Shield, StaminaDrain, KnockbackTaken, RespawnDelay) 
                // removed from cleaned enum as they're not currently implemented
                default:
                    Debug.LogWarning($"Player stat {e.stat} not supported in current 2D platformer implementation");
                    break;
            }
        }
    }
    static float Mod(float v, RuleEffect e) { if (e.multiply != 0f) v *= e.multiply; v += e.addFlat; v *= (1f + e.addPct); return v; }
}
