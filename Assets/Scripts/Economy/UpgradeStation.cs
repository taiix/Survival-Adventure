using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>A single tool upgrade entry, editable in the Inspector.</summary>
[Serializable]
public sealed class ToolUpgrade
{
    public string toolName = "Axe";
    [Min(1)] public int cost = 10;
    [Min(1)] public int maxLevel = 3;
    public int currentLevel = 1;

    public bool IsMaxLevel => currentLevel >= maxLevel;
}

/// <summary>
/// Place this on a city/shop GameObject. Players within the interaction radius
/// can purchase tool upgrades using the <see cref="Economy"/> component.
/// </summary>
public sealed class UpgradeStation : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField, Min(0f)] private float interactionRadius = 3f;

    [Header("Upgrades")]
    [SerializeField] private List<ToolUpgrade> availableUpgrades = new List<ToolUpgrade>();

    public IReadOnlyList<ToolUpgrade> AvailableUpgrades => availableUpgrades;

    public event Action<ToolUpgrade> OnUpgradePurchased;

    /// <summary>Returns true when the player is within the interaction radius.</summary>
    public bool IsPlayerInRange(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, playerTransform.position) <= interactionRadius;
    }

    /// <summary>
    /// Attempts to purchase the upgrade at <paramref name="upgradeIndex"/>.
    /// Returns true on success, false when the player cannot afford it or the
    /// tool is already at max level.
    /// </summary>
    public bool TryUpgrade(int upgradeIndex, Economy economy)
    {
        if (economy == null)
        {
            return false;
        }

        if (upgradeIndex < 0 || upgradeIndex >= availableUpgrades.Count)
        {
            return false;
        }

        ToolUpgrade upgrade = availableUpgrades[upgradeIndex];

        if (upgrade.IsMaxLevel)
        {
            return false;
        }

        if (!economy.SpendCoins(upgrade.cost))
        {
            return false;
        }

        upgrade.currentLevel++;
        OnUpgradePurchased?.Invoke(upgrade);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
