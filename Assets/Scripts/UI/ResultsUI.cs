using UnityEngine;
using UnityEngine.UI;
public class ResultsUI : MonoBehaviour
{
    public Text title; public Button nextBtn;
    void Start()
    {
        bool final = GameStateManager.I.currentRound >= GameStateManager.I.totalRounds;
        title.text = final ? "Final Results" : "Round Results";
        nextBtn.onClick.AddListener(() => { if (final) GameStateManager.I.LoadLobby(); else GameStateManager.I.LoadDraft(); });
    }
}
