using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
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

    private bool previousUsingControllerInput;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

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

        if (inputHandler.UsingControllerInput != previousUsingControllerInput)
        {
            playerMotor.ResetInputState();
            previousUsingControllerInput = inputHandler.UsingControllerInput;
        }

        float moveSpeed = inputHandler.IsSprinting ? sprintSpeed : walkSpeed;

        if (inputHandler.UsingControllerInput)
        {
            playerMotor.Move(
                inputHandler.MoveInput,
                moveSpeed,
                gravity,
                Time.deltaTime);

            playerMotor.Rotate(
                inputHandler.MoveInputVector,
                rotationSpeed,
                smoothRotation,
                Time.deltaTime);
        }
        else
        {
            playerMotor.Move(
                inputHandler.MoveInput,
                moveSpeed,
                gravity,
                Time.deltaTime);

            playerMotor.Rotate(
                inputHandler.TurnInput,
                rotationSpeed,
                smoothRotation,
                Time.deltaTime);
        }

        UpdateAnimator(moveSpeed);
    }

    private void UpdateAnimator(float moveSpeed)
    {
        if (animator == null)
        {
            return;
        }

        float inputAmount = Mathf.Clamp01(Mathf.Abs(inputHandler.MoveInput));

        float normalizedSpeed;
        if (inputAmount <= 0f)
        {
            normalizedSpeed = 0f;
        }
        else if (inputHandler.IsSprinting)
        {
            normalizedSpeed = Mathf.Lerp(sprintThreshold, 1f, inputAmount);
        }
        else
        {
            normalizedSpeed = Mathf.Lerp(0f, sprintThreshold, inputAmount);
        }

        animator.SetFloat("Speed", normalizedSpeed);
    }
}
