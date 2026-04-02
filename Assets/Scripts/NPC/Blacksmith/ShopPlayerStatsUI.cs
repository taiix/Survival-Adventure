using TMPro;
using UnityEngine;

public class ShopPlayerStatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gold;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI staminaText;

    private IPlayerStatsService playerStats;
    private bool isInitialized;

    private void OnEnable()
    {
        if (!isInitialized)
        {
            playerStats = ServiceLocator.GetPlayerStatsService();
            
            if (playerStats == null)
            {
                Debug.LogError("ShopPlayerStatsUI: PlayerStatsService not found in ServiceLocator!");
                return;
            }

            playerStats.OnGoldChanged.AddListener(UpdateGoldDisplay);
            isInitialized = true;
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
        }
    }

    private void RefreshAllStats()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("ShopPlayerStatsUI: Cannot refresh stats, PlayerStatsService is null!");
            return;
        }

        healthText.text = $"Health: {playerStats.GetMaxHealth()}";
        damageText.text = $"Damage: {playerStats.GetMinDamage()} - {playerStats.GetMaxDamage()}";
        attackSpeedText.text = $"Attack Speed: {playerStats.GetAttackSpeed()}";
        armorText.text = $"Armor: {playerStats.GetDefense()}";
        UpdateGoldDisplay(playerStats.GetGold());
    }

    private void UpdateGoldDisplay(int currentGold)
    {
        if (gold != null)
        {
            gold.text = $"Gold: {currentGold}";
        }
    }
}
