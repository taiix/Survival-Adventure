using UnityEngine;
using UnityEngine.AI;

public abstract class BaseEnemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 50f;
    [SerializeField] protected float walkSpeed = 2f;
    [SerializeField] protected float chasingSpeed = 5f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackCooldown = 1.5f;

    [Header("Detection")]
    [SerializeField] protected float detectionRange = 15f;

    [Header("Patrol")]
    [SerializeField] protected float minWaitTime = 2f;
    [SerializeField] protected float maxWaitTime = 5f;
    [SerializeField] private bool canPatrol = true;

    [Header("Animator")]
    [SerializeField, Range(0f, 1f)] protected float sprintThreshold = 0.5f;
    [SerializeField, Min(0.01f)] private float speedSmoothTime = 0.1f;

    protected enum AIState { Idle, Patrol, Chase, Attack }
    protected AIState currentState = AIState.Idle;

    protected float currentHealth;
    protected float lastAttackTime;
    protected Transform playerTransform;
    protected NavMeshAgent navMeshAgent;
    protected Animator animator;
    protected HitFeedback hitFeedback;

    protected float waitTimer;
    protected float currentWaitTime;

    private float currentAnimatorSpeed;
    private float animatorSpeedVelocity;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        hitFeedback = GetComponent<HitFeedback>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
        currentAnimatorSpeed = 0f;
    }

    protected virtual void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        UpdateBehavior();
        UpdateAnimator();
    }

    /// <summary>
    /// Default behavior: patrol when no player detected, chase and attack when player is detected.
    /// Override this method to provide custom AI behavior.
    /// </summary>
    protected virtual void UpdateBehavior()
    {
        if (IsPlayerInDetectionRange())
        {
            HandleCombatState();
        }
        else if (canPatrol)
        {
            HandlePatrolState();
        }
    }

    protected virtual void HandleCombatState()
    {
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

    protected virtual void HandlePatrolState()
    {
        if (currentState == AIState.Chase || currentState == AIState.Attack)
        {
            currentState = AIState.Idle;
            StopMovement();
            waitTimer = 0f;
        }

        switch (currentState)
        {
            case AIState.Idle:
                waitTimer += Time.deltaTime;
                if (waitTimer >= currentWaitTime)
                {
                    SetRandomPatrolDestination();
                }
                break;

            case AIState.Patrol:
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    currentState = AIState.Idle;
                    StopMovement();
                    waitTimer = 0f;
                    currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
                }
                break;
        }
    }

    protected virtual void SetRandomPatrolDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * detectionRange;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
        {
            currentState = AIState.Patrol;
            navMeshAgent.speed = walkSpeed;
            navMeshAgent.isStopped = false;

            Vector3 directionToPoint = (hit.position - transform.position).normalized;
            if (directionToPoint.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPoint);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            navMeshAgent.SetDestination(hit.position);
        }
    }

    /// <summary>
    /// Make the enemy face the player.
    /// </summary>
    protected void FacePlayer()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
    }

    /// <summary>
    /// Stops the enemy's movement and syncs the animator.
    /// </summary>
    protected void StopMovement()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
    }

    protected virtual void UpdateAnimator()
    {
        if (animator == null || navMeshAgent == null)
            return;

        float targetSpeed = 0f;

        // Calculate target speed based on NavMeshAgent state
        if (!navMeshAgent.isStopped)
        {
            float speed = navMeshAgent.velocity.magnitude;

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
        }

        // Clamp target speed BEFORE smoothing
        targetSpeed = Mathf.Clamp01(targetSpeed);

        // Smoothly interpolate the animator speed parameter
        currentAnimatorSpeed = Mathf.SmoothDamp(
            currentAnimatorSpeed,
            targetSpeed,
            ref animatorSpeedVelocity,
            speedSmoothTime);

        // Clamp after smooth damp (critical!)
        currentAnimatorSpeed = Mathf.Clamp01(currentAnimatorSpeed);

        animator.SetFloat("Speed", currentAnimatorSpeed);
    }

    protected bool IsPlayerInDetectionRange()
    {
        return playerTransform != null &&
               Vector3.Distance(transform.position, playerTransform.position) <= detectionRange;
    }

    protected bool IsPlayerInAttackRange()
    {
        return playerTransform != null &&
               Vector3.Distance(transform.position, playerTransform.position) <= attackRange;
    }

    protected virtual void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (hitFeedback != null)
        {
            hitFeedback.PlayHitFeedback();
        }

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (navMeshAgent != null && navMeshAgent.hasPath)
        {
            Gizmos.color = currentState == AIState.Chase ? Color.red : Color.green;
            var path = navMeshAgent.path;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}