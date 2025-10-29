using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("HUD")]
    public HUDController hud;

    [Header("Audio")]
    public AudioManager audioManager;

    [Header("Scores & Timings")]
    public int pelletScore = 10;
    public int cherryScore = 100;
    public int powerPillScore = 50;
    public int ghostScore = 300;
    public float scaredDuration = 10f;
    public float ghostRespawnTime = 3f;

    [Header("Death and Respawn")]
    public int startingLives = 3;
    public PacStudentController pacStudent;
    public Transform pacStart;
    public Animator[] ghostAnimators;
    public Transform[] ghostStarts;
    public float deathAnimDuration = 1.2f;
    public GhostStateController ghostStateCtrl;

    [Header("Countdown UI")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;

    [Header("Game over UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;

    public event Action<float> OnGhostScaredStart;
    public event Action OnGhostRecovering;
    public event Action OnGhostNormal;

    int score;
    float gameTime;
    int lives;
    bool gameStarted = false;
    bool gameOver = false;

    float scaredLeft;
    bool recoveringSent;

    float[] ghostRespawnTimers = new float[4];
    bool[] ghostIsDead = new bool[4];
    int deadGhostCount = 0;

    public bool IsScaredActive => scaredLeft > 0f;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    void Start()
    {
        lives = hud ? hud.GetCurrentLives() : startingLives;

        if (hud)
        {
            hud.UpdateScore(0);
            hud.UpdateTimer(0f);
            hud.HideGhostScaredTimer();
            hud.UpdateLivesDisplay(lives);
        }

        score = 0;
        gameTime = 0f;
        scaredLeft = 0f;
        recoveringSent = false;
        gameStarted = false;
        gameOver = false;

        for (int i = 0; i < 4; i++)
        {
            ghostRespawnTimers[i] = 0f;
            ghostIsDead[i] = false;
        }
        deadGhostCount = 0;

        StartCoroutine(RoundStartCountdown());
    }

    void Update()
    {
        if (!gameStarted || gameOver) return;

        gameTime += Time.deltaTime;
        if (hud) hud.UpdateTimer(gameTime);

        if (scaredLeft > 0f)
        {
            scaredLeft = Mathf.Max(0f, scaredLeft - Time.deltaTime);
            if (hud) hud.UpdateGhostScaredTimer(scaredLeft);

            if (!recoveringSent && scaredLeft <= 3f && scaredLeft > 0f)
            {
                recoveringSent = true;
                OnGhostRecovering?.Invoke();
            }
            if (Mathf.Approximately(scaredLeft, 0f))
            {
                if (hud) hud.HideGhostScaredTimer();
                recoveringSent = false;
                
                if (deadGhostCount == 0)
                {
                    audioManager?.PlayNormalLoop();
                }
                
                OnGhostNormal?.Invoke();
            }
        }

        UpdateGhostRespawnTimers();

        if (Input.GetKeyDown(KeyCode.K)) AddLife(+1);
        if (Input.GetKeyDown(KeyCode.L)) AddLife(-1);
    }

    IEnumerator RoundStartCountdown()
    {
        // disable player movement
        if (pacStudent) 
        {
            pacStudent.SetInputEnabled(false);
        }
        
        // disable ghost state controller
        if (ghostStateCtrl) 
        {
            ghostStateCtrl.enabled = false;
        }
        
        // disable all ghost movement
        GhostController[] allGhosts = FindObjectsByType<GhostController>(FindObjectsSortMode.None);
        foreach (var gc in allGhosts)
        {
            if (gc) gc.DisableMovement();
        }
        
        if (countdownPanel == null)
        {
            gameStarted = true;
            if (pacStudent) pacStudent.SetInputEnabled(true);
            if (ghostStateCtrl) ghostStateCtrl.enabled = true;
            foreach (var gc in allGhosts)
            {
                if (gc) gc.EnableMovement();
            }
            yield break;
        }
        
        countdownPanel.SetActive(true);

        string[] numbers = { "3", "2", "1", "GO!" };
        foreach (string num in numbers)
        {
            if (countdownText != null)
            {
                countdownText.text = num;
            }
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1f);
        countdownPanel.SetActive(false);

        gameStarted = true;
        if (pacStudent) pacStudent.SetInputEnabled(true);
        if (ghostStateCtrl) ghostStateCtrl.enabled = true;
        if (hud) hud.StartTimer();
        
        // enable all ghost movement after countdown
        foreach (var gc in allGhosts)
        {
            if (gc) gc.EnableMovement();
        }
    }

    void UpdateGhostRespawnTimers()
    {
        for (int i = 0; i < 4; i++)
        {
            if (ghostIsDead[i])
            {
                ghostRespawnTimers[i] -= Time.deltaTime;
                
                if (ghostRespawnTimers[i] <= 0f)
                {
                    RespawnGhost(i);
                }
            }
        }
    }

    public void AddPellet()   
    { 
        AddScore(pelletScore);
    }

    public void AddCherry()   
    { 
        AddScore(cherryScore); 
    }
    
    public void AddPowerPill()
    {
        AddScore(powerPillScore);
        StartScared(scaredDuration);
    }
    
    void AddScore(int delta)
    {
        score += delta;
        if (hud) hud.UpdateScore(score);
    }

    public void StartScared(float duration)
    {
        scaredLeft = duration;
        recoveringSent = false;
        if (hud) hud.StartGhostScaredTimer(scaredLeft);
        
        if (deadGhostCount == 0)
        {
            audioManager?.PlayScaredLoop();
        }
        
        OnGhostScaredStart?.Invoke(duration);
    }

    public void GhostEaten(int ghostIndex)
    {
        if (ghostIndex < 0 || ghostIndex >= 4) return;
        if (ghostIsDead[ghostIndex]) return;

        ghostIsDead[ghostIndex] = true;
        ghostRespawnTimers[ghostIndex] = ghostRespawnTime;
        deadGhostCount++;

        if (ghostStateCtrl) ghostStateCtrl.SetGhostDead(ghostIndex);

        AddScore(ghostScore);

        audioManager?.PlayDeadGhostLoop();
    }

    void RespawnGhost(int ghostIndex)
    {
        if (!ghostIsDead[ghostIndex]) return;

        ghostIsDead[ghostIndex] = false;
        deadGhostCount--;

        int newState = 0;
        
        if (scaredLeft > 3f)
        {
            newState = 1;
        }
        else if (scaredLeft > 0f)
        {
            newState = 2;
        }

        if (ghostStateCtrl) ghostStateCtrl.ReviveGhost(ghostIndex, newState);

        if (ghostAnimators != null && ghostStarts != null && 
            ghostIndex < ghostAnimators.Length && ghostIndex < ghostStarts.Length)
        {
            var a = ghostAnimators[ghostIndex];
            var t = ghostStarts[ghostIndex];
            if (a && t)
            {
                a.transform.position = t.position;
            }
        }

        if (deadGhostCount == 0)
        {
            if (scaredLeft > 0f)
            {
                audioManager?.PlayScaredLoop();
            }
            else
            {
                audioManager?.PlayNormalLoop();
            }
        }
    }

    bool deathRunning;
    public void PlayerDied()
    {
        if (!deathRunning && !gameOver) StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        deathRunning = true;

        pacStudent?.SetInputEnabled(false);
        if (ghostStateCtrl) ghostStateCtrl.enabled = false;
        FreezeGhostAnimators(true);

        audioManager?.PlayDeath();

        pacStudent?.GetComponent<PacDeathFX>()?.Play();

        var anim = pacStudent ? pacStudent.GetComponent<Animator>() : null;
        if (anim)
        {
            anim.enabled = true;
            anim.speed = 1f;
            anim.SetInteger("State", 4);
        }
        pacStudent?.PlayDeath();

        yield return new WaitForSeconds(deathAnimDuration);

        AddLife(-1);

        if (lives <= 0)
        {
            StartCoroutine(GameOverRoutine());
            deathRunning = false;
            yield break;
        }

        RespawnPacStudent();
        ResetAllGhosts();

        audioManager?.PlayNormalLoop();
        OnGhostNormal?.Invoke();
        if (hud) hud.HideGhostScaredTimer();
        scaredLeft = 0f; 
        recoveringSent = false;

        for (int i = 0; i < 4; i++)
        {
            ghostIsDead[i] = false;
            ghostRespawnTimers[i] = 0f;
        }
        deadGhostCount = 0;

        yield return StartCoroutine(WaitForAnyMoveKey());

        if (ghostStateCtrl) ghostStateCtrl.enabled = true;
        pacStudent?.SetInputEnabled(true);
        pacStudent?.EndDeath();
        FreezeGhostAnimators(false);

        deathRunning = false;
    }

    IEnumerator GameOverRoutine()
    {
        gameOver = true;

        if (pacStudent) pacStudent.SetInputEnabled(false);
        if (ghostStateCtrl) ghostStateCtrl.enabled = false;
        FreezeGhostAnimators(true);
        
        if (gameOverPanel == null)
        {
            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene(0);
            yield break;
        }

        gameOverPanel.SetActive(true);
        if (gameOverText) gameOverText.text = "GAME OVER";

        SaveHighScore();
        
        yield return new WaitForSeconds(3f);
        
        SceneManager.LoadScene(0);
    }

    void SaveHighScore()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        string scoreKey = "Level" + sceneIndex + "_HighScore";
        string timeKey = "Level" + sceneIndex + "_BestTime";

        int previousHighScore = PlayerPrefs.GetInt(scoreKey, 0);
        float previousBestTime = PlayerPrefs.GetFloat(timeKey, 99999f);

        if (score > previousHighScore || (score == previousHighScore && gameTime < previousBestTime))
        {
            PlayerPrefs.SetInt(scoreKey, score);
            PlayerPrefs.SetFloat(timeKey, gameTime);
            PlayerPrefs.Save();
        }
    }

    void RespawnPacStudent()
    {
        if (!pacStudent || !pacStart) return;
        pacStudent.TeleportTo(pacStart.position);
        pacStudent.FaceRightIdle();
    }

    void ResetAllGhosts()
    {
        if (ghostAnimators == null || ghostStarts == null) return;
        int n = Mathf.Min(ghostAnimators.Length, ghostStarts.Length);
        for (int i = 0; i < n; i++)
        {
            var a = ghostAnimators[i];
            var t = ghostStarts[i];
            if (!a || !t) continue;
            a.transform.position = t.position;
            a.speed = 1f;
            a.SetInteger("GhostState", 0);
            a.Play("Walking", 0, 0f);
        }
    }

    void FreezeGhostAnimators(bool freeze)
    {
        if (ghostAnimators == null) return;
        foreach (var a in ghostAnimators)
            if (a) a.speed = freeze ? 0f : 1f;
    }

    IEnumerator WaitForAnyMoveKey()
    {
        while (!Input.GetKeyDown(KeyCode.W) &&
               !Input.GetKeyDown(KeyCode.A) &&
               !Input.GetKeyDown(KeyCode.S) &&
               !Input.GetKeyDown(KeyCode.D))
            yield return null;
    }

    void AddLife(int delta)
    {
        if (!hud) { lives = Mathf.Clamp(lives + delta, 0, 99); return; }

        if (delta > 0) for (int i = 0; i < delta; i++) hud.AddLife();
        if (delta < 0) for (int i = 0; i < -delta; i++) hud.RemoveLife();

        lives = hud.GetCurrentLives();
    }
}