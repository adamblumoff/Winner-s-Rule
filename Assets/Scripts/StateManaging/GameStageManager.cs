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
                Debug.Log($"GameStateManager already exists with game data (round {I.currentRound}/{I.totalRounds}), destroying new duplicate");
                Destroy(gameObject);
                return;
            }
            // If existing instance has no data, replace it with this one
            else
            {
                Debug.Log("Replacing empty GameStateManager with new instance");
                Destroy(I.gameObject);
                I = this;
            }
        }
        else
        {
            // First instance
            I = this;
        }
        
        // Ensure this GameObject is at root level
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        DontDestroyOnLoad(gameObject); 
        
        // Only set seed if it's not already set (in case we're preserving state)
        if (seed == 0)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        
        Debug.Log($"GameStateManager initialized. Instance: {gameObject.name}, Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, Current Round: {currentRound}/{totalRounds}");
    }


    public void StartMatch() 
    { 
        Debug.Log("GameStateManager.StartMatch() called");
        currentRound = 0; 
        activeRules.Clear(); 
        LoadRace(); 
    }
    public void OnRoundEnd(int winnerId)
    {
        lastWinnerId = winnerId; 
        currentRound++;
        Debug.Log($"Round {currentRound}/{totalRounds} completed. Winner: Player {winnerId + 1}");
        SceneManager.LoadScene("Results");
    }
    public void ApplyDraftChoice(RuleCard c) { if (c) activeRules.Add(ScriptableObject.Instantiate(c)); LoadRace(); }
    public void DecrementDurations() { for (int i = activeRules.Count - 1; i >= 0; i--) { activeRules[i].remainingRounds--; if (activeRules[i].remainingRounds <= 0) activeRules.RemoveAt(i); } }
    public void LoadRace() 
    { 
        Debug.Log("GameStateManager.LoadRace() called - Loading GameRace_2D scene");
        DecrementDurations(); 
        SceneManager.LoadScene("GameRace_2D"); 
    }
    public void LoadDraft() { SceneManager.LoadScene("Draft"); }
    public void LoadLobby() { SceneManager.LoadScene("Lobby"); }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureGameStateManager()
    {
        // Wait a frame to let scene objects initialize first
        if (I == null)
        {
            // Check if there's one in the scene first
            GameStateManager existing = FindFirstObjectByType<GameStateManager>();
            if (existing == null)
            {
                GameObject gsmObj = new GameObject("GameStateManager (Auto-Created)");
                gsmObj.AddComponent<GameStateManager>();
                Debug.Log("GameStateManager created automatically");
            }
            else
            {
                Debug.Log("Found existing GameStateManager in scene");
            }
        }
        else
        {
            Debug.Log("GameStateManager already exists");
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
