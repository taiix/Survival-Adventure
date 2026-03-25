using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages which biomes are unlocked and which is currently active.
/// Addresses issues #40 (biome unlock logic).
/// </summary>
public class BiomeManager : MonoBehaviour
{
    public static BiomeManager Instance { get; private set; }

    [SerializeField] private List<BiomeData> allBiomes = new List<BiomeData>();

    private readonly HashSet<string> unlockedBiomes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private BiomeData activeBiome;

    public IReadOnlyList<BiomeData> AllBiomes => allBiomes;
    public BiomeData ActiveBiome => activeBiome;

    public event Action<BiomeData> OnBiomeUnlocked;
    public event Action<BiomeData> OnBiomeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Unlock default biomes
        foreach (BiomeData biome in allBiomes)
        {
            if (biome != null && biome.unlockedByDefault)
                unlockedBiomes.Add(biome.biomeName);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool IsBiomeUnlocked(BiomeData biome)
    {
        if (biome == null) return false;
        return biome.unlockedByDefault || unlockedBiomes.Contains(biome.biomeName);
    }

    public void UnlockBiome(string biomeName)
    {
        if (string.IsNullOrWhiteSpace(biomeName)) return;
        if (unlockedBiomes.Add(biomeName))
        {
            BiomeData found = allBiomes.Find(b => b != null &&
                string.Equals(b.biomeName, biomeName, StringComparison.OrdinalIgnoreCase));
            if (found != null)
                OnBiomeUnlocked?.Invoke(found);
        }
    }

    public void SelectBiome(BiomeData biome)
    {
        if (biome == null || !IsBiomeUnlocked(biome)) return;
        activeBiome = biome;

        if (biome.biomeMusic != null)
            AudioManager.Instance?.PlayMusic(biome.biomeMusic);

        OnBiomeChanged?.Invoke(activeBiome);

        if (biome.sceneIndex >= 0)
            UnityEngine.SceneManagement.SceneManager.LoadScene(biome.sceneIndex);
    }

    /// <summary>Called by BossEncounterManager after a boss is defeated.</summary>
    public void OnBossDefeated(string defeatedBossName = "")
    {
        foreach (BiomeData biome in allBiomes)
        {
            if (biome == null) continue;
            if (!string.IsNullOrEmpty(biome.requiredBossDefeat) &&
                string.Equals(biome.requiredBossDefeat, defeatedBossName, StringComparison.OrdinalIgnoreCase))
            {
                UnlockBiome(biome.biomeName);
            }
        }
    }
}
