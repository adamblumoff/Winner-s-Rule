using UnityEngine;

public enum RuleScope { Global, Targeted, Personal }
public enum Stat { 
    // Currently implemented stats for Gravity Flip Dodge
    MoveSpeed, DashSpeed, DashCooldown, ItemSpawnRate, GameDuration, HitPoints,
    // Special gameplay modifiers
    DashLimitedPerCycle
}
public enum MinigameType { Any, GravityFlipDodge, ColorRush, ReactionRacer, NumberHunt }

[CreateAssetMenu(menuName = "WR/RuleCard")]
public class RuleCard : ScriptableObject
{
    [Header("Basic Info")]
    public string id, title; 
    [TextArea] public string description;
    
    [Header("Game Targeting")]
    public MinigameType[] compatibleGames = { MinigameType.Any };
    [TextArea] public string gameSpecificDescription; // How it affects specific games
    
    [Header("Rule Properties")]
    public RuleScope scope = RuleScope.Personal; 
    public int remainingRounds = 1;
    public RuleEffect[] effects; 
    public RuleEffect[] drawbacks;
    public string[] tags; 
    public int impactScore = 1;
    
    public bool IsCompatibleWith(MinigameType gameType)
    {
        foreach (var compatible in compatibleGames)
        {
            if (compatible == MinigameType.Any || compatible == gameType)
                return true;
        }
        return false;
    }
    
    public string GetDescriptionForGame(MinigameType gameType)
    {
        if (!string.IsNullOrEmpty(gameSpecificDescription) && IsCompatibleWith(gameType))
        {
            return gameSpecificDescription;
        }
        return description;
    }
}
