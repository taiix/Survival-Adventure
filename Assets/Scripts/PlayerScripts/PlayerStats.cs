using System;
using UnityEngine;

/// <summary>Tracks player hunger and thirst, applying damage when either reaches zero.</summary>
public sealed class PlayerStats : MonoBehaviour
{
    [Header("Hunger")]
    [SerializeField, Min(1f)] private float maxHunger = 100f;
    [SerializeField, Min(0f)] private float hungerDrainRate = 2f;

    [Header("Thirst")]
    [SerializeField, Min(1f)] private float maxThirst = 100f;
    [SerializeField, Min(0f)] private float thirstDrainRate = 3f;

    [Header("Damage on Empty")]
    [SerializeField, Min(0f)] private float starvationDamageRate = 5f;
    [SerializeField, Min(0f)] private float dehydrationDamageRate = 8f;

    public float MaxHunger => maxHunger;
    public float MaxThirst => maxThirst;
    public float CurrentHunger { get; private set; }
    public float CurrentThirst { get; private set; }
    public bool IsHungry => CurrentHunger <= 0f;
    public bool IsThirsty => CurrentThirst <= 0f;

    public event Action<float, float> OnHungerChanged;
    public event Action<float, float> OnThirstChanged;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        CurrentHunger = maxHunger;
        CurrentThirst = maxThirst;
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        DrainStats();
        ApplyDeprivationDamage();
    }

    private void DrainStats()
    {
        if (CurrentHunger > 0f)
        {
            CurrentHunger = Mathf.Max(0f, CurrentHunger - hungerDrainRate * Time.deltaTime);
            OnHungerChanged?.Invoke(CurrentHunger, maxHunger);
        }

        if (CurrentThirst > 0f)
        {
            CurrentThirst = Mathf.Max(0f, CurrentThirst - thirstDrainRate * Time.deltaTime);
            OnThirstChanged?.Invoke(CurrentThirst, maxThirst);
        }
    }

    private void ApplyDeprivationDamage()
    {
        if (playerHealth == null || !playerHealth.IsAlive)
        {
            return;
        }

        if (IsHungry)
        {
            playerHealth.TakeDamage(starvationDamageRate * Time.deltaTime);
        }

        if (IsThirsty)
        {
            playerHealth.TakeDamage(dehydrationDamageRate * Time.deltaTime);
        }
    }

    /// <summary>Restores the given amount of hunger (e.g. from eating food).</summary>
    public void Eat(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        CurrentHunger = Mathf.Min(maxHunger, CurrentHunger + amount);
        OnHungerChanged?.Invoke(CurrentHunger, maxHunger);
    }

    /// <summary>Restores the given amount of thirst (e.g. from drinking water).</summary>
    public void Drink(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        CurrentThirst = Mathf.Min(maxThirst, CurrentThirst + amount);
        OnThirstChanged?.Invoke(CurrentThirst, maxThirst);
    }
}
