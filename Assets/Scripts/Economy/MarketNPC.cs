using UnityEngine;

/// <summary>
/// Market NPC that opens the market shop UI on interaction.
/// Addresses issues #35, #37 (market UI, purchase logic).
/// </summary>
public class MarketNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Merchant";
    [SerializeField] private GameObject marketUI;

    public string DisplayName => npcName;
    public bool CanInteract => enabled && gameObject.activeSelf;

    private void Start()
    {
        if (marketUI != null)
            marketUI.SetActive(false);
    }

    public void OnInteract()
    {
        if (marketUI == null) return;
        marketUI.SetActive(!marketUI.activeSelf);
        AudioManager.Instance?.PlayUIClick();
    }

    public GameObject GetAssociatedUI() => marketUI;
}
