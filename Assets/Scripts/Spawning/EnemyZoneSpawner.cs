using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Spawns a wave of enemies within a defined zone.
/// Supports respawning and elite enemy variants.
/// Addresses issues #17 (spawn enemy zones), #21 (spawn elite enemies).
/// </summary>
public class EnemyZoneSpawner : MonoBehaviour
{
    [Header("Zone")]
    [SerializeField] private float zoneRadius = 10f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Enemies")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject eliteEnemyPrefab;
    [SerializeField, Min(1)] private int minEnemies = 2;
    [SerializeField, Min(1)] private int maxEnemies = 5;
    [SerializeField, Range(0f, 1f)] private float eliteChance = 0.15f;

    [Header("Respawn")]
    [SerializeField] private bool respawnEnabled = true;
    [SerializeField, Min(5f)] private float respawnDelay = 30f;
    [SerializeField, Min(0f)] private float activationRadius = 25f;

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();
    private float respawnTimer;
    private bool playerInRange;

    private void Start()
    {
        SpawnEnemies();
    }

    private void Update()
    {
        CheckPlayerProximity();

        if (!respawnEnabled) return;
        if (!playerInRange) return;

        CleanDestroyedEnemies();
        if (spawnedEnemies.Count == 0)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnDelay)
            {
                respawnTimer = 0f;
                SpawnEnemies();
            }
        }
    }

    private void CheckPlayerProximity()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { playerInRange = false; return; }
        playerInRange = Vector3.Distance(transform.position, player.transform.position) <= activationRadius;
    }

    private void SpawnEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        int count = Random.Range(minEnemies, maxEnemies + 1);
        for (int i = 0; i < count; i++)
        {
            bool spawnElite = eliteEnemyPrefab != null && Random.value < eliteChance;
            GameObject prefab = spawnElite
                ? eliteEnemyPrefab
                : enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            if (prefab == null) continue;

            Vector3 spawnPos;
            if (TryGetNavMeshPosition(out spawnPos))
            {
                GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0, 360f), 0));
                spawnedEnemies.Add(enemy);
            }
        }
    }

    private bool TryGetNavMeshPosition(out Vector3 result)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 rand2D = Random.insideUnitCircle * zoneRadius;
            Vector3 candidate = transform.position + new Vector3(rand2D.x, 0, rand2D.y);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = transform.position;
        return false;
    }

    private void CleanDestroyedEnemies()
    {
        spawnedEnemies.RemoveAll(e => e == null);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
        Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
