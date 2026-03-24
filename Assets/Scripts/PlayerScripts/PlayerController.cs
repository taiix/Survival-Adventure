using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
    [Header("Player State")]
    [SerializeField] private PlayerState playerState;

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset moveAction;
    [SerializeField] private string keyboardActionMapName = "Keyboard";
    [SerializeField] private string controllerActionMapName = "Controller";

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float turnAngle = 90f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private bool smoothRotation;
    [SerializeField, Min(0.1f)] private float jumpHeight = 1.5f;

    [Header("Stamina")]
    [SerializeField, Min(1f)] private float maxStamina = 100f;
    [SerializeField, Min(0.1f)] private float staminaDrainRate = 20f;
    [SerializeField, Min(0.1f)] private float staminaRegenRate = 10f;
    [SerializeField, Min(0f)] private float staminaRegenDelay = 1.5f;

    [Header("Water Detection")]
    [SerializeField] private float sphereDistance;
    [SerializeField] private float sphereYOffset;
    [SerializeField] private float sphereRadius;
    [SerializeField] private float raycastDistance;

    private const string moveForwardActionName = "MoveForward";
    private const string turnActionName = "Turn";
    private const string controllerMoveActionName = "Moving";
    private const string sprintActionName = "Sprint";
    private const string jumpActionName = "Jump";
    private const float gravity = -9.81f;

    private PlayerInputHandler inputHandler;
    private PlayerStateManager stateManager;
    private PlayerMovementController movementController;
    private PlayerStaminaController staminaController;

    private void Awake()
    {
        inputHandler = new PlayerInputHandler(
            moveAction,
            keyboardActionMapName,
            controllerActionMapName,
            moveForwardActionName,
            turnActionName,
            controllerMoveActionName,
            sprintActionName,
            jumpActionName);

        stateManager = new PlayerStateManager();

        staminaController = new PlayerStaminaController(
            maxStamina,
            staminaDrainRate,
            staminaRegenRate,
            staminaRegenDelay);

        movementController = new PlayerMovementController(
            GetComponent<CharacterController>(),
            GetComponent<Animator>(),
            transform,
            turnAngle,
            walkSpeed,
            sprintSpeed,
            rotationSpeed,
            smoothRotation,
            jumpHeight,
            gravity,
            sphereDistance,
            sphereYOffset,
            sphereRadius,
            raycastDistance);

        GetComponent<PlayerAttack>()?.Initialize(inputHandler, stateManager);
        GetComponent<PlayerInteraction>()?.Initialize(inputHandler, stateManager);
    }

    private void OnEnable() => inputHandler?.Enable();

    private void OnDisable() => inputHandler?.Disable();

    private void Update()
    {
        playerState = stateManager.CurrentState;

        inputHandler.UpdateInputState();

        if (stateManager.IsMovementAllowed())
        {
            bool isSprinting = inputHandler.IsSprinting && staminaController.HasStamina;

            if (isSprinting)
            {
                staminaController.DrainStamina(Time.deltaTime);
            }

            staminaController.Update(Time.deltaTime);
            movementController.Update(inputHandler, isSprinting, Time.deltaTime);
        }
        else
        {
            movementController.Idle();
            staminaController.Update(Time.deltaTime);
        }
    }

    public InputAction GetInputAction(string actionKey) => inputHandler.GetInputAction(actionKey);
    public PlayerStateManager GetStateManager() => stateManager;
}

