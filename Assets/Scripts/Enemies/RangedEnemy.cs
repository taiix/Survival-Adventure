using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RangedEnemy : BaseEnemy
{
    private enum State { Idle, Patrol, Chase, Attack }
    private State currentState = State.Idle;

    [Header("Patrol")]
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwDelay = 0.5f; 
    [SerializeField] private float projectileTravelTime = 1.5f;
    [SerializeField] private float projectileArcHeight = 3f;

    private float waitTimer;
    private float currentWaitTime;
    private bool isAttacking;

    protected override void Awake()
    {
        base.Awake();
        currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
    }

    protected override void UpdateBehavior()
    {
        if (isAttacking)
        {
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

    private void HandleCombatState()
    {
        if (IsPlayerInAttackRange())
        {
            if (currentState != State.Attack)
            {
                currentState = State.Attack;
                navMeshAgent.isStopped = true;
                navMeshAgent.velocity = Vector3.zero;
            }

            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            directionToPlayer.y = 0;
            if (directionToPlayer.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(directionToPlayer);
            }

            if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
            {
                if (animator != null)
                {
                    animator.SetTrigger("Attack");
                }
            }
        }
        else
        {
            if (currentState != State.Chase)
            {
                currentState = State.Chase;
                navMeshAgent.speed = chasingSpeed;
                navMeshAgent.isStopped = false;
            }

            // Face player while chasing
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            directionToPlayer.y = 0;
            if (directionToPlayer.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(directionToPlayer);
            }

            navMeshAgent.SetDestination(playerTransform.position);
        }
    }

    private void HandlePatrolState()
    {
        if (currentState == State.Chase || currentState == State.Attack)
        {
            currentState = State.Idle;
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            waitTimer = 0f;
        }

        switch (currentState)
        {
            case State.Idle:
                waitTimer += Time.deltaTime;
                if (waitTimer >= currentWaitTime)
                {
                    SetRandomPatrolDestination();
                }
                break;

            case State.Patrol:
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    currentState = State.Idle;
                    navMeshAgent.isStopped = true;
                    navMeshAgent.velocity = Vector3.zero;
                    waitTimer = 0f;
                    currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
                }
                break;
        }
    }

    private void SetRandomPatrolDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * detectionRange;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
        {
            currentState = State.Patrol;
            navMeshAgent.speed = walkSpeed;
            navMeshAgent.isStopped = false;

            Vector3 directionToPoint = (hit.position - transform.position).normalized;
            if (directionToPoint.sqrMagnitude > 0.001f)
            {
                Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(directionToPoint), Time.deltaTime * 5f);
            }

            navMeshAgent.SetDestination(hit.position);
        }
    }

    private IEnumerator PerformRangedAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        Vector3 targetPos = playerTransform.position;
        GameObject activeIndicator = null;

        if (indicatorPrefab != null)
        {
            
            activeIndicator = Instantiate(indicatorPrefab, targetPos, Quaternion.identity);
        }

        
        yield return new WaitForSeconds(throwDelay);

        if (projectilePrefab != null && throwPoint != null)
        {
            GameObject projObj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
            EnemyProjectile proj = projObj.GetComponent<EnemyProjectile>();

            if (proj != null)
            {
                proj.Initialize(
                    throwPoint.position,
                    targetPos,
                    projectileTravelTime,
                    projectileArcHeight,
                    attackDamage,
                    activeIndicator);
            }
        }
        else if (activeIndicator != null)
        {
           
            Destroy(activeIndicator);
        }

        isAttacking = false;
    }

    private void OnDrawGizmos()
    {
        if (navMeshAgent != null && navMeshAgent.hasPath)
        {
            Gizmos.color = currentState == State.Chase ? Color.red : Color.green;
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