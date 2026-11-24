using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [SerializeField] private TMP_Text instructionsText;
    [SerializeField] private string preferencesSceneName = "Preferences";
    [SerializeField] private string gameSceneName = "FlappyBird";

    private void Start()
    {
        Time.timeScale = 1f;
        if (instructionsText != null)
        {
            instructionsText.text =
                "FLAPPY BIRD\n\n" +
                "Controls:\n" +
                "• Press SPACE or Left-Click Mouse to flap upward.\n" +
                "• Press P during gameplay to Pause or Resume.\n" +
                "• Use the Exit/Stop button anytime to end early and save stats.\n\n" +
                "Fly through the pipe gaps to score points without hitting obstacles.";
        }
    }

    public void GoToPreferences()
    {
        SceneManager.LoadScene(preferencesSceneName);
    }

    public void StartGameDirectly()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}