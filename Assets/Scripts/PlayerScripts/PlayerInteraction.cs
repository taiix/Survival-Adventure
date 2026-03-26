using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;

    private PlayerInputHandler inputHandler;
    private PlayerStateManager stateManager;
    private IInteractable currentTarget;
    private IInteractable activeInteractable;
    private GameObject activeUI;
    private Collider[] detectionBuffer = new Collider[10];
    private bool previousInteractPressed;

    public void Initialize(PlayerInputHandler inputHandler, PlayerStateManager stateManager)
    {
        this.inputHandler = inputHandler;
        this.stateManager = stateManager;
    }

    private void Update()
    {
        if (inputHandler == null || stateManager == null)
        {
            return;
        }

        DetectNearbyInteractables();
        HandleInteractInput();
        SyncStateWithUI();
    }

    private void HandleInteractInput()
    {
        InputAction interactAction = inputHandler.GetInputAction("Interact");
        if (interactAction == null)
        {
            return;
        }

        bool isInteractPressed = interactAction.ReadValue<float>() > 0.5f;

        if (isInteractPressed && !previousInteractPressed)
        {
            if (currentTarget != null && !stateManager.IsState(PlayerState.Interacting))
            {
                TryInteract();
            }
            else if (stateManager.IsState(PlayerState.Interacting) && activeInteractable == currentTarget)
            {
                CloseInteraction();
            }
        }

        previousInteractPressed = isInteractPressed;
    }

    private void TryInteract()
    {
        if (currentTarget != null && currentTarget.CanInteract)
        {
            activeInteractable = currentTarget;

            NPC_Interaction npcInteraction = currentTarget as NPC_Interaction;
            if (npcInteraction != null)
            {
                activeUI = npcInteraction.GetAssociatedUI();
            }
            
            activeInteractable.OnInteract();
        }
    }

    private void CloseInteraction()
    {
        if (activeUI != null)
        {
            activeUI.SetActive(false);
        }
    }

    private void SyncStateWithUI()
    {
        if (activeUI == null || !activeUI.activeSelf)
        {
            if (stateManager.IsState(PlayerState.Interacting))
            {
                stateManager.SetState(PlayerState.Normal);
                activeInteractable = null;
            }
        }
        else if (activeUI.activeSelf)
        {
            if (!stateManager.IsState(PlayerState.Interacting))
            {
                stateManager.SetState(PlayerState.Interacting);
            }
        }
    }

    private void DetectNearbyInteractables()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            interactionRange,
            detectionBuffer,
            interactableLayer);

        currentTarget = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (detectionBuffer[i].TryGetComponent(out IInteractable interactable) && interactable.CanInteract)
            {
                float distance = Vector3.Distance(transform.position, detectionBuffer[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    currentTarget = interactable;
                }
            }
        }
    }

    public IInteractable GetCurrentTarget() => currentTarget;
    public bool IsInteracting() => stateManager.IsState(PlayerState.Interacting);

    private void OnDrawGizmosSelected()
    {
        Color color = currentTarget != null ? Color.green : Color.red;
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
