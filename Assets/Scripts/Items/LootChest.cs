using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A loot chest the player can interact with to receive random items.
/// Addresses issue #19 (spawn loot chests).
/// </summary>
public class LootChest : MonoBehaviour, IInteractable
{
    [Header("Loot")]
    [SerializeField] private List<ItemData> possibleLoot = new List<ItemData>();
    [SerializeField, Min(1)] private int minDrops = 1;
    [SerializeField, Min(1)] private int maxDrops = 3;
    [SerializeField] private int bonusGold = 20;

    [Header("Visual")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private float dropSpreadRadius = 0.8f;

    private bool opened;

    public string DisplayName => opened ? "Empty Chest" : "Chest";
    public bool CanInteract => !opened;

    public void OnInteract()
    {
        if (opened) return;
        opened = true;

        if (chestAnimator != null)
            chestAnimator.SetTrigger("Open");

        AudioManager.Instance?.PlayChestOpen();

        if (bonusGold > 0)
            GoldManager.Instance?.AddGold(bonusGold);

        SpawnLoot();
    }

    private void SpawnLoot()
    {
        if (possibleLoot == null || possibleLoot.Count == 0 || itemDropPrefab == null)
            return;

        int count = Random.Range(minDrops, maxDrops + 1);
        for (int i = 0; i < count; i++)
        {
            ItemData chosen = possibleLoot[Random.Range(0, possibleLoot.Count)];
            if (chosen == null) continue;

            Vector2 offset2D = Random.insideUnitCircle * dropSpreadRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset2D.x, 0.5f, offset2D.y);

            GameObject drop = Instantiate(itemDropPrefab, spawnPos, Quaternion.identity);
            ItemPickup pickup = drop.GetComponent<ItemPickup>();
            pickup?.Initialize(chosen);
        }
    }
}
