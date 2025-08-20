using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text titleText;
    public Button playButton;
    public Button quitButton;
    public TMP_Text versionText;

    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        if (titleText) titleText.text = "Winner's Rule";
        if (versionText) versionText.text = "v1.0";

        if (playButton)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartGame);
        }

        if (quitButton)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}