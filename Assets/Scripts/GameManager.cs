using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Player player;
    [SerializeField] private Spawner spawner;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text timeText;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private string exitSceneName = "Exit";

    public int score { get; private set; } = 0;
    public float timePlayed { get; private set; } = 0f;
    public bool isPaused { get; private set; } = false;

    private bool isRunning = false;
    private bool isSubmittingAndExiting = false;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        string currentPlayer = GameSessionData.Instance != null ? GameSessionData.Instance.playerName : "Player";
        if (playerNameText != null)
        {
            playerNameText.text = $"Player: {currentPlayer}";
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Pause();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isSubmittingAndExiting)
        {
            if (isPaused)
            {
                Resume();
            }
            else if (isRunning)
            {
                PauseGameWithMenu();
            }
        }

        if (isRunning && !isPaused)
        {
            timePlayed += Time.deltaTime;
            UpdateTimeUI();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (player != null)
        {
            player.enabled = false;
        }
    }

    public void PauseGameWithMenu()
    {
        Pause();
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void Resume()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        isPaused = false;
        Time.timeScale = 1f;
        if (player != null)
        {
            player.enabled = true;
        }
    }

    public void Play()
    {
        score = 0;
        timePlayed = 0f;
        isRunning = true;
        isPaused = false;

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        UpdateTimeUI();

        if (pausePanel != null) pausePanel.SetActive(false);
        if (playButton != null) playButton.SetActive(false);
        if (gameOver != null) gameOver.SetActive(false);

        Time.timeScale = 1f;
        if (player != null)
        {
            player.enabled = true;
        }

        Pipes[] pipes = FindObjectsByType<Pipes>(FindObjectsSortMode.None);
        for (int i = 0; i < pipes.Length; i++)
        {
            Destroy(pipes[i].gameObject);
        }
    }

    public void GameOver()
    {
        isRunning = false;
        if (playButton != null) playButton.SetActive(true);
        if (gameOver != null) gameOver.SetActive(true);

        Pause();
        SaveAndGoToExit();
    }

    public void StopEarlyAndExit()
    {
        if (isSubmittingAndExiting) return;
        isRunning = false;
        Pause();
        SaveAndGoToExit();
    }

    public void IncreaseScore()
    {
        score++;
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    private void UpdateTimeUI()
    {
        if (timeText != null)
        {
            timeText.text = $"Time: {timePlayed:F1}s";
        }
    }

    private void SaveAndGoToExit()
    {
        if (isSubmittingAndExiting) return;
        isSubmittingAndExiting = true;
        StartCoroutine(SaveStatsRoutine());
    }

    private IEnumerator SaveStatsRoutine()
    {
        Time.timeScale = 1f;

        string nameToSave = GameSessionData.Instance != null ? GameSessionData.Instance.playerName : "Player";
        if (string.IsNullOrWhiteSpace(nameToSave)) nameToSave = "Player";

        int currentLevel = Mathf.Max(1, (score / 5) + 1);

        if (GameSessionData.Instance != null)
        {
            GameSessionData.Instance.lastScore = score;
            GameSessionData.Instance.lastLevel = currentLevel;
            GameSessionData.Instance.lastTimePlayed = timePlayed;
        }

        ScoreEntry entry = new ScoreEntry
        {
            playerName = nameToSave,
            level = currentLevel,
            score = score,
            timePlayed = timePlayed
        };

        if (OnlineLeaderboardService.Instance != null)
        {
            yield return OnlineLeaderboardService.Instance.SubmitScore(entry);
        }

        SceneManager.LoadScene(exitSceneName);
    }
}