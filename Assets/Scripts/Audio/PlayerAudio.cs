using UnityEngine;

/// <summary>
/// Hooks into the player's attack and plays the sword-swing sound.
/// Attach to the same GameObject as PlayerAttack.
/// Addresses issue #49 (sword swings audio).
/// </summary>
[RequireComponent(typeof(PlayerAttack))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Optional Overrides (leave empty to use AudioManager defaults)")]
    [SerializeField] private AudioClip swordSwingOverride;
    [SerializeField] private AudioClip hurtOverride;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    /// <summary>Called by Animation Event on the sword-swing frame.</summary>
    public void OnSwordSwing()
    {
        if (swordSwingOverride != null)
            AudioManager.Instance?.PlaySFX(swordSwingOverride);
        else
            AudioManager.Instance?.PlaySwordSwing();
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (hurtOverride != null)
            AudioManager.Instance?.PlaySFX(hurtOverride);
        else
            AudioManager.Instance?.PlayPlayerHit();
    }
}
