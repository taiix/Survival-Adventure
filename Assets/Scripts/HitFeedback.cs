using System.Collections;
using UnityEngine;

/// <summary>
/// Handles visual hit feedback by flashing an entity red and fading back to original color.
/// Optionally plays a particle effect on hit.
/// Can be attached to any entity that needs hit feedback.
/// </summary>
public class HitFeedback : MonoBehaviour
{
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Optional Effect")]
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private Vector3 effectSpawnOffset = Vector3.zero;
    [SerializeField] private float effectLifetime = 1f;

    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Color[] originalColors;
    private Coroutine currentFeedbackCoroutine;

    private void Awake()
    {
        CacheRendererData();
    }

    private void CacheRendererData()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            // Create a new material instance for this object so changes don't affect the shared material
            originalMaterials[i] = renderers[i].material;
            originalColors[i] = originalMaterials[i].color;
        }
    }

    /// <summary>
    /// Play the hit feedback animation and optional effect.
    /// </summary>
    public void PlayHitFeedback()
    {
        // Stop any ongoing feedback coroutine
        if (currentFeedbackCoroutine != null)
        {
            StopCoroutine(currentFeedbackCoroutine);
        }

        // Spawn optional effect
        if (effectPrefab != null)
        {
            SpawnEffect();
        }

        currentFeedbackCoroutine = StartCoroutine(HitFeedbackCoroutine());
    }

    private void SpawnEffect()
    {
        Vector3 spawnPosition = transform.position + effectSpawnOffset;
        GameObject effectInstance = Instantiate(
            effectPrefab,
            spawnPosition,
            Quaternion.identity);

        // Auto-destroy the effect after its lifetime
        Destroy(effectInstance, effectLifetime);
    }

    private IEnumerator HitFeedbackCoroutine()
    {
        // Flash red
        SetAllMaterialColors(hitColor);
        yield return new WaitForSeconds(flashDuration);

        // Fade back to original color
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;

            for (int i = 0; i < renderers.Length; i++)
            {
                Color fadeColor = Color.Lerp(hitColor, originalColors[i], progress);
                originalMaterials[i].color = fadeColor;
            }

            yield return null;
        }

        // Ensure we end at the original color
        SetAllMaterialColors(originalColors);
        currentFeedbackCoroutine = null;
    }

    private void SetAllMaterialColors(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i].color = color;
        }
    }

    private void SetAllMaterialColors(Color[] colors)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (i < colors.Length)
            {
                originalMaterials[i].color = colors[i];
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up material instances
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            if (originalMaterials[i] != null)
            {
                Destroy(originalMaterials[i]);
            }
        }
    }
}