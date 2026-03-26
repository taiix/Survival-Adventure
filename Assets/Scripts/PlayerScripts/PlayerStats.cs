using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int minDamage;
    [SerializeField] private int maxDamage;
    [SerializeField] private float attackSpeed;

    [SerializeField] private int defense;
    [SerializeField] private int maxHealth;

    public int GetMinDamage() => minDamage;
    public int GetMaxDamage() => maxDamage;
    public float GetAttackSpeed() => attackSpeed;
    public int GetDefense() => defense;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentDamage() => Random.Range(minDamage, maxDamage + 1);

    private void OnEnable()
    {
        ItemEvents.OnWeaponEquipped.AddListener(UpdateWeaponStats);
        ItemEvents.OnArmorEquipped.AddListener(UpdateArmorStats);
    }

    private void OnDisable()
    {
        ItemEvents.OnWeaponEquipped.RemoveListener(UpdateWeaponStats);
        ItemEvents.OnArmorEquipped.RemoveListener(UpdateArmorStats);
    }

    private void UpdateWeaponStats(WeaponItem weapon)
    {
        minDamage = weapon.minDamage;
        maxDamage = weapon.maxDamage;

        attackSpeed = weapon.attackSpeed;
    }

    private void UpdateArmorStats(ArmorItem armor)
    {
        defense = armor.defense;
        maxHealth = armor.maxHealthBonus;
    }
}
