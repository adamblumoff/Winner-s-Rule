using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultsUI_TMP : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text title;
    public TMP_Text subtitle;
    public TMP_Text winnerText;
    public Button nextButton;
    public TMP_Text nextButtonLabel;
    public Button mainMenuButton; // optional

    void Start()
    {
        var gsm = GameStateManager.I;

        if (gsm == null)
        {
            // Fallback if Results is run directly
            title.text = "Round Results";
            subtitle.text = "Round 1/1";
            winnerText.text = "Winner: Player 1";
            nextButtonLabel.text = "Back to Lobby";
            nextButton.onClick.AddListener(() =>
                UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby"));
            return;
        }

        bool final = gsm.currentRound >= gsm.totalRounds;

        title.text = final ? "Final Results" : "Round Results";
        subtitle.text = $"Round {gsm.currentRound}/{gsm.totalRounds}";
        winnerText.text = $"Winner: Player {gsm.lastWinnerId + 1}";

        nextButtonLabel.text = final ? "Back to Lobby" : "Go to Draft";
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            if (final) gsm.LoadLobby();
            else gsm.LoadDraft();
        });

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => gsm.LoadLobby());
        }
    }
}
