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
    
    [Header("Character Selection")]
    public CharacterSpriteDatabase characterDatabase;
    public Button[] characterLeftButtons = new Button[4];
    public Button[] characterRightButtons = new Button[4];
    public Image[] characterPreviewImages = new Image[4];
    public TMP_Text[] characterNameTexts = new TMP_Text[4];

    private int selectedRounds = 6;
    private int[] playerSelectedCharacters = new int[4]; // Track each player's selected character index

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
        SetupCharacterSelection();
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
    
    void SetupCharacterSelection()
    {
        if (characterDatabase == null)
        {
            Debug.LogWarning("CharacterSpriteDatabase not assigned in LobbyUI!");
            return;
        }
        
        for (int i = 0; i < 4; i++)
        {
            // Initialize each player with a unique default character
            playerSelectedCharacters[i] = i % characterDatabase.GetCharacterCount();
            
            // Setup left button
            if (characterLeftButtons[i] != null)
            {
                int playerIndex = i; // Capture for closure
                characterLeftButtons[i].onClick.RemoveAllListeners();
                characterLeftButtons[i].onClick.AddListener(() => OnCharacterLeft(playerIndex));
            }
            
            // Setup right button  
            if (characterRightButtons[i] != null)
            {
                int playerIndex = i; // Capture for closure
                characterRightButtons[i].onClick.RemoveAllListeners();
                characterRightButtons[i].onClick.AddListener(() => OnCharacterRight(playerIndex));
            }
            
        }
        
        // Update all displays after setup
        RefreshAllCharacterDisplays();
    }
    
    void OnCharacterLeft(int playerIndex)
    {
        if (characterDatabase == null) return;
        
        int originalSelection = playerSelectedCharacters[playerIndex];
        int attempts = 0;
        
        do
        {
            playerSelectedCharacters[playerIndex]--;
            if (playerSelectedCharacters[playerIndex] < 0)
            {
                playerSelectedCharacters[playerIndex] = characterDatabase.GetCharacterCount() - 1;
            }
            attempts++;
        } 
        while (IsCharacterAlreadySelected(playerSelectedCharacters[playerIndex], playerIndex) && 
               attempts < characterDatabase.GetCharacterCount());
        
        // If all characters are taken, revert to original
        if (attempts >= characterDatabase.GetCharacterCount())
        {
            playerSelectedCharacters[playerIndex] = originalSelection;
        }
        
        RefreshAllCharacterDisplays();
        Debug.Log($"Player {playerIndex + 1} selected character: {characterDatabase.GetCharacter(playerSelectedCharacters[playerIndex])?.characterName}");
    }
    
    void OnCharacterRight(int playerIndex)
    {
        if (characterDatabase == null) return;
        
        int originalSelection = playerSelectedCharacters[playerIndex];
        int attempts = 0;
        
        do
        {
            playerSelectedCharacters[playerIndex]++;
            if (playerSelectedCharacters[playerIndex] >= characterDatabase.GetCharacterCount())
            {
                playerSelectedCharacters[playerIndex] = 0;
            }
            attempts++;
        } 
        while (IsCharacterAlreadySelected(playerSelectedCharacters[playerIndex], playerIndex) && 
               attempts < characterDatabase.GetCharacterCount());
        
        // If all characters are taken, revert to original
        if (attempts >= characterDatabase.GetCharacterCount())
        {
            playerSelectedCharacters[playerIndex] = originalSelection;
        }
        
        RefreshAllCharacterDisplays();
        Debug.Log($"Player {playerIndex + 1} selected character: {characterDatabase.GetCharacter(playerSelectedCharacters[playerIndex])?.characterName}");
    }
    
    bool IsCharacterAlreadySelected(int characterIndex, int excludePlayerIndex)
    {
        for (int i = 0; i < playerActiveToggles.Length; i++)
        {
            // Skip the player we're checking for and inactive players
            if (i == excludePlayerIndex || !IsPlayerActive(i))
                continue;
                
            if (playerSelectedCharacters[i] == characterIndex)
                return true;
        }
        return false;
    }
    
    bool IsPlayerActive(int playerIndex)
    {
        return playerActiveToggles[playerIndex] != null && playerActiveToggles[playerIndex].isOn;
    }
    
    void RefreshAllCharacterDisplays()
    {
        for (int i = 0; i < 4; i++)
        {
            UpdateCharacterDisplay(i);
        }
    }
    
    void UpdateCharacterDisplay(int playerIndex)
    {
        if (characterDatabase == null) return;
        
        var characterData = characterDatabase.GetCharacter(playerSelectedCharacters[playerIndex]);
        if (characterData == null) return;
        
        // Check if this character is selected by another active player
        bool isSelectedElsewhere = IsCharacterAlreadySelected(playerSelectedCharacters[playerIndex], playerIndex);
        
        // Update preview image
        if (characterPreviewImages[playerIndex] != null)
        {
            characterPreviewImages[playerIndex].sprite = characterData.previewSprite;
            
            // Dim the image if character is taken by someone else (visual feedback)
            Color imageColor = characterPreviewImages[playerIndex].color;
            imageColor.a = isSelectedElsewhere ? 0.5f : 1.0f;
            characterPreviewImages[playerIndex].color = imageColor;
        }
        
        // Update character name text
        if (characterNameTexts[playerIndex] != null)
        {
            string displayName = characterData.characterName;
            if (isSelectedElsewhere)
            {
                displayName += " (Taken)";
            }
            characterNameTexts[playerIndex].text = displayName;
            
            // Also dim the text
            Color textColor = characterNameTexts[playerIndex].color;
            textColor.a = isSelectedElsewhere ? 0.5f : 1.0f;
            characterNameTexts[playerIndex].color = textColor;
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
        string[] playerNames = new string[4];
        int[] characterSelections = new int[4];
        
        for (int i = 0; i < playerActiveToggles.Length; i++)
        {
            bool isActive = playerActiveToggles[i] && playerActiveToggles[i].isOn;
            Debug.Log($"Player {i + 1}: Toggle={playerActiveToggles[i] != null}, IsOn={playerActiveToggles[i]?.isOn}, Active={isActive}");
            
            if (isActive)
            {
                activePlayers++;
                // Get the player name from input field
                if (playerNameInputs[i] != null && !string.IsNullOrEmpty(playerNameInputs[i].text))
                    playerNames[i] = playerNameInputs[i].text;
                else
                    playerNames[i] = $"Player {i + 1}";
                
                // Get the selected character
                characterSelections[i] = playerSelectedCharacters[i];
                    
                Debug.Log($"Player {i + 1} name: {playerNames[i]}, character: {characterSelections[i]}");
            }
        }

        Debug.Log($"Total active players: {activePlayers}");

        if (activePlayers < 2)
        {
            Debug.LogWarning("Need at least 2 players to start!");
            return;
        }

        var gsm = GameStateManager.I;
        gsm.totalRounds = selectedRounds;
        gsm.StartMatch(activePlayers, playerNames, characterSelections);
    }

    void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void EnableAllPlayers()
    {
        for (int i = 0; i < playerActiveToggles.Length; i++)
        {
            if (playerActiveToggles[i] != null)
            {
                playerActiveToggles[i].isOn = true;
            }
        }
        Debug.Log("All player toggles enabled");
    }
}