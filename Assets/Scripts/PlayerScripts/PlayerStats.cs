using System;
using UnityEngine;

/// <summary>
/// Centralised player statistics. Holds base values and upgrade bonuses.
/// Other systems query this component for final computed values.
/// Addresses issues #1 (player stats) and #27 (upgrade types).
/// </summary>
public sealed class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Base Stats")]
    [SerializeField, Min(1f)] private float baseDamage = 10f;
    [SerializeField, Min(1f)] private float baseMaxHealth = 100f;
    [SerializeField, Min(0.1f)] private float baseAttackSpeed = 1f;
    [SerializeField, Range(0f, 0.9f)] private float baseDefense = 0f;
    [SerializeField, Min(0.1f)] private float baseDashCooldown = 1f;

    // Upgrade bonuses (additive for flat, multiplicative where noted)
    private float bonusDamage;
    private float bonusMaxHealth;
    private float attackSpeedMultiplier = 1f;
    private float defenseBonus;
    private float dashCooldownReduction;

    public float TotalDamage => baseDamage + bonusDamage;
    public float BaseDamage  => baseDamage;
    public float TotalMaxHealth => baseMaxHealth + bonusMaxHealth;
    public float TotalDefense => Mathf.Clamp01(baseDefense + defenseBonus);
    public float AttackSpeedMultiplier => Mathf.Max(0.1f, attackSpeedMultiplier);
    public float TotalDashCooldown => Mathf.Max(0.1f, baseDashCooldown - dashCooldownReduction);

    public event Action OnStatsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Adds a flat damage bonus.</summary>
    public void AddDamageBoost(float amount)
    {
        bonusDamage += amount;
        OnStatsChanged?.Invoke();
    }

    /// <summary>Adds bonus max health. Caller should also heal the player by the same amount.</summary>
    public void AddMaxHealthBoost(float amount)
    {
        bonusMaxHealth += amount;
        OnStatsChanged?.Invoke();
    }

    /// <summary>Multiplies attack speed (e.g. 1.2 = 20% faster).</summary>
    public void AddAttackSpeedBoost(float multiplier)
    {
        attackSpeedMultiplier *= multiplier;
        OnStatsChanged?.Invoke();
    }

    /// <summary>Adds flat defense (damage reduction fraction).</summary>
    public void AddDefenseBoost(float amount)
    {
        defenseBonus += amount;
        OnStatsChanged?.Invoke();
    }

    /// <summary>Reduces dash cooldown by the given seconds.</summary>
    public void ReduceDashCooldown(float seconds)
    {
        dashCooldownReduction += seconds;
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// Applies incoming damage after defense reduction.
    /// </summary>
    public float ApplyDefense(float incomingDamage)
    {
        return incomingDamage * (1f - TotalDefense);
    }
}
