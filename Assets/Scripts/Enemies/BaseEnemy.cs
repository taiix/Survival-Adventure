using UnityEngine;
using UnityEngine.AI;

public abstract class BaseEnemy : MonoBehaviour
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

    // Threshold matching your blend trees (e.g., 0.5 is walk, 1.0 is run)
    [Header("Animator")]
    [SerializeField, Range(0f, 1f)] protected float sprintThreshold = 0.5f;

    protected float currentHealth;
    protected float lastAttackTime;
    protected Transform playerTransform;
    protected NavMeshAgent navMeshAgent;
    protected Animator animator;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
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

    // Override this in derived classes
    protected abstract void UpdateBehavior();

    protected virtual void UpdateAnimator()
    {
        if (animator == null || navMeshAgent == null) return;

        // Update animation based on velocity converted to normalized animator parameters
        float speed = navMeshAgent.velocity.magnitude;
        float normalizedSpeed = 0f;

        if (speed > 0.1f)
        {
            if (speed > walkSpeed)
            {
                float t = (speed - walkSpeed) / (chasingSpeed - walkSpeed);
                normalizedSpeed = Mathf.Lerp(sprintThreshold, 1f, t);
            }
            else
            {
                float t = speed / walkSpeed;
                normalizedSpeed = Mathf.Lerp(0f, sprintThreshold, t);
            }
        }

        animator.SetFloat("Speed", normalizedSpeed);
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

        // TRIGGER THE ANIMATION
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}