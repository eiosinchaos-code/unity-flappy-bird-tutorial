using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitController : MonoBehaviour
{
    [SerializeField] private TMP_Text lastGameStatsText;
    [SerializeField] private TMP_Text topTenStatsText;
    [SerializeField] private string gameSceneName = "FlappyBird";

    private void Start()
    {
        Time.timeScale = 1f;
        DisplayLastGameStats();

        if (OnlineLeaderboardService.Instance != null)
        {
            StartCoroutine(OnlineLeaderboardService.Instance.GetTopTen(DisplayTopTen));
        }
    }

    private void DisplayLastGameStats()
    {
        if (lastGameStatsText == null) return;

        if (GameSessionData.Instance != null)
        {
            lastGameStatsText.text =
                $"Last Game — {GameSessionData.Instance.playerName}\n" +
                $"Score: {GameSessionData.Instance.lastScore} | Level: {GameSessionData.Instance.lastLevel} | Time: {GameSessionData.Instance.lastTimePlayed:F1}s";
        }
    }

    private void DisplayTopTen(List<ScoreEntry> entries)
    {
        if (topTenStatsText == null) return;

        if (entries == null || entries.Count == 0)
        {
            topTenStatsText.text = "Top 10 Scores:\nNo scores recorded yet.";
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("TOP 10 SCORES");
        for (int i = 0; i < entries.Count; i++)
        {
            ScoreEntry e = entries[i];
            sb.AppendLine($"{i + 1}. {e.playerName} — {e.score} pts | Lv.{e.level} ({e.timePlayed:F1}s)");
        }

        topTenStatsText.text = sb.ToString();
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}