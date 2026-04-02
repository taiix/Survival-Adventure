using UnityEngine;

[CreateAssetMenu(fileName = "ArmorItem", menuName = "Scriptable Objects/ArmorItem")]
public class ArmorItem : ItemBase
{
    [Space]
    [Header("Armor Specific")]
    public int defense;
    public int maxHealthBonus;

    public override void ApplyUpgrade()
    {
        currentUpgradeLevel++;
        defense = Mathf.RoundToInt(defense * upgradeStatMultiplier);
        maxHealthBonus = Mathf.RoundToInt(maxHealthBonus * upgradeStatMultiplier);
    }
}
