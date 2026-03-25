using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple singleton AudioManager.
/// Handles one-shot SFX and looping music tracks.
/// Addresses issues #49 (sword swings), #50 (city theme), #51 (boss theme),
/// #52 (enemy hit), #58 (UI clicks).
/// </summary>
public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip swordSwingClip;
    [SerializeField] private AudioClip enemyHitClip;
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip itemPickupClip;
    [SerializeField] private AudioClip uiClickClip;
    [SerializeField] private AudioClip chestOpenClip;
    [SerializeField] private AudioClip levelUpClip;
    [SerializeField] private AudioClip purchaseClip;

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip cityTheme;
    [SerializeField] private AudioClip forestTheme;
    [SerializeField] private AudioClip bossTheme;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;

    private AudioClip currentMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ── SFX ────────────────────────────────────────────────────────────────

    public void PlaySwordSwing() => PlaySFX(swordSwingClip);
    public void PlayEnemyHit()   => PlaySFX(enemyHitClip);
    public void PlayPlayerHit()  => PlaySFX(playerHitClip);
    public void PlayItemPickup() => PlaySFX(itemPickupClip);
    public void PlayUIClick()    => PlaySFX(uiClickClip);
    public void PlayChestOpen()  => PlaySFX(chestOpenClip);
    public void PlayLevelUp()    => PlaySFX(levelUpClip);
    public void PlayPurchase()   => PlaySFX(purchaseClip);

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ── Music ───────────────────────────────────────────────────────────────

    public void PlayMainMenu()  => PlayMusic(mainMenuMusic);
    public void PlayCityTheme() => PlayMusic(cityTheme);
    public void PlayForest()    => PlayMusic(forestTheme);
    public void PlayBossTheme() => PlayMusic(bossTheme);
    public void StopMusic()     => musicSource?.Stop();

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        if (currentMusic == clip && musicSource.isPlaying) return;
        currentMusic = clip;
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    // ── Settings ────────────────────────────────────────────────────────────

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public float SFXVolume   => sfxVolume;
    public float MusicVolume => musicVolume;
}
