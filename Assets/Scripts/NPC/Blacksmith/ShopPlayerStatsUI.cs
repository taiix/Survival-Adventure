using TMPro;
using UnityEngine;

public class ShopPlayerStatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI defenseText;

    private IPlayerStatsService playerStats;
    private bool isInitialized;

    private void OnEnable()
    {
        if (!isInitialized)
        {
            InitializePlayerStats();
        }

        if (playerStats != null)
        {
            RefreshAllStats();
        }
    }

    private void OnDisable()
    {
        if (playerStats != null && isInitialized)
        {
            playerStats.OnGoldChanged.RemoveListener(UpdateGoldDisplay);
            playerStats.OnWeaponEquipped.RemoveListener(_ => RefreshAllStats());
            playerStats.OnArmorEquipped.RemoveListener(_ => RefreshAllStats());
        }
    }

    private void InitializePlayerStats()
    {
        playerStats = ServiceLocator.GetPlayerStatsService();

        if (playerStats == null)
        {
            Debug.LogError("ShopPlayerStatsUI: PlayerStatsService not found in ServiceLocator!");
            return;
        }

        playerStats.OnGoldChanged.AddListener(UpdateGoldDisplay);
        playerStats.OnWeaponEquipped.AddListener(_ => RefreshAllStats());
        playerStats.OnArmorEquipped.AddListener(_ => RefreshAllStats());

        isInitialized = true;
        Debug.Log("ShopPlayerStatsUI: Initialized successfully");
    }

    private void RefreshAllStats()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("ShopPlayerStatsUI: PlayerStatsService is null!");
            return;
        }

        UpdateHealthDisplay(playerStats.GetMaxHealth());
        UpdateDamageDisplay(playerStats.GetMinDamage(), playerStats.GetMaxDamage());
        UpdateAttackSpeedDisplay(playerStats.GetAttackSpeed());
        UpdateDefenseDisplay(playerStats.GetDefense());
        UpdateGoldDisplay(playerStats.GetGold());
    }

    private void UpdateGoldDisplay(int gold)
    {
        if (goldText != null)
        {
            goldText.text = $"<color=yellow><b>Gold:</b> {gold}</color>";
        }
    }

    private void UpdateHealthDisplay(int health)
    {
        if (healthText != null)
        {
            healthText.text = $"<b>Health:</b> {health}";
        }
    }

    private void UpdateDamageDisplay(int minDmg, int maxDmg)
    {
        if (damageText != null)
        {
            damageText.text = $"<b>Damage:</b> {minDmg} - {maxDmg}";
        }
    }

    private void UpdateAttackSpeedDisplay(float speed)
    {
        if (attackSpeedText != null)
        {
            attackSpeedText.text = $"<b>Attack Speed:</b> {speed:F2}";
        }
    }

    private void UpdateDefenseDisplay(int defense)
    {
        if (defenseText != null)
        {
            defenseText.text = $"<b>Defense:</b> {defense}";
        }
    }
}
