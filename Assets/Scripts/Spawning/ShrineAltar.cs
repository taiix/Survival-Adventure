using UnityEngine;

/// <summary>
/// Interactable shrine or altar. Grants the player a blessing (heal or stat buff)
/// and requires gold or a cooldown.
/// Addresses issue #20 (spawn shrines / altars).
/// </summary>
public class ShrineAltar : MonoBehaviour, IInteractable
{
    public enum BlessingType { HealPlayer, BoostDamage, BoostDefense, FullHeal }

    [Header("Identity")]
    [SerializeField] private string shrineName = "Ancient Shrine";
    [SerializeField] private BlessingType blessingType = BlessingType.HealPlayer;
    [SerializeField] private float blessingValue = 30f;

    [Header("Cost & Cooldown")]
    [SerializeField, Min(0)] private int goldCost = 0;
    [SerializeField, Min(0f)] private float cooldown = 60f;

    [Header("Visual")]
    [SerializeField] private GameObject activatedEffect;

    private float lastUsedTime = -9999f;

    public string DisplayName => shrineName;
    public bool CanInteract => Time.time - lastUsedTime >= cooldown;

    public void OnInteract()
    {
        if (!CanInteract) return;
        if (goldCost > 0 && (GoldManager.Instance == null || !GoldManager.Instance.TrySpend(goldCost))) return;

        lastUsedTime = Time.time;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerStats stats = player.GetComponent<PlayerStats>();

        switch (blessingType)
        {
            case BlessingType.HealPlayer:
                health?.Heal(blessingValue);
                break;
            case BlessingType.FullHeal:
                if (health != null) health.Heal(health.MaxHealth);
                break;
            case BlessingType.BoostDamage:
                stats?.AddDamageBoost(blessingValue);
                break;
            case BlessingType.BoostDefense:
                stats?.AddDefenseBoost(blessingValue / 100f);
                break;
        }

        AudioManager.Instance?.PlayLevelUp();

        if (activatedEffect != null)
            activatedEffect.SetActive(true);
    }
}
