using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardView_TMP : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text descText;
    public TMP_Text tagsText;
    public TMP_Text impactText;
    public Button pickButton;
    public TMP_Text pickLabel;
    public Image icon; // optional

    RuleCard card;
    Action<RuleCard> onPick;

    public void Bind(RuleCard rc, Action<RuleCard> onPickCb)
    {
        card = rc;
        onPick = onPickCb;

        if (rc == null)
        {
            SetUnavailable();
            return;
        }

        if (titleText) titleText.text = rc.title;
        if (descText) descText.text = string.IsNullOrEmpty(rc.description) ? "No description" : rc.description;

        if (tagsText)
        {
            string scope = rc.scope.ToString();
            string tagStr = (rc.tags != null && rc.tags.Length > 0) ? string.Join(" • ", rc.tags) : "";
            tagsText.text = string.IsNullOrEmpty(tagStr) ? scope : $"{scope} • {tagStr}";
        }

        if (impactText) impactText.text = $"Impact: {Mathf.Clamp(rc.impactScore, 1, 5)}/5";

        if (pickLabel) pickLabel.text = "Pick";
        if (pickButton)
        {
            pickButton.interactable = true;
            pickButton.onClick.RemoveAllListeners();
            pickButton.onClick.AddListener(() => { onPick?.Invoke(card); });
        }
    }

    void SetUnavailable()
    {
        if (titleText) titleText.text = "Unavailable";
        if (descText) descText.text = "No card";
        if (tagsText) tagsText.text = "";
        if (impactText) impactText.text = "";
        if (pickLabel) pickLabel.text = "N/A";
        if (pickButton) pickButton.interactable = false;
        if (icon) icon.enabled = false;
    }
}
