using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager I;
    public int totalRounds = 6, currentRound = 0, lastWinnerId = 0;
    public List<RuleCard> activeRules = new();
    public int seed;

    void Awake()
    {
        // If there's already an instance and it's not this one
        if (I != null && I != this) 
        { 
            // Preserve the existing instance if it has game data, destroy the new one
            if (I.currentRound > 0 || I.totalRounds != 6 || I.activeRules.Count > 0)
            {
                Destroy(gameObject);
                return;
            }
            // If existing instance has no data, replace it with this one
            else
            {
                Destroy(I.gameObject);
                I = this;
            }
        }
        else
        {
            I = this;
        }
        
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        DontDestroyOnLoad(gameObject); 
        
        if (seed == 0)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
    }


    public void StartMatch() 
    { 
        currentRound = 0; 
        activeRules.Clear(); 
        LoadRace(); 
    }
    public void OnRoundEnd(int winnerId)
    {
        lastWinnerId = winnerId; 
        currentRound++;
        SceneManager.LoadScene("Results");
    }
    public void ApplyDraftChoice(RuleCard c) 
    { 
        Debug.Log($"ApplyDraftChoice called with card: {c?.title ?? "null"}");
        if (c) 
        {
            var instantiated = ScriptableObject.Instantiate(c);
            activeRules.Add(instantiated);
            Debug.Log($"Added card to activeRules. New count: {activeRules.Count}");
            Debug.Log($"Card details: {instantiated.title}, ID: {instantiated.id}, Compatible with GravityFlipDodge: {instantiated.IsCompatibleWith(MinigameType.GravityFlipDodge)}");
        }
        else
        {
            Debug.LogError("ApplyDraftChoice received null card!");
        }
        LoadRace(); 
    }
    public void DecrementDurations() 
    { 
        Debug.Log($"DecrementDurations: Starting with {activeRules.Count} rules");
        for (int i = activeRules.Count - 1; i >= 0; i--) 
        { 
            activeRules[i].remainingRounds--; 
            Debug.Log($"Rule '{activeRules[i].title}' now has {activeRules[i].remainingRounds} rounds remaining");
            if (activeRules[i].remainingRounds <= 0) 
            {
                Debug.Log($"Removing expired rule: {activeRules[i].title}");
                activeRules.RemoveAt(i); 
            }
        } 
        Debug.Log($"DecrementDurations: Ending with {activeRules.Count} rules");
    }
    public void LoadRace() 
    { 
        DecrementDurations(); 
        SceneManager.LoadScene("GravityFlipDodge"); 
    }
    public void LoadDraft() { SceneManager.LoadScene("Draft"); }
    public void LoadLobby() { SceneManager.LoadScene("Lobby"); }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureGameStateManager()
    {
        if (I == null)
        {
            GameStateManager existing = FindFirstObjectByType<GameStateManager>();
            if (existing == null)
            {
                GameObject gsmObj = new GameObject("GameStateManager (Auto-Created)");
                gsmObj.AddComponent<GameStateManager>();
            }
        }
    }

    void OnDestroy()
    {
        if (I == this)
        {
            I = null;
        }
    }
}
