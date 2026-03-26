using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Collider swordCollider;

    private Animator animator;
    private PlayerInputHandler inputHandler;
    private PlayerStateManager stateManager;
    private bool previousAttackPressed;
    private PlayerStats playerStats;

    public void Initialize(PlayerInputHandler inputHandler, PlayerStateManager stateManager)
    {
        this.inputHandler = inputHandler;
        this.stateManager = stateManager;
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
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

        // Attack completes immediately, return to Normal state
        stateManager.SetState(PlayerState.Normal);
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
                damageable.TakeDamage(playerStats.GetCurrentDamage());
            }
        }
    }
}
