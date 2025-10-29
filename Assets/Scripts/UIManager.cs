using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Level 1 High Score UI (Scene Index 1)")]
    public TMP_Text highScoreText1;
    public TMP_Text bestTimeText1;
    
    [Header("Level 2 High Score UI (Scene Index 2)")]
    public TMP_Text highScoreText2;
    public TMP_Text bestTimeText2;

    void Start()
    {
        LoadAndDisplayHighScores();
    }

    void LoadAndDisplayHighScores()
    {
        // Level 1 (Scene index 1)
        int score1 = PlayerPrefs.GetInt("Level1_HighScore", 0);
        float time1 = PlayerPrefs.GetFloat("Level1_BestTime", 0f);
        
        if (highScoreText1) 
            highScoreText1.text = $"HIGH SCORE\n{score1:000000}";
        
        if (bestTimeText1)
            bestTimeText1.text = $"BEST TIME\n{FormatTime(time1)}";
        
        // Level 2 (Scene index 2 - if it exists)
        int score2 = PlayerPrefs.GetInt("Level2_HighScore", 0);
        float time2 = PlayerPrefs.GetFloat("Level2_BestTime", 0f);
        
        if (highScoreText2) 
            highScoreText2.text = $"HIGH SCORE\n{score2:000000}";
        
        if (bestTimeText2)
            bestTimeText2.text = $"BEST TIME\n{FormatTime(time2)}";
    }

    string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds <= 0) return "00:00:00";
        
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int centiseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        
        return $"{minutes:D2}:{seconds:D2}:{centiseconds:D2}";
    }

    // Load Level 1 - PacStudent scene (index 1)
    public void LoadLevel1()
    {
        SceneManager.LoadScene(1);
    }

    // Load Level 2 - Innovation scene (index 2 - add to build settings when ready)
    public void LoadLevel2()
    {
        SceneManager.LoadScene(1);
    }

    // Alternative: Load by scene name
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