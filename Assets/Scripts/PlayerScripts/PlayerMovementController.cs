using UnityEngine;

public sealed class PlayerMovementController
{
    private const string inAirBoolName = "InAir";
    private const string verticalSpeedFloatName = "VerticalSpeed";

    private readonly CharacterController characterController;
    private readonly Animator animator;
    private readonly PlayerMotor playerMotor;
    private readonly PlayerWaterDetection waterDetection;
    private readonly float walkSpeed;
    private readonly float sprintSpeed;
    private readonly float rotationSpeed;
    private readonly bool smoothRotation;
    private readonly float jumpHeight;
    private readonly float gravity;
    private readonly float sprintThreshold;

    private bool previousUsingControllerInput;
    private bool previousJumpPressed;

    public PlayerMovementController(
        CharacterController characterController,
        Animator animator,
        Transform playerTransform,
        float turnAngle,
        float walkSpeed,
        float sprintSpeed,
        float rotationSpeed,
        bool smoothRotation,
        float jumpHeight,
        float gravity,
        float sphereDistance,
        float sphereYOffset,
        float sphereRadius,
        float raycastDistance)
    {
        this.characterController = characterController;
        this.animator = animator;
        this.walkSpeed = walkSpeed;
        this.sprintSpeed = sprintSpeed;
        this.rotationSpeed = rotationSpeed;
        this.smoothRotation = smoothRotation;
        this.jumpHeight = jumpHeight;
        this.gravity = gravity;
        sprintThreshold = 0.5f;

        playerMotor = new PlayerMotor(characterController, playerTransform, turnAngle);
        waterDetection = new PlayerWaterDetection(sphereDistance, sphereYOffset, sphereRadius, raycastDistance);
    }

    public void Update(PlayerInputHandler inputHandler, bool isSprinting, float deltaTime)
    {
        HandleInputModeSwitch(inputHandler);

        float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;
        HandleMovement(inputHandler, moveSpeed, deltaTime);

        UpdateAirStateAnimatorParams();
        UpdateAnimator(inputHandler, isSprinting);
    }

    private void UpdateAirStateAnimatorParams()
    {
        if (animator == null)
        {
            return;
        }

        bool inAir = !playerMotor.IsGrounded;
        animator.SetBool(inAirBoolName, inAir);
    }

    public void Idle()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    private void HandleInputModeSwitch(PlayerInputHandler inputHandler)
    {
        if (inputHandler.UsingControllerInput == previousUsingControllerInput)
        {
            return;
        }

        playerMotor.ResetInputState();
        previousUsingControllerInput = inputHandler.UsingControllerInput;
    }

    private void HandleMovement(PlayerInputHandler inputHandler, float moveSpeed, float deltaTime)
    {
        if (inputHandler.IsAttacking)
        {
            previousJumpPressed = inputHandler.IsJumping;
            return;
        }

        bool jumpPressedThisFrame = inputHandler.IsJumping && !previousJumpPressed;

        if (!waterDetection.IsDetectingWater(characterController.transform))
        {
            bool wasGrounded = playerMotor.IsGrounded;

            playerMotor.Move(
                inputHandler.MoveInput,
                moveSpeed,
                gravity,
                deltaTime,
                jumpPressedThisFrame,
                jumpHeight);

            // Fire the trigger only when we just left the ground due to a jump input.
            bool isNowInAir = !playerMotor.IsGrounded;
        }

        previousJumpPressed = inputHandler.IsJumping;

        if (inputHandler.UsingControllerInput)
        {
            playerMotor.Rotate(
                inputHandler.MoveInputVector,
                rotationSpeed,
                smoothRotation,
                deltaTime);
        }
        else
        {
            playerMotor.Rotate(
                inputHandler.TurnInput,
                rotationSpeed,
                smoothRotation,
                deltaTime);
        }
    }

    private void UpdateAnimator(PlayerInputHandler inputHandler, bool isSprinting)
    {
        if (animator == null)
        {
            return;
        }

        float inputAmount = Mathf.Clamp01(Mathf.Abs(inputHandler.MoveInput));
        float normalizedSpeed = inputAmount > 0f
            ? isSprinting
                ? Mathf.Lerp(sprintThreshold, 1f, inputAmount)
                : Mathf.Lerp(0f, sprintThreshold, inputAmount)
            : 0f;

        animator.SetFloat("Speed", normalizedSpeed);
    }
}