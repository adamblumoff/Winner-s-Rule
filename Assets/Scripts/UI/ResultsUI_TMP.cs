using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

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
        StartCoroutine(WaitForGameStateManager());
    }

    System.Collections.IEnumerator WaitForGameStateManager()
    {
        // Wait a frame or two for GameStateManager to initialize
        yield return null;
        yield return null;

        var gsm = GameStateManager.I;

        if (gsm == null)
        {
            // Try to find it in the scene
            gsm = FindFirstObjectByType<GameStateManager>();
        }

        if (gsm == null)
        {
            // Fallback if Results is run directly
            Debug.LogWarning("GameStateManager not found! Using fallback values.");
            title.text = "Round Results";
            subtitle.text = "Round 1/1";
            winnerText.text = "Winner: Player 1";
            nextButtonLabel.text = "Back to Lobby";
            nextButton.onClick.AddListener(() =>
                UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby"));
            yield break;
        }

        SetupUI(gsm);
    }

    void SetupUI(GameStateManager gsm)
    {

        bool final = gsm.currentRound >= gsm.totalRounds;

        Debug.Log("setup ui is getting called");
        title.text = final ? "Final Results" : "Round Results";
        subtitle.text = $"Round {gsm.currentRound}/{gsm.totalRounds}";
        winnerText.text = $"Winner: Player {gsm.currentRoundWinner + 1}";

        nextButtonLabel.text = final ? "Back to Lobby" : "Go to Draft";
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            if (final) 
            {
                gsm.LoadLobby();
            }
            else 
            {
                gsm.LoadDraft();
            }
        });

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => gsm.LoadLobby());
        }
    }
}
