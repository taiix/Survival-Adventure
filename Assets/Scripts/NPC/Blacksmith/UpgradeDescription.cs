using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeDescription : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;

    private IUpgradeService upgradeService;
    private IInputService inputService;
    private ItemBase currentItem;
    private InputAction upgradeInputAction;
    private bool isInitialized;

    private void Start()
    {
        if (descriptionText == null)
        {
            Debug.LogError("UpgradeDescription: descriptionText is not assigned in the inspector!");
        }
    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            upgradeService = ServiceLocator.GetUpgradeService();
            inputService = ServiceLocator.GetInputService();
            
            if (upgradeService == null)
            {
                Debug.LogWarning("UpgradeDescription: UpgradeService not available yet.");
                return;
            }

            if (inputService == null)
            {
                Debug.LogWarning("UpgradeDescription: InputService not available yet.");
                return;
            }

            upgradeInputAction = inputService.GetInputAction("Upgrade");
            if (upgradeInputAction != null)
            {
                Debug.Log("UpgradeDescription: Got 'Upgrade' input action from InputService");
            }
            else
            {
                Debug.LogError("UpgradeDescription: 'Upgrade' action not found in InputService!");
            }

            isInitialized = true;
            Debug.Log("UpgradeDescription: Successfully initialized");
        }

        if (currentItem != null)
        {
            Refresh();
            Debug.Log($"UpgradeDescription: Refreshed with item {currentItem.itemName}");
        }
    }

    private void Update()
    {
        if (upgradeInputAction == null || !isInitialized || currentItem == null)
            return;

        if (upgradeInputAction.WasPerformedThisFrame())
        {
            Debug.Log("UpgradeDescription: Upgrade input detected!");
            AttemptUpgrade();
        }
    }

    public void SetCurrentItem(ItemBase item)
    {
        if (item == null)
        {
            Debug.LogWarning("UpgradeDescription: Trying to set null item!");
            return;
        }

        currentItem = item;
        Debug.Log($"UpgradeDescription: SetCurrentItem called with {item.itemName}");
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

        if (descriptionText == null)
        {
            Debug.LogError("UpgradeDescription: descriptionText is null!");
            return;
        }

        string description = $"<b>Level:</b> {currentItem.currentUpgradeLevel}\n\n";
        description += GetItemStatsText(currentItem, true);
        description += $"\n\n<b>Cost:</b> {currentItem.GetUpgradeCost()} Gold";

        descriptionText.text = description;
        Debug.Log($"UpgradeDescription: Display updated for {currentItem.itemName}");
    }

    private string GetItemStatsText(ItemBase item, bool upgraded)
    {
        if (item is WeaponItem weapon)
            return GetWeaponStatsText(weapon, upgraded);
        
        if (item is ArmorItem armor)
            return GetArmorStatsText(armor, upgraded);

        return "Unknown item type";
    }

    private string GetWeaponStatsText(WeaponItem weapon, bool upgraded)
    {
        int currentMinDmg = weapon.minDamage;
        int currentMaxDmg = weapon.maxDamage;
        int upgradedMinDmg = Mathf.RoundToInt(currentMinDmg * weapon.upgradeStatMultiplier);
        int upgradedMaxDmg = Mathf.RoundToInt(currentMaxDmg * weapon.upgradeStatMultiplier);

        if (!upgraded)
        {
            string stats = $"<b>Damage:</b> {currentMinDmg} - {currentMaxDmg}\n";
            stats += $"<b>Attack Speed:</b> {weapon.attackSpeed}";
            return stats;
        }

        string upgradedStats = $"<b>Min Damage:</b> {upgradedMinDmg}";
        if (upgradedMinDmg != currentMinDmg)
            upgradedStats += $" <color=green>(+{upgradedMinDmg - currentMinDmg})</color>";
        upgradedStats += "\n";

        upgradedStats += $"<b>Max Damage:</b> {upgradedMaxDmg}";
        if (upgradedMaxDmg != currentMaxDmg)
            upgradedStats += $" <color=green>(+{upgradedMaxDmg - currentMaxDmg})</color>";
        upgradedStats += "\n";

        upgradedStats += $"<b>Attack Speed:</b> {weapon.attackSpeed}";

        return upgradedStats;
    }

    private string GetArmorStatsText(ArmorItem armor, bool upgraded)
    {
        int currentDef = armor.defense;
        int currentHealth = armor.maxHealthBonus;
        int upgradedDef = Mathf.RoundToInt(currentDef * armor.upgradeStatMultiplier);
        int upgradedHealth = Mathf.RoundToInt(currentHealth * armor.upgradeStatMultiplier);

        if (!upgraded)
        {
            string stats = $"<b>Defense:</b> {currentDef}\n";
            stats += $"<b>Max Health Bonus:</b> {currentHealth}";

            return stats;
        }

        string upgradedStats = $"<b>Defense:</b> {upgradedDef}";
        if (upgradedDef != currentDef)
            upgradedStats += $" <color=green>(+{upgradedDef - currentDef})</color>";
        upgradedStats += "\n";

        upgradedStats += $"<b>Max Health Bonus:</b> {upgradedHealth}";
        if (upgradedHealth != currentHealth)
            upgradedStats += $" <color=green>(+{upgradedHealth - currentHealth})</color>";

        return upgradedStats;
    }

    private void ClearDisplay()
    {
        currentItem = null;
        if (descriptionText != null)
        {
            descriptionText.text = string.Empty;
        }
    }
}
