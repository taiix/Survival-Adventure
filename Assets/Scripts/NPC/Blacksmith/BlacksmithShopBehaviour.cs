using System.Collections.Generic;
using UnityEngine;

public class BlacksmithShopBehaviour : GridNavigationBehaviour
{
    [SerializeField] private List<ItemSlot> itemSlots;
    [SerializeField] private UpgradeDescription upgradeDescription;
    [SerializeField] private GameObject renderCamera;

    private ItemBase selectedItem;
    private IUpgradeService upgradeService;

    protected override void OnEnable()
    {
        RefreshSlots();
        currentSlotIndex = -1;
        previousNavigateInput = Vector2.zero;
        SelectFirstValidSlot();

        upgradeService = ServiceLocator.GetUpgradeService();

        if (upgradeService == null)
        {
            Debug.LogWarning("BlacksmithShopBehaviour: UpgradeService not found in ServiceLocator!");
        }

        // Enable navigation input
        if (navigateAction != null && navigateAction.action != null)
        {
            navigateAction.action.Enable();
        }
    }

    protected override void OnDisable()
    {
        selectedItem = null;
        HideCamera();

        if (navigateAction != null && navigateAction.action != null)
        {
            navigateAction.action.Disable();
        }
    }

    protected override void Update()
    {
        SyncCameraVisibility();
        base.Update();
    }

    private void SyncCameraVisibility()
    {
        if (renderCamera == null)
            return;

        bool shouldCameraBeActive = gameObject.activeSelf;
        bool cameraIsActive = renderCamera.activeSelf;

        if (shouldCameraBeActive != cameraIsActive)
        {
            renderCamera.SetActive(shouldCameraBeActive);
        }
    }

    private void HideCamera()
    {
        if (renderCamera != null)
        {
            renderCamera.SetActive(false);
        }
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
        return slot != null && GetSlotPosition(index) != null;
    }

    protected override Transform GetSlotPosition(int index)
    {
        if (!IsValidSlotIndex(index))
        {
            return null;
        }

        ItemSlot slot = itemSlots[index];
        if (slot == null)
        {
            return null;
        }

        Transform slotPosition = slot.GetSlotPosition();
        
        if (slotPosition == null)
        {
            // Fallback: use the slot's RectTransform directly
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            if (slotRect != null)
            {
                return slotRect;
            }
        }
        
        return slotPosition;
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
