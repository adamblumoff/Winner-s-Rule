// Assets/Scripts/UI/DraftUI_TMP.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DraftUI : MonoBehaviour
{
    [Header("Data")]
    public RuleDatabase db;

    [Header("Header UI")]
    public TMP_Text header;
    public TMP_Text subheader;

    [Header("Card 0")]
    public TMP_Text title0;
    public TMP_Text desc0;
    public TMP_Text tags0;
    public TMP_Text impact0;
    public Button pick0;

    [Header("Card 1")]
    public TMP_Text title1;
    public TMP_Text desc1;
    public TMP_Text tags1;
    public TMP_Text impact1;
    public Button pick1;

    [Header("Card 2")]
    public TMP_Text title2;
    public TMP_Text desc2;
    public TMP_Text tags2;
    public TMP_Text impact2;
    public Button pick2;

    private RuleCard[] offered;

    void Start()
    {
        var gsm = GameStateManager.I;

        // Fallback if someone runs scene directly
        if (gsm == null || db == null || db.cards == null || db.cards.Length < 3)
        {
            header.text = "Winner's Draft";
            subheader.text = "Round 1/1";
            // Disable buttons to avoid null actions
            if (pick0) pick0.interactable = false;
            if (pick1) pick1.interactable = false;
            if (pick2) pick2.interactable = false;
            return;
        }

        header.text = "Winner's Draft";
        subheader.text = $"Round {gsm.currentRound}/{gsm.totalRounds}";

        // Seeded RNG so runs are deterministic per round
        Random.InitState(gsm.seed + gsm.currentRound);

        offered = db.DrawThree();
        BindCard(0, offered[0], title0, desc0, tags0, impact0, pick0);
        BindCard(1, offered[1], title1, desc1, tags1, impact1, pick1);
        BindCard(2, offered[2], title2, desc2, tags2, impact2, pick2);
    }

    void BindCard(int index, RuleCard rc,
                  TMP_Text title, TMP_Text desc, TMP_Text tags, TMP_Text impact, Button pick)
    {
        if (rc == null) { DisableCard(title, desc, tags, impact, pick); return; }

        if (title) title.text = rc.title;
        if (desc) 
        {
            string gameSpecificDesc = rc.GetDescriptionForGame(MinigameType.GravityFlipDodge);
            desc.text = string.IsNullOrEmpty(gameSpecificDesc) ? "No description" : gameSpecificDesc;
        }

        // Tags: join rc.tags plus scope
        if (tags)
        {
            string scope = rc.scope.ToString(); // Global/Targeted/Personal
            string tagStr = (rc.tags != null && rc.tags.Length > 0) ? string.Join(" • ", rc.tags) : "";
            tags.text = string.IsNullOrEmpty(tagStr) ? scope : $"{scope} • {tagStr}";
        }

        if (impact) impact.text = $"Impact: {Mathf.Clamp(rc.impactScore, 1, 5)}/5";

        if (pick)
        {
            pick.onClick.RemoveAllListeners();
            pick.onClick.AddListener(() => OnPick(index));
        }
    }

    void DisableCard(TMP_Text title, TMP_Text desc, TMP_Text tags, TMP_Text impact, Button pick)
    {
        if (title) title.text = "Unavailable";
        if (desc) desc.text = "No card";
        if (tags) tags.text = "";
        if (impact) impact.text = "";
        if (pick) pick.interactable = false;
    }

    void OnPick(int index)
    {
        if (offered == null || index < 0 || index >= offered.Length) 
        {
            Debug.LogError($"OnPick failed: offered={offered}, index={index}");
            return;
        }
        
        var chosen = offered[index];
        Debug.Log($"Player picked card {index}: {chosen.title} (ID: {chosen.id})");
        Debug.Log($"GameStateManager.I exists: {GameStateManager.I != null}");
        Debug.Log($"ActiveRules count before: {GameStateManager.I?.activeRules?.Count ?? -1}");
        
        GameStateManager.I.ApplyDraftChoice(chosen); // loads Game_Race_2D next
    }
}
