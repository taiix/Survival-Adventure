using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Displays upgrade information and handles upgrade purchase input.
/// Shows item stats, costs, and level progression.
/// </summary>
public class UpgradeDescription : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI levelText;

    private IUpgradeService upgradeService;
    private IInputService inputService;
    private ItemBase currentItem;
    private InputAction upgradeInputAction;
    private bool isInitialized;

    private void OnEnable()
    {
        if (!isInitialized)
        {
            InitializeServices();
        }

        if (currentItem != null)
        {
            Refresh();
        }
    }

    private void InitializeServices()
    {
        upgradeService = ServiceLocator.GetUpgradeService();
        inputService = ServiceLocator.GetInputService();

        if (upgradeService == null)
        {
            Debug.LogError("UpgradeDescription: UpgradeService not found!");
            return;
        }

        if (inputService == null)
        {
            Debug.LogError("UpgradeDescription: InputService not found!");
            return;
        }

        upgradeInputAction = inputService.GetInputAction("Upgrade");

        if (upgradeInputAction == null)
        {
            Debug.LogError("UpgradeDescription: 'Upgrade' input action not found!");
            return;
        }

        isInitialized = true;
        Debug.Log("UpgradeDescription: Services initialized successfully");
    }

    private void Update()
    {
        if (!isInitialized || upgradeInputAction == null || currentItem == null)
            return;

        if (upgradeInputAction.WasPerformedThisFrame())
        {
            AttemptUpgrade();
        }
    }

    public void SetCurrentItem(ItemBase item)
    {
        if (item == null)
        {
            Debug.LogWarning("UpgradeDescription: Attempted to set null item!");
            return;
        }

        currentItem = item;
        Refresh();
    }

    private void AttemptUpgrade()
    {
        if (upgradeService == null || currentItem == null)
            return;

        if (upgradeService.TryUpgradeItem(currentItem))
        {
            Debug.Log($"✓ Successfully upgraded {currentItem.itemName}!");
            Refresh();
        }
        else
        {
            Debug.LogWarning("✗ Upgrade failed: Not enough gold");
        }
    }

    private void Refresh()
    {
        if (currentItem == null)
        {
            ClearDisplay();
            return;
        }

        UpdateLevelDisplay();
        UpdateStatsDisplay();
        UpdateCostDisplay();
    }

    private void UpdateLevelDisplay()
    {
        if (levelText != null)
        {
            levelText.text = $"<b>Level:</b> {currentItem.currentUpgradeLevel}";
        }
    }

    private void UpdateStatsDisplay()
    {
        if (descriptionText == null)
        {
            Debug.LogError("UpgradeDescription: descriptionText not assigned!");
            return;
        }

        string statsText = GetItemStatsText(currentItem);
        descriptionText.text = statsText;
    }

    private void UpdateCostDisplay()
    {
        if (costText != null)
        {
            int cost = currentItem.GetUpgradeCost();
            costText.text = $"<b>Cost:</b> <color=yellow>{cost} Gold</color>";
        }
    }

    private string GetItemStatsText(ItemBase item)
    {
        if (item is WeaponItem weapon)
            return GetWeaponStatsText(weapon);
        
        if (item is ArmorItem armor)
            return GetArmorStatsText(armor);

        return "<color=red>Unknown item type</color>";
    }

    private string GetWeaponStatsText(WeaponItem weapon)
    {
        int currentMinDmg = weapon.minDamage;
        int currentMaxDmg = weapon.maxDamage;
        int upgradedMinDmg = Mathf.RoundToInt(currentMinDmg * weapon.upgradeStatMultiplier);
        int upgradedMaxDmg = Mathf.RoundToInt(currentMaxDmg * weapon.upgradeStatMultiplier);

        string stats = $"<b>Current Damage:</b> {currentMinDmg} - {currentMaxDmg}\n";
        stats += $"<b>Attack Speed:</b> {weapon.attackSpeed}\n\n";

        stats += $"<b>Next Level Damage:</b> {upgradedMinDmg} - {upgradedMaxDmg}";
        if (upgradedMinDmg != currentMinDmg || upgradedMaxDmg != currentMaxDmg)
        {
            stats += $" <color=green>(+{upgradedMinDmg - currentMinDmg} / +{upgradedMaxDmg - currentMaxDmg})</color>";
        }

        return stats;
    }

    private string GetArmorStatsText(ArmorItem armor)
    {
        int currentDef = armor.defense;
        int currentHealth = armor.maxHealthBonus;
        int upgradedDef = Mathf.RoundToInt(currentDef * armor.upgradeStatMultiplier);
        int upgradedHealth = Mathf.RoundToInt(currentHealth * armor.upgradeStatMultiplier);

        string stats = $"<b>Current Defense:</b> {currentDef}\n";
        stats += $"<b>Current Health Bonus:</b> {currentHealth}\n\n";

        stats += $"<b>Next Level Defense:</b> {upgradedDef}";
        if (upgradedDef != currentDef)
        {
            stats += $" <color=green>(+{upgradedDef - currentDef})</color>";
        }
        stats += "\n";

        stats += $"<b>Next Level Health:</b> {upgradedHealth}";
        if (upgradedHealth != currentHealth)
        {
            stats += $" <color=green>(+{upgradedHealth - currentHealth})</color>";
        }

        return stats;
    }

    private void ClearDisplay()
    {
        currentItem = null;
        if (descriptionText != null) descriptionText.text = string.Empty;
        if (costText != null) costText.text = string.Empty;
        if (levelText != null) levelText.text = string.Empty;
    }
}
