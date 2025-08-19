// Assets/Scripts/UI/UIButtonAction.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonAction : MonoBehaviour
{
    public enum ActionType { LoadScene, StartMatch, LoadLobby, LoadDraft, QuitApp }
    [Header("Choose action")]
    public ActionType action = ActionType.LoadScene;

    [Header("For LoadScene")]
    public string sceneName = "";      // e.g., "MainMenu", "Lobby", "Game_Race_2D", "Results", "Draft"

    [Header("Optional")]
    public float delaySeconds = 0f;    // small UI delay if desired

    bool busy;

    public void Invoke()
    {
        if (busy) return;
        busy = true;
        if (delaySeconds > 0f) StartCoroutine(DelayThenRun());
        else Run();
    }

    System.Collections.IEnumerator DelayThenRun()
    {
        yield return new WaitForSeconds(delaySeconds);
        Run();
    }

    void Run()
    {
        switch (action)
        {
            case ActionType.LoadScene:
                if (!string.IsNullOrEmpty(sceneName))
                    SceneManager.LoadScene(sceneName);
                break;

            case ActionType.StartMatch:
                if (GameStateManager.I != null) GameStateManager.I.StartMatch();
                break;

            case ActionType.LoadLobby:
                if (GameStateManager.I != null) GameStateManager.I.LoadLobby();
                break;

            case ActionType.LoadDraft:
                if (GameStateManager.I != null) GameStateManager.I.LoadDraft();
                break;

            case ActionType.QuitApp:
                Application.Quit();
                break;
        }
        busy = false;
    }
}
