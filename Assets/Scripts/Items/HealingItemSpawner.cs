using UnityEngine;

/// <summary>
/// Spawns healing item pickups randomly in a radius around the spawner.
/// Addresses issue #22 (spawn healing items).
/// </summary>
public class HealingItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject healingPickupPrefab;
    [SerializeField, Min(1)] private int spawnCount = 3;
    [SerializeField, Min(0.5f)] private float spawnRadius = 6f;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField, Min(0f)] private float respawnInterval = 0f; // 0 = no respawn

    private float respawnTimer;

    private void Start()
    {
        if (spawnOnStart)
            SpawnItems();
    }

    private void Update()
    {
        if (respawnInterval <= 0f) return;
        respawnTimer += Time.deltaTime;
        if (respawnTimer >= respawnInterval)
        {
            respawnTimer = 0f;
            SpawnItems();
        }
    }

    public void SpawnItems()
    {
        if (healingPickupPrefab == null) return;
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 pos = transform.position + new Vector3(offset.x, 0.5f, offset.y);
            Instantiate(healingPickupPrefab, pos, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
