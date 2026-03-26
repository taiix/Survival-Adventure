using TMPro;
using UnityEngine;

public class ShopPlayerStatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;

    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI staminaText;

    private PlayerStats playerStats;

    private void OnEnable()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();

        healthText.text = $"Health {playerStats.GetMaxHealth().ToString()}";
        damageText.text = $"Damage: {playerStats.GetMinDamage().ToString()} - {playerStats.GetMaxDamage().ToString()}";
        attackSpeedText.text = $"Attack Speed: {playerStats.GetAttackSpeed().ToString()}";
        armorText.text = $"Armor: {playerStats.GetDefense().ToString()}";
        staminaText.text = $"Stamina: {playerStats.GetCurrentDamage().ToString()}";
    }
}
