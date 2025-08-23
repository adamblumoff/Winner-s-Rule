using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager I;
    public int totalRounds = 6, currentRound = 0, lastWinnerId = 0;
    public List<RuleCard> activeRules = new();
    public int seed;
    
    // Player turn tracking
    public int totalPlayers = 2;
    public int currentPlayerIndex = 0;
    public int[] playerScores = new int[4]; // Track scores for up to 4 players
    public int currentRoundWinner = 0;
    
    // Player-specific cards
    public List<RuleCard>[] playerCards = new List<RuleCard>[4]; // Each player's cards

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


    public void StartMatch(int playerCount = 2) 
    { 
        currentRound = 0; 
        activeRules.Clear(); 
        totalPlayers = playerCount;
        currentPlayerIndex = 0;
        
        // Initialize player cards arrays
        for (int i = 0; i < 4; i++)
        {
            if (playerCards[i] == null)
                playerCards[i] = new List<RuleCard>();
            else
                playerCards[i].Clear();
        }
        
        LoadRace(); 
    }
    public void OnRoundEnd(int playerScore)
    {
        // Store the current player's score
        playerScores[currentPlayerIndex] = playerScore;
        currentPlayerIndex++;
        
        // If all players have played, determine round winner and go to Results scene
        if (currentPlayerIndex >= totalPlayers)
        {
            // Find the winner (highest score)
            currentRoundWinner = 0;
            Debug.Log($"Determining round winner from {totalPlayers} players:");
            for (int i = 0; i < totalPlayers; i++)
            {
                Debug.Log($"Player {i + 1}: Score = {playerScores[i]}");
                if (playerScores[i] > playerScores[currentRoundWinner])
                {
                    currentRoundWinner = i;
                }
            }
            Debug.Log($"Round winner: Player {currentRoundWinner + 1} with score {playerScores[currentRoundWinner]}");

            DecrementCurrentPlayerCards();
            currentPlayerIndex = 0;
            currentRound++;
            SceneManager.LoadScene("Results");
        }
        else
        {
            // Decrement the current player's cards after they played
            DecrementCurrentPlayerCards();
            
            // More players need to play - restart the game for next player
            LoadRace();
        }
    }
    public void ApplyDraftChoice(RuleCard c) 
    { 
        Debug.Log($"ApplyDraftChoice called with card: {c?.title ?? "null"} for winner Player {currentRoundWinner + 1}");
        if (c) 
        {
            var instantiated = ScriptableObject.Instantiate(c);
            
            // Add card to the winner's collection
            playerCards[currentRoundWinner].Add(instantiated);
            Debug.Log($"Added card '{instantiated.title}' to Player {currentRoundWinner + 1}. They now have {playerCards[currentRoundWinner].Count} cards.");
        }
        else
        {
            Debug.LogError("ApplyDraftChoice received null card!");
        }
        
        // Reset scores for next round
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScores[i] = 0;
        }
        Debug.Log("Player scores reset for new round");
        
        LoadRace(); 
    }
    
    void DecrementCurrentPlayerCards()
    {
        int playerIndex = currentPlayerIndex - 1; // Previous player who just finished
        if (playerIndex < 0) return;
        
        Debug.Log($"Decrementing cards for Player {playerIndex + 1} who just finished playing");
        
        if (playerCards[playerIndex] == null) return;
        
        Debug.Log($"Player {playerIndex + 1} has {playerCards[playerIndex].Count} cards before cleanup");
        
        for (int i = playerCards[playerIndex].Count - 1; i >= 0; i--)
        {
            var card = playerCards[playerIndex][i];
            Debug.Log($"Player {playerIndex + 1}'s card '{card.title}' had {card.remainingRounds} rounds before decrement");
            card.remainingRounds--;
            Debug.Log($"Player {playerIndex + 1}'s card '{card.title}' now has {card.remainingRounds} rounds remaining");
            
            if (card.remainingRounds <= 0)
            {
                Debug.Log($"Removing expired card '{card.title}' from Player {playerIndex + 1}");
                playerCards[playerIndex].RemoveAt(i);
            }
        }
        
        Debug.Log($"Player {playerIndex + 1} has {playerCards[playerIndex].Count} cards after cleanup");
    }

    void DecrementPlayerCardDurations()
    {
        Debug.Log("Decrementing card durations for all players after full round");
        
        for (int playerIndex = 0; playerIndex < totalPlayers; playerIndex++)
        {
            if (playerCards[playerIndex] == null) continue;
            
            Debug.Log($"Player {playerIndex + 1} has {playerCards[playerIndex].Count} cards before cleanup");
            
            for (int i = playerCards[playerIndex].Count - 1; i >= 0; i--)
            {
                var card = playerCards[playerIndex][i];
                Debug.Log($"Player {playerIndex + 1}'s card '{card.title}' had {card.remainingRounds} rounds before decrement");
                card.remainingRounds--;
                Debug.Log($"Player {playerIndex + 1}'s card '{card.title}' now has {card.remainingRounds} rounds remaining");
                
                if (card.remainingRounds <= 0)
                {
                    Debug.Log($"Removing expired card '{card.title}' from Player {playerIndex + 1}");
                    playerCards[playerIndex].RemoveAt(i);
                }
            }
            
            Debug.Log($"Player {playerIndex + 1} has {playerCards[playerIndex].Count} cards after cleanup");
        }
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
