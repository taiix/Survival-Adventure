using UnityEngine;

/// <summary>
/// ScriptableObject that defines a game item.
/// Addresses issues #32, #59 (item pickup).
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Survival Adventure/Item Data")]
public class ItemData : ScriptableObject
{
    public enum ItemType { Gold, Healing, Material, Consumable, Artifact }

    [Header("Identity")]
    public string itemName = "Item";
    [TextArea] public string description;
    public Sprite icon;
    public ItemType itemType;

    [Header("Values")]
    [Min(0)] public int goldValue = 5;
    [Min(0f)] public float healAmount = 0f;
    [Min(1)] public int stackSize = 1;

    [Header("Consumable Effect")]
    [Tooltip("Used when itemType == Consumable")]
    public float consumableHealAmount = 30f;
}
