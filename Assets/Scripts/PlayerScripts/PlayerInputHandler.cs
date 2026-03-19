using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputHandler
{
    private const float inputDeadzone = 0.1f;

    private readonly InputActionAsset actionAsset;
    private readonly string keyboardActionMapName;
    private readonly string controllerActionMapName;
    private readonly string moveForwardActionName;
    private readonly string turnActionName;
    private readonly string controllerMoveActionName;
    private readonly string sprintActionName;

    private InputActionMap keyboardInputActionMap;
    private InputActionMap controllerInputActionMap;

    private InputAction keyboardMoveInputAction;
    private InputAction keyboardTurnInputAction;
    private InputAction keyboardSprintInputAction;

    private InputAction controllerMoveInputAction;
    private InputAction controllerTurnInputAction;
    private InputAction controllerSprintInputAction;

    public float MoveInput { get; private set; }
    public Vector2 MoveInputVector { get; private set; }
    public float TurnInput { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool UsingControllerInput { get; private set; }

    public PlayerInputHandler(
        InputActionAsset actionAsset,
        string keyboardActionMapName,
        string controllerActionMapName,
        string moveForwardActionName,
        string turnActionName,
        string controllerMoveActionName,
        string sprintActionName)
    {
        this.actionAsset = actionAsset;
        this.keyboardActionMapName = keyboardActionMapName;
        this.controllerActionMapName = controllerActionMapName;
        this.moveForwardActionName = moveForwardActionName;
        this.turnActionName = turnActionName;
        this.controllerMoveActionName = controllerMoveActionName;
        this.sprintActionName = sprintActionName;

        ResolveActions();
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

        float keyboardMove = ReadForwardInput(keyboardMoveInputAction);
        float keyboardTurn = ReadTurnInput(keyboardTurnInputAction, null);

        Vector2 controllerMoveVector = ReadMoveVector(controllerMoveInputAction);
        float controllerTurn = ReadTurnInput(controllerTurnInputAction, controllerMoveInputAction);

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

        if (UsingControllerInput)
        {
            MoveInputVector = controllerMoveVector;
            MoveInput = controllerMoveVector.magnitude;
            TurnInput = 0f;
            IsSprinting = ReadSprintInput(controllerSprintInputAction ?? keyboardSprintInputAction);
        }
        else
        {
            MoveInputVector = new Vector2(0f, keyboardMove);
            MoveInput = keyboardMove;
            TurnInput = keyboardTurn;
            IsSprinting = ReadSprintInput(keyboardSprintInputAction ?? controllerSprintInputAction);
        }

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

        keyboardMoveInputAction = ResolveAction(keyboardInputActionMap, moveForwardActionName, true);
        keyboardTurnInputAction = ResolveAction(keyboardInputActionMap, turnActionName, true);
        keyboardSprintInputAction = ResolveAction(keyboardInputActionMap, sprintActionName, false);

        controllerMoveInputAction = ResolveAction(controllerInputActionMap, controllerMoveActionName, false);
        controllerTurnInputAction = ResolveAction(controllerInputActionMap, turnActionName, false);
        controllerSprintInputAction = ResolveAction(controllerInputActionMap, sprintActionName, false);
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
        if (actionAsset == null)
        {
            return null;
        }

        if (actionMap != null)
        {
            return actionMap.FindAction(actionName, throwIfNotFound);
        }

        return actionAsset.FindAction(actionName, throwIfNotFound);
    }

    private static float ReadForwardInput(InputAction action)
    {
        return ReadAxisInput(action, useYComponentForVector2: true);
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

        float value = action.ReadValue<float>();
        return new Vector2(0f, value);
    }

    private static float ReadTurnInput(InputAction action, InputAction vectorFallbackAction)
    {
        float turnInput = ReadAxisInput(action, useYComponentForVector2: false);
        if (Mathf.Abs(turnInput) > 0.0001f)
        {
            return turnInput;
        }

        return ReadAxisInput(vectorFallbackAction, useYComponentForVector2: false);
    }

    private static bool ReadSprintInput(InputAction action)
    {
        if (action == null)
        {
            return false;
        }

        if (IsVector2Action(action))
        {
            Vector2 value = action.ReadValue<Vector2>();
            return value.y > 0.5f;
        }

        return action.ReadValue<float>() > 0.5f;
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
}
