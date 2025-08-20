using UnityEngine;

public class FinishVolume2D : MonoBehaviour
{
    private bool raceFinished = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player") || raceFinished) return;
        
        PlayerStats2D playerStats = col.GetComponent<PlayerStats2D>();
        if (playerStats == null) 
        {
            Debug.LogWarning("Player object hit finish line but has no PlayerStats2D component!");
            return;
        }

        if (GameStateManager.I == null)
        {
            Debug.LogError("GameStateManager not found when player finished!");
            return;
        }

        raceFinished = true;
        int winnerId = playerStats.playerId;
        
        Debug.Log($"Race finished! Winner: Player {winnerId + 1} (ID: {winnerId})");
        GameStateManager.I.OnRoundEnd(winnerId);
    }
}
