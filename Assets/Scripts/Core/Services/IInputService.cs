using UnityEngine.InputSystem;

public interface IInputService
{
    InputAction GetInputAction(string inputKey);
    bool IsUsingController { get; }
}