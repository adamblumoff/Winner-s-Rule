using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ActiveEffectsUI : MonoBehaviour
{
    [Header("Active Effects Display")]
    public Transform effectsContainer;
    public GameObject effectPrefab;
    public int maxEffectsShown = 5;
    
    [Header("Effect Prefab Components (on prefab)")]
    public TMP_Text effectTitle;
    public TMP_Text effectDescription;
    public Image effectIcon;
    public Image effectBackground;
    
    [Header("Visual Styling")]
    public Color positiveEffectColor = Color.green;
    public Color negativeEffectColor = Color.red;
    public Color neutralEffectColor = Color.white;
    
    private List<GameObject> activeEffectElements = new List<GameObject>();
    private Dictionary<string, GameObject> effectElementMap = new Dictionary<string, GameObject>();
    
    void Start()
    {
        // Clear any existing effects and refresh from GameStateManager
        ClearAllEffects();
        RefreshActiveEffects();
    }
    
    void OnDestroy()
    {
        // Clean up
    }
    
    // These methods can be called manually when rules are applied/removed
    public void OnRuleApplied(RuleCard card)
    {
        AddEffectToUI(card);
    }
    
    public void OnRuleRemoved(RuleCard card)
    {
        RemoveEffectFromUI(card);
    }
    
    void AddEffectToUI(RuleCard card)
    {
        // Don't add if already showing
        if (effectElementMap.ContainsKey(card.id)) return;
        
        // Create effect element
        GameObject effectElement = CreateEffectElement(card);
        if (effectElement == null) return;
        
        // Add to tracking
        activeEffectElements.Add(effectElement);
        effectElementMap[card.id] = effectElement;
        
        // Limit number of effects shown
        while (activeEffectElements.Count > maxEffectsShown)
        {
            RemoveOldestEffect();
        }
        
        // Update layout
        RefreshLayout();
    }
    
    void RemoveEffectFromUI(RuleCard card)
    {
        if (!effectElementMap.ContainsKey(card.id)) return;
        
        GameObject effectElement = effectElementMap[card.id];
        
        // Remove from tracking
        activeEffectElements.Remove(effectElement);
        effectElementMap.Remove(card.id);
        
        // Destroy UI element
        if (effectElement != null)
        {
            Destroy(effectElement);
        }
        
        RefreshLayout();
    }
    
    GameObject CreateEffectElement(RuleCard card)
    {
        if (effectPrefab == null || effectsContainer == null) return null;
        
        GameObject element = Instantiate(effectPrefab, effectsContainer);
        
        // Get components from the prefab
        TMP_Text titleText = element.GetComponentInChildren<TMP_Text>();
        Image backgroundImage = element.GetComponent<Image>();
        
        // Set content
        if (titleText != null)
        {
            titleText.text = card.title;
        }
        
        // Set color based on effect type
        if (backgroundImage != null)
        {
            backgroundImage.color = GetEffectColor(card);
        }
        
        // Add tooltip or additional info component
        AddTooltipToElement(element, card);
        
        return element;
    }
    
    Color GetEffectColor(RuleCard card)
    {
        // Determine if card is positive, negative, or neutral
        bool hasPositiveEffects = card.effects != null && card.effects.Length > 0;
        bool hasNegativeEffects = card.drawbacks != null && card.drawbacks.Length > 0;
        
        if (hasPositiveEffects && !hasNegativeEffects)
            return positiveEffectColor;
        else if (!hasPositiveEffects && hasNegativeEffects)
            return negativeEffectColor;
        else
            return neutralEffectColor;
    }
    
    void AddTooltipToElement(GameObject element, RuleCard card)
    {
        // Add a simple tooltip component that shows full description on hover
        var tooltip = element.AddComponent<SimpleTooltip>();
        tooltip.tooltipText = card.GetDescriptionForGame(MinigameType.GravityFlipDodge);
    }
    
    void RemoveOldestEffect()
    {
        if (activeEffectElements.Count > 0)
        {
            GameObject oldest = activeEffectElements[0];
            activeEffectElements.RemoveAt(0);
            
            // Remove from map
            foreach (var kvp in effectElementMap)
            {
                if (kvp.Value == oldest)
                {
                    effectElementMap.Remove(kvp.Key);
                    break;
                }
            }
            
            if (oldest != null)
            {
                Destroy(oldest);
            }
        }
    }
    
    void RefreshLayout()
    {
        // Force layout rebuild
        if (effectsContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(effectsContainer.GetComponent<RectTransform>());
        }
    }
    
    public void ClearAllEffects()
    {
        foreach (GameObject element in activeEffectElements)
        {
            if (element != null)
            {
                Destroy(element);
            }
        }
        
        activeEffectElements.Clear();
        effectElementMap.Clear();
    }
    
    int GetCurrentPlayerId()
    {
        // For now, return 0 (single player)
        // You can extend this later for multiplayer support
        return 0;
    }
    
    // Public method to manually refresh effects from GameStateManager
    [ContextMenu("Refresh Active Effects")]
    public void RefreshActiveEffects()
    {
        ClearAllEffects();
        
        // Re-add all active effects from GameStateManager
        if (GameStateManager.I != null && GameStateManager.I.activeRules != null)
        {
            foreach (RuleCard rule in GameStateManager.I.activeRules)
            {
                // Only show cards compatible with current minigame
                if (rule.IsCompatibleWith(MinigameType.GravityFlipDodge))
                {
                    AddEffectToUI(rule);
                }
            }
        }
    }
    
    // Call this from your game logic when rules are applied/removed
    public void UpdateFromGameState()
    {
        RefreshActiveEffects();
    }
}