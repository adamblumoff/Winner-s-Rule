using UnityEngine;
public enum RuleScope { Global, Targeted, Personal }
public enum Stat { MoveSpeed, Jump, Shield, PlatformSpeed, StaminaDrain, KnockbackTaken, RespawnDelay, Friction, Gravity }
[CreateAssetMenu(menuName = "WR/RuleCard")]
public class RuleCard : ScriptableObject
{
    public string id, title; [TextArea] public string description;
    public RuleScope scope = RuleScope.Personal; public int remainingRounds = 1;
    public RuleEffect[] effects; public RuleEffect[] drawbacks;
    public string[] tags; public int impactScore = 1;
}
