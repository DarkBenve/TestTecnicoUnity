using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public const string MainMenuScene = "MainMenu";
    public const string GameplayScene = "Gameplay";
    public const string ResultScene = "Result";

    public void StartGame()
    {
        GameSessionData.GetOrCreate().BeginNewGame();
        LoadScene(GameplayScene);
    }

    public void ReplayGame()
    {
        StartGame();
    }

    public void GoToMainMenu()
    {
        GameSessionData.GetOrCreate().CancelGame();
        LoadScene(MainMenuScene);
    }

    public void GoToResults()
    {
        GameSessionData.GetOrCreate().CompleteGame();
        LoadResultScene();
    }

    public static void LoadResultScene()
    {
        LoadScene(ResultScene);
    }

    private static void LoadScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not present in Build Settings.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
