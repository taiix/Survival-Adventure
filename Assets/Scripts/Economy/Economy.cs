using System;
using UnityEngine;

/// <summary>Manages the player's coin balance and exposes simple add/spend helpers.</summary>
public sealed class Economy : MonoBehaviour
{
    [Header("Starting Economy")]
    [SerializeField, Min(0)] private int startingCoins = 0;

    public int Coins { get; private set; }

    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        Coins = startingCoins;
    }

    /// <summary>Adds the given number of coins (must be positive).</summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    /// <summary>
    /// Tries to spend coins. Returns true and deducts the amount when the player
    /// can afford it; returns false otherwise.
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || Coins < amount)
        {
            return false;
        }

        Coins -= amount;
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }
}
