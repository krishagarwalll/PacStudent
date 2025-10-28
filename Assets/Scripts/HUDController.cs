using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI ghostScaredTimerText;
    public Image livesImage;

    [Header("Lives Sprites")]
    public Sprite threeHeartsSprite;
    public Sprite twoHeartsSprite;
    public Sprite oneHeartSprite;
    public Sprite noHeartsSprite;

    [Header("Game State")]
    private int currentScore = 0;
    private float gameTime = 0f;
    private float ghostScaredTime = 0f;
    private bool isTimerRunning = false;
    private int currentLives = 3;

    void Start()
    {
        // Initialize UI
        UpdateScore(0);
        UpdateTimer(0f);
        HideGhostScaredTimer();
        UpdateLivesDisplay(3);
        
        // Start timer automatically
        StartTimer();
    }

    void Update()
    { // Update game timer
        if (isTimerRunning)
        {
            gameTime += Time.deltaTime;
            UpdateTimer(gameTime);
        }

        // Update ghost scared timer
        if (ghostScaredTime > 0)
        {
            ghostScaredTime -= Time.deltaTime;
            if (ghostScaredTime <= 0)
            {
                ghostScaredTime = 0;
                HideGhostScaredTimer();
            }
            else
            {
                UpdateGhostScaredTimer(ghostScaredTime);
            }
        }
    }
    
    public static string FormatScore(int score)
    {
        return score.ToString("D6"); // D6 = Decimal with 6 digits, zero-padded
    }

    public static string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void UpdateScore(int newScore)
    {
        currentScore = newScore;
        if (scoreText != null)
        {
            scoreText.text = FormatScore(currentScore);
        }
    }

    public void AddScore(int points)
    {
        UpdateScore(currentScore + points);
    }

    public void UpdateTimer(float time)
    {
        gameTime = time;
        if (timerText != null)
        {
            timerText.text = FormatTime(gameTime);
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void StartGhostScaredTimer(float duration)
    {
        ghostScaredTime = duration;
        if (ghostScaredTimerText != null)
        {
            ghostScaredTimerText.gameObject.SetActive(true);
        }
    }

    public void UpdateGhostScaredTimer(float time)
    {
        if (ghostScaredTimerText != null)
        {
            ghostScaredTimerText.text = Mathf.CeilToInt(time).ToString();
        }
    }

    public void HideGhostScaredTimer()
    {
        if (ghostScaredTimerText != null)
        {
            ghostScaredTimerText.gameObject.SetActive(false);
        }
    }
    
    public void UpdateLivesDisplay(int lives)
    {
        currentLives = lives;
        
        if (livesImage == null) return;

        // Change sprite based on lives remaining
        switch (currentLives)
        {
            case 3:
                livesImage.sprite = threeHeartsSprite;
                break;
            case 2:
                livesImage.sprite = twoHeartsSprite;
                break;
            case 1:
                livesImage.sprite = oneHeartSprite;
                break;
            case 0:
                if (noHeartsSprite != null)
                {
                    livesImage.sprite = noHeartsSprite;
                }
                else
                {
                    livesImage.enabled = false;
                }
                break;
            default:
                livesImage.sprite = threeHeartsSprite;
                break;
        }
    }

    public void RemoveLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
            UpdateLivesDisplay(currentLives);
        }
    }

    public void AddLife()
    {
        if (currentLives < 3)
        {
            currentLives++;
            UpdateLivesDisplay(currentLives);
        }
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("StartScene");
    }

    public float GetCurrentTime()
    {
        return gameTime;
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }
}