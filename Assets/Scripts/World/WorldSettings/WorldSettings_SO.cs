using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldSettings_SO", menuName = "Scriptable Objects/WorldSettings")]
public class WorldSettings_SO : ScriptableObject
{
    public GameObject[] landObjects;
    public GameObject[] grassObjects;
    public GameObject[] treeObjects;
    public GameObject[] topObjects;

    [Header("World Settings")]
    [Min(1)] public int worldWidth = 32;
    [Min(1)] public int worldHeight = 32;
    [Min(0.1f)] public float tileSize = 1f;
    public bool generateOnStart = true;
    public int seed = 12345;
    [Min(0.01f)] public float noiseScale = 10f;
    [Space]
    [Range(0f, 1f)] public float lowThreshold = 0.35f;
    [Range(0f, 1f)] public float highThreshold = 0.75f;
    [Space]
    [Range(0f, 1f)] public float grassChance = 0.35f;
    [Range(0f, 1f)] public float treeChance = 0.15f;
    [Range(0f, 1f)] public float topChance = 0.08f;

    [Header("Spawn Offsets")]
    [Min(0f)] public float landYOffset = 0f;
    [Min(0f)] public float grassYOffset = 0.05f;
    [Min(0f)] public float treeYOffset = 0.5f;
    [Min(0f)] public float topYOffset = 0.5f;
    [Min(0f)] public float grassTileOffset = 0.35f;
    [Min(0f)] public float treeTileOffset = 0.25f;

    public GameObject[] GetGrassObjects() => grassObjects;

    public GameObject[] GetLandObjects() => landObjects;

    public GameObject[] GetTopObjects() => topObjects;

    public GameObject[] GetTreeObjects() => treeObjects;

}
