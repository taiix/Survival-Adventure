using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's purchased upgrades.
/// Addresses issues #13, #27, #34 (weapon upgrade logic, upgrade types).
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private readonly Dictionary<UpgradeData, int> purchasedLevels = new Dictionary<UpgradeData, int>();

    private PlayerHealth cachedPlayerHealth;
    private PlayerStats cachedPlayerStats;

    public event Action<UpgradeData, int> OnUpgradePurchased;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void CachePlayerComponents()
    {
        if (cachedPlayerHealth != null && cachedPlayerStats != null) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        cachedPlayerHealth = player.GetComponent<PlayerHealth>();
        cachedPlayerStats  = player.GetComponent<PlayerStats>();
    }

    public int GetLevel(UpgradeData data)
    {
        return purchasedLevels.TryGetValue(data, out int lvl) ? lvl : 0;
    }

    public bool IsMaxLevel(UpgradeData data) => GetLevel(data) >= data.maxLevel;

    /// <summary>
    /// Attempts to purchase one level of the given upgrade.
    /// Returns true on success.
    /// </summary>
    public bool TryPurchase(UpgradeData data)
    {
        if (data == null) return false;
        if (IsMaxLevel(data)) return false;
        if (!GoldManager.Instance.TrySpend(data.goldCost)) return false;

        int newLevel = GetLevel(data) + 1;
        purchasedLevels[data] = newLevel;

        CachePlayerComponents();
        ApplyUpgrade(data);
        AudioManager.Instance?.PlayLevelUp();
        OnUpgradePurchased?.Invoke(data, newLevel);
        return true;
    }

    private void ApplyUpgrade(UpgradeData data)
    {
        if (cachedPlayerStats == null) return;

        switch (data.upgradeType)
        {
            case UpgradeData.UpgradeType.DamageBoost:
                cachedPlayerStats.AddDamageBoost(data.upgradeValue);
                break;
            case UpgradeData.UpgradeType.AttackSpeedBoost:
                cachedPlayerStats.AddAttackSpeedBoost(1f + data.upgradeValue / 100f);
                break;
            case UpgradeData.UpgradeType.MaxHPBoost:
                cachedPlayerStats.AddMaxHealthBoost(data.upgradeValue);
                // Increase the health component's max and heal the player for the added amount
                if (cachedPlayerHealth != null)
                {
                    cachedPlayerHealth.SetMaxHealth(cachedPlayerStats.TotalMaxHealth);
                    cachedPlayerHealth.Heal(data.upgradeValue);
                }
                break;
            case UpgradeData.UpgradeType.DashCooldownReduction:
                cachedPlayerStats.ReduceDashCooldown(data.upgradeValue);
                break;
            case UpgradeData.UpgradeType.DefenseBoost:
                cachedPlayerStats.AddDefenseBoost(data.upgradeValue / 100f);
                break;
        }
    }
}
