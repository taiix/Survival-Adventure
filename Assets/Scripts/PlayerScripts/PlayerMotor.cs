using UnityEngine;

public sealed class PlayerMotor
{
    private readonly CharacterController characterController;
    private readonly Transform playerTransform;
    private readonly float turnAngle;

    private Quaternion targetRotation;
    private bool isTurning;
    private float previousTurnInput;
    private float verticalVelocity;

    public PlayerMotor(CharacterController characterController, Transform playerTransform, float turnAngle)
    {
        this.characterController = characterController;
        this.playerTransform = playerTransform;
        this.turnAngle = turnAngle;
        targetRotation = playerTransform.rotation;
    }

    public void ResetInputState()
    {
        targetRotation = playerTransform.rotation;
        isTurning = false;
        previousTurnInput = 0f;
    }

    public void Move(float forwardInput, float moveSpeed, float gravity, float deltaTime)
    {
        Vector3 localForward = Vector3.forward;
        Vector3 move = playerTransform.TransformDirection(localForward) * forwardInput;

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * deltaTime);
    }

    public void Rotate(float turnInput, float rotationSpeed, bool smoothRotation, float deltaTime)
    {
        if (isTurning)
        {
            playerTransform.rotation = Quaternion.RotateTowards(
                playerTransform.rotation,
                targetRotation,
                rotationSpeed * deltaTime);

            if (Quaternion.Angle(playerTransform.rotation, targetRotation) <= 0.01f)
            {
                playerTransform.rotation = targetRotation;
                isTurning = false;
            }

            return;
        }

        if (smoothRotation)
        {
            if (Mathf.Abs(turnInput) > 0.01f)
            {
                playerTransform.Rotate(0f, turnInput * rotationSpeed * deltaTime, 0f, Space.World);
            }

            previousTurnInput = turnInput;
            return;
        }

        if (Mathf.Abs(turnInput) > 0.01f &&
            Mathf.Abs(previousTurnInput) <= 0.01f)
        {
            float turnDirection = Mathf.Sign(turnInput);
            targetRotation = playerTransform.rotation * Quaternion.Euler(0f, turnDirection * turnAngle, 0f);
            isTurning = true;
        }

        previousTurnInput = turnInput;
    }

    public void Rotate(Vector2 moveInput, float rotationSpeed, bool smoothRotation, float deltaTime)
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        if (moveInput.sqrMagnitude > 0.01f && direction.sqrMagnitude > 0.1f)
            playerTransform.localRotation =
                Quaternion.LookRotation(direction, Vector3.up);
            Debug.Log(moveInput);
    }
}
