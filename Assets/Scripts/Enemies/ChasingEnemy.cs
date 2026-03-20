using UnityEngine;
using UnityEngine.AI;

public class ChasingEnemy : BaseEnemy
{
    private enum State { Idle, Patrol, Chase, Attack }
    private State currentState = State.Idle;

    [Header("Patrol")]
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;

    private float waitTimer;
    private float currentWaitTime;

    protected override void Awake()
    {
        base.Awake();
        // Initialize random wait time
        currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
    }

    protected override void UpdateBehavior()
    {
        // 1. Priority: Check Player Detection
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
        // While engaging the player, ensure we aren't stuck in patrol logic
        if (IsPlayerInAttackRange())
        {
            if (currentState != State.Attack)
            {
                currentState = State.Attack;
                navMeshAgent.isStopped = true;
                navMeshAgent.velocity = Vector3.zero;
            }

            // Face player instantly (ignoring verticality)
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            directionToPlayer.y = 0;
            if (directionToPlayer.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(directionToPlayer);
            }

            Attack();
        }
        else
        {
            if (currentState != State.Chase)
            {
                currentState = State.Chase;
                navMeshAgent.speed = chasingSpeed; // Run speed
                navMeshAgent.isStopped = false;
            }

            // Keep chasing the moving player
            navMeshAgent.SetDestination(playerTransform.position);
        }
    }

    private void HandlePatrolState()
    {
        // If we just lost the player or finished a fight, reset to Idle first
        if (currentState == State.Chase || currentState == State.Attack)
        {
            currentState = State.Idle;
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            waitTimer = 0f; // Start waiting immediately
        }

        switch (currentState)
        {
            case State.Idle:
                // Wait for a bit before moving to next patrol point
                waitTimer += Time.deltaTime;
                if (waitTimer >= currentWaitTime)
                {
                    SetRandomPatrolDestination();
                }
                break;

            case State.Patrol:
                // Check if we reached the destination
                // pathPending is crucial: "Are we still calculating the path?"
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    // Arrived at patrol point, go back to Idle
                    currentState = State.Idle;
                    navMeshAgent.isStopped = true;
                    navMeshAgent.velocity = Vector3.zero;

                    // Pick a new random wait time
                    waitTimer = 0f;
                    currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
                }
                break;
        }
    }

    private void SetRandomPatrolDestination()
    {
        // Try to find a valid point on the NavMesh within detectionRange
        Vector3 randomDirection = Random.insideUnitSphere * detectionRange;
        randomDirection += transform.position;

        NavMeshHit hit;
        // Look for a point on the NavMesh within 5 units of our random point
        if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
        {
            currentState = State.Patrol;
            navMeshAgent.speed = walkSpeed; // Walk speed
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(hit.position);
        }
        // If we fail to find a point (e.g. wall void), we stay in Idle and try again next frame
    }

    private void OnDrawGizmos()
    {
        // Visualize Path
        if (navMeshAgent != null && navMeshAgent.hasPath)
        {
            Gizmos.color = currentState == State.Chase ? Color.red : Color.green;
            var path = navMeshAgent.path;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
        }

        // Visualize Detection Range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}