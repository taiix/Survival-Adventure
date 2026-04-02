using System.Collections.Generic;
using UnityEngine;

public class BlacksmithShopBehaviour : GridNavigationBehaviour
{
    [SerializeField] private List<ItemSlot> itemSlots;
    [SerializeField] private UpgradeDescription upgradeDescription;

    private ItemBase selectedSlot;

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
        
        if (itemSlots.Count > 0)
        {
            selectedSlot = itemSlots[0].GetItem();
            // Update the upgrade description with the first item
            if (upgradeDescription != null && selectedSlot != null)
            {
                upgradeDescription.SetCurrentItem(selectedSlot);
                Debug.Log($"BlacksmithShop: Initial item set to {selectedSlot.itemName}");
            }
        }
        else
        {
            Debug.LogWarning("BlacksmithShop: No item slots found!");
        }

        Debug.Log($"BlacksmithShop: Found {itemSlots.Count} item slots");
    }

    protected override int GetSlotCount()
    {
        return itemSlots?.Count ?? 0;
    }

    protected override bool IsValidSlotIndex(int index)
    {
        return index >= 0 && index < GetSlotCount();
    }

    protected override bool IsValidSlot(int index)
    {
        if (!IsValidSlotIndex(index))
        {
            return false;
        }

        ItemSlot slot = itemSlots[index];
        if (slot == null)
        {
            return false;
        }

        Transform slotPosition = slot.GetSlotPosition();
        return slotPosition != null;
    }

    protected override void OnSlotSelected(int index)
    {
        if (slotSelector == null)
        {
            Debug.LogError("BlacksmithShop: slotSelector is null!");
            return;
        }

        if (!IsValidSlot(index))
        {
            Debug.LogWarning($"BlacksmithShop: slot at index {index} is invalid!");
            return;
        }

        Transform slotPosition = itemSlots[index].GetSlotPosition();
        slotSelector.rectTransform.position = slotPosition.position;
        selectedSlot = itemSlots[index].GetItem();

        if (selectedSlot != null)
        {
            if (upgradeDescription != null)
            {
                upgradeDescription.SetCurrentItem(selectedSlot);
                Debug.Log($"BlacksmithShop: Selected item {selectedSlot.itemName}");
            }
            else
            {
                Debug.LogWarning("BlacksmithShop: UpgradeDescription not assigned!");
            }

            ShopEvents.OnSlotSelected?.Invoke(selectedSlot);
        }
    }
}
