using UnityEngine.Events;

public interface IUpgradeService
{
    bool TryUpgradeItem(ItemBase item);
    bool TryUpgradeWeapon();
    bool TryUpgradeArmor();
    
    UnityEvent OnUpgradePurchased { get; }
    UnityEvent<ItemBase> OnUpgradeApplied { get; }
}