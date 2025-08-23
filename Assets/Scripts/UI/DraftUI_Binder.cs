// Assets/Scripts/UI/DraftUI_Binder.cs
using UnityEngine;

public class DraftUI_Binder : MonoBehaviour
{
    public RuleDatabase db;
    public CardView_TMP card0, card1, card2;
    public TMPro.TMP_Text header, subheader;

    RuleCard[] offered;

    void Start()
    {
        var gsm = GameStateManager.I;
        if (gsm == null || db == null || db.cards == null || db.cards.Length < 3)
        {
            header.text = "Winner's Draft";
            subheader.text = "Round 1/1";
            card0.Bind(null, null); card1.Bind(null, null); card2.Bind(null, null);
            return;
        }

        header.text = $"Player {gsm.currentRoundWinner + 1}'s Draft";
        subheader.text = $"Round {gsm.currentRound}/{gsm.totalRounds}";

        Random.InitState(gsm.seed + gsm.currentRound);
        offered = db.DrawThree();

        card0.Bind(offered[0], OnPick);
        card1.Bind(offered[1], OnPick);
        card2.Bind(offered[2], OnPick);
    }

    void OnPick(RuleCard chosen)
    {
        GameStateManager.I.ApplyDraftChoice(chosen); // loads next scene
    }
}
