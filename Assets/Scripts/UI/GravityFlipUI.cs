using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GravityFlipUI : MonoBehaviour
{
    [Header("Game UI")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public Slider timerSlider;
    public TMP_Text hitsRemainingText;
    
    [Header("Dash Cooldown")]
    public Image dashCooldownRadial;
    public TMP_Text dashCooldownText;
    
    [Header("Gravity Indicator")]
    public Image gravityArrow;
    public Transform gravityIndicatorParent;
    
    [Header("Countdown")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;
    
    [Header("Feedback")]
    public GameObject hitFeedbackPanel;
    public TMP_Text pickupFeedbackText;
    public GameObject pickupFeedbackPanel;
    
    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button menuButton;
    
    [Header("Results")]
    public GameObject resultsPanel;
    public TMP_Text finalScoreText;
    public TMP_Text timeScoreText;
    public TMP_Text pickupScoreText;
    public TMP_Text accuracyText;
    public TMP_Text timeSurvivedText;
    public TMP_Text hitsTakenText;
    public TMP_Text draftBonusText;
    public Button continueButton;
    
    [Header("Active Effects")]
    public Transform activeEffectsContainer;
    public GameObject activeEffectPrefab;
    public TMP_Text activeEffectsTitle;
    
    [Header("Visual Effects")]
    public Color warningColor = Color.red;
    public Color normalColor = Color.white;
    
    // References
    private GravityFlipPlayerController player;
    private GravityFlipController gravityController;
    private GravityFlipGameController gameController;
    
    // State
    private Coroutine timerWarningCoroutine;
    private Coroutine hitFeedbackCoroutine;
    private Coroutine pickupFeedbackCoroutine;
    
    void Start()
    {
        // Find references
        player = FindFirstObjectByType<GravityFlipPlayerController>();
        gravityController = FindFirstObjectByType<GravityFlipController>();
        gameController = FindFirstObjectByType<GravityFlipGameController>();
        
        // Setup button events
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToMenu);
        
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueToResults);
        
        // Subscribe to gravity flip events
        if (gravityController != null)
        {
            gravityController.OnGravityFlipped += OnGravityFlipped;
            gravityController.OnFlipWarning += OnFlipWarning;
            gravityController.OnFlipWarningEnd += OnFlipWarningEnd;
        }
        
        // Load active effects from GameStateManager
        LoadActiveEffectsFromGameState();
        
        // Initialize panels
        HideAllPanels();
    }
    
    void Update()
    {
        UpdateDashCooldown();
    }
    
    public void Initialize(MinigameConfig config)
    {
        // Initialize UI elements
        UpdateScore(0);
        UpdateTimer(config.durationSeconds, config.durationSeconds);
        UpdateHitsRemaining(config.hitsToFail);
        
        // Setup gravity indicator
        if (gravityArrow != null && gravityController != null)
        {
            UpdateGravityIndicator(gravityController.IsGravityDown);
        }
    }
    
    void HideAllPanels()
    {
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (hitFeedbackPanel != null) hitFeedbackPanel.SetActive(false);
        if (pickupFeedbackPanel != null) pickupFeedbackPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    public void UpdateTimer(float currentTime, float maxTime)
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(currentTime);
            timerText.text = $"Time: {seconds}";
            
            // Change color when low on time
            if (currentTime <= 10f)
            {
                timerText.color = warningColor;
                
                // Start pulsing if not already
                if (timerWarningCoroutine == null)
                {
                    timerWarningCoroutine = StartCoroutine(PulseTimerText());
                }
            }
            else
            {
                timerText.color = normalColor;
                
                // Stop pulsing
                if (timerWarningCoroutine != null)
                {
                    StopCoroutine(timerWarningCoroutine);
                    timerWarningCoroutine = null;
                }
            }
        }
        
        if (timerSlider != null)
        {
            timerSlider.value = currentTime / maxTime;
        }
    }
    
    public void UpdateHitsRemaining(int hits)
    {
        if (hitsRemainingText != null)
        {
            hitsRemainingText.text = $"Lives: {hits}";
            
            // Change color based on remaining hits
            if (hits <= 1)
            {
                hitsRemainingText.color = warningColor;
            }
            else
            {
                hitsRemainingText.color = normalColor;
            }
        }
    }
    
    void UpdateDashCooldown()
    {
        if (player == null) return;
        
        if (dashCooldownRadial != null)
        {
            dashCooldownRadial.fillAmount = player.DashCooldownProgress;
        }
        
        if (dashCooldownText != null)
        {
            if (player.CanDash)
            {
                dashCooldownText.text = "DASH";
                dashCooldownText.color = Color.white;
            }
            else if (!player.CanDashThisCycle)
            {
                dashCooldownText.text = "CYCLE";
                dashCooldownText.color = Color.yellow;
            }
            else
            {
                float cooldownTime = (1f - player.DashCooldownProgress) * 1.5f; // Assuming 1.5s cooldown
                dashCooldownText.text = $"{cooldownTime:F1}s";
                dashCooldownText.color = Color.gray;
            }
        }
    }
    
    void UpdateGravityIndicator(bool isGravityDown)
    {
        if (gravityArrow != null)
        {
            // Rotate arrow to show gravity direction
            float rotation = isGravityDown ? 0f : 180f;
            gravityArrow.transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
    }
    
    void OnGravityFlipped(bool isGravityDown)
    {
        UpdateGravityIndicator(isGravityDown);
        
        // Show brief flip indicator
        StartCoroutine(ShowGravityFlipFeedback());
    }
    
    void OnFlipWarning()
    {
        // Start warning visual on gravity indicator
        if (gravityArrow != null)
        {
            StartCoroutine(PulseGravityIndicator());
        }
    }
    
    void OnFlipWarningEnd()
    {
        // Warning visuals will stop naturally when flip occurs
    }
    
    public void ShowCountdown(int count)
    {
        if (countdownPanel != null && countdownText != null)
        {
            countdownPanel.SetActive(true);
            
            if (count > 0)
            {
                countdownText.text = count.ToString();
            }
            else
            {
                countdownText.text = "GO!";
            }
            
            // Add punch scale animation
            StartCoroutine(PunchScale(countdownText.transform));
        }
    }
    
    public void HideCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
    }
    
    public void ShowHitFeedback()
    {
        if (hitFeedbackCoroutine != null)
        {
            StopCoroutine(hitFeedbackCoroutine);
        }
        hitFeedbackCoroutine = StartCoroutine(HitFeedbackCoroutine());
    }
    
    public void ShowPickupFeedback(int points)
    {
        if (pickupFeedbackText != null)
        {
            pickupFeedbackText.text = $"+{points}";
        }
        
        if (pickupFeedbackCoroutine != null)
        {
            StopCoroutine(pickupFeedbackCoroutine);
        }
        pickupFeedbackCoroutine = StartCoroutine(PickupFeedbackCoroutine());
    }
    
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }
    
    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }
    
    public void ShowResults(GameResultBreakdown results)
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            
            // Hide active effects when results are shown
            ClearActiveEffects();
            
            // Fill in result values
            if (finalScoreText != null) finalScoreText.text = $"Final Score: {results.finalScore}";
            if (timeScoreText != null) timeScoreText.text = $"Time Score: {results.timeScore}";
            if (pickupScoreText != null) pickupScoreText.text = $"Pickup Score: {results.pickupScore}";
            if (accuracyText != null) accuracyText.text = $"Accuracy: {results.accuracy:F1}%";
            if (timeSurvivedText != null) timeSurvivedText.text = $"Time Survived: {results.timeSurvived:F1}s";
            if (hitsTakenText != null) hitsTakenText.text = $"Hits Taken: {results.hitsTaken}";
            if (draftBonusText != null) draftBonusText.text = $"Draft Bonus: {results.draftBonus}";
        }
    }
    
    // Button event handlers
    void ResumeGame()
    {
        if (gameController != null)
        {
            gameController.ResumeFromPause();
        }
    }
    
    void ReturnToMenu()
    {
        if (gameController != null)
        {
            gameController.ReturnToMenu();
        }
    }
    
    void ContinueToResults()
    {
        if (gameController != null)
        {
            gameController.ContinueToResults();
        }
    }
    
    // Coroutines for visual effects
    IEnumerator PulseTimerText()
    {
        while (true)
        {
            if (timerText != null)
            {
                timerText.transform.localScale = Vector3.one * 1.2f;
                yield return new WaitForSeconds(0.2f);
                timerText.transform.localScale = Vector3.one;
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                break;
            }
        }
    }
    
    IEnumerator PulseGravityIndicator()
    {
        if (gravityArrow == null) yield break;
        
        Vector3 originalScale = gravityArrow.transform.localScale;
        
        while (gravityController != null && gravityController.IsWarningActive)
        {
            // Pulse scale
            float pulse = 1f + 0.3f * Mathf.Sin(Time.time * 8f);
            gravityArrow.transform.localScale = originalScale * pulse;
            
            yield return null;
        }
        
        // Reset scale
        gravityArrow.transform.localScale = originalScale;
    }
    
    IEnumerator ShowGravityFlipFeedback()
    {
        // Brief screen flash or other flip indicator
        // For now, just pulse the gravity arrow
        if (gravityArrow != null)
        {
            Vector3 originalScale = gravityArrow.transform.localScale;
            gravityArrow.transform.localScale = originalScale * 1.5f;
            
            yield return new WaitForSeconds(0.1f);
            
            gravityArrow.transform.localScale = originalScale;
        }
    }
    
    IEnumerator HitFeedbackCoroutine()
    {
        if (hitFeedbackPanel != null)
        {
            hitFeedbackPanel.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            hitFeedbackPanel.SetActive(false);
        }
    }
    
    IEnumerator PickupFeedbackCoroutine()
    {
        if (pickupFeedbackPanel != null)
        {
            pickupFeedbackPanel.SetActive(true);
            
            // Animate upward movement and fade
            Vector3 startPos = pickupFeedbackPanel.transform.localPosition;
            Vector3 endPos = startPos + Vector3.up * 50f;
            
            for (float t = 0; t < 1f; t += Time.deltaTime * 2f)
            {
                pickupFeedbackPanel.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                
                // Fade out
                CanvasGroup cg = pickupFeedbackPanel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f - t;
                }
                
                yield return null;
            }
            
            pickupFeedbackPanel.SetActive(false);
            pickupFeedbackPanel.transform.localPosition = startPos;
            
            // Reset alpha
            CanvasGroup canvasGroup = pickupFeedbackPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
    }
    
    IEnumerator PunchScale(Transform target)
    {
        if (target == null) yield break;
        
        Vector3 originalScale = target.localScale;
        
        // Scale up
        target.localScale = originalScale * 1.3f;
        yield return new WaitForSeconds(0.1f);
        
        // Scale back
        target.localScale = originalScale;
    }
    
    // Active Effects Management
    private List<GameObject> activeEffectElements = new List<GameObject>();
    private Dictionary<string, GameObject> effectElementMap = new Dictionary<string, GameObject>();
    
    public void AddActiveEffect(RuleCard card)
    {
        if (activeEffectsContainer == null || activeEffectPrefab == null) return;
        if (effectElementMap.ContainsKey(card.id)) return;
        
        GameObject effectElement = Instantiate(activeEffectPrefab, activeEffectsContainer);
        
        // Configure the effect element
        TMP_Text titleText = effectElement.GetComponentInChildren<TMP_Text>();
        if (titleText != null)
        {
            titleText.text = card.title;
            titleText.color = GetEffectTextColor(card);
        }
        
        // Add tooltip
        SimpleTooltip tooltip = effectElement.GetComponent<SimpleTooltip>();
        if (tooltip == null)
        {
            tooltip = effectElement.AddComponent<SimpleTooltip>();
        }
        tooltip.tooltipText = $"{card.title}\n{card.GetDescriptionForGame(MinigameType.GravityFlipDodge)}";
        
        // Track the element
        activeEffectElements.Add(effectElement);
        effectElementMap[card.id] = effectElement;
        
        // Show effects container if hidden
        if (activeEffectsContainer.gameObject.activeSelf == false)
        {
            activeEffectsContainer.gameObject.SetActive(true);
        }
        
        // Update title
        if (activeEffectsTitle != null)
        {
            activeEffectsTitle.text = $"Active Effects ({activeEffectElements.Count})";
        }
    }
    
    public void RemoveActiveEffect(string cardId)
    {
        if (!effectElementMap.ContainsKey(cardId)) return;
        
        GameObject effectElement = effectElementMap[cardId];
        activeEffectElements.Remove(effectElement);
        effectElementMap.Remove(cardId);
        
        if (effectElement != null)
        {
            Destroy(effectElement);
        }
        
        // Update title
        if (activeEffectsTitle != null)
        {
            if (activeEffectElements.Count > 0)
            {
                activeEffectsTitle.text = $"Active Effects ({activeEffectElements.Count})";
            }
            else
            {
                activeEffectsTitle.text = "No Active Effects";
                // Optionally hide the container
                activeEffectsContainer.gameObject.SetActive(false);
            }
        }
    }
    
    public void ClearActiveEffects()
    {
        foreach (GameObject element in activeEffectElements)
        {
            if (element != null)
            {
                Destroy(element);
            }
        }
        
        activeEffectElements.Clear();
        effectElementMap.Clear();
        
        if (activeEffectsTitle != null)
        {
            activeEffectsTitle.text = "No Active Effects";
        }
        
        if (activeEffectsContainer != null)
        {
            activeEffectsContainer.gameObject.SetActive(false);
        }
    }
    
    Color GetEffectTextColor(RuleCard card)
    {
        // Green for positive effects, red for negative, white for mixed
        bool hasPositive = card.effects != null && card.effects.Length > 0;
        bool hasNegative = card.drawbacks != null && card.drawbacks.Length > 0;
        
        if (hasPositive && !hasNegative) return Color.green;
        if (!hasPositive && hasNegative) return Color.red;
        return Color.white;
    }
    
    // Load active effects from GameStateManager when scene starts
    void LoadActiveEffectsFromGameState()
    {
        if (GameStateManager.I != null && GameStateManager.I.activeRules != null)
        {
            Debug.Log($"Loading {GameStateManager.I.activeRules.Count} active effects from GameStateManager");
            
            foreach (RuleCard rule in GameStateManager.I.activeRules)
            {
                // Only show cards compatible with current minigame
                if (rule.IsCompatibleWith(MinigameType.GravityFlipDodge))
                {
                    Debug.Log($"Adding active effect: {rule.title}");
                    AddActiveEffect(rule);
                }
            }
            
            // Show effects title
            if (activeEffectsTitle != null)
            {
                if (activeEffectElements.Count > 0)
                {
                    activeEffectsTitle.text = $"Active Effects ({activeEffectElements.Count})";
                }
                else
                {
                    activeEffectsTitle.text = "No Active Effects";
                }
            }
        }
        else
        {
            Debug.Log("No GameStateManager or active rules found");
        }
    }
    
    // Public methods for testing
    [ContextMenu("Test Add Effect")]
    void TestAddEffect()
    {
        // This is just for testing in the editor
        Debug.Log("Test Add Effect called - connect this to your card system");
    }
    
    [ContextMenu("Reload Active Effects")]
    void TestReloadEffects()
    {
        ClearActiveEffects();
        LoadActiveEffectsFromGameState();
    }
}