using UnityEngine;

/// <summary>
/// In-world item that can be picked up by the player on proximity or interaction.
/// Addresses issues #32, #59 (item pickups).
/// </summary>
public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private float autoPickupRadius = 1.2f;
    [SerializeField] private bool autoPickup = true;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float rotationSpeed = 90f;

    public void Initialize(ItemData data) { itemData = data; }

    private Vector3 startPosition;
    private bool collected;

    public string DisplayName => itemData != null ? itemData.itemName : "Item";
    public bool CanInteract => !collected && itemData != null;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (collected) return;

        // Bob and rotate
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        if (autoPickup)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && Vector3.Distance(transform.position, player.transform.position) <= autoPickupRadius)
            {
                Collect(player);
            }
        }
    }

    public void OnInteract()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            Collect(player);
    }

    private void Collect(GameObject player)
    {
        if (collected || itemData == null) return;
        collected = true;

        switch (itemData.itemType)
        {
            case ItemData.ItemType.Gold:
                GoldManager.Instance?.AddGold(itemData.goldValue);
                break;

            case ItemData.ItemType.Healing:
                player.GetComponent<PlayerHealth>()?.Heal(itemData.healAmount);
                break;

            case ItemData.ItemType.Material:
                GoldManager.Instance?.AddGold(itemData.goldValue);
                break;

            case ItemData.ItemType.Consumable:
            case ItemData.ItemType.Artifact:
                // These are added to inventory – handled by higher-level systems
                break;
        }

        AudioManager.Instance?.PlayItemPickup();
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, autoPickupRadius);
    }
}
