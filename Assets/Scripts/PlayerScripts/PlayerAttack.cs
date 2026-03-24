using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private Collider swordCollider;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Attack()
    {
        Debug.Log($"Player attacks with {attackDamage} damage!");

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        //DealDamageInRange();
    }

    private void DealDamageInRange()
    {
        if (swordCollider == null)
        {
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(
            swordCollider.bounds.center,
            swordCollider.bounds.extents.magnitude);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
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
