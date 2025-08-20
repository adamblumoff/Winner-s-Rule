using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text titleText;
    public TMP_Text roundsText;
    public Button startGameButton;
    public Button mainMenuButton;
    public Slider roundsSlider;
    public TMP_Text sliderValueText;

    [Header("Player Setup")]
    public TMP_InputField[] playerNameInputs = new TMP_InputField[4];
    public Toggle[] playerActiveToggles = new Toggle[4];

    private int selectedRounds = 6;

    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        if (titleText) titleText.text = "Winner's Rule - Lobby";
        
        if (roundsSlider)
        {
            roundsSlider.minValue = 3;
            roundsSlider.maxValue = 10;
            roundsSlider.value = selectedRounds;
            roundsSlider.onValueChanged.AddListener(OnRoundsChanged);
        }

        UpdateRoundsDisplay();

        if (startGameButton)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(StartGame);
            startGameButton.interactable = true;
        }
        else
        {
            Debug.LogError("Start Game Button is null! Check UI references in inspector.");
        }

        if (mainMenuButton)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        SetupPlayerInputs();
    }

    void SetupPlayerInputs()
    {
        for (int i = 0; i < playerNameInputs.Length; i++)
        {
            if (playerNameInputs[i])
            {
                playerNameInputs[i].text = $"Player {i + 1}";
            }

            if (playerActiveToggles[i])
            {
                playerActiveToggles[i].isOn = i < 2; // First 2 players active by default
            }
        }
    }

    void OnRoundsChanged(float value)
    {
        selectedRounds = Mathf.RoundToInt(value);
        UpdateRoundsDisplay();
    }

    void UpdateRoundsDisplay()
    {
        if (roundsText) roundsText.text = $"Rounds: {selectedRounds}";
        if (sliderValueText) sliderValueText.text = selectedRounds.ToString();
    }

    void StartGame()
    {
        if (GameStateManager.I == null) 
        {
            Debug.LogError("GameStateManager not found! Cannot start game.");
            return;
        }

        int activePlayers = 0;
        for (int i = 0; i < playerActiveToggles.Length; i++)
        {
            if (playerActiveToggles[i] && playerActiveToggles[i].isOn)
                activePlayers++;
        }

        if (activePlayers < 2)
        {
            Debug.LogWarning("Need at least 2 players to start!");
            return;
        }

        var gsm = GameStateManager.I;
        gsm.totalRounds = selectedRounds;
        gsm.StartMatch();
    }

    void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}