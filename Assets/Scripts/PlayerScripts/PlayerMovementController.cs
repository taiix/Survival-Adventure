using UnityEngine;

public sealed class PlayerMovementController
{
    private const string inAirBoolName = "InAir";
    private const string verticalSpeedFloatName = "VerticalSpeed";
    private const string jumpStartTriggerName = "JumpStart";
    private const string landTriggerName = "Land";

    private readonly PlayerController playerController;
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
    private bool previousGrounded;

    private const float jumpGroundedGraceSeconds = 0.08f;
    private float jumpGroundedGraceTimer;

    public PlayerMovementController(
        PlayerController playerController,
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
        this.playerController = playerController;
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

        previousGrounded = characterController != null && characterController.isGrounded;
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

        bool grounded = playerMotor.IsGrounded;
        bool inAir = !grounded || jumpGroundedGraceTimer > 0f;

        animator.SetBool(inAirBoolName, inAir);
        animator.SetFloat(verticalSpeedFloatName, playerMotor.VerticalVelocity);
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

        bool wasGroundedBeforeMove = playerMotor.IsGrounded;

        if (!waterDetection.IsDetectingWater(characterController.transform))
        {
            playerMotor.Move(
                inputHandler.MoveInput,
                moveSpeed,
                gravity,
                deltaTime,
                jumpPressedThisFrame,
                jumpHeight);
        }

        bool isGroundedAfterMove = playerMotor.IsGrounded;

        // Jump start: only when jump input pressed AND we actually left the ground.
        if (animator != null && jumpPressedThisFrame && wasGroundedBeforeMove && !isGroundedAfterMove)
        {
            jumpGroundedGraceTimer = jumpGroundedGraceSeconds;

            animator.ResetTrigger(jumpStartTriggerName);
            animator.SetTrigger(jumpStartTriggerName);
        }
        else
        {
            if (jumpGroundedGraceTimer > 0f)
            {
                jumpGroundedGraceTimer -= deltaTime;
            }
        }

        // Land: only when we were in air and become grounded.
        if (animator != null && !previousGrounded && isGroundedAfterMove)
        {
            animator.ResetTrigger(landTriggerName);
            animator.SetTrigger(landTriggerName);
        }

        previousGrounded = isGroundedAfterMove;
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

        // Don't scale playback speed while airborne (prevents JumpStart/InAir/Land issues).
        if (animator.GetBool(inAirBoolName))
        {
            animator.speed = 1f;
            return;
        }

        float targetMoveSpeed = isSprinting ? sprintSpeed : walkSpeed;

        if (inputAmount <= 0.01f || targetMoveSpeed <= 0.01f)
        {
            animator.speed = 1f;
            return;
        }

        float referenceWalkSpeed = playerController.GetWalkingSpeed();
        float referenceSprintSpeed = playerController.GetSprintingSpeed();

        float reference = isSprinting ? referenceSprintSpeed : referenceWalkSpeed;
        float playback = (targetMoveSpeed / reference);

        animator.speed = Mathf.Clamp(playback, 0.75f, 1.5f);
    }
}