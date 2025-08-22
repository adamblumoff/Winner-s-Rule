using UnityEngine;

public class SceneSetupHelper : MonoBehaviour
{
    [Header("Scene Setup")]
    public bool setupOnStart = true;
    public GameObject gameManagerPrefab;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupScene();
        }
    }

    void SetupScene()
    {
        EnsureGameManager();
    }

    void EnsureGameManager()
    {
        if (GameStateManager.I == null)
        {
            if (gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
                Debug.Log("GameStateManager created from prefab");
            }
            else
            {
                GameObject gameManagerObj = new GameObject("GameStateManager");
                gameManagerObj.AddComponent<GameStateManager>();
                Debug.Log("GameStateManager created dynamically");
            }
        }
    }

    [ContextMenu("Setup Scene Now")]
    public void SetupSceneManually()
    {
        SetupScene();
    }
}