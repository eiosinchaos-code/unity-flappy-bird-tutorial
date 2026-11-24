using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class FloppyBirdServerRow
{
    public string name;
    public string playerName;
    public string level;
    public string score;
    public string time;
    public string timePlayed;
}

[Serializable]
public class FloppyBirdServerWrapper
{
    public List<FloppyBirdServerRow> entries;
    public List<FloppyBirdServerRow> data;
    public List<FloppyBirdServerRow> scores;
}

[Serializable]
public class ScoreEntry
{
    public string playerName;
    public int level;
    public int score;
    public float timePlayed;
}

[Serializable]
public class ScoreEntryListWrapper
{
    public List<ScoreEntry> entries = new List<ScoreEntry>();
}

public class OnlineLeaderboardService : MonoBehaviour
{
    public static OnlineLeaderboardService Instance { get; private set; }

    [Header("floppy_birds Endpoints")]
    [SerializeField] private string getScoresUrl = "https://104-129-130-168.us-tx-cloud-xip.com/getFloppyBirds.php";
    [SerializeField] private string updateScoreBaseUrl = "https://104-129-130-168.us-tx-cloud-xip.com/updateFloppyBirds.php";

    private const string LocalFallbackKey = "FloppyLocalLeaderboardCache";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator SubmitScore(ScoreEntry entry, Action<bool> onComplete = null)
    {
        SaveToLocalFallback(entry);

        string safeName = UnityWebRequest.EscapeURL(string.IsNullOrWhiteSpace(entry.playerName) ? "Player" : entry.playerName);
        string safeTime = UnityWebRequest.EscapeURL(entry.timePlayed.ToString("0.##", CultureInfo.InvariantCulture));
        string url = $"{updateScoreBaseUrl}?name={safeName}&level={entry.level}&score={entry.score}&time={safeTime}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            bool success = request.result == UnityWebRequest.Result.Success;
            if (!success)
            {
                Debug.LogWarning($"Failed to submit score to floppy_birds: {request.error}");
            }

            onComplete?.Invoke(success);
        }
    }

    public IEnumerator GetTopTen(Action<List<ScoreEntry>> onComplete)
    {
        List<ScoreEntry> results = new List<ScoreEntry>();

        using (UnityWebRequest request = UnityWebRequest.Get(getScoresUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success && !string.IsNullOrWhiteSpace(request.downloadHandler.text))
            {
                string rawJson = request.downloadHandler.text.Trim();
                results = ParseServerScores(rawJson);
            }
            else if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Failed to fetch scores from floppy_birds: {request.error}");
            }
        }

        if (results.Count == 0)
        {
            results = LoadFromLocalFallback();
        }

        results.Sort((a, b) =>
        {
            int scoreCompare = b.score.CompareTo(a.score);
            if (scoreCompare != 0) return scoreCompare;
            int levelCompare = b.level.CompareTo(a.level);
            return levelCompare != 0 ? levelCompare : b.timePlayed.CompareTo(a.timePlayed);
        });

        if (results.Count > 10)
        {
            results = results.GetRange(0, 10);
        }

        onComplete?.Invoke(results);
    }

    private List<ScoreEntry> ParseServerScores(string rawJson)
    {
        List<ScoreEntry> parsed = new List<ScoreEntry>();
        List<FloppyBirdServerRow> rows = null;

        if (rawJson.StartsWith("["))
        {
            string wrappedArray = "{\"entries\":" + rawJson + "}";
            FloppyBirdServerWrapper arrWrapper = JsonUtility.FromJson<FloppyBirdServerWrapper>(wrappedArray);
            rows = arrWrapper?.entries;
        }
        else
        {
            FloppyBirdServerWrapper objWrapper = JsonUtility.FromJson<FloppyBirdServerWrapper>(rawJson);
            if (objWrapper != null)
            {
                rows = objWrapper.entries ?? objWrapper.data ?? objWrapper.scores;
            }
        }

        if (rows == null) return parsed;

        foreach (FloppyBirdServerRow row in rows)
        {
            string resolvedName = !string.IsNullOrWhiteSpace(row.playerName) ? row.playerName : row.name;
            if (string.IsNullOrWhiteSpace(resolvedName)) resolvedName = "Player";

            int.TryParse(row.score, out int parsedScore);
            int.TryParse(row.level, out int parsedLevel);
            if (parsedLevel <= 0) parsedLevel = 1;

            string rawTime = !string.IsNullOrWhiteSpace(row.time) ? row.time : row.timePlayed;
            float.TryParse(rawTime, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedTime);

            parsed.Add(new ScoreEntry
            {
                playerName = resolvedName,
                level = parsedLevel,
                score = parsedScore,
                timePlayed = parsedTime
            });
        }

        return parsed;
    }

    private void SaveToLocalFallback(ScoreEntry entry)
    {
        List<ScoreEntry> list = LoadFromLocalFallback();
        list.Add(entry);
        ScoreEntryListWrapper wrapper = new ScoreEntryListWrapper { entries = list };
        PlayerPrefs.SetString(LocalFallbackKey, JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }

    private List<ScoreEntry> LoadFromLocalFallback()
    {
        string raw = PlayerPrefs.GetString(LocalFallbackKey, "");
        if (string.IsNullOrEmpty(raw))
        {
            return new List<ScoreEntry>();
        }

        ScoreEntryListWrapper wrapper = JsonUtility.FromJson<ScoreEntryListWrapper>(raw);
        return wrapper?.entries ?? new List<ScoreEntry>();
    }
}