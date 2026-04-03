using System.Collections.Generic;
using UnityEngine;

public class BlacksmithShopBehaviour : GridNavigationBehaviour
{
    [SerializeField] private List<ItemSlot> itemSlots;
    [SerializeField] private UpgradeDescription upgradeDescription;

    private ItemBase selectedItem;
    private IUpgradeService upgradeService;

    private void OnEnable()
    {
        RefreshSlots();
        SelectFirstValidSlot();
        upgradeService = ServiceLocator.GetUpgradeService();

        if (upgradeService == null)
        {
            Debug.LogWarning("BlacksmithShopBehaviour: UpgradeService not found in ServiceLocator!");
        }
    }

    private void OnDisable()
    {
        selectedItem = null;
    }

    protected override void RefreshSlots()
    {
        if (itemSlots == null)
        {
            itemSlots = new List<ItemSlot>();
        }
        else
        {
            itemSlots.Clear();
        }

        itemSlots.AddRange(GetComponentsInChildren<ItemSlot>(true));
        
        if (itemSlots.Count == 0)
        {
            Debug.LogWarning("BlacksmithShopBehaviour: No item slots found!");
        }
        else
        {
            Debug.Log($"BlacksmithShopBehaviour: Found {itemSlots.Count} item slots");
        }
    }

    protected override int GetSlotCount() => itemSlots?.Count ?? 0;

    protected override bool IsValidSlotIndex(int index) => index >= 0 && index < GetSlotCount();

    protected override bool IsValidSlot(int index)
    {
        if (!IsValidSlotIndex(index))
            return false;

        ItemSlot slot = itemSlots[index];
        return slot != null && slot.GetSlotPosition() != null;
    }

    protected override void OnSlotSelected(int index)
    {
        if (!IsValidSlot(index))
        {
            Debug.LogWarning($"BlacksmithShopBehaviour: Invalid slot index {index}");
            return;
        }

        ItemSlot slot = itemSlots[index];
        selectedItem = slot.GetItem();

        // Update slot selector visual
        if (slotSelector != null)
        {
            Transform slotPosition = slot.GetSlotPosition();
            slotSelector.rectTransform.position = slotPosition.position;
        }

        // Update description panel
        if (upgradeDescription != null && selectedItem != null)
        {
            upgradeDescription.SetCurrentItem(selectedItem);
            Debug.Log($"BlacksmithShopBehaviour: Selected {selectedItem.itemName}");
        }
    }

    public ItemBase GetSelectedItem() => selectedItem;
    public IUpgradeService GetUpgradeService() => upgradeService;
}
