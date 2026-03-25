using System;
using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;

    [SerializeField] private float currentHealth;

    private HitFeedback hitFeedback;
    private PlayerStats playerStats;
    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0f;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        hitFeedback = GetComponent<HitFeedback>();
        playerStats = GetComponent<PlayerStats>();
        CurrentHealth = maxHealth;
        currentHealth = CurrentHealth;
    }

    /// <summary>Sets a new max health ceiling (e.g. after an HP upgrade).</summary>
    public void SetMaxHealth(float newMax)
    {
        maxHealth = Mathf.Max(1f, newMax);
        CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        currentHealth = CurrentHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return;
        }

        // Apply defense reduction if PlayerStats is present
        float mitigatedAmount = playerStats != null ? playerStats.ApplyDefense(amount) : amount;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - mitigatedAmount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        currentHealth = CurrentHealth;

        hitFeedback.PlayHitFeedback();
        AudioManager.Instance?.PlayPlayerHit();
        if (!IsAlive)
        {
            OnDeath?.Invoke();
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
