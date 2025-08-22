using UnityEngine;

[System.Serializable]
public class SpawnItem
{
    public GameObject prefab;
    public int weight = 1;
    public float speedMin = 3f;
    public float speedMax = 6f;
    public float sizeMin = 1f;
    public float sizeMax = 1f;
}

[CreateAssetMenu(menuName = "WR/SpawnTable")]
public class SpawnTable : ScriptableObject
{
    [Header("Hazard Items")]
    public SpawnItem[] hazards;
    
    [Header("Good Items")]
    public SpawnItem[] goods;
    
    public SpawnItem GetRandomHazard()
    {
        return GetRandomFromArray(hazards);
    }
    
    public SpawnItem GetRandomGood()
    {
        return GetRandomFromArray(goods);
    }
    
    private SpawnItem GetRandomFromArray(SpawnItem[] items)
    {
        if (items == null || items.Length == 0) return null;
        
        int totalWeight = 0;
        foreach (var item in items)
        {
            totalWeight += item.weight;
        }
        
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        foreach (var item in items)
        {
            currentWeight += item.weight;
            if (randomValue < currentWeight)
            {
                return item;
            }
        }
        
        return items[0]; // fallback
    }
}