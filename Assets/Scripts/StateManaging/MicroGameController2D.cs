using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MicroGameController2D : MonoBehaviour
{
    [Header("Race Settings")]
    public float roundTime = 30f;
    
    [Header("UI Elements")]
    public Text timerText;
    public TMP_Text timerTextTMP;
    public TMP_Text roundInfoText;
    
    [Header("Player Setup")]
    public Transform[] playerSpawnPoints = new Transform[4];
    public GameObject playerPrefab;
    
    private float timeRemaining;
    private bool raceActive = true;
    private GameObject[] playerObjects = new GameObject[4];

    void Start()
    {
        timeRemaining = roundTime;
        SetupRace();
        RuleApplier2D.ApplyAll();
        
        if (roundInfoText && GameStateManager.I)
        {
            var gsm = GameStateManager.I;
            roundInfoText.text = $"Round {gsm.currentRound + 1}/{gsm.totalRounds}";
        }
    }

    void SetupRace()
    {
        SetupPlayers();
    }

    void SetupPlayers()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i < playerSpawnPoints.Length && playerSpawnPoints[i] != null && playerPrefab != null)
            {
                playerObjects[i] = Instantiate(playerPrefab, playerSpawnPoints[i].position, playerSpawnPoints[i].rotation);
                
                PlayerStats2D stats = playerObjects[i].GetComponent<PlayerStats2D>();
                if (stats != null)
                {
                    stats.playerId = i;
                    stats.playerName = $"Player {i + 1}";
                }

                PlayerController2D controller = playerObjects[i].GetComponent<PlayerController2D>();
                if (controller != null && i > 0)
                {
                    Destroy(controller);
                }
            }
        }
    }

    void Update()
    {
        if (!raceActive) return;

        timeRemaining -= Time.deltaTime;
        UpdateTimerDisplay();

        if (timeRemaining <= 0f)
        {
            EndRaceByTimeout();
        }
    }

    void UpdateTimerDisplay()
    {
        string timeText = Mathf.CeilToInt(timeRemaining).ToString();
        
        if (timerText) timerText.text = timeText;
        if (timerTextTMP) timerTextTMP.text = timeText;
    }

    void EndRaceByTimeout()
    {
        if (!raceActive) return;
        
        raceActive = false;
        Debug.Log("Race ended by timeout - determining winner by position");
        
        int winnerId = DetermineWinnerByPosition();
        GameStateManager.I.OnRoundEnd(winnerId);
    }

    int DetermineWinnerByPosition()
    {
        int bestPlayer = 0;
        float bestProgress = float.MinValue;

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (playerObjects[i] != null)
            {
                float progress = playerObjects[i].transform.position.x;
                if (progress > bestProgress)
                {
                    bestProgress = progress;
                    bestPlayer = i;
                }
            }
        }

        return bestPlayer;
    }

    public void OnPlayerFinished(int playerId)
    {
        if (!raceActive) return;
        
        raceActive = false;
        Debug.Log($"Player {playerId + 1} finished the race!");
        
        if (GameStateManager.I != null)
        {
            GameStateManager.I.OnRoundEnd(playerId);
        }
        else
        {
            Debug.LogError("GameStateManager not found when trying to end race!");
        }
    }
}
