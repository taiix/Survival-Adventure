using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Settings menu with volume, resolution, and graphics quality controls.
/// Addresses issue #48 (settings menu).
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    [Header("Volume Controls")]
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TMP_Text sfxVolumeLabel;
    [SerializeField] private TMP_Text musicVolumeLabel;

    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("Navigation")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button applyButton;

    private void OnEnable()
    {
        // Initialise sliders with current values
        if (AudioManager.Instance != null)
        {
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.SFXVolume;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.MusicVolume;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        if (closeButton != null)  closeButton.onClick.AddListener(Close);
        if (applyButton != null)  applyButton.onClick.AddListener(Apply);

        RefreshLabels();
    }

    private void OnDisable()
    {
        sfxVolumeSlider?.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        musicVolumeSlider?.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        qualityDropdown?.onValueChanged.RemoveListener(OnQualityChanged);
        closeButton?.onClick.RemoveListener(Close);
        applyButton?.onClick.RemoveListener(Apply);
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        RefreshLabels();
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
        RefreshLabels();
    }

    private void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }

    private void RefreshLabels()
    {
        if (sfxVolumeLabel != null && sfxVolumeSlider != null)
            sfxVolumeLabel.text = $"SFX: {(sfxVolumeSlider.value * 100f):0}%";
        if (musicVolumeLabel != null && musicVolumeSlider != null)
            musicVolumeLabel.text = $"Music: {(musicVolumeSlider.value * 100f):0}%";
    }

    private void Apply()
    {
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider != null ? musicVolumeSlider.value : 0.6f);
        PlayerPrefs.Save();
        AudioManager.Instance?.PlayUIClick();
    }

    private void Close()
    {
        AudioManager.Instance?.PlayUIClick();
        gameObject.SetActive(false);
    }
}
