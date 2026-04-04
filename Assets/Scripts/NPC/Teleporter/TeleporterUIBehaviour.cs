using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeleporterUIBehaviour : GridNavigationBehaviour
{
    [SerializeField] private List<Slot> teleportSlots;
    
    private IInputService inputService;
    private InputAction interactAction;
    private int selectedTeleportIndex = -1;

    protected override void OnEnable()
    {
        RefreshSlots();
        currentSlotIndex = -1;
        previousNavigateInput = Vector2.zero;

        // Enable navigation input
        if (navigateAction != null && navigateAction.action != null)
        {
            navigateAction.action.Enable();
        }

        // Initialize input service
        inputService = ServiceLocator.GetInputService();
        if (inputService != null)
        {
            interactAction = inputService.GetInputAction("Interact");
        }

        SelectFirstValidSlot();
    }

    protected override void OnDisable()
    {
        selectedTeleportIndex = -1;

        if (navigateAction != null && navigateAction.action != null)
        {
            navigateAction.action.Disable();
        }
    }

    protected override void Update()
    {
        base.Update();

        // Handle interaction
        if (interactAction != null && interactAction.WasPerformedThisFrame())
        {
            OnInteractPressed();
        }
    }

    private void OnInteractPressed()
    {
        if (currentSlotIndex >= 0 && IsValidSlot(currentSlotIndex))
        {
            selectedTeleportIndex = currentSlotIndex;
            Slot slot = teleportSlots[currentSlotIndex];
            object locationData = slot.GetSlotData();
            
            Debug.Log($"Teleporting to location: {locationData}");
            // TODO: Implement actual teleportation logic
        }
    }

    protected override void RefreshSlots()
    {
        if (teleportSlots == null)
        {
            teleportSlots = new List<Slot>();
        }
        else
        {
            teleportSlots.Clear();
        }

        teleportSlots.AddRange(GetComponentsInChildren<Slot>(true));
    }

    protected override int GetSlotCount() => teleportSlots?.Count ?? 0;

    protected override bool IsValidSlotIndex(int index) => index >= 0 && index < GetSlotCount();

    protected override bool IsValidSlot(int index)
    {
        if (!IsValidSlotIndex(index))
            return false;

        Slot slot = teleportSlots[index];
        return slot != null;
    }

    protected override Transform GetSlotPosition(int index)
    {
        if (!IsValidSlot(index))
            return null;

        Slot slot = teleportSlots[index];
        Transform slotPosition = slot.GetSlotPosition();
        
        if (slotPosition == null)
        {
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
            return;

        Slot slot = teleportSlots[index];
        selectedTeleportIndex = index;
    }

    public int GetSelectedTeleportIndex() => selectedTeleportIndex;
}
