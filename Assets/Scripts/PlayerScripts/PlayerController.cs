using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
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

    [Header("Animator")]
    [SerializeField, Range(0f, 1f)] private float sprintThreshold = 0.5f;

    [Header("Stamina")]
    [SerializeField, Min(1f)] private float maxStamina = 100f;
    [SerializeField, Min(0.1f)] private float staminaDrainRate = 20f;
    [SerializeField, Min(0.1f)] private float staminaRegenRate = 10f;
    [SerializeField, Min(0f)] private float staminaRegenDelay = 1.5f;

    [Header("Water Detectiom")]
    [SerializeField] private float sphereDistance;
    [SerializeField] private float sphereYOffset;
    [SerializeField] private float sphereRadius;
    [SerializeField] private float raycastDistance;

    private const string moveForwardActionName = "MoveForward";
    private const string turnActionName = "Turn";
    private const string controllerMoveActionName = "Moving";
    private const string sprintActionName = "Sprint";
    private const float gravity = -9.81f;

    private CharacterController characterController;
    private Animator animator;
    private PlayerInputHandler inputHandler;
    private PlayerMotor playerMotor;
    private PlayerStamina playerStamina;
    private PlayerAttack playerAttack;
    private PlayerWaterDetection playerWaterDetection;

    private bool previousUsingControllerInput;
    private bool previousAttackPressed;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerAttack = GetComponent<PlayerAttack>();

        playerStamina = new PlayerStamina(
            maxStamina,
            staminaDrainRate,
            staminaRegenRate,
            staminaRegenDelay);

        inputHandler = new PlayerInputHandler(
            moveAction,
            keyboardActionMapName,
            controllerActionMapName,
            moveForwardActionName,
            turnActionName,
            controllerMoveActionName,
            sprintActionName);

        playerMotor = new PlayerMotor(characterController, transform, turnAngle);

        playerWaterDetection = new PlayerWaterDetection(
            sphereDistance,
            sphereYOffset,
            sphereRadius,
            raycastDistance);

        previousUsingControllerInput = inputHandler.UsingControllerInput;
    }

    private void OnEnable() => inputHandler?.Enable();

    private void OnDisable() => inputHandler?.Disable();

    private void Update()
    {
        inputHandler.UpdateInputState();
        HandleInputModeSwitch();

        bool isSprinting = ResolveSprintState();
        float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;

        HandleMovement(moveSpeed);
        AttackControl();
        playerStamina.Update(Time.deltaTime);
        UpdateAnimator(isSprinting);
    }

    private void HandleInputModeSwitch()
    {
        if (inputHandler.UsingControllerInput == previousUsingControllerInput)
        {
            return;
        }

        playerMotor.ResetInputState();
        previousUsingControllerInput = inputHandler.UsingControllerInput;
    }

    private bool ResolveSprintState()
    {
        bool isSprinting = inputHandler.IsSprinting && playerStamina.HasStamina;

        if (isSprinting)
        {
            playerStamina.UseStamina(Time.deltaTime);
        }

        return isSprinting;
    }

    private void HandleMovement(float moveSpeed)
    {
        if (inputHandler.IsAttacking)
        {
            return;
        }

        if (!playerWaterDetection.IsDetectingWater(this.transform))
        {
            playerMotor.Move(
            inputHandler.MoveInput,
            moveSpeed,
            gravity,
            Time.deltaTime);
        }

        if (inputHandler.UsingControllerInput)
        {
            playerMotor.Rotate(
                inputHandler.MoveInputVector,
                rotationSpeed,
                smoothRotation,
                Time.deltaTime);
        }
        else
        {
            playerMotor.Rotate(
                inputHandler.TurnInput,
                rotationSpeed,
                smoothRotation,
                Time.deltaTime);
        }
    }

    private void AttackControl()
    {
        if (playerAttack == null)
        {
            return;
        }

        bool isAttackPressed = inputHandler.IsAttacking;

        if (isAttackPressed && !previousAttackPressed)
        {
            playerAttack.Attack();
            Debug.Log("Attack input detected, performing attack.");
        }

        previousAttackPressed = isAttackPressed;
    }

    private void UpdateAnimator(bool isSprinting)
    {
        if (animator == null)
        {
            return;
        }

        float inputAmount = Mathf.Clamp01(Mathf.Abs(inputHandler.MoveInput));
        float normalizedSpeed = 0f;

        if (inputAmount > 0f)
        {
            normalizedSpeed = isSprinting
                ? Mathf.Lerp(sprintThreshold, 1f, inputAmount)
                : Mathf.Lerp(0f, sprintThreshold, inputAmount);
        }

        animator.SetFloat("Speed", normalizedSpeed);
    }

    private void OnDrawGizmos()
    {
        Vector3 spherePosition =
            transform.position +
            transform.forward * sphereDistance +
            Vector3.up * sphereYOffset;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(spherePosition, sphereRadius);
        Gizmos.DrawLine(spherePosition, spherePosition + Vector3.down * raycastDistance);
    }
}

