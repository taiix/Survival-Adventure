using System.Collections.Generic;
using UnityEngine;

public class BlacksmithShopBehaviour : GridNavigationBehaviour
{
    [SerializeField] private List<ItemSlot> itemSlots;

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
        Debug.Log($"BlacksmithShop: moved selector to slot {index}");
    }
}
