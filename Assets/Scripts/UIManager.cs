using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // Load Level 1 - PacStudent scene
    public void LoadLevel1()
    {
        SceneManager.LoadScene(1);
    }

    // Load Level 2 - Innovation scene (for 100% HD band)
    public void LoadLevel2()
    {
        SceneManager.LoadScene(1);
    }

    // Alternative: Load by scene name instead of index
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Quit the game
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}