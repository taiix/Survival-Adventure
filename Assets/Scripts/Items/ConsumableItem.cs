using UnityEngine;

/// <summary>
/// A consumable item in the player's quick-use slot.
/// Addresses issues #36 (consumable items).
/// </summary>
public class ConsumableItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;

    public ItemData Data => itemData;
    public int Quantity => quantity;

    public bool TryUse(PlayerHealth target)
    {
        if (quantity <= 0 || itemData == null || target == null) return false;

        target.Heal(itemData.consumableHealAmount);
        quantity--;
        AudioManager.Instance?.PlayItemPickup();
        return true;
    }

    public void AddQuantity(int amount)
    {
        quantity += Mathf.Max(0, amount);
    }
}
