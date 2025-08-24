using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSpriteDatabase", menuName = "Winner's Rule/Character Sprite Database")]
public class CharacterSpriteDatabase : ScriptableObject
{
    [System.Serializable]
    public class CharacterData
    {
        [Header("Character Info")]
        public string characterName = "Default Character";
        public Sprite previewSprite; // Used in lobby UI
        
        [Header("Animation")]
        public RuntimeAnimatorController animatorController;
        
        [Header("Debug Info")]
        [TextArea(2, 4)]
        public string description = "Character description...";
    }
    
    [Header("Available Characters")]
    public CharacterData[] characters = new CharacterData[0];
    
    [Header("Default Selection")]
    public int defaultCharacterIndex = 0;
    
    // Helper methods
    public CharacterData GetCharacter(int index)
    {
        if (index >= 0 && index < characters.Length)
            return characters[index];
        return null;
    }
    
    public CharacterData GetDefaultCharacter()
    {
        return GetCharacter(defaultCharacterIndex);
    }
    
    public int GetCharacterCount()
    {
        return characters.Length;
    }
    
    public string[] GetCharacterNames()
    {
        string[] names = new string[characters.Length];
        for (int i = 0; i < characters.Length; i++)
        {
            names[i] = characters[i]?.characterName ?? $"Character {i + 1}";
        }
        return names;
    }
    
    public int FindCharacterIndex(string characterName)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i]?.characterName == characterName)
                return i;
        }
        return defaultCharacterIndex;
    }
    
    void OnValidate()
    {
        // Ensure default index is valid
        if (defaultCharacterIndex < 0 || defaultCharacterIndex >= characters.Length)
        {
            defaultCharacterIndex = 0;
        }
    }
}