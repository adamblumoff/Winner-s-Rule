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
            case Stat.PlatformSpeed: platformSpeedMul *= (e.multiply == 0 ? 1 : e.multiply); platformSpeedMul += e.addPct; break;
            case Stat.Friction: frictionAdd += e.addFlat; break;
            case Stat.Gravity: gravityMul *= (e.multiply == 0 ? 1 : e.multiply); gravityMul *= (1f + e.addPct); break;
        }
    }
    static void ApplyPlayers(RuleEffect e)
    {
        foreach (var ps in Object.FindObjectsOfType<PlayerStats2D>())
        {
            switch (e.stat)
            {
                case Stat.MoveSpeed: ps.moveSpeed = Mod(ps.moveSpeed, e); break;
                case Stat.Jump: ps.jumpForce = Mod(ps.jumpForce, e); break;
                case Stat.Shield: ps.shieldHits += Mathf.RoundToInt(e.addFlat); break;
                case Stat.StaminaDrain: ps.staminaDrainRate = Mod(ps.staminaDrainRate, e); break;
                case Stat.KnockbackTaken: ps.knockbackTakenMul = Mod(ps.knockbackTakenMul, e); break;
                case Stat.RespawnDelay: ps.respawnDelay = Mod(ps.respawnDelay, e); break;
            }
        }
    }
    static float Mod(float v, RuleEffect e) { if (e.multiply != 0f) v *= e.multiply; v += e.addFlat; v *= (1f + e.addPct); return v; }
}
