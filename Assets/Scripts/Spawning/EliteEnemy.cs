using UnityEngine;

/// <summary>
/// Elite enemy variant – inherits BaseEnemy behaviour but with boosted stats.
/// Addresses issue #21 (spawn elite enemies).
/// </summary>
public class EliteEnemy : ChasingEnemy
{
    [Header("Elite Modifiers")]
    [SerializeField, Min(1f)] private float healthMultiplier = 2f;
    [SerializeField, Min(1f)] private float damageMultiplier = 1.5f;
    [SerializeField, Min(1f)] private float speedMultiplier = 1.2f;
    [SerializeField] private int eliteGoldDrop = 25;

    [Header("Visual")]
    [SerializeField] private Renderer[] eliteRenderers;
    [SerializeField] private Color eliteTint = new Color(0.8f, 0.2f, 0.8f);

    protected override void Awake()
    {
        base.Awake();

        // Boost stats via the serialized fields exposed in BaseEnemy.
        // We use the multipliers to scale the protected fields directly.
        maxHealth *= healthMultiplier;
        currentHealth = maxHealth;
        attackDamage *= damageMultiplier;
        walkSpeed *= speedMultiplier;
        chasingSpeed *= speedMultiplier;
        goldDrop = eliteGoldDrop;

        ApplyEliteTint();
    }

    private void ApplyEliteTint()
    {
        if (eliteRenderers == null || eliteRenderers.Length == 0) return;
        foreach (Renderer r in eliteRenderers)
        {
            if (r != null)
                r.material.color = eliteTint;
        }
    }
}
