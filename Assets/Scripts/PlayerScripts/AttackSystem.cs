using System;
using UnityEngine;

/// <summary>Handles melee attacks for the player using an overlap-sphere hit-check.</summary>
public sealed class AttackSystem : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField, Min(0f)] private float attackDamage = 15f;
    [SerializeField, Min(0f)] private float attackRange = 2f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    public float AttackDamage => attackDamage;
    public float AttackRange => attackRange;
    public bool IsReady => attackTimer <= 0f;

    public event Action OnAttack;

    private float attackTimer;

    private void Update()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Attempts a melee attack. Returns true when the attack was executed,
    /// false when still on cooldown.
    /// </summary>
    public bool TryAttack()
    {
        if (attackTimer > 0f)
        {
            return false;
        }

        attackTimer = attackCooldown;
        OnAttack?.Invoke();

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        foreach (Collider hit in hits)
        {
            EnemyStats enemy = hit.GetComponentInParent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
