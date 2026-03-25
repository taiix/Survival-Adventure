using UnityEngine;

/// <summary>
/// Blacksmith NPC that opens the upgrade UI when the player interacts.
/// Addresses issues #4, #14 (city with upgrade tools, blacksmith).
/// </summary>
public class BlacksmithNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Blacksmith";
    [SerializeField] private GameObject upgradeUI;

    public string DisplayName => npcName;
    public bool CanInteract => enabled && gameObject.activeSelf;

    private void Start()
    {
        if (upgradeUI != null)
            upgradeUI.SetActive(false);
    }

    public void OnInteract()
    {
        if (upgradeUI == null) return;
        bool show = !upgradeUI.activeSelf;
        upgradeUI.SetActive(show);
        AudioManager.Instance?.PlayUIClick();
    }

    public GameObject GetAssociatedUI() => upgradeUI;
}
