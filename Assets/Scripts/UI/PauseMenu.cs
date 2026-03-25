using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause menu controller. Pauses/resumes the game and provides navigation buttons.
/// Addresses issue #56 (pause menu).
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        if (resumeButton != null)   resumeButton.onClick.AddListener(Resume);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else          Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        AudioManager.Instance?.PlayUIClick();
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)   pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        AudioManager.Instance?.PlayUIClick();
    }

    private void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        AudioManager.Instance?.PlayUIClick();
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.PlayUIClick();
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }
}
