using UnityEngine.Events;

public static class EventManager
{
    // Stats Events
    public static UnityEvent<float> OnPlayerHealthChanged = new();
    public static UnityEvent<float> OnPlayerStaminaChanged = new();
}

public static class ItemEvents
{
    public static UnityEvent<WeaponItem> OnWeaponEquipped = new();
    public static UnityEvent<ArmorItem> OnArmorEquipped = new();
}