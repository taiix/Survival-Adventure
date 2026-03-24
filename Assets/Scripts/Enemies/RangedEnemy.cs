using System.Collections;
using UnityEngine;

public class RangedEnemy : BaseEnemy
{
    [Header("Ranged Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwDelay = 0.5f;
    [SerializeField] private float projectileTravelTime = 1.5f;
    [SerializeField] private float projectileArcHeight = 3f;

    private bool isAttacking;

    protected override void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown || isAttacking)
            return;

        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        StartCoroutine(PerformRangedAttack());
    }

    private IEnumerator PerformRangedAttack()
    {
        isAttacking = true;

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
}