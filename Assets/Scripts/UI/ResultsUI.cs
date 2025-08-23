using UnityEngine;
using UnityEngine.UI;
public class ResultsUI : MonoBehaviour
{
    public Text title; 
    public Text playerText; // Shows current player
    public Text scoreText; // Shows player's score
    public Button nextBtn;
    void Start()
    {
        var gsm = GameStateManager.I;
        bool final = gsm.currentRound >= gsm.totalRounds;
        bool roundComplete = gsm.currentPlayerIndex == 0; // All players have played this round
        
        Debug.Log($"ResultsUI Start: Winner is Player {gsm.currentRoundWinner + 1}, roundComplete = {roundComplete}");
        
        // Show previous player's result (the one who just played)
        int previousPlayer = gsm.currentPlayerIndex == 0 ? gsm.totalPlayers - 1 : gsm.currentPlayerIndex - 1;
        
        if (playerText) playerText.text = $"Player {previousPlayer + 1}";
        if (scoreText) scoreText.text = $"Score: {gsm.playerScores[previousPlayer]}";
        
        if (final)
        {
            title.text = "Final Results";
            nextBtn.GetComponentInChildren<Text>().text = "Lobby";
            nextBtn.onClick.AddListener(() => gsm.LoadLobby());
        }
        else if (roundComplete)
        {
            title.text = $"Round {gsm.currentRound} Complete!";
            if (playerText) playerText.text = $"Winner: Player {gsm.currentRoundWinner + 1}";
            if (scoreText) scoreText.text = $"Winning Score: {gsm.playerScores[gsm.currentRoundWinner]}";
            nextBtn.GetComponentInChildren<Text>().text = "Draft";
            nextBtn.onClick.AddListener(() => {
                Debug.Log($"Results scene: Winner Player {gsm.currentRoundWinner + 1} going to draft");
                gsm.LoadDraft();
            });
        }
        else
        {
            title.text = $"Player {gsm.currentPlayerIndex + 1}'s Turn";
            nextBtn.GetComponentInChildren<Text>().text = "Next";
            nextBtn.onClick.AddListener(() => gsm.LoadRace());
        }
    }
}
