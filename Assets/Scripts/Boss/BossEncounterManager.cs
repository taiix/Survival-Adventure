using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the full boss encounter: spawning, intro, health bar, defeat/reward.
/// Addresses issues #28, #29, #38, #43 (detect boss defeat, transfer reward, boss intro UI, reward drop).
/// </summary>
public class BossEncounterManager : MonoBehaviour
{
    public static BossEncounterManager Instance { get; private set; }

    [Header("Boss")]
    [SerializeField] private KnightBossEnemy bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;

    [Header("Reward")]
    [SerializeField] private int goldReward = 200;
    [SerializeField] private GameObject[] possibleDropPrefabs;
    [SerializeField, Min(1)] private int dropCount = 2;

    [Header("UI")]
    [SerializeField] private GameObject bossEncounterUI;
    [SerializeField] private GameObject bossIntroPanel;
    [SerializeField] private TMP_Text bossNameLabel;
    [SerializeField] private string bossDisplayName = "The Iron Knight";

    [Header("Audio")]
    [SerializeField] private bool playBossMusic = true;

    private KnightBossEnemy activeBoss;
    private bool encounterActive;

    public event System.Action OnBossEncounterStart;
    public event System.Action OnBossEncounterEnd;

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

    /// <summary>Triggers the boss encounter from an external trigger zone.</summary>
    public void TriggerEncounter()
    {
        if (encounterActive) return;
        encounterActive = true;

        if (playBossMusic)
            AudioManager.Instance?.PlayBossTheme();

        SpawnBoss();
        ShowIntro();
        OnBossEncounterStart?.Invoke();
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null) return;

        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        activeBoss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        activeBoss.OnBossDefeated = HandleBossDefeated;
        activeBoss.OnBossIntroComplete = () =>
        {
            HideBossIntroPanel();
        };

        // Show boss HP bar
        BossHealthBar healthBar = FindObjectOfType<BossHealthBar>();
        if (healthBar != null)
        {
            healthBar.SetBoss(activeBoss);
            healthBar.gameObject.SetActive(true);
        }
    }

    private void ShowIntro()
    {
        if (bossIntroPanel != null)
        {
            bossIntroPanel.SetActive(true);
            if (bossNameLabel != null)
                bossNameLabel.text = bossDisplayName;
        }

        // Delay intro completion by 3 seconds
        activeBoss?.StartIntro(3f);
    }

    private void HideBossIntroPanel()
    {
        if (bossIntroPanel != null)
            bossIntroPanel.SetActive(false);
    }

    private void HandleBossDefeated()
    {
        encounterActive = false;

        // Grant gold reward
        GoldManager.Instance?.AddGold(goldReward);

        // Spawn drops
        if (activeBoss != null)
            SpawnDrops(activeBoss.transform.position);

        // Hide UI
        BossHealthBar healthBar = FindObjectOfType<BossHealthBar>();
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        // Switch back to normal music
        AudioManager.Instance?.PlayForest();

        activeBoss = null;
        OnBossEncounterEnd?.Invoke();

        // Notify biome manager that boss is defeated
        BiomeManager.Instance?.OnBossDefeated();
    }

    private void SpawnDrops(Vector3 position)
    {
        if (possibleDropPrefabs == null || possibleDropPrefabs.Length == 0) return;

        for (int i = 0; i < dropCount; i++)
        {
            GameObject drop = possibleDropPrefabs[Random.Range(0, possibleDropPrefabs.Length)];
            if (drop == null) continue;

            Vector2 offset = Random.insideUnitCircle * 1.5f;
            Vector3 spawnPos = position + new Vector3(offset.x, 0.5f, offset.y);
            Instantiate(drop, spawnPos, Quaternion.identity);
        }
    }
}
