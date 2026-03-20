using System;
using UnityEngine;

/// <summary>Holds health, combat and reward stats for an enemy.</summary>
public sealed class EnemyStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(1f)] private float maxHealth = 50f;

    [Header("Combat")]
    [SerializeField, Min(0f)] private float attackDamage = 10f;
    [SerializeField, Min(0f)] private float attackRange = 1.5f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 1.5f;

    [Header("Rewards")]
    [SerializeField, Min(0)] private int coinReward = 5;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public float AttackDamage => attackDamage;
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public int CoinReward => coinReward;
    public bool IsAlive => CurrentHealth > 0f;

    public event Action<float, float> OnHealthChanged;
    public event Action<EnemyStats> OnDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (!IsAlive)
        {
            OnDeath?.Invoke(this);
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
