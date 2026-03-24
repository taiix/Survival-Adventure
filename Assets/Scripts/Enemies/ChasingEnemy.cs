using UnityEngine;

public class ChasingEnemy : BaseEnemy
{

    [Header("Attack")]
    [SerializeField] private Collider headCollider;

    private bool canDealDamage;

    protected override void Awake()
    {
        base.Awake();
        currentWaitTime = Random.Range(minWaitTime, maxWaitTime);

        // Ensure head collider is set as trigger
        if (headCollider != null)
        {
            headCollider.isTrigger = true;
        }
    }
    private void DealDamageInRange()
    {
        if (headCollider == null)
        {
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(
            headCollider.bounds.center,
            headCollider.bounds.extents.magnitude);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                continue;
            }
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"Dealt {attackDamage} damage to {hitCollider.name}");
            }
        }
    }
}