using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private Collider swordCollider;

    private Animator animator;
    private PlayerInputHandler inputHandler;
    private PlayerStateManager stateManager;
    private PlayerStats playerStats;
    private bool previousAttackPressed;

    public void Initialize(PlayerInputHandler inputHandler, PlayerStateManager stateManager)
    {
        this.inputHandler = inputHandler;
        this.stateManager = stateManager;
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (inputHandler == null || stateManager == null)
        {
            return;
        }

        bool isAttackPressed = inputHandler.IsAttacking;

        if (isAttackPressed && !previousAttackPressed && stateManager.IsMovementAllowed())
        {
            Attack();
        }

        previousAttackPressed = isAttackPressed;
    }

    public void Attack()
    {
        stateManager.SetState(PlayerState.Attacking);
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        AudioManager.Instance?.PlaySwordSwing();

        // Attack completes immediately, return to Normal state
        stateManager.SetState(PlayerState.Normal);
    }

    private void DealDamageInRange()
    {
        if (swordCollider == null)
        {
            return;
        }

        float damage = playerStats != null ? playerStats.TotalDamage : attackDamage;

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
                damageable.TakeDamage(damage);
            }
        }
    }
}
