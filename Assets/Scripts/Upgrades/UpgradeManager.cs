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

        ApplyUpgrade(data);
        AudioManager.Instance?.PlayLevelUp();
        OnUpgradePurchased?.Invoke(data, newLevel);
        return true;
    }

    private void ApplyUpgrade(UpgradeData data)
    {
        PlayerStats stats = PlayerStats.Instance;
        if (stats == null) return;

        switch (data.upgradeType)
        {
            case UpgradeData.UpgradeType.DamageBoost:
                stats.AddDamageBoost(data.upgradeValue);
                break;
            case UpgradeData.UpgradeType.AttackSpeedBoost:
                stats.AddAttackSpeedBoost(1f + data.upgradeValue / 100f);
                break;
            case UpgradeData.UpgradeType.MaxHPBoost:
                stats.AddMaxHealthBoost(data.upgradeValue);
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                player?.GetComponent<PlayerHealth>()?.Heal(data.upgradeValue);
                break;
            case UpgradeData.UpgradeType.DashCooldownReduction:
                stats.ReduceDashCooldown(data.upgradeValue);
                break;
            case UpgradeData.UpgradeType.DefenseBoost:
                stats.AddDefenseBoost(data.upgradeValue / 100f);
                break;
        }
    }
}
