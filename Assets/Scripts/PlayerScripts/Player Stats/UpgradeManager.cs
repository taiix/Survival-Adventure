using UnityEngine;
using UnityEngine.Events;

public class UpgradeManager : MonoBehaviour, IUpgradeService
{
    [SerializeField] private PlayerEquipment playerEquipment;

    private IPlayerStatsService playerStats;
    private UnityEvent onUpgradePurchased = new();
    private UnityEvent<ItemBase> onUpgradeApplied = new();
    private bool isInitialized = false;

    public UnityEvent OnUpgradePurchased => onUpgradePurchased;
    public UnityEvent<ItemBase> OnUpgradeApplied => onUpgradeApplied;

    private void Awake()
    {
        if (playerEquipment == null)
        {
            playerEquipment = GetComponent<PlayerEquipment>();
        }

        if (playerEquipment == null)
        {
            Debug.LogError("UpgradeManager: PlayerEquipment not found! Assign it in the inspector or ensure it's on the same GameObject.");
        }

        ServiceLocator.RegisterUpgradeService(this);
    }

    private void Start()
    {
        EnsureInitialized();
    }

    private void OnDestroy()
    {
        ServiceLocator.UnregisterAll();
    }

    private void EnsureInitialized()
    {
        if (isInitialized)
            return;

        playerStats = ServiceLocator.GetPlayerStatsService();
        
        if (playerStats == null)
        {
            Debug.LogError("UpgradeManager: PlayerStatsService not found in ServiceLocator!\n" +
                "Make sure PlayerStats component exists in the scene and initializes.");
            return;
        }

        isInitialized = true;
    }

    public bool TryUpgradeItem(ItemBase item)
    {
        // Ensure we're initialized before trying to upgrade
        if (!isInitialized)
        {
            EnsureInitialized();
        }

        if (item == null || playerStats == null)
        {
            Debug.LogWarning("UpgradeManager: Cannot upgrade - item or playerStats is null");
            return false;
        }

        int cost = item.GetUpgradeCost();
        if (!playerStats.TrySpendGold(cost))
        {
            Debug.LogWarning($"UpgradeManager: Not enough gold. Need {cost}, have {playerStats.GetGold()}");
            return false;
        }

        item.ApplyUpgrade();
        onUpgradePurchased?.Invoke();
        onUpgradeApplied?.Invoke(item);

        return true;
    }

    public bool TryUpgradeWeapon()
    {
        if (!isInitialized)
        {
            EnsureInitialized();
        }

        return playerEquipment != null && TryUpgradeItem(playerEquipment.EquippedWeapon);
    }

    public bool TryUpgradeArmor()
    {
        if (!isInitialized)
        {
            EnsureInitialized();
        }

        return playerEquipment != null && TryUpgradeItem(playerEquipment.EquippedArmor);
    }
}
