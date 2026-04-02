using UnityEngine;

public class InputService : MonoBehaviour, IInputService
{
    private PlayerInputHandler playerInputHandler;
    private PlayerController playerController;
    private bool isInitialized = false;

    public bool IsUsingController => playerInputHandler?.UsingControllerInput ?? false;

    private void Start()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (isInitialized)
            return;


        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
            
            if (playerController == null)
            {
                Debug.LogError("InputService: PlayerController not found on same GameObject!");
                return;
            }
        }

        playerInputHandler = playerController.GetInputHandler();
        
        if (playerInputHandler == null)
        {
            Debug.LogError("InputService: Could not get PlayerInputHandler from PlayerController!");
            return;
        }


        ServiceLocator.RegisterInputService(this);
        
        isInitialized = true;
    }

    public UnityEngine.InputSystem.InputAction GetInputAction(string inputKey)
    {
        if (!isInitialized)
        {
            EnsureInitialized();
        }

        if (playerInputHandler == null)
        {
            Debug.LogError("InputService: PlayerInputHandler is null!");
            return null;
        }

        UnityEngine.InputSystem.InputAction action = playerInputHandler.GetInputAction(inputKey);
        if (action == null)
        {
            Debug.LogWarning($"InputService: Could not find input action '{inputKey}'");
        }
        return action;
    }

    private void OnDestroy()
    {
        if (isInitialized)
        {
            ServiceLocator.UnregisterInputService();
        }
    }
}