using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Runtime lighting tweaks – adjusts ambient intensity, fog, and optional
/// post-processing volume weight based on time of day or zone.
/// Addresses issue #46 (lighting tweaks).
/// </summary>
public class LightingTweaks : MonoBehaviour
{
    [Header("Ambient Light")]
    [SerializeField] private bool overrideAmbient = false;
    [SerializeField] private Color ambientColor = new Color(0.2f, 0.2f, 0.3f);
    [SerializeField, Range(0f, 8f)] private float ambientIntensity = 1f;

    [Header("Fog")]
    [SerializeField] private bool enableFog = true;
    [SerializeField] private Color fogColorDay = new Color(0.7f, 0.8f, 0.9f);
    [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;
    [SerializeField, Min(0f)] private float fogDensity = 0.008f;

    [Header("Directional Light")]
    [SerializeField] private Light mainDirectionalLight;
    [SerializeField, Range(0f, 8f)] private float lightIntensity = 1.2f;
    [SerializeField] private Color lightColor = Color.white;

    [Header("Post Processing (URP)")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField, Range(0f, 1f)] private float volumeWeight = 1f;

    private void OnEnable()
    {
        Apply();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Apply();
    }
#endif

    [ContextMenu("Apply Lighting")]
    public void Apply()
    {
        if (overrideAmbient)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = ambientIntensity;
        }

        RenderSettings.fog = enableFog;
        if (enableFog)
        {
            RenderSettings.fogColor = fogColorDay;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogDensity = fogDensity;
        }

        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.intensity = lightIntensity;
            mainDirectionalLight.color = lightColor;
        }

        if (postProcessVolume != null)
            postProcessVolume.weight = volumeWeight;
    }
}
