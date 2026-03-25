using UnityEngine;

public class KnightBossEnemy : BaseEnemy
{
    [Header("Attacks")]
    [SerializeField] private Collider swordCollider;

    private readonly int runtimeStateHash = Animator.StringToHash("Base Layer.KnightBossQuickAttack");
    private readonly int speedHash = Animator.StringToHash("Speed");

    // When normalizedTime reaches this value, we consider the attack "done enough" to resume AI decisions.
    private const float AttackAnimationExitTime = 0.95f;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void UpdateBehavior()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isInAttackAnimation = stateInfo.fullPathHash == runtimeStateHash;
        bool isAttackActive = currentState == AIState.Attack || isInAttackAnimation;

        // While attack is active, never move. Once the animation is basically done, decide next action.
        if (isAttackActive)
        {
            StopMovement();
            FacePlayer();

            // Still in the middle of the animation -> keep locking movement.
            if (isInAttackAnimation && stateInfo.normalizedTime < AttackAnimationExitTime)
            {
                return;
            }

            // Attack animation finished (or we left the state). Decide what to do next.
            if (IsPlayerInAttackRange())
            {
                currentState = AIState.Attack;
                Attack(); // cooldown in BaseEnemy prevents spamming
                return;
            }

            if (IsPlayerInDetectionRange())
            {
                currentState = AIState.Chase;
                navMeshAgent.speed = chasingSpeed;
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(playerTransform.position);
                return;
            }

            // Player not detected anymore -> fall back to patrol
            HandlePatrolState();
            return;
        }

        // Normal behavior when not attacking
        if (IsPlayerInDetectionRange())
        {
            HandleCombatState();
        }
        else
        {
            HandlePatrolState();
        }
    }

    protected override void HandleCombatState()
    {
        // Attack or chase logic
        if (IsPlayerInAttackRange())
        {
            if (currentState != AIState.Attack)
            {
                currentState = AIState.Attack;
                StopMovement();
            }

            FacePlayer();
            Attack();
        }
        else
        {
            // Player is out of attack range - start chasing
            if (currentState != AIState.Chase)
            {
                currentState = AIState.Chase;
                navMeshAgent.speed = chasingSpeed;
                navMeshAgent.isStopped = false;
            }

            FacePlayer();
            navMeshAgent.SetDestination(playerTransform.position);
        }
    }

    protected override void UpdateAnimator()
    {
        if (animator == null || navMeshAgent == null)
            return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isInAttackAnimation = stateInfo.fullPathHash == runtimeStateHash;
        bool isAttackActive = currentState == AIState.Attack || isInAttackAnimation;

        // During attack (including transitions), don't let blend tree fall to 0 (prevents snapping to idle).
        if (isAttackActive)
        {
            animator.SetFloat(speedHash, 0.1f);
            return;
        }

        // For chase/patrol, calculate speed based on NavMeshAgent velocity
        if (currentState == AIState.Chase || currentState == AIState.Patrol)
        {
            float speed = navMeshAgent.velocity.magnitude;
            float targetSpeed = 0f;

            if (speed > 0.1f)
            {
                if (speed > walkSpeed)
                {
                    float t = (speed - walkSpeed) / (chasingSpeed - walkSpeed);
                    targetSpeed = Mathf.Lerp(sprintThreshold, 1f, t);
                }
                else
                {
                    float t = speed / walkSpeed;
                    targetSpeed = Mathf.Lerp(0f, sprintThreshold, t);
                }
            }

            targetSpeed = Mathf.Clamp01(targetSpeed);
            animator.SetFloat(speedHash, targetSpeed);
        }
        else
        {
            animator.SetFloat(speedHash, 0f);
        }
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
