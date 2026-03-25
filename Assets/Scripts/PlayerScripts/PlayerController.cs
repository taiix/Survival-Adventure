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

    [Header("Dash")]
    [SerializeField] private string dashTriggerName = "Dash";
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField, Min(0.01f)] private float dashDuration = 0.2f;
    [SerializeField, Min(0.1f)] private float dashSpeed = 12f;
    [SerializeField, Min(0f)] private float dashCooldown = 0.5f;

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

    private Animator animator;
    private CharacterController characterController;
    private PlayerWaterDetection waterDetection;

    private bool previousDashPressed;

    private float dashTimeRemaining;
    private float dashCooldownRemaining;
    private Vector3 dashDirection;

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

        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        waterDetection = new PlayerWaterDetection(
            sphereDistance,
            sphereYOffset,
            sphereRadius,
            raycastDistance);

        movementController = new PlayerMovementController(
            characterController,
            animator,
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

        TickDash(Time.deltaTime);
        HandleDashInput();

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

    private void HandleDashInput()
    {
        bool dashPressed = inputHandler.IsDashing;
        bool dashPressedThisFrame = dashPressed && !previousDashPressed;

        if (dashPressedThisFrame)
        {
            TryStartDash();
        }

        previousDashPressed = dashPressed;
    }

    private void TryStartDash()
    {
        if (!stateManager.IsMovementAllowed())
        {
            return;
        }

        // 1) Ground-only dash
        if (!characterController.isGrounded)
        {
            return;
        }

        if (dashCooldownRemaining > 0f)
        {
            return;
        }

        // 2) Don't start a dash if we would dash into water (using your existing detection logic)
        if (waterDetection != null && waterDetection.IsDetectingWater(transform))
        {
            return;
        }

        stateManager.SetState(PlayerState.Dashing);

        dashDirection = GetDashDirection();
        dashTimeRemaining = dashDuration;
        dashCooldownRemaining = dashCooldown;

        if (dashParticles != null)
        {
            dashParticles.Play(true);
        }

        if (animator != null && !string.IsNullOrWhiteSpace(dashTriggerName))
        {
            animator.ResetTrigger(dashTriggerName);
            animator.SetTrigger(dashTriggerName);
        }
    }

    private Vector3 GetDashDirection()
    {
        Vector2 input = inputHandler.MoveInputVector;

        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 local = new Vector3(input.x, 0f, input.y).normalized;
            return transform.TransformDirection(local).normalized;
        }

        return transform.forward;
    }

    private void TickDash(float deltaTime)
    {
        if (dashCooldownRemaining > 0f)
        {
            dashCooldownRemaining -= deltaTime;
        }

        if (!stateManager.IsState(PlayerState.Dashing))
        {
            return;
        }

        // If water is detected during dash, end immediately to avoid getting stuck.
        if (waterDetection != null && waterDetection.IsDetectingWater(transform))
        {
            dashTimeRemaining = 0f;
            EndDash();
            return;
        }

        if (dashTimeRemaining > 0f)
        {
            Vector3 velocity = dashDirection * dashSpeed;
            characterController.Move(velocity * deltaTime);

            dashTimeRemaining -= deltaTime;
            return;
        }

        EndDash();
    }

    private void EndDash()
    {
        if (dashParticles != null)
        {
            dashParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (stateManager.IsState(PlayerState.Dashing))
        {
            stateManager.SetState(PlayerState.Normal);
        }
    }

    public void OnDashAnimationFinished()
    {
        if (stateManager.IsState(PlayerState.Dashing))
        {
            dashTimeRemaining = 0f;
        }
    }

    public InputAction GetInputAction(string actionKey) => inputHandler.GetInputAction(actionKey);
    public PlayerStateManager GetStateManager() => stateManager;
    public PlayerStaminaController GetStaminaController() => staminaController;

    private void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position + transform.forward * sphereDistance + Vector3.up * sphereYOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(p, sphereRadius);
    }
}

