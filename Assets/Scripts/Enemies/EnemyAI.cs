using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Simple three-state AI: Patrol → Chase → Attack.
/// Requires a NavMeshAgent on the same GameObject and the scene to be baked.
/// </summary>
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(NavMeshAgent))]
public sealed class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField, Min(0f)] private float detectionRange = 10f;
    [SerializeField, Min(0f)] private float chaseRange = 15f;

    [Header("Patrol")]
    [SerializeField, Min(0f)] private float patrolRadius = 5f;
    [SerializeField, Min(0f)] private float patrolWaitTime = 2f;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float patrolSpeed = 2f;
    [SerializeField, Min(0f)] private float chaseSpeed = 4f;

    private enum State { Patrol, Chase, Attack }

    private State currentState = State.Patrol;
    private Transform playerTransform;
    private EnemyStats enemyStats;
    private NavMeshAgent agent;
    private Economy economy;
    private Vector3 patrolOrigin;
    private float patrolWaitTimer;
    private float attackTimer;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
        agent = GetComponent<NavMeshAgent>();
        patrolOrigin = transform.position;
    }

    private void Start()
    {
        enemyStats.OnDeath += HandleDeath;
        economy = FindObjectOfType<Economy>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    private void OnDestroy()
    {
        if (enemyStats != null)
        {
            enemyStats.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        if (!enemyStats.IsAlive)
        {
            return;
        }

        switch (currentState)
        {
            case State.Patrol:
                UpdatePatrol();
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.Attack:
                UpdateAttack();
                break;
        }
    }

    private void UpdatePatrol()
    {
        if (playerTransform != null &&
            Vector3.Distance(transform.position, playerTransform.position) <= detectionRange)
        {
            currentState = State.Chase;
            return;
        }

        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
            {
                Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
                Vector3 randomPoint = patrolOrigin + new Vector3(randomCircle.x, 0f, randomCircle.y);

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }

                patrolWaitTimer = patrolWaitTime;
            }
        }
    }

    private void UpdateChase()
    {
        if (playerTransform == null)
        {
            currentState = State.Patrol;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > chaseRange)
        {
            currentState = State.Patrol;
            return;
        }

        if (distanceToPlayer <= enemyStats.AttackRange)
        {
            agent.ResetPath();
            currentState = State.Attack;
            return;
        }

        agent.speed = chaseSpeed;
        agent.SetDestination(playerTransform.position);
    }

    private void UpdateAttack()
    {
        if (playerTransform == null)
        {
            currentState = State.Patrol;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > enemyStats.AttackRange)
        {
            currentState = State.Chase;
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(enemyStats.AttackDamage);
            }

            attackTimer = enemyStats.AttackCooldown;
        }
    }

    private void HandleDeath(EnemyStats stats)
    {
        if (economy != null)
        {
            economy.AddCoins(stats.CoinReward);
        }

        Destroy(gameObject, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
