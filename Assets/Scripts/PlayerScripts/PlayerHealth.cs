using System;
using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;

    [SerializeField] private float currentHealth;

    private HitFeedback hitFeedback;
    private PlayerStateManager stateManager;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0f;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        hitFeedback = GetComponent<HitFeedback>();
        stateManager = GetComponent<PlayerController>()?.GetStateManager();

        CurrentHealth = maxHealth;
        currentHealth = CurrentHealth;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return;
        }

        if (stateManager != null && stateManager.IsInvulnerable())
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        EventManager.OnPlayerHealthChanged?.Invoke(CurrentHealth);
        currentHealth = CurrentHealth;

        hitFeedback.PlayHitFeedback();
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
        EventManager.OnPlayerHealthChanged?.Invoke(CurrentHealth);
    }
}
