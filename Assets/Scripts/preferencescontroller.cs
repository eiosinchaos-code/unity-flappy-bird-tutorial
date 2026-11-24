using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PreferencesController : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Slider spawnRateSlider;
    [SerializeField] private TMP_Text spawnRateValueText;
    [SerializeField] private TMP_Dropdown verticalGapDropdown;
    [SerializeField] private string gameSceneName = "FlappyBird";

    private readonly float[] gapOptions = { 2.0f, 2.5f, 3.0f, 3.5f, 4.0f };

    private void Start()
    {
        Time.timeScale = 1f;

        if (GameSessionData.Instance == null)
        {
            new GameObject("GameSessionData").AddComponent<GameSessionData>();
        }

        if (playerNameInput != null)
        {
            playerNameInput.text = GameSessionData.Instance.playerName;
        }

        if (spawnRateSlider != null)
        {
            spawnRateSlider.minValue = 0.25f;
            spawnRateSlider.maxValue = 3.0f;
            spawnRateSlider.value = GameSessionData.Instance.spawnRate;
            spawnRateSlider.onValueChanged.AddListener(OnSpawnRateChanged);
            OnSpawnRateChanged(spawnRateSlider.value);
        }

        if (verticalGapDropdown != null)
        {
            verticalGapDropdown.ClearOptions();
            List<string> labels = new List<string>();
            int defaultIndex = 2;

            for (int i = 0; i < gapOptions.Length; i++)
            {
                labels.Add(gapOptions[i].ToString("0.0"));
                if (Mathf.Approximately(gapOptions[i], GameSessionData.Instance.verticalGap))
                {
                    defaultIndex = i;
                }
            }

            verticalGapDropdown.AddOptions(labels);
            verticalGapDropdown.value = defaultIndex;
        }
    }

    private void OnSpawnRateChanged(float value)
    {
        if (spawnRateValueText != null)
        {
            spawnRateValueText.text = $"{value:0.00}s";
        }
    }

    public void StartGameFromPreferences()
    {
        if (GameSessionData.Instance != null)
        {
            string enteredName = playerNameInput != null ? playerNameInput.text.Trim() : "";
            GameSessionData.Instance.playerName = string.IsNullOrEmpty(enteredName) ? "Player" : enteredName;

            if (spawnRateSlider != null)
            {
                GameSessionData.Instance.spawnRate = spawnRateSlider.value;
            }

            if (verticalGapDropdown != null && verticalGapDropdown.value >= 0 && verticalGapDropdown.value < gapOptions.Length)
            {
                GameSessionData.Instance.verticalGap = gapOptions[verticalGapDropdown.value];
            }
        }

        SceneManager.LoadScene(gameSceneName);
    }
}