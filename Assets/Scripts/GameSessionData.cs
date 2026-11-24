using UnityEngine;

public class GameSessionData : MonoBehaviour
{
    public static GameSessionData Instance { get; private set; }

    [Header("Player & Preferences")]
    public string playerName = "Player";
    public float spawnRate = 1f;
    public float verticalGap = 3f;

    [Header("Current Game Stats")]
    public int lastScore = 0;
    public int lastLevel = 1;
    public float lastTimePlayed = 0f;

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

    public void ResetPreferencesToDefaults()
    {
        spawnRate = 1f;
        verticalGap = 3f;
    }
}