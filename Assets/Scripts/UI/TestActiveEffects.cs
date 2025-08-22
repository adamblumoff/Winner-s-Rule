using UnityEngine;

// Simple test script to demonstrate active effects UI
public class TestActiveEffects : MonoBehaviour
{
    [Header("Test Settings")]
    public RuleCard testCard1;
    public RuleCard testCard2;
    public RuleCard testCard3;
    
    [Header("UI References")]
    public GravityFlipUI gravityUI;
    public ActiveEffectsUI activeEffectsUI;
    
    void Start()
    {
        // Find UI components if not assigned
        if (gravityUI == null)
            gravityUI = FindFirstObjectByType<GravityFlipUI>();
            
        if (activeEffectsUI == null)
            activeEffectsUI = FindFirstObjectByType<ActiveEffectsUI>();
    }
    
    void Update()
    {
        // Test keys for adding/removing effects
        if (Input.GetKeyDown(KeyCode.Alpha1) && testCard1 != null)
        {
            TestAddEffect(testCard1);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2) && testCard2 != null)
        {
            TestAddEffect(testCard2);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3) && testCard3 != null)
        {
            TestAddEffect(testCard3);
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            TestClearEffects();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            TestRefreshFromGameState();
        }
    }
    
    void TestAddEffect(RuleCard card)
    {
        Debug.Log($"Testing add effect: {card.title}");
        
        // Add to GameStateManager (simulating card selection)
        if (GameStateManager.I != null)
        {
            if (!GameStateManager.I.activeRules.Contains(card))
            {
                GameStateManager.I.activeRules.Add(card);
            }
        }
        
        // Update UI
        if (gravityUI != null)
        {
            gravityUI.AddActiveEffect(card);
        }
        
        if (activeEffectsUI != null)
        {
            activeEffectsUI.OnRuleApplied(card);
        }
    }
    
    void TestClearEffects()
    {
        Debug.Log("Testing clear all effects");
        
        // Clear from GameStateManager
        if (GameStateManager.I != null)
        {
            GameStateManager.I.activeRules.Clear();
        }
        
        // Clear UI
        if (gravityUI != null)
        {
            gravityUI.ClearActiveEffects();
        }
        
        if (activeEffectsUI != null)
        {
            activeEffectsUI.ClearAllEffects();
        }
    }
    
    void TestRefreshFromGameState()
    {
        Debug.Log("Testing refresh from GameState");
        
        if (activeEffectsUI != null)
        {
            activeEffectsUI.UpdateFromGameState();
        }
    }
    
    void OnGUI()
    {
        // Simple on-screen instructions
        GUI.Label(new Rect(10, 10, 300, 100), 
            "Active Effects UI Test:\n" +
            "1, 2, 3 - Add test cards\n" +
            "C - Clear all effects\n" +
            "R - Refresh from GameState"
        );
    }
}