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
        //if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject); 
        seed = Random.Range(int.MinValue, int.MaxValue);
    }


    public void StartMatch() { currentRound = 0; activeRules.Clear(); LoadRace(); }
    public void OnRoundEnd(int winnerId)
    {
        lastWinnerId = winnerId; currentRound++;
        SceneManager.LoadScene("Results");
    }
    public void ApplyDraftChoice(RuleCard c) { if (c) activeRules.Add(ScriptableObject.Instantiate(c)); LoadRace(); }
    public void DecrementDurations() { for (int i = activeRules.Count - 1; i >= 0; i--) { activeRules[i].remainingRounds--; if (activeRules[i].remainingRounds <= 0) activeRules.RemoveAt(i); } }
    public void LoadRace() { DecrementDurations(); SceneManager.LoadScene("GameRace_2D"); }
    public void LoadDraft() { SceneManager.LoadScene("Draft"); }
    public void LoadLobby() { SceneManager.LoadScene("Lobby"); }
}
