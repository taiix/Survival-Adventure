using UnityEngine.Events;

public interface IPlayerStatsService
{
    int GetMinDamage();
    int GetMaxDamage();
    float GetAttackSpeed();
    int GetDefense();
    int GetMaxHealth();
    int GetGold();
    
    void AddGold(int amount);
    bool TrySpendGold(int amount);
    
    UnityEvent<int> OnGoldChanged { get; }
    UnityEvent<WeaponItem> OnWeaponEquipped { get; }
    UnityEvent<ArmorItem> OnArmorEquipped { get; }
}