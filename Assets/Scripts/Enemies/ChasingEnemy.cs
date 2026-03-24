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
        currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
    }

    protected override void UpdateBehavior()
    {
        
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

            Attack();
        }
        else
        {
            if (currentState != State.Chase)
            {
                currentState = State.Chase;
                navMeshAgent.speed = chasingSpeed; 
                navMeshAgent.isStopped = false;
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

                    // Pick a new random wait time
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
                //transform.rotation = Quaternion.LookRotation(directionToPoint);
                Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(directionToPoint), Time.deltaTime * 5f);
            }

            navMeshAgent.SetDestination(hit.position);
        }
     
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