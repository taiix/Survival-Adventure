using UnityEngine;

/// <summary>
/// Day / night cycle controller.
/// Rotates a directional light and blends ambient/fog colours over a configurable day length.
/// Addresses issue #62 (day/night cycle, optional).
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Min(10f)] private float dayLengthSeconds = 300f;
    [SerializeField, Range(0f, 1f)] private float startTimeOfDay = 0.25f; // 0=midnight, 0.5=noon

    [Header("Sun")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Gradient sunColor;
    [SerializeField] private AnimationCurve sunIntensity;

    [Header("Ambient")]
    [SerializeField] private Gradient ambientColor;

    [Header("Fog")]
    [SerializeField] private bool controlFog = true;
    [SerializeField] private Gradient fogColor;

    [Header("Music")]
    [SerializeField] private bool switchMusicAtNight = false;
    [SerializeField] private AudioClip nightMusic;

    private float currentTime;
    private bool isNight;

    private void Start()
    {
        currentTime = startTimeOfDay;
    }

    private void Update()
    {
        currentTime += Time.deltaTime / dayLengthSeconds;
        if (currentTime >= 1f) currentTime -= 1f;

        UpdateSun();
        UpdateAmbient();
        if (controlFog) UpdateFog();
        HandleNightMusic();
    }

    private void UpdateSun()
    {
        if (sunLight == null) return;

        // Rotate so that 0.25 = sunrise (east), 0.5 = noon (overhead), 0.75 = sunset
        float angle = currentTime * 360f;
        sunLight.transform.localRotation = Quaternion.Euler(angle - 90f, 170f, 0f);

        if (sunColor != null)
            sunLight.color = sunColor.Evaluate(currentTime);

        if (sunIntensity != null)
            sunLight.intensity = sunIntensity.Evaluate(currentTime);
    }

    private void UpdateAmbient()
    {
        if (ambientColor != null)
            RenderSettings.ambientLight = ambientColor.Evaluate(currentTime);
    }

    private void UpdateFog()
    {
        if (fogColor != null)
            RenderSettings.fogColor = fogColor.Evaluate(currentTime);
    }

    private void HandleNightMusic()
    {
        if (!switchMusicAtNight) return;

        bool nowNight = currentTime < 0.2f || currentTime > 0.8f;
        if (nowNight != isNight)
        {
            isNight = nowNight;
            // Night/day music transitions – assign night/day clips in the inspector
            // and call AudioManager directly. Falling back to no-op if not set.
            if (isNight)
                AudioManager.Instance?.PlayMusic(nightMusic);
            else
                AudioManager.Instance?.PlayForest();
        }
    }

    /// <summary>0–1 representing the current time of day.</summary>
    public float TimeOfDay => currentTime;

    /// <summary>True when the sun is below the horizon.</summary>
    public bool IsNight => currentTime < 0.2f || currentTime > 0.8f;
}
