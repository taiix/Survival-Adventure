using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset moveAction;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float referenceWalkSpeed = 5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private bool smoothRotation;

    private const string moveForwardActionName = "MoveForward";
    private const string turnActionName = "Turn";

    private const float turnAngle = 90f;
    private const float gravity = -9.81f;

    private CharacterController characterController;
    private InputAction moveForwardInputAction;
    private InputAction turnInputAction;
    private Animator animator;

    private Quaternion targetRotation;
    private bool isTurning;
    private float previousTurnInput;
    private float previousForwardInput;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        targetRotation = transform.rotation;

        if (moveAction != null)
        {
            moveForwardInputAction = moveAction.FindAction(moveForwardActionName, true);
            turnInputAction = moveAction.FindAction(turnActionName, true);
        }
    }

    private void OnEnable()
    {
        moveForwardInputAction?.Enable();
        turnInputAction?.Enable();
    }

    private void OnDisable()
    {
        moveForwardInputAction?.Disable();
        turnInputAction?.Disable();
    }

    private void Update()
    {
        MoveControl();
        RotateControl();
        AnimationSync();
    }

    private void MoveControl()
    {
        if (moveForwardInputAction == null)
        {
            return;
        }

        float forwardInput = moveForwardInputAction.ReadValue<float>();

        if (forwardInput < -0.5f && previousForwardInput >= -0.5f)
        {
            targetRotation = transform.rotation * Quaternion.Euler(0f, 180f, 0f);
            isTurning = true;
            forwardInput = 0f;
        }

        previousForwardInput = forwardInput;

        Vector3 localForward = transform.localRotation * Vector3.forward;
        Vector3 move = localForward * forwardInput;

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * walkSpeed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void RotateControl()
    {
        if (isTurning)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) <= 0.01f)
            {
                transform.rotation = targetRotation;
                isTurning = false;
            }

            return;
        }

        if (turnInputAction == null)
        {
            return;
        }

        float turnInput = turnInputAction.ReadValue<float>();

        if (smoothRotation)
        {
            Quaternion targetRotation =
                transform.rotation * Quaternion.Euler(0f, turnInput * rotationSpeed * Time.deltaTime, 0f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }
        else
        {
            if (!isTurning &&
                Mathf.Abs(turnInput) > 0.01f &&
                Mathf.Abs(previousTurnInput) <= 0.01f)
            {
                float turnDirection = Mathf.Sign(turnInput);
                targetRotation = transform.rotation * Quaternion.Euler(0f, turnDirection * turnAngle, 0f);
                isTurning = true;
            }

            if (isTurning)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);

                if (Quaternion.Angle(transform.rotation, targetRotation) <= 0.01f)
                {
                    transform.rotation = targetRotation;
                    isTurning = false;
                }
            }

            previousTurnInput = turnInput;
        }
    }

    private void AnimationSync()
    {
        if (moveForwardInputAction == null)
        {
            return;
        }

        float forwardInput = moveForwardInputAction.ReadValue<float>();
        float currentSpeed = walkSpeed * Mathf.Abs(forwardInput);
        float speedRatio = currentSpeed / referenceWalkSpeed;

        animator.SetFloat("Speed", speedRatio);
        animator.speed = Mathf.Max(speedRatio, 0.1f);
    }
}
