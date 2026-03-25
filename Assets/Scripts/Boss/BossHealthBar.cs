using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the boss's health bar in the HUD.
/// Addresses issues #42 (boss health bar).
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private UnityEngine.UI.Image fillImage;
    [SerializeField] private Color healthyColor = Color.red;
    [SerializeField] private Color criticalColor = new Color(1f, 0.4f, 0f);

    private KnightBossEnemy trackedBoss;

    private void Update()
    {
        if (trackedBoss == null) return;

        // Access health via reflection-free approach: BaseEnemy exposes these via protected fields.
        // We use the public accessor added below.
        float ratio = trackedBoss.HealthRatio;
        if (healthSlider != null)
            healthSlider.value = ratio;

        if (fillImage != null)
            fillImage.color = Color.Lerp(criticalColor, healthyColor, ratio);
    }

    public void SetBoss(KnightBossEnemy boss)
    {
        trackedBoss = boss;
        if (healthSlider != null)
            healthSlider.value = 1f;
    }
}
