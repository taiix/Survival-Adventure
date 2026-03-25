using UnityEngine;

/// <summary>
/// Spawns a particle effect at a position with automatic cleanup.
/// Also provides a convenience static factory method.
/// Addresses issue #61 (particle effects).
/// </summary>
public class ParticleEffectSpawner : MonoBehaviour
{
    [Header("Effects Library")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private GameObject levelUpEffectPrefab;
    [SerializeField] private GameObject itemPickupEffectPrefab;

    public static ParticleEffectSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SpawnHitEffect(Vector3 position)    => Spawn(hitEffectPrefab, position);
    public void SpawnDeathEffect(Vector3 position)  => Spawn(deathEffectPrefab, position);
    public void SpawnLevelUp(Vector3 position)      => Spawn(levelUpEffectPrefab, position);
    public void SpawnPickup(Vector3 position)       => Spawn(itemPickupEffectPrefab, position);

    private void Spawn(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;
        GameObject fx = Instantiate(prefab, position, Quaternion.identity);

        // Auto-destroy: use longest possible particle lifetime
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        float lifetime = 3f;
        if (ps != null)
        {
            var main = ps.main;
            float startLifetime = main.startLifetime.mode == ParticleSystemCurveMode.Constant
                ? main.startLifetime.constant
                : main.startLifetime.constantMax;
            lifetime = main.duration + startLifetime;
        }
        Destroy(fx, lifetime);
    }
}
