using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackDamage = 10f;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Attack()
    {
        // Play attack animation
        Debug.Log($"Player attacks with {attackDamage} damage!");
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        // Implement attack logic here (e.g., detect enemies in range and apply damage)
    }
}
