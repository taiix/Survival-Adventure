using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private WeaponItem equippedWeapon;
    [SerializeField] private ArmorItem equippedArmor;

    public WeaponItem EquippedWeapon => equippedWeapon;
    public ArmorItem EquippedArmor => equippedArmor;

    private void Start()
    {
        // Initialize equipped items on startup
        if (equippedWeapon != null)
        {
            EquipWeapon(equippedWeapon);
        }
        else
        {
            Debug.LogWarning("PlayerEquipment: No weapon equipped at startup!");
        }

        if (equippedArmor != null)
        {
            EquipArmor(equippedArmor);
        }
        else
        {
            Debug.LogWarning("PlayerEquipment: No armor equipped at startup!");
        }
    }

    public void EquipWeapon(WeaponItem weapon)
    {
        if (weapon == null)
        {
            Debug.LogWarning("PlayerEquipment: Trying to equip null weapon!");
            return;
        }

        equippedWeapon = weapon;
        ItemEvents.OnWeaponEquipped?.Invoke(weapon);
    }

    public void EquipArmor(ArmorItem armor)
    {
        if (armor == null)
        {
            Debug.LogWarning("PlayerEquipment: Trying to equip null armor!");
            return;
        }

        equippedArmor = armor;
        ItemEvents.OnArmorEquipped?.Invoke(armor);
    }
}
