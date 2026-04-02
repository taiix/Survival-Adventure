using UnityEngine;

public abstract class ItemBase : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public int price;

    public float upgradeCostMultiplier = 1.5f;
    public float upgradeStatMultiplier = 1.2f;

    public int currentUpgradeLevel;

    public int GetUpgradeCost()
    {
        return Mathf.RoundToInt(price * upgradeCostMultiplier * (currentUpgradeLevel + 1));
    }

    public abstract void ApplyUpgrade();
}
