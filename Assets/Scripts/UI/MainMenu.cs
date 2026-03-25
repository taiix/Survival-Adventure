using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main menu controller with Play, Settings, and Quit buttons.
/// Addresses issue #57 (main menu).
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private string gameSceneName = "SampleScene";

    private void Start()
    {
        if (playButton != null)     playButton.onClick.AddListener(Play);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (quitButton != null)     quitButton.onClick.AddListener(Quit);

        if (settingsPanel != null)  settingsPanel.SetActive(false);

        AudioManager.Instance?.PlayMainMenu();
    }

    private void Play()
    {
        AudioManager.Instance?.PlayUIClick();
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }

    private void OpenSettings()
    {
        AudioManager.Instance?.PlayUIClick();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    private void Quit()
    {
        AudioManager.Instance?.PlayUIClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
