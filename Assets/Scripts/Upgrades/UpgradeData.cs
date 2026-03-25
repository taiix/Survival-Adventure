using UnityEngine;

/// <summary>
/// ScriptableObject defining a single upgrade option.
/// Addresses issues #13, #14, #27, #34 (upgrade types).
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Survival Adventure/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public enum UpgradeType
    {
        DamageBoost,
        AttackSpeedBoost,
        MaxHPBoost,
        DashCooldownReduction,
        DefenseBoost
    }

    [Header("Identity")]
    public string upgradeName = "Upgrade";
    [TextArea] public string description;
    public Sprite icon;
    public UpgradeType upgradeType;

    [Header("Cost & Value")]
    [Min(1)] public int goldCost = 50;
    [Min(0.01f)] public float upgradeValue = 5f;

    [Header("Levels")]
    [Min(1)] public int maxLevel = 3;
}
