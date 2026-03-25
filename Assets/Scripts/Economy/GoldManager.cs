using System;
using UnityEngine;

/// <summary>
/// Manages the player's gold / currency.
/// Addresses issues #9, #30 (gold/economy system).
/// </summary>
public sealed class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    [SerializeField, Min(0)] private int startingGold = 0;

    private int currentGold;

    public int Gold => currentGold;

    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentGold = startingGold;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Resets gold to the starting amount. Call when starting a new game session.</summary>
    public void ResetGold()
    {
        currentGold = startingGold;
        OnGoldChanged?.Invoke(currentGold);
    }

    /// <summary>Returns true if the player can afford the cost.</summary>
    public bool CanAfford(int cost) => currentGold >= cost;

    /// <summary>Adds gold to the player's wallet and fires the changed event.</summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        currentGold += amount;
        OnGoldChanged?.Invoke(currentGold);
    }

    /// <summary>
    /// Attempts to spend gold. Returns true and deducts the cost if affordable.
    /// </summary>
    public bool TrySpend(int cost)
    {
        if (!CanAfford(cost)) return false;
        currentGold -= cost;
        OnGoldChanged?.Invoke(currentGold);
        return true;
    }
}
