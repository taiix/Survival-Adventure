using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 startPoint;
    private Vector3 targetPoint;
    private float travelTime;
    private float arcHeight;
    private float damageAmount;
    private float elapsedTime;

    private GameObject landingIndicator;

    public void Initialize(Vector3 start, Vector3 target, float duration, float height, float damage, GameObject indicator)
    {
        startPoint = start;
        targetPoint = target;
        travelTime = duration;
        arcHeight = height;
        damageAmount = damage;
        landingIndicator = indicator;
        elapsedTime = 0f;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / travelTime;

        if (progress >= 1f)
        {
            DestroyProjectileAndIndicator();
            return;
        }

        // Lerp position across a straight line while adjusting verticality using a sin wave
        Vector3 currentPosition = Vector3.Lerp(startPoint, targetPoint, progress);
        currentPosition.y += Mathf.Sin(progress * Mathf.PI) * arcHeight;

        transform.position = currentPosition;
    }

    private void DestroyProjectileAndIndicator()
    {
        if (landingIndicator != null)
        {
            Destroy(landingIndicator);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.TryGetComponent(out IDamageable playerHealth);
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }

            // Immediately destroy upon impacting the player
            DestroyProjectileAndIndicator();
        }
    }
}