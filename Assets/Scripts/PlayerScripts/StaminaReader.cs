using UnityEngine;

/// <summary>
/// Thin bridge to expose stamina ratio to UI without modifying PlayerController.
/// Attach to the same GameObject as PlayerController.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class StaminaReader : MonoBehaviour
{
    private PlayerStamina staminaInstance;

    /// <summary>0–1 stamina ratio. Returns 1 if stamina cannot be found.</summary>
    public float StaminaRatio
    {
        get
        {
            if (staminaInstance != null)
                return staminaInstance.MaxStamina > 0f
                    ? staminaInstance.CurrentStamina / staminaInstance.MaxStamina
                    : 1f;
            return 1f;
        }
    }

    /// <summary>Called by PlayerController or manually to register the stamina instance.</summary>
    public void Register(PlayerStamina stamina)
    {
        staminaInstance = stamina;
    }
}
