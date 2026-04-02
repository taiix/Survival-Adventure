using UnityEngine.Events;

public static class EventManager
{
    // Stats Events
    public static UnityEvent<float> OnPlayerHealthChanged = new();
    public static UnityEvent<float> OnPlayerStaminaChanged = new();
    public static UnityEvent<int> OnPlayerGoldChanged = new();
}

public static class ItemEvents
{
    public static UnityEvent<WeaponItem> OnWeaponEquipped = new();
    public static UnityEvent<ArmorItem> OnArmorEquipped = new();
}

public static class ShopEvents
{
    public static UnityEvent<ItemBase> OnSlotSelected = new();
    public static UnityEvent OnUpgradeCompleted = new();
}

public static class UpgradeEvents
{
    public static UnityEvent OnUpgradePurchased = new();
    public static UnityEvent<ItemBase> OnUpgradeApplied = new();
}