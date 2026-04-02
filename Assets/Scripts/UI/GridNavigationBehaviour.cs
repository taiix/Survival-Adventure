using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class GridNavigationBehaviour : MonoBehaviour
{
    [SerializeField] protected Image slotSelector;

    [SerializeField, Min(1)] protected int rows = 5;
    [SerializeField, Min(1)] protected int columns = 4;
    [SerializeField, Range(0.1f, 0.9f)] protected float navigationDeadzone = 0.5f;

    [Header("Input System")]
    [SerializeField] protected InputActionReference navigateAction;

    protected int currentSlotIndex = -1;
    private Vector2 previousNavigateInput;

    protected virtual void Awake()
    {
        RefreshSlots();
    }

    protected virtual void OnEnable()
    {
        RefreshSlots();
        currentSlotIndex = -1;
        previousNavigateInput = Vector2.zero;

        if (navigateAction != null && navigateAction.action != null)
        {
            navigateAction.action.Enable();
        }

        SelectFirstValidSlot();
    }

    protected virtual void Update()
    {
        if (slotSelector == null || navigateAction == null || navigateAction.action == null)
        {
            return;
        }

        HandleNavigationInput();
    }

    protected virtual void OnDisable()
    {
        previousNavigateInput = Vector2.zero;
        currentSlotIndex = -1;

        if (navigateAction != null && navigateAction.action != null)
        {
            navigateAction.action.Disable();
        }
    }

    private void HandleNavigationInput()
    {
        Vector2 input = navigateAction.action.ReadValue<Vector2>();

        if (input.sqrMagnitude <= navigationDeadzone * navigationDeadzone)
        {
            previousNavigateInput = Vector2.zero;
            return;
        }

        bool wasCentered = previousNavigateInput.sqrMagnitude <= navigationDeadzone * navigationDeadzone;
        if (wasCentered)
        {
            if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
            {
                MoveSelection(input.x > 0f ? 1 : -1, 0);
            }
            else
            {
                MoveSelection(0, input.y > 0f ? -1 : 1);
            }
        }

        previousNavigateInput = input;
    }

    protected virtual void MoveSelection(int columnOffset, int rowOffset)
    {
        if (currentSlotIndex < 0 || !IsValidSlotIndex(currentSlotIndex))
        {
            SelectFirstValidSlot();
            return;
        }

        int currentRow = currentSlotIndex / columns;
        int currentColumn = currentSlotIndex % columns;

        int targetRow = currentRow + rowOffset;
        int targetColumn = currentColumn + columnOffset;

        if (columnOffset != 0)
        {
            if (targetColumn >= columns)
            {
                targetColumn = 0;
                targetRow += 1;
            }
            else if (targetColumn < 0)
            {
                targetColumn = columns - 1;
                targetRow -= 1;
            }
        }

        targetRow = (targetRow % rows + rows) % rows;

        int targetIndex = (targetRow * columns) + targetColumn;

        if (IsValidSlot(targetIndex))
        {
            currentSlotIndex = targetIndex;
            OnSlotSelected(targetIndex);
        }
    }

    protected virtual void SelectFirstValidSlot()
    {
        for (int i = 0; i < GetSlotCount(); i++)
        {
            if (IsValidSlot(i))
            {
                currentSlotIndex = i;
                OnSlotSelected(i);
                return;
            }
        }

        Debug.LogWarning("SelectFirstValidSlot: No valid slots found!");
        currentSlotIndex = -1;
    }

    protected abstract void RefreshSlots();
    protected abstract int GetSlotCount();
    protected abstract bool IsValidSlotIndex(int index);
    protected abstract bool IsValidSlot(int index);
    protected abstract void OnSlotSelected(int index);

}
