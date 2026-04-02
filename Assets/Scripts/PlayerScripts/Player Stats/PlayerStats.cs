using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour, IPlayerStatsService
{
    [SerializeField] private int minDamage;
    [SerializeField] private int maxDamage;
    [SerializeField] private float attackSpeed;

    [SerializeField] private int defense;
    [SerializeField] private int maxHealth;

    [SerializeField] private int gold;

    private UnityEvent<int> onGoldChanged = new();
    private UnityEvent<WeaponItem> onWeaponEquipped = new();
    private UnityEvent<ArmorItem> onArmorEquipped = new();

    public UnityEvent<int> OnGoldChanged => onGoldChanged;
    public UnityEvent<WeaponItem> OnWeaponEquipped => onWeaponEquipped;
    public UnityEvent<ArmorItem> OnArmorEquipped => onArmorEquipped;

    public int GetMinDamage() => minDamage;
    public int GetMaxDamage() => maxDamage;
    public float GetAttackSpeed() => attackSpeed;
    public int GetDefense() => defense;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentDamage() => Random.Range(minDamage, maxDamage + 1);
    public int GetGold() => gold;

    public void SetGold(int amount)
    {
        gold = amount;
        onGoldChanged?.Invoke(gold);
    }

    public void AddGold(int amount)
    {
        SetGold(gold + amount);
    }

    public bool TrySpendGold(int amount)
    {
        if (gold < amount)
            return false;

        SetGold(gold - amount);
        return true;
    }

    private void Awake()
    {
        ServiceLocator.RegisterPlayerStatsService(this);
    }

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

    public void UpdateWeaponStats(WeaponItem weapon)
    {
        if (weapon == null)
        {
            Debug.LogWarning("PlayerStats: Trying to equip null weapon!");
            return;
        }

        minDamage = weapon.minDamage;
        maxDamage = weapon.maxDamage;
        attackSpeed = weapon.attackSpeed;
        onWeaponEquipped?.Invoke(weapon);
    }

    public void UpdateArmorStats(ArmorItem armor)
    {
        if (armor == null)
        {
            Debug.LogWarning("PlayerStats: Trying to equip null armor!");
            return;
        }

        defense = armor.defense;
        maxHealth = armor.maxHealthBonus;
        onArmorEquipped?.Invoke(armor);
    }
}
