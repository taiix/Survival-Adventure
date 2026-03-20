using UnityEngine;

[RequireComponent(typeof(Light))]
public sealed class DayNightCycle : MonoBehaviour
{
    [Header("Cycle Settings")]
    [SerializeField, Min(1f)] private float dayDuration = 120f;
    [SerializeField, Range(0f, 1f)] private float startTimeOfDay = 0.25f;

    [Header("Sun Light")]
    [SerializeField] private Gradient sunColor;
    [SerializeField] private AnimationCurve sunIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Ambient Light")]
    [SerializeField] private Gradient ambientColor;
    [SerializeField] private AnimationCurve ambientIntensity = AnimationCurve.Linear(0f, 0.2f, 1f, 1f);

    public float TimeOfDay { get; private set; }

    /// <summary>Returns true when the sun is above the horizon (between dawn and dusk).</summary>
    public bool IsDay => TimeOfDay >= 0.25f && TimeOfDay <= 0.75f;

    private Light sunLight;

    private void Awake()
    {
        sunLight = GetComponent<Light>();
        TimeOfDay = startTimeOfDay;
    }

    private void Update()
    {
        TimeOfDay = (TimeOfDay + Time.deltaTime / dayDuration) % 1f;
        ApplySunRotation();
        ApplyLighting();
    }

    private void ApplySunRotation()
    {
        float sunAngle = TimeOfDay * 360f - 90f;
        transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);
    }

    private void ApplyLighting()
    {
        // Normalised 0-1 position within the day window (dawn=0.25 → dusk=0.75)
        const float dawnTime = 0.25f;
        const float dayDurationFraction = 0.5f;
        float normalizedDayTime = IsDay ? (TimeOfDay - dawnTime) / dayDurationFraction : 0f;

        if (sunColor != null)
        {
            sunLight.color = sunColor.Evaluate(TimeOfDay);
        }

        sunLight.intensity = sunIntensity.Evaluate(normalizedDayTime);

        if (ambientColor != null)
        {
            RenderSettings.ambientLight = ambientColor.Evaluate(TimeOfDay);
        }

        RenderSettings.ambientIntensity = ambientIntensity.Evaluate(normalizedDayTime);
    }
}
