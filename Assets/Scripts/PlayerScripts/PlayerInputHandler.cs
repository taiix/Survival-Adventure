using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputHandler
{
    private const float inputDeadzone = 0.1f;
    private const float buttonThreshold = 0.5f;

    private const string moveForwardInputKey = "MoveForward";
    private const string turnInputKey = "Turn";
    private const string moveVectorInputKey = "MoveVector";
    private const string sprintInputKey = "Sprint";
    private const string attackInputKey = "Attack";
    private const string jumpInputKey = "Jump";
    private const string dashInputKey = "Dash";
    private const string upgradeInputKey = "Upgrade";
    private const string teleporterInputKey = "TeleporterInteraction";


    private readonly InputActionAsset actionAsset;
    private readonly string keyboardActionMapName;
    private readonly string controllerActionMapName;
    private readonly Dictionary<string, InputBinding> bindings;

    private InputActionMap keyboardInputActionMap;
    private InputActionMap controllerInputActionMap;

    public float MoveInput { get; private set; }
    public Vector2 MoveInputVector { get; private set; }
    public float TurnInput { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool UsingControllerInput { get; private set; }

    public PlayerInputHandler(
        InputActionAsset actionAsset,
        string keyboardActionMapName,
        string controllerActionMapName,
        string moveForwardActionName,
        string turnActionName,
        string controllerMoveActionName,
        string sprintActionName,
        string jumpActionName)
    {
        this.actionAsset = actionAsset;
        this.keyboardActionMapName = keyboardActionMapName;
        this.controllerActionMapName = controllerActionMapName;
        bindings = new Dictionary<string, InputBinding>(StringComparer.Ordinal);

        RegisterInput(moveForwardInputKey, moveForwardActionName, null, true, false);
        RegisterInput(turnInputKey, turnActionName, turnActionName, true, false);
        RegisterInput(moveVectorInputKey, null, controllerMoveActionName, false, false);
        RegisterInput(sprintInputKey, sprintActionName, sprintActionName, false, false);
        RegisterInput(attackInputKey, "Attack", "Attack", false, false);
        RegisterInput(jumpInputKey, jumpActionName, jumpActionName, false, false);
        RegisterInput(dashInputKey, "Dash", "Dash", false, false);
        RegisterInput("Interact", "Interact", "Interact", false, false);
        RegisterInput(upgradeInputKey, "Upgrade", "Upgrade", false, false);
        RegisterInput(teleporterInputKey, teleporterInputKey, teleporterInputKey, false, false);

        ResolveActions();
    }

    public void RegisterInput(
        string inputKey,
        string keyboardActionName,
        string controllerActionName,
        bool throwIfKeyboardActionMissing,
        bool throwIfControllerActionMissing)
    {
        if (string.IsNullOrWhiteSpace(inputKey))
        {
            return;
        }

        bindings[inputKey] = new InputBinding(
            keyboardActionName,
            controllerActionName,
            throwIfKeyboardActionMissing,
            throwIfControllerActionMissing);
    }

    public InputAction GetInputAction(string inputKey)
    {
        return GetPreferredAction(inputKey);
    }

    public void Enable()
    {
        keyboardInputActionMap?.Enable();
        controllerInputActionMap?.Enable();
    }

    public void Disable()
    {
        keyboardInputActionMap?.Disable();
        controllerInputActionMap?.Disable();
    }

    public void UpdateInputState()
    {
        if (actionAsset == null)
        {
            ResetState();
            return;
        }

        float keyboardMove = ReadAxisInput(GetKeyboardAction(moveForwardInputKey), true);
        float keyboardTurn = ReadTurnInput(GetKeyboardAction(turnInputKey), null);

        InputAction controllerMoveAction = GetControllerAction(moveVectorInputKey);
        Vector2 controllerMoveVector = ReadMoveVector(controllerMoveAction);
        float controllerTurn = ReadTurnInput(GetControllerAction(turnInputKey), controllerMoveAction);

        float keyboardActivity = Mathf.Max(Mathf.Abs(keyboardMove), Mathf.Abs(keyboardTurn));
        float controllerActivity = Mathf.Max(controllerMoveVector.magnitude, Mathf.Abs(controllerTurn));

        bool keyboardHasInput = keyboardActivity > inputDeadzone;
        bool controllerHasInput = controllerActivity > inputDeadzone;

        if (controllerHasInput && controllerActivity >= keyboardActivity)
        {
            UsingControllerInput = true;
        }
        else if (keyboardHasInput)
        {
            UsingControllerInput = false;
        }

        InputAction sprintAction = GetPreferredAction(sprintInputKey);
        InputAction attackAction = GetPreferredAction(attackInputKey);
        InputAction jumpAction = GetPreferredAction(jumpInputKey);
        InputAction dashAction = GetPreferredAction(dashInputKey);

        if (UsingControllerInput)
        {
            MoveInputVector = controllerMoveVector;
            MoveInput = controllerMoveVector.magnitude;
            TurnInput = 0f;
        }
        else
        {
            MoveInputVector = new Vector2(0f, keyboardMove);
            MoveInput = keyboardMove;
            TurnInput = keyboardTurn;
        }

        IsSprinting = ReadButtonInput(sprintAction, buttonThreshold);
        if (!IsSprinting) IsAttacking = ReadButtonInput(attackAction, buttonThreshold);
        IsJumping = ReadButtonInput(jumpAction, buttonThreshold);
        IsDashing = ReadButtonInput(dashAction, buttonThreshold);

        if (Mathf.Abs(MoveInput) < inputDeadzone)
        {
            MoveInput = 0f;
        }

        if (MoveInputVector.magnitude < inputDeadzone)
        {
            MoveInputVector = Vector2.zero;
        }

        if (Mathf.Abs(TurnInput) < inputDeadzone)
        {
            TurnInput = 0f;
        }
    }

    private void ResetState()
    {
        MoveInput = 0f;
        MoveInputVector = Vector2.zero;
        TurnInput = 0f;
        IsSprinting = false;
        IsAttacking = false;
        IsJumping = false;
        IsDashing = false;
        UsingControllerInput = false;
    }

    private void ResolveActions()
    {
        if (actionAsset == null)
        {
            return;
        }

        keyboardInputActionMap = ResolveActionMap(keyboardActionMapName);
        controllerInputActionMap = ResolveActionMap(controllerActionMapName);

        foreach (KeyValuePair<string, InputBinding> pair in bindings)
        {
            InputBinding binding = pair.Value;
            binding.KeyboardAction = ResolveAction(
                keyboardInputActionMap,
                binding.KeyboardActionName,
                binding.ThrowIfKeyboardActionMissing);

            binding.ControllerAction = ResolveAction(
                controllerInputActionMap,
                binding.ControllerActionName,
                binding.ThrowIfControllerActionMissing);
        }
    }

    private InputAction GetPreferredAction(string inputKey)
    {
        return UsingControllerInput
            ? (GetControllerAction(inputKey) ?? GetKeyboardAction(inputKey))
            : (GetKeyboardAction(inputKey) ?? GetControllerAction(inputKey));
    }

    private InputAction GetKeyboardAction(string inputKey)
    {
        return GetAction(inputKey, true);
    }

    private InputAction GetControllerAction(string inputKey)
    {
        return GetAction(inputKey, false);
    }

    private InputAction GetAction(string inputKey, bool keyboard)
    {
        InputBinding binding;
        if (inputKey == null || !bindings.TryGetValue(inputKey, out binding))
        {
            return null;
        }

        return keyboard ? binding.KeyboardAction : binding.ControllerAction;
    }

    private InputActionMap ResolveActionMap(string actionMapName)
    {
        if (actionAsset == null || string.IsNullOrWhiteSpace(actionMapName))
        {
            return null;
        }

        return actionAsset.FindActionMap(actionMapName, false);
    }

    private InputAction ResolveAction(InputActionMap actionMap, string actionName, bool throwIfNotFound)
    {
        if (actionAsset == null || string.IsNullOrWhiteSpace(actionName))
        {
            return null;
        }

        if (actionMap != null)
        {
            return actionMap.FindAction(actionName, throwIfNotFound);
        }

        return actionAsset.FindAction(actionName, throwIfNotFound);
    }

    private static Vector2 ReadMoveVector(InputAction action)
    {
        if (action == null)
        {
            return Vector2.zero;
        }

        if (IsVector2Action(action))
        {
            return action.ReadValue<Vector2>();
        }

        return new Vector2(0f, action.ReadValue<float>());
    }

    private static float ReadTurnInput(InputAction action, InputAction vectorFallbackAction)
    {
        float turnInput = ReadAxisInput(action, false);
        if (Mathf.Abs(turnInput) > 0.0001f)
        {
            return turnInput;
        }

        return ReadAxisInput(vectorFallbackAction, false);
    }

    private static bool ReadButtonInput(InputAction action, float threshold)
    {
        if (action == null)
        {
            return false;
        }

        if (IsVector2Action(action))
        {
            return action.ReadValue<Vector2>().y > threshold;
        }

        return action.ReadValue<float>() > threshold;
    }

    private static float ReadAxisInput(InputAction action, bool useYComponentForVector2)
    {
        if (action == null)
        {
            return 0f;
        }

        if (IsVector2Action(action))
        {
            Vector2 value = action.ReadValue<Vector2>();
            return useYComponentForVector2 ? value.y : value.x;
        }

        return action.ReadValue<float>();
    }

    private static bool IsVector2Action(InputAction action)
    {
        if (action == null)
        {
            return false;
        }

        if (string.Equals(action.expectedControlType, "Vector2", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (action.activeControl != null && action.activeControl.valueType == typeof(Vector2))
        {
            return true;
        }

        for (int i = 0; i < action.controls.Count; i++)
        {
            if (action.controls[i] != null && action.controls[i].valueType == typeof(Vector2))
            {
                return true;
            }
        }

        return false;
    }

    private sealed class InputBinding
    {
        public InputBinding(
            string keyboardActionName,
            string controllerActionName,
            bool throwIfKeyboardActionMissing,
            bool throwIfControllerActionMissing)
        {
            KeyboardActionName = keyboardActionName;
            ControllerActionName = controllerActionName;
            ThrowIfKeyboardActionMissing = throwIfKeyboardActionMissing;
            ThrowIfControllerActionMissing = throwIfControllerActionMissing;
        }

        public string KeyboardActionName { get; private set; }
        public string ControllerActionName { get; private set; }
        public bool ThrowIfKeyboardActionMissing { get; private set; }
        public bool ThrowIfControllerActionMissing { get; private set; }
        public InputAction KeyboardAction { get; set; }
        public InputAction ControllerAction { get; set; }
    }
}
