using System.Collections.Generic;
using UnityEngine;

public class WorldCreator : MonoBehaviour
{
    public WorldSettings_SO worldSettings;
    private GameObject[] landObjects;
    private GameObject[] grassObjects;
    private GameObject[] treeObjects;
    private GameObject[] topObjects;

    [Header("World Settings")]
    private int worldWidth = 32;
    private int worldHeight = 32;
    private float tileSize = 1f;
    private bool generateOnStart = true;
    private int seed = 12345;
    private float noiseScale = 10f;
    private float lowThreshold = 0.35f;
    private float highThreshold = 0.75f;

    private float grassChance = 0.35f;
    private float treeChance = 0.15f;
    private float topChance = 0.08f;

    [Header("Spawn Offsets")]
    private float landYOffset = 0f;
    private float grassYOffset = 0.05f;
    private float treeYOffset = 0.5f;
    private float topYOffset = 0.5f;
    private float grassTileOffset = 0.35f;
    private float treeTileOffset = 0.25f;

    [Header("World Parents")]
    private Transform landParent;
    private Transform grassParent;
    private Transform treeParent;
    private Transform topParent;

    private readonly HashSet<Vector2Int> occupiedDecorativeCells = new HashSet<Vector2Int>();

    private void Start()
    {
        InitializeSettings();
        if (generateOnStart)
        {
            Generate();
        }
    }
    private void InitializeSettings()
    {
        landObjects = worldSettings != null ? worldSettings.GetLandObjects() : null;
        grassObjects = worldSettings != null ? worldSettings.GetGrassObjects() : null;
        treeObjects = worldSettings != null ? worldSettings.GetTreeObjects() : null;
        topObjects = worldSettings != null ? worldSettings.GetTopObjects() : null;
        worldWidth = worldSettings != null ? worldSettings.worldWidth : worldWidth;
        worldHeight = worldSettings != null ? worldSettings.worldHeight : worldHeight;
        tileSize = worldSettings != null ? worldSettings.tileSize : tileSize;
        seed = worldSettings != null ? worldSettings.seed : seed;
        noiseScale = worldSettings != null ? worldSettings.noiseScale : noiseScale;
        lowThreshold = worldSettings != null ? worldSettings.lowThreshold : lowThreshold;
        highThreshold = worldSettings != null ? worldSettings.highThreshold : highThreshold;
        treeChance = worldSettings != null ? worldSettings.treeChance : treeChance;
        topChance = worldSettings != null ? worldSettings.topChance : topChance;
        landYOffset = worldSettings != null ? worldSettings.landYOffset : landYOffset;
        grassYOffset = worldSettings != null ? worldSettings.grassYOffset : grassYOffset;
        treeYOffset = worldSettings != null ? worldSettings.treeYOffset : treeYOffset;
        topYOffset = worldSettings != null ? worldSettings.topYOffset : topYOffset;
        grassTileOffset = worldSettings != null ? worldSettings.grassTileOffset : grassTileOffset;
        treeTileOffset = worldSettings != null ? worldSettings.treeTileOffset : treeTileOffset;
    }

    public void Generate()
    {
        ClearWorld();
        GenerateWorld();
    }

    [ContextMenu("Generate World")]
    public void GenerateWorld()
    {
        InitializeSettings();
        Random.InitState(seed);

        EnsureParents();

        float offsetX = Random.Range(-10000f, 10000f);
        float offsetY = Random.Range(-10000f, 10000f);

        float[,] heightMap = GenerateHeightMap(offsetX, offsetY);

        for (int y = 0; y < worldHeight; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                float height = heightMap[x, y];

                if (height < lowThreshold)
                {
                    continue;
                }

                Vector2Int cell = new Vector2Int(x, y);
                Vector3 basePosition = new Vector3(x * tileSize, landYOffset, y * tileSize);

                SpawnRandomPrefab(landObjects, basePosition, Quaternion.identity, landParent, 1f, false);

                if (height >= highThreshold)
                {
                    Vector3 topPosition = basePosition + Vector3.up * topYOffset;
                    SpawnRandomPrefab(topObjects, topPosition, Quaternion.identity, topParent, topChance, true);
                }
                else
                {
                    TrySpawnDecorativePrefab(
                        grassObjects,
                        cell,
                        basePosition,
                        grassParent,
                        grassChance,
                        grassTileOffset,
                        grassYOffset,
                        false);

                    TrySpawnDecorativePrefab(
                        treeObjects,
                        cell,
                        basePosition,
                        treeParent,
                        treeChance,
                        treeTileOffset,
                        treeYOffset,
                        false);
                }
            }
        }
    }

    [ContextMenu("Clear World")]
    public void ClearWorld()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        landParent = null;
        grassParent = null;
        treeParent = null;
        topParent = null;
        occupiedDecorativeCells.Clear();
    }

    private float[,] GenerateHeightMap(float offsetX, float offsetY)
    {
        float[,] heightMap = new float[worldWidth, worldHeight];

        for (int y = 0; y < worldHeight; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                float sampleX = (x / noiseScale) + offsetX;
                float sampleY = (y / noiseScale) + offsetY;
                heightMap[x, y] = Mathf.PerlinNoise(sampleX, sampleY);
            }
        }

        return heightMap;
    }

    private void EnsureParents()
    {
        if (landParent == null)
        {
            landParent = CreateParent("Land");
        }

        if (grassParent == null)
        {
            grassParent = CreateParent("Grass");
        }

        if (treeParent == null)
        {
            treeParent = CreateParent("Trees");
        }

        if (topParent == null)
        {
            topParent = CreateParent("Top");
        }
    }

    private Transform CreateParent(string parentName)
    {
        GameObject parentObject = new GameObject(parentName);
        parentObject.transform.SetParent(transform, false);
        return parentObject.transform;
    }

    private void TrySpawnDecorativePrefab(
        GameObject[] prefabs,
        Vector2Int cell,
        Vector3 basePosition,
        Transform parent,
        float chance,
        float xzOffset,
        float yOffset,
        bool addBoxCollider)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return;
        }

        if (occupiedDecorativeCells.Contains(cell))
        {
            return;
        }

        if (Random.value >= chance)
        {
            return;
        }

        Vector3 position = basePosition + new Vector3(
            Random.Range(-xzOffset, xzOffset),
            yOffset,
            Random.Range(-xzOffset, xzOffset));

        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        if (SpawnRandomPrefab(prefabs, position, rotation, parent, 1f, addBoxCollider) != null)
        {
            occupiedDecorativeCells.Add(cell);
        }
    }

    private GameObject SpawnRandomPrefab(GameObject[] prefabs, Vector3 position, Quaternion rotation, Transform parent, float chance, bool addBoxCollider)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        if (Random.value >= chance)
        {
            return null;
        }

        int index = Random.Range(0, prefabs.Length);
        GameObject prefab = prefabs[index];

        if (prefab == null)
        {
            return null;
        }

        GameObject instance = Instantiate(prefab, position, rotation, parent);
        instance.name = prefab.name;

        if (addBoxCollider)
        {
            EnsureBoxCollider(instance);
        }

        return instance;
    }

    private void EnsureBoxCollider(GameObject instance)
    {
        if (instance == null || instance.GetComponentInChildren<Collider>() != null)
        {
            return;
        }

        instance.AddComponent<BoxCollider>();
    }
}
