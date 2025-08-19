using UnityEngine;
[CreateAssetMenu(menuName = "WR/RuleDatabase")]
public class RuleDatabase : ScriptableObject
{
    public RuleCard[] cards;
    public RuleCard[] DrawThree()
    {
        RuleCard a = cards[Random.Range(0, cards.Length)];
        RuleCard b = cards[Random.Range(0, cards.Length)]; while (b == a) b = cards[Random.Range(0, cards.Length)];
        RuleCard c = cards[Random.Range(0, cards.Length)]; while (c == a || c == b) c = cards[Random.Range(0, cards.Length)];
        return new[] { a, b, c };
    }
}
