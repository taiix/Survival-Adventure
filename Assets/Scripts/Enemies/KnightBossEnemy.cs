using System.Collections;
using UnityEngine;

/// <summary>
/// Full-featured Knight Boss enemy with:
///  - Three-phase AI state machine (patrol → aggressive → enraged)
///  - Multiple attack patterns (melee, charge, area sweep)
///  - Phase transitions at 60 % and 30 % HP
///  - Boss intro / defeat events for the BossEncounterManager
/// Addresses issues #15, #23-26, #44-45 (boss system, AI state machine, attack patterns).
/// </summary>
public class KnightBossEnemy : BaseEnemy
{
    // ── Boss-specific phase enum ────────────────────────────────────────────
    private enum BossPhase { Phase1, Phase2, Phase3 }

    [Header("Boss Phases")]
    [SerializeField, Range(0f, 1f)] private float phase2Threshold = 0.6f;
    [SerializeField, Range(0f, 1f)] private float phase3Threshold = 0.3f;

    [Header("Charge Attack")]
    [SerializeField] private float chargeSpeed = 14f;
    [SerializeField] private float chargeDistance = 8f;
    [SerializeField] private float chargePreparationTime = 0.8f;
    [SerializeField] private float chargeDuration = 0.5f;
    [SerializeField] private float chargeAttackDamage = 20f;
    [SerializeField] private float chargeCooldown = 5f;

    [Header("Area Sweep")]
    [SerializeField] private float sweepRadius = 3f;
    [SerializeField] private float sweepDamage = 15f;
    [SerializeField] private float sweepCooldown = 7f;

    [Header("Enraged Multipliers")]
    [SerializeField] private float phase2SpeedMultiplier = 1.3f;
    [SerializeField] private float phase3SpeedMultiplier = 1.7f;
    [SerializeField] private float phase3DamageMultiplier = 1.5f;

    [Header("Melee Collider")]
    [SerializeField] private Collider swordCollider;

    [Header("Events")]
    public System.Action OnBossDefeated;
    public System.Action OnBossIntroComplete;

    private BossPhase currentPhase = BossPhase.Phase1;
    private float lastChargeTime = -100f;
    private float lastSweepTime = -100f;
    private bool isCharging;
    private bool performingSweep;
    private bool introComplete;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        if (!introComplete) return;
        CheckPhaseTransition();
        base.Update();
    }

    // ── Phase management ────────────────────────────────────────────────────

    private void CheckPhaseTransition()
    {
        float hpRatio = currentHealth / maxHealth;

        if (hpRatio <= phase3Threshold && currentPhase != BossPhase.Phase3)
            EnterPhase(BossPhase.Phase3);
        else if (hpRatio <= phase2Threshold && currentPhase == BossPhase.Phase1)
            EnterPhase(BossPhase.Phase2);
    }

    private void EnterPhase(BossPhase phase)
    {
        currentPhase = phase;

        switch (phase)
        {
            case BossPhase.Phase2:
                navMeshAgent.speed = chasingSpeed * phase2SpeedMultiplier;
                break;
            case BossPhase.Phase3:
                navMeshAgent.speed = chasingSpeed * phase3SpeedMultiplier;
                attackDamage *= phase3DamageMultiplier;
                break;
        }

        if (animator != null)
            animator.SetTrigger("PhaseTransition");
    }

    // ── Override combat to add special attacks ──────────────────────────────

    protected override void HandleCombatState()
    {
        if (isCharging || performingSweep)
            return;

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Try charge if player is far enough and phase allows
        if (currentPhase >= BossPhase.Phase2 &&
            distToPlayer >= chargeDistance * 0.5f &&
            Time.time - lastChargeTime >= chargeCooldown)
        {
            StartCoroutine(ChargeAttack());
            return;
        }

        // Try area sweep if player is close and phase allows
        if (currentPhase >= BossPhase.Phase2 &&
            distToPlayer <= sweepRadius &&
            Time.time - lastSweepTime >= sweepCooldown)
        {
            StartCoroutine(AreaSweep());
            return;
        }

        base.HandleCombatState();
    }

    // ── Charge attack ───────────────────────────────────────────────────────

    private IEnumerator ChargeAttack()
    {
        isCharging = true;
        lastChargeTime = Time.time;

        StopMovement();
        FacePlayer();

        if (animator != null)
            animator.SetTrigger("ChargePrep");

        yield return new WaitForSeconds(chargePreparationTime);

        if (playerTransform == null)
        {
            isCharging = false;
            yield break;
        }

        Vector3 chargeDirection = (playerTransform.position - transform.position).normalized;
        Vector3 chargeTarget = transform.position + chargeDirection * chargeDistance;
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = chargeSpeed;

        float elapsed = 0f;
        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;
            navMeshAgent.SetDestination(chargeTarget);

            if (Vector3.Distance(transform.position, playerTransform.position) < attackRange)
            {
                playerTransform.GetComponent<IDamageable>()?.TakeDamage(chargeAttackDamage);
                break;
            }

            yield return null;
        }

        // Restore appropriate speed for current phase
        navMeshAgent.speed = currentPhase == BossPhase.Phase3
            ? chasingSpeed * phase3SpeedMultiplier
            : currentPhase == BossPhase.Phase2
                ? chasingSpeed * phase2SpeedMultiplier
                : chasingSpeed;

        isCharging = false;
    }

    // ── Area sweep ──────────────────────────────────────────────────────────

    private IEnumerator AreaSweep()
    {
        performingSweep = true;
        lastSweepTime = Time.time;

        StopMovement();

        if (animator != null)
            animator.SetTrigger("Sweep");

        yield return new WaitForSeconds(0.4f);

        Collider[] hits = Physics.OverlapSphere(transform.position, sweepRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
                hit.GetComponent<IDamageable>()?.TakeDamage(sweepDamage);
        }

        yield return new WaitForSeconds(0.4f);
        performingSweep = false;
    }

    // ── Melee (called via Animation Event) ──────────────────────────────────

    private void DealMeleeDamage()
    {
        if (swordCollider == null) return;

        Collider[] hits = Physics.OverlapSphere(
            swordCollider.bounds.center,
            swordCollider.bounds.extents.magnitude);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
        }
    }

    // ── Intro sequence ───────────────────────────────────────────────────────

    /// <summary>Call this from BossEncounterManager to start the boss.</summary>
    public void StartIntro(float delay = 1f)
    {
        StartCoroutine(IntroRoutine(delay));
    }

    private IEnumerator IntroRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        introComplete = true;
        OnBossIntroComplete?.Invoke();
    }

    // ── Death ────────────────────────────────────────────────────────────────

    protected override void Die()
    {
        OnBossDefeated?.Invoke();
        base.Die();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, sweepRadius);
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, chargeDistance);
    }
}
