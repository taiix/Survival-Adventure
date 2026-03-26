using UnityEngine;

public class KnightBossEnemy : BaseEnemy
{
    [Header("Attacks")]
    [SerializeField] private Collider swordCollider;

    [Header("Special Attack")]
    [SerializeField, Range(0f, 1f)] private float specialAttackChance = 0.25f;
    [SerializeField, Min(0f)] private float specialAttackCooldown = 6f;
    [SerializeField] private string specialAttackTriggerName = "SpecialAttack";
    [SerializeField, Min(0f)] private float specialAttackRange = 12f;

    [Header("Special Attack FX (spawned)")]
    [SerializeField] private GameObject specialAttackFxPrefab;
    [SerializeField] private GameObject specialAttackChargeFxPrefab;
    [SerializeField] private Transform specialAttackFxSpawnPoint;
    [SerializeField, Min(0.1f)] private float specialAttackFxLifetime = 2.5f;

    [Header("Special Attack Hitbox")]
    [SerializeField] private KnightBossSpecialAttackHitbox rockTrailHitbox;

    private readonly int runtimeStateHash = Animator.StringToHash("Base Layer.KnightBossQuickAttack");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int specialAttackStateHash = Animator.StringToHash("Base Layer.KnightBossSpecialAttack");
    private readonly int inMeleeRangeHash = Animator.StringToHash("InMeleeRange");

    private const float AttackAnimationExitTime = 0.95f;

    private float nextSpecialAttackTime;
    private bool isSpecialAttackInProgress;

    // Prevent multiple spawns if the FX cue event fires more than once.
    private bool specialFxPlayedThisAttack;

    protected override void UpdateBehavior()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        bool isInQuickAttackAnimation = stateInfo.fullPathHash == runtimeStateHash;
        bool isInSpecialAttackAnimation = stateInfo.fullPathHash == specialAttackStateHash;

        bool isAttackActive =
            currentState == AIState.Attack ||
            isInQuickAttackAnimation ||
            isInSpecialAttackAnimation ||
            isSpecialAttackInProgress;

        if (isAttackActive)
        {
            StopMovement();
            FacePlayer();

            // Keep locking movement during the animation.
            if ((isInQuickAttackAnimation || isInSpecialAttackAnimation) &&
                stateInfo.normalizedTime < AttackAnimationExitTime)
            {
                return;
            }

            // After the animation is basically done, decide what to do next.
            if (IsPlayerInDetectionRange())
            {
                currentState = AIState.Chase;
                navMeshAgent.speed = chasingSpeed;
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(playerTransform.position);
                return;
            }

            HandlePatrolState();
            return;
        }

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
        // Priority:
        // 1) If in melee range -> normal Attack() (quick attack)
        // 2) Else if in special range -> maybe special
        // 3) Else -> chase
        if (IsPlayerInAttackRange())
        {
            if (currentState != AIState.Attack)
            {
                currentState = AIState.Attack;
                StopMovement();
            }

            FacePlayer();
            base.Attack();
            return;
        }

        if (IsPlayerInSpecialAttackRange())
        {
            // Special can be used at range; if we fail to use it, we chase (no melee attack here).
            if (CanUseSpecialAttack() && RollSpecialAttack())
            {
                currentState = AIState.Attack;
                StopMovement();
                FacePlayer();
                StartSpecialAttack();
                return;
            }
        }

        // Chase when not in melee, and also when special is unavailable/failed roll.
        if (currentState != AIState.Chase)
        {
            currentState = AIState.Chase;
            navMeshAgent.speed = chasingSpeed;
            navMeshAgent.isStopped = false;
        }

        FacePlayer();
        navMeshAgent.SetDestination(playerTransform.position);
    }

    private bool IsPlayerInSpecialAttackRange()
    {
        return playerTransform != null &&
               Vector3.Distance(transform.position, playerTransform.position) <= specialAttackRange;
    }

    private bool CanUseSpecialAttack() => Time.time >= nextSpecialAttackTime;

    private bool RollSpecialAttack() => Random.value <= specialAttackChance;

    private void StartSpecialAttack()
    {
        // Use base cooldown gate too (prevents starting another attack immediately).
        lastAttackTime = Time.time;

        nextSpecialAttackTime = Time.time + specialAttackCooldown;
        isSpecialAttackInProgress = true;
        specialFxPlayedThisAttack = false;

        if (animator != null && !string.IsNullOrWhiteSpace(specialAttackTriggerName))
        {
            animator.ResetTrigger(specialAttackTriggerName);
            animator.SetTrigger(specialAttackTriggerName);

            specialAttackChargeFxPrefab.SetActive(true);
        }
    }


    public void OnSpecialAttackFxCue()
    {
        if (!isSpecialAttackInProgress || specialFxPlayedThisAttack)
        {
            return;
        }

        specialFxPlayedThisAttack = true;

        SpawnSpecialAttackFx();

        if (rockTrailHitbox != null)
        {
            rockTrailHitbox.Play();
        }
    }

    // Animation Event: call on the LAST frame (or near end) to unlock AI and allow chase/other actions.
    public void OnSpecialAttackAnimationFinished()
    {
        if (!isSpecialAttackInProgress)
        {
            return;
        }

        isSpecialAttackInProgress = false;

        // Always unstop here to prevent "idle stuck" if AI doesn't tick chase immediately.
        navMeshAgent.isStopped = false;

        if (IsPlayerInDetectionRange())
        {
            currentState = AIState.Chase;
            navMeshAgent.speed = chasingSpeed;
            navMeshAgent.SetDestination(playerTransform.position);
        }
    }

    private void SpawnSpecialAttackFx()
    {
        if (specialAttackFxPrefab == null)
        {
            return;
        }

        Transform spawn = specialAttackFxSpawnPoint != null ? specialAttackFxSpawnPoint : transform;

        GameObject fxInstance = Instantiate(
            specialAttackFxPrefab,
            spawn.position,
            spawn.rotation);

        Destroy(fxInstance, specialAttackFxLifetime);
        specialAttackChargeFxPrefab.SetActive(false);
    }

    protected override void UpdateAnimator()
    {
        if (animator == null || navMeshAgent == null)
        {
            return;
        }

        // Drive Animator melee-range bool for chaining logic
        animator.SetBool(inMeleeRangeHash, IsPlayerInAttackRange());

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isInQuickAttackAnimation = stateInfo.fullPathHash == runtimeStateHash;
        bool isInSpecialAttackAnimation = stateInfo.fullPathHash == specialAttackStateHash;

        bool isAttackActive =
            currentState == AIState.Attack ||
            isInQuickAttackAnimation ||
            isInSpecialAttackAnimation ||
            isSpecialAttackInProgress;

        if (isAttackActive)
        {
            animator.SetFloat(speedHash, 0.1f);
            return;
        }

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
