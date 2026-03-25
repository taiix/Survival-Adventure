using UnityEngine;

/// <summary>
/// In-world rare artifact pickup.
/// Addresses issue #31 (Rare artifact system).
/// </summary>
public class ArtifactPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ArtifactData artifactData;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 60f;

    private Vector3 startPosition;
    private bool collected;

    public string DisplayName => artifactData != null ? artifactData.artifactName : "Artifact";
    public bool CanInteract => !collected && artifactData != null;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (collected) return;
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    public void OnInteract()
    {
        if (collected || artifactData == null) return;

        ArtifactInventory inventory = ArtifactInventory.Instance;
        if (inventory == null || !inventory.TryAdd(artifactData))
        {
            Debug.Log("Artifact inventory full or duplicate.");
            return;
        }

        collected = true;
        Destroy(gameObject);
    }
}
