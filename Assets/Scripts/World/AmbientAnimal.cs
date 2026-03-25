using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Simple ambient animal that wanders and occasionally plays an idle animation.
/// Flees from the player when nearby.
/// Addresses issue #16 (animals that walk and jump for ambience).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AmbientAnimal : MonoBehaviour
{
    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float minIdleTime = 2f;
    [SerializeField] private float maxIdleTime = 6f;

    [Header("Flee")]
    [SerializeField] private float fleeRange = 6f;
    [SerializeField] private float fleeSpeed = 5f;
    [SerializeField] private float normalSpeed = 1.5f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private float idleTimer;
    private float currentIdleTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = normalSpeed;

        if (animator == null)
            animator = GetComponent<Animator>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        currentIdleTime = Random.Range(minIdleTime, maxIdleTime);
    }

    private void Update()
    {
        if (playerTransform != null &&
            Vector3.Distance(transform.position, playerTransform.position) < fleeRange)
        {
            Flee();
            return;
        }

        Wander();
        UpdateAnimator();
    }

    private void Flee()
    {
        agent.speed = fleeSpeed;
        Vector3 fleeDir = (transform.position - playerTransform.position).normalized;
        Vector3 target = transform.position + fleeDir * fleeRange;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 3f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    private void Wander()
    {
        agent.speed = normalSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= currentIdleTime)
            {
                idleTimer = 0f;
                currentIdleTime = Random.Range(minIdleTime, maxIdleTime);
                SetRandomDestination();
            }
        }
    }

    private void SetRandomDestination()
    {
        Vector2 rand = Random.insideUnitCircle * wanderRadius;
        Vector3 target = transform.position + new Vector3(rand.x, 0, rand.y);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 3f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, fleeRange);
    }
}
