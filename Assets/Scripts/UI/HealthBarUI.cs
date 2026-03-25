using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the player's health bar in the HUD.
/// Addresses issue #54 (health bar).
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.3f;

    private PlayerHealth playerHealth;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateBar;
            UpdateBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateBar;
    }

    private void UpdateBar(float current, float max)
    {
        float ratio = max > 0f ? current / max : 0f;

        if (healthSlider != null)
            healthSlider.value = ratio;

        if (fillImage != null)
            fillImage.color = ratio <= lowHealthThreshold
                ? lowHealthColor
                : Color.Lerp(lowHealthColor, fullHealthColor, (ratio - lowHealthThreshold) / (1f - lowHealthThreshold));
    }
}
