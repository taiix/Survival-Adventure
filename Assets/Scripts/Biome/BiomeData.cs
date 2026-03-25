using UnityEngine;

/// <summary>
/// ScriptableObject representing one biome (e.g. Forest, City, Dungeon).
/// Addresses issues #39, #40, #60 (biome selection UI, unlock logic, forest theme).
/// </summary>
[CreateAssetMenu(fileName = "NewBiome", menuName = "Survival Adventure/Biome Data")]
public class BiomeData : ScriptableObject
{
    [Header("Identity")]
    public string biomeName = "Forest";
    [TextArea] public string description;
    public Sprite preview;

    [Header("Unlock")]
    public bool unlockedByDefault = false;
    [Tooltip("Name of the boss that must be defeated to unlock this biome.")]
    public string requiredBossName = "";

    [Header("Music")]
    public AudioClip biomeMusic;

    [Header("Scene")]
    [Tooltip("Build index of the scene for this biome. -1 = no scene transition.")]
    public int sceneIndex = -1;
}
