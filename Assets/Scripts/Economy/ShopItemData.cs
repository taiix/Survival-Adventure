using UnityEngine;

/// <summary>
/// ScriptableObject defining an item available in the market shop.
/// Addresses issues #35, #36, #37 (market UI, consumables, purchase logic).
/// </summary>
[CreateAssetMenu(fileName = "NewShopItem", menuName = "Survival Adventure/Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemName = "Shop Item";
    [TextArea] public string description;
    public Sprite icon;

    [Header("Purchase")]
    [Min(1)] public int goldCost = 25;
    [Min(1)] public int quantityPerPurchase = 1;

    [Header("Effect")]
    public ItemData.ItemType itemType;
    [Tooltip("Heal amount for healing items")]
    public float healAmount = 30f;
}
