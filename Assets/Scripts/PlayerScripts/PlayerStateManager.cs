using System;
using UnityEngine;

/// <summary>
/// Manages the player's current state and state transitions.
/// </summary>
public sealed class PlayerStateManager
{
    private PlayerState currentState = PlayerState.Normal;

    public PlayerState CurrentState => currentState;

    public event Action<PlayerState, PlayerState> OnStateChanged;

    public void SetState(PlayerState newState)
    {
        if (newState == currentState)
        {
            return;
        }

        PlayerState previousState = currentState;
        currentState = newState;
        OnStateChanged?.Invoke(previousState, newState);
    }

    public bool IsState(PlayerState state) => currentState == state;
    public bool IsMovementAllowed() => currentState == PlayerState.Normal;
}