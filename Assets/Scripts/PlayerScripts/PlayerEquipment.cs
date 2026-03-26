using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private WeaponItem equippedWeapon;
    [SerializeField] private ArmorItem equippedArmor;

    public WeaponItem EquippedWeapon => equippedWeapon;
    public ArmorItem EquippedArmor => equippedArmor;

    private void Start()
    {
        EquipWeapon(equippedWeapon);
        EquipArmor(equippedArmor);
    }

    public void EquipWeapon(WeaponItem weapon)
    {
        equippedWeapon = weapon;
        ItemEvents.OnWeaponEquipped?.Invoke(weapon);
    }

    public void EquipArmor(ArmorItem armor)
    {
        equippedArmor = armor;
        ItemEvents.OnArmorEquipped?.Invoke(armor);
    }
}
