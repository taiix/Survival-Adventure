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
        InitializeComponents();
        InitializeSystems();
    }

    private void OnEnable()
    {
        inputHandler.Enable();
    }

    private void OnDisable()
    {
        inputHandler.Disable();
    }

    private void Update()
    {
        inputHandler.UpdateInputState();
        HandleInputModeSwitch();

        bool isSprinting = ResolveSprintState();
        float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;

        HandleMovement(moveSpeed);
        AttackControl();
        UpdateAnimator(isSprinting);
    }

    private void InitializeComponents()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerStamina = GetComponent<PlayerStamina>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void InitializeSystems()
    {
        inputHandler = new PlayerInputHandler(
            moveAction,
            keyboardActionMapName,
            controllerActionMapName,
            moveForwardActionName,
            turnActionName,
            controllerMoveActionName,
            sprintActionName);

        playerMotor = new PlayerMotor(characterController, transform, turnAngle);
        previousUsingControllerInput = inputHandler.UsingControllerInput;
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
        bool isSprinting = inputHandler.IsSprinting && (playerStamina == null || playerStamina.HasStamina);
        if (isSprinting && playerStamina != null)
        {
            playerStamina.UseStamina(Time.deltaTime);
        }

        return isSprinting;
    }

    private void HandleMovement(float moveSpeed)
    {
        if (inputHandler.IsAttacking) return;

       //if()
            playerMotor.Move(
            inputHandler.MoveInput,
            moveSpeed,
            gravity,
            Time.deltaTime);

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
}

