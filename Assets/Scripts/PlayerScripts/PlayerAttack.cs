using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Collider swordCollider;

    [Header("Quick Attacks")]
    [SerializeField, Min(1)] private int quickAttackVariants = 3;
    [SerializeField] private string quickAttackIndexParam = "QuickAttackIndex";
    [SerializeField] private string quickAttackTriggerParam = "QuickAttack";

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
        bool pressedThisFrame = isAttackPressed && !previousAttackPressed;

        if (pressedThisFrame && stateManager.IsMovementAllowed())
        {
            Attack();
        }

        previousAttackPressed = isAttackPressed;
    }

    public void Attack()
    {
        // Don't restart an attack if we're already in the attack state.
        if (stateManager.IsState(PlayerState.Attacking))
        {
            return;
        }

        stateManager.SetState(PlayerState.Attacking);

        if (animator != null)
        {
            int max = Mathf.Max(1, quickAttackVariants);
            int attackIndex = Random.Range(0, max);

            animator.SetInteger(quickAttackIndexParam, attackIndex);
            animator.ResetTrigger(quickAttackTriggerParam);
            animator.SetTrigger(quickAttackTriggerParam);
        }
    }

    // Animation Event: call near the hit frame(s)
    public void DealDamageInRange()
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

    // Animation Event: call on the LAST frame of every quick attack clip
    public void OnQuickAttackFinished()
    {
        if (stateManager != null && stateManager.IsState(PlayerState.Attacking))
        {
            stateManager.SetState(PlayerState.Normal);
        }
    }
}
